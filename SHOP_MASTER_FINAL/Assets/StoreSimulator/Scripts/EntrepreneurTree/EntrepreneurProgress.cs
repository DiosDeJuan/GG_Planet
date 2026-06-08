//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurProgress
    {
        public static event Action onProgressChanged;

        private static readonly HashSet<string> unlockedNodeIds = new HashSet<string>();
        private static int progressPoints;

        static EntrepreneurProgress()
        {
            ResetToDefaults();
        }

        public static int ProgressPoints => progressPoints;
        public static int SecurityLevel => GetUnlockedSecurityLevel();
        public static float EmployeeWorkSpeedMultiplier => IsUpgradeUnlocked("mejora_cafeina") ? 1.1f : 1f;
        public static float CashierRevenueMultiplier => IsUpgradeUnlocked("mejora_carismatico") ? 1.05f : 1f;

        public static void ResetToDefaults()
        {
            unlockedNodeIds.Clear();
            unlockedNodeIds.Add(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId);
            progressPoints = 0;
            onProgressChanged?.Invoke();
        }

        public static bool IsUnlocked(string nodeId)
        {
            return !string.IsNullOrEmpty(nodeId) && unlockedNodeIds.Contains(nodeId);
        }

        public static bool IsProductUnlocked(ProductScriptableObject product)
        {
            if (product == null)
                return true;

            string nodeId = EntrepreneurTreeDefinitions.GetKnownProductNodeId(product);
            return string.IsNullOrEmpty(nodeId) || IsUnlocked(nodeId);
        }

        public static bool IsEmployeeUnlocked(int employeeNumber)
        {
            return employeeNumber > 0 && IsUnlocked("empleado_" + employeeNumber);
        }

        public static bool IsSecurityLevelUnlocked(int level)
        {
            return level <= 0 || GetUnlockedSecurityLevel() >= level;
        }

        public static bool IsProductGroupUnlocked(string groupId)
        {
            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(groupId);
            return node != null && node.UnlockType == EntrepreneurTreeNodeUnlockType.ProductGroup && IsUnlocked(groupId);
        }

        public static bool IsUpgradeUnlocked(string upgradeId)
        {
            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(upgradeId);
            return node != null && node.UnlockType == EntrepreneurTreeNodeUnlockType.Upgrade && IsUnlocked(upgradeId);
        }

        public static int GetUnlockedSecurityLevel()
        {
            if (IsUnlocked("seguridad_3")) return 3;
            if (IsUnlocked("seguridad_2")) return 2;
            if (IsUnlocked("seguridad_1")) return 1;
            return 0;
        }

        public static float GetEmployeeSpeedMultiplier()
        {
            return EmployeeWorkSpeedMultiplier;
        }

        public static float GetCashierSalesMultiplier()
        {
            return CashierRevenueMultiplier;
        }

        public static EntrepreneurTreeNodeState GetState(EntrepreneurTreeNodeDefinition node)
        {
            if (node == null)
                return EntrepreneurTreeNodeState.Locked;

            if (IsUnlocked(node.Id))
                return EntrepreneurTreeNodeState.Unlocked;

            return ArePrerequisitesUnlocked(node) && progressPoints >= node.Cost
                ? EntrepreneurTreeNodeState.Available
                : EntrepreneurTreeNodeState.Locked;
        }

        public static bool ArePrerequisitesUnlocked(EntrepreneurTreeNodeDefinition node)
        {
            return node != null && node.RequiredNodeIds.All(IsUnlocked);
        }

        public static List<string> GetMissingPrerequisites(EntrepreneurTreeNodeDefinition node)
        {
            if (node == null)
                return new List<string>();

            return node.RequiredNodeIds
                .Where(requiredNodeId => !IsUnlocked(requiredNodeId))
                .Select(EntrepreneurTreeDefinitions.GetDisplayName)
                .ToList();
        }

        public static string GetNodeBlockReason(EntrepreneurTreeNodeDefinition node)
        {
            if (node == null)
                return "Nodo no encontrado.";

            if (IsUnlocked(node.Id))
                return "Desbloqueado";

            List<string> missing = GetMissingPrerequisites(node);
            if (missing.Count > 0)
                return "Falta: " + string.Join(", ", missing);

            if (progressPoints < node.Cost)
                return "Puntos insuficientes";

            return "Disponible";
        }

        public static bool TryUnlock(string nodeId, out string message)
        {
            EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
            if (node == null)
            {
                message = "Nodo no encontrado.";
                return false;
            }

            if (IsUnlocked(node.Id))
            {
                message = node.DisplayName + " ya está desbloqueado.";
                return false;
            }

            List<string> missingPrerequisites = GetMissingPrerequisites(node);
            if (missingPrerequisites.Count > 0)
            {
                message = "Falta: " + string.Join(", ", missingPrerequisites);
                return false;
            }

            if (progressPoints < node.Cost)
            {
                message = "Puntos insuficientes.";
                return false;
            }

            progressPoints -= Mathf.Max(0, node.Cost);
            unlockedNodeIds.Add(node.Id);
            Normalize();
            message = node.DisplayName + " desbloqueado.";
            onProgressChanged?.Invoke();
            return true;
        }

        public static string GetStateDescription(EntrepreneurTreeNodeDefinition node)
        {
            EntrepreneurTreeNodeState state = GetState(node);
            if (state == EntrepreneurTreeNodeState.Unlocked)
                return "Desbloqueado";

            if (state == EntrepreneurTreeNodeState.Available)
                return "Disponible";

            string reason = GetNodeBlockReason(node);
            return reason.StartsWith("Falta:")
                ? "Bloqueado\n" + reason
                : "Bloqueado\nPuntos insuficientes";
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["progressPoints"] = progressPoints;

            JSONArray unlocked = new JSONArray();
            foreach (string nodeId in unlockedNodeIds.OrderBy(id => id))
                unlocked.Add(nodeId);
            data["unlockedNodeIds"] = unlocked;

            data["securityLevel"] = GetUnlockedSecurityLevel();
            data["employeeWorkSpeedMultiplier"] = EmployeeWorkSpeedMultiplier;
            data["cashierRevenueMultiplier"] = CashierRevenueMultiplier;
            data["unlockedEmployeeCount"] = unlockedNodeIds.Count(id => id.StartsWith("empleado_", StringComparison.Ordinal));
            return data;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            ResetToDefaults();
            if (data == null || data.Count == 0)
            {
                Normalize();
                onProgressChanged?.Invoke();
                return;
            }

            progressPoints = Mathf.Max(0, data["progressPoints"].AsInt);

            JSONArray unlocked = data["unlockedNodeIds"].AsArray;
            if (unlocked != null)
            {
                for (int i = 0; i < unlocked.Count; i++)
                {
                    string nodeId = unlocked[i].Value;
                    if (EntrepreneurTreeDefinitions.Get(nodeId) != null)
                        unlockedNodeIds.Add(nodeId);
                }
            }

            Normalize();
            onProgressChanged?.Invoke();
        }

        [Conditional("UNITY_EDITOR")]
        public static void AddPointsForInternalTesting(int amount)
        {
            progressPoints = Mathf.Max(0, progressPoints + amount);
            onProgressChanged?.Invoke();
        }

        private static void Normalize()
        {
            unlockedNodeIds.Add(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId);

            bool changed;
            do
            {
                changed = false;
                foreach (string nodeId in unlockedNodeIds.ToArray())
                {
                    if (nodeId == EntrepreneurTreeDefinitions.DefaultUnlockedNodeId)
                        continue;

                    EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
                    if (node == null || !node.RequiredNodeIds.All(IsUnlocked))
                    {
                        unlockedNodeIds.Remove(nodeId);
                        changed = true;
                    }
                }
            }
            while (changed);
        }
    }
}
