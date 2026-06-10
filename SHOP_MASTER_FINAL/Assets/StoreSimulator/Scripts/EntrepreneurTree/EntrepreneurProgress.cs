//Adaptado por POMPIC 20100333
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class EntrepreneurProgress
    {
        public static event Action onProgressChanged;
        public static event Action<string> onNodeUnlocked;
        public static event Action<string> onProgressPointAwarded;
        public static event Action onTreeCompleted;

        private static readonly HashSet<string> unlockedNodeIds = new HashSet<string>();
        private static int progressPoints;
        private static bool treeCompletionNotified;

        static EntrepreneurProgress()
        {
            ResetToDefaults();
        }

        public static int ProgressPoints => progressPoints;
        public static int AvailablePoints => progressPoints;
        public static int SpentPoints => unlockedNodeIds.Select(EntrepreneurTreeDefinitions.Get).Where(node => node != null).Sum(node => node.Cost);
        public static float EmployeeWorkSpeedMultiplier => IsUnlocked("mejora_cafeina") ? 1.1f : 1f;
        public static float CashierRevenueMultiplier => IsUnlocked("mejora_carismatico") ? 1.05f : 1f;
        public static int SecurityLevel => IsUnlocked("seguridad_3") ? 3 : IsUnlocked("seguridad_2") ? 2 : IsUnlocked("seguridad_1") ? 1 : 0;

        public static void ResetToDefaults()
        {
            unlockedNodeIds.Clear();
            unlockedNodeIds.Add(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId);
            progressPoints = 0;
            treeCompletionNotified = false;
            EntrepreneurAchievementManager.ResetToDefaults();
            onProgressChanged?.Invoke();
        }

        public static bool IsUnlocked(string nodeId)
        {
            return unlockedNodeIds.Contains(nodeId);
        }

        public static bool IsProductUnlocked(ProductScriptableObject product)
        {
            if (product == null)
                return true;

            string nodeId = EntrepreneurTreeDefinitions.GetKnownProductNodeId(product);
            return string.IsNullOrEmpty(nodeId) || IsUnlocked(nodeId);
        }

        public static bool TryGetProductUnlockNode(ProductScriptableObject product, out EntrepreneurTreeNodeDefinition node)
        {
            return EntrepreneurTreeDefinitions.TryGetProductNode(product, out node);
        }

        public static bool TryGetProductLockedMessage(ProductScriptableObject product, out string message)
        {
            message = string.Empty;
            if (product == null || IsProductUnlocked(product))
                return false;

            if (TryGetProductUnlockNode(product, out EntrepreneurTreeNodeDefinition node))
                message = "Producto bloqueado. Desbloquea " + node.Title + " en el Arbol del Emprendedor.";
            else
                message = "Producto bloqueado. Desbloquea el nodo requerido en el Arbol del Emprendedor.";

            return true;
        }

        public static EntrepreneurTreeNodeState GetState(EntrepreneurTreeNodeDefinition node)
        {
            if (node == null)
                return EntrepreneurTreeNodeState.Locked;

            if (IsUnlocked(node.Id))
                return EntrepreneurTreeNodeState.Unlocked;

            return ArePrerequisitesUnlocked(node) && progressPoints >= node.Cost ? EntrepreneurTreeNodeState.Available : EntrepreneurTreeNodeState.Locked;
        }

        public static bool CanAfford(EntrepreneurTreeNodeDefinition node)
        {
            return node != null && progressPoints >= node.Cost;
        }

        public static bool ArePrerequisitesUnlocked(EntrepreneurTreeNodeDefinition node)
        {
            return node != null && node.Prerequisites.All(IsUnlocked);
        }

        public static List<string> GetMissingPrerequisites(EntrepreneurTreeNodeDefinition node)
        {
            if (node == null)
                return new List<string>();

            return node.Prerequisites.Where(prerequisite => !IsUnlocked(prerequisite)).Select(EntrepreneurTreeDefinitions.GetTitle).ToList();
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
                message = node.Title + " ya esta desbloqueado.";
                return false;
            }

            List<string> missingPrerequisites = GetMissingPrerequisites(node);
            if (missingPrerequisites.Count > 0)
            {
                message = "Falta desbloquear: " + string.Join(", ", missingPrerequisites);
                return false;
            }

            if (progressPoints < node.Cost)
            {
                message = "No tienes puntos de progreso suficientes.";
                return false;
            }

            progressPoints -= node.Cost;
            unlockedNodeIds.Add(node.Id);
            Normalize();
            message = node.Title + " desbloqueado.";
            onNodeUnlocked?.Invoke(node.Id);
            EntrepreneurAchievementManager.RegisterNodeUnlocked(node.Id);
            NotifyTreeCompletionIfNeeded(true);
            onProgressChanged?.Invoke();
            return true;
        }

        public static string GetStateDescription(EntrepreneurTreeNodeDefinition node)
        {
            switch (GetState(node))
            {
                case EntrepreneurTreeNodeState.Unlocked:
                    return "Desbloqueado";
                case EntrepreneurTreeNodeState.Available:
                    return "Disponible";
                default:
                    List<string> missing = GetMissingPrerequisites(node);
                    if (missing.Count > 0)
                        return "Bloqueado\nFalta desbloquear: " + string.Join(", ", missing);
                    return "Bloqueado\nNo tienes puntos de progreso suficientes";
            }
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["progressPoints"] = progressPoints;

            JSONArray unlocked = new JSONArray();
            foreach (string nodeId in unlockedNodeIds.OrderBy(id => id))
                unlocked.Add(nodeId);

            data["unlockedNodeIds"] = unlocked;
            data["securityLevel"] = SecurityLevel;
            data["employeeWorkSpeedMultiplier"] = EmployeeWorkSpeedMultiplier;
            data["cashierRevenueMultiplier"] = CashierRevenueMultiplier;
            data["treeCompletionNotified"] = treeCompletionNotified;
            data["achievements"] = EntrepreneurAchievementManager.SaveToJSON();
            return data;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            ResetToDefaults();
            if (data == null || data.Count == 0)
                return;

            progressPoints = Mathf.Max(0, data["progressPoints"].AsInt);
            treeCompletionNotified = data["treeCompletionNotified"].AsBool;
            JSONArray unlocked = data["unlockedNodeIds"].AsArray;
            for (int i = 0; i < unlocked.Count; i++)
            {
                string nodeId = unlocked[i].Value;
                if (EntrepreneurTreeDefinitions.Get(nodeId) != null)
                    unlockedNodeIds.Add(nodeId);
            }

            EntrepreneurAchievementManager.LoadFromJSON(data["achievements"]);
            Normalize();
            NotifyTreeCompletionIfNeeded(false);
            onProgressChanged?.Invoke();
        }

        public static bool HasClaimedProgressReward(string sourceId)
        {
            return EntrepreneurAchievementManager.IsRewardClaimed(sourceId);
        }

        public static bool AddProgressPoint(string sourceId, string displayReason)
        {
            return EntrepreneurAchievementManager.TryAwardKnownAchievement(sourceId, displayReason);
        }

        public static int GetUnlockedNodeCount()
        {
            return EntrepreneurTreeDefinitions.Nodes.Count(node => IsUnlocked(node.Id));
        }

        public static int GetTotalNodeCount()
        {
            return EntrepreneurTreeDefinitions.Nodes.Count;
        }

        public static int GetTotalUnlockableCount()
        {
            return EntrepreneurTreeDefinitions.Nodes.Count(node => node.Cost > 0);
        }

        public static float GetTreeCompletionPercent()
        {
            int total = GetTotalNodeCount();
            return total == 0 ? 0f : Mathf.Clamp01((float)GetUnlockedNodeCount() / total);
        }

        public static bool IsTreeComplete()
        {
            return GetUnlockedNodeCount() >= GetTotalNodeCount();
        }

        internal static void AddProgressPointsFromAchievement(int amount, string sourceId, string displayReason)
        {
            if (amount <= 0)
                return;

            progressPoints = Mathf.Max(0, progressPoints + amount);
            onProgressPointAwarded?.Invoke(displayReason);
            onProgressChanged?.Invoke();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || UNITY_INCLUDE_TESTS
        public static void AddPointsForInternalTesting(int amount)
        {
            progressPoints = Mathf.Max(0, progressPoints + amount);
            onProgressChanged?.Invoke();
        }
#endif

        private static void Normalize()
        {
            unlockedNodeIds.Add(EntrepreneurTreeDefinitions.DefaultUnlockedNodeId);

            bool removed;
            do
            {
                removed = false;
                foreach (string nodeId in unlockedNodeIds.ToArray())
                {
                    if (nodeId == EntrepreneurTreeDefinitions.DefaultUnlockedNodeId)
                        continue;

                    EntrepreneurTreeNodeDefinition node = EntrepreneurTreeDefinitions.Get(nodeId);
                    if (node == null || !ArePrerequisitesUnlocked(node))
                    {
                        unlockedNodeIds.Remove(nodeId);
                        removed = true;
                    }
                }
            }
            while (removed);

            if (!IsTreeComplete())
                treeCompletionNotified = false;
        }

        private static void NotifyTreeCompletionIfNeeded(bool showNotification)
        {
            if (!IsTreeComplete() || treeCompletionNotified)
                return;

            treeCompletionNotified = true;
            onTreeCompleted?.Invoke();
            if (showNotification && UIGame.Instance != null)
                UIGame.AddNotification("Arbol del Emprendedor completado.", otherColor: Color.green, otherDuration: 5f);
        }
    }
}
