//Adaptado por POMPIC 20100333
using SimpleJSON;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    public static class GameEndingService
    {
        private static bool initialized;
        private static bool monopolyAvailable;
        private static bool bankruptcyRisk;

        public static bool MonopolyAvailable => monopolyAvailable;
        public static bool BankruptcyRisk => bankruptcyRisk;

        static GameEndingService()
        {
            EnsureInitialized();
        }

        public static void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;
            DayCycleSystem.onDayFinished += OnDayFinished;
            EntrepreneurProgress.onProgressChanged += EvaluateMonopoly;
            ShopExpansionManager.onSpacesChanged += EvaluateMonopoly;
        }

        public static void ResetToDefaults()
        {
            monopolyAvailable = false;
            bankruptcyRisk = false;
        }

        public static void EvaluateMonopoly()
        {
            bool available = EntrepreneurProgress.IsTreeComplete() && ShopExpansionManager.AreAllSpacesPurchased;
            if (!available || monopolyAvailable)
                return;

            monopolyAvailable = true;
            ShowNotification("Final de Monopolio disponible.");
        }

        public static void EvaluateBankruptcy()
        {
            if (StoreDatabase.Instance == null || StoreDatabase.Instance.currentMoney >= 0)
                return;

            bankruptcyRisk = true;
            ShowNotification("Riesgo de bancarrota: saldo negativo.");
        }

        public static JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
            data["monopolyAvailable"] = monopolyAvailable;
            data["bankruptcyRisk"] = bankruptcyRisk;
            return data;
        }

        public static void LoadFromJSON(JSONNode data)
        {
            monopolyAvailable = data != null && data["monopolyAvailable"].AsBool;
            bankruptcyRisk = data != null && data["bankruptcyRisk"].AsBool;
            EvaluateMonopoly();
            EvaluateBankruptcy();
        }

        private static void OnDayFinished()
        {
            EvaluateMonopoly();
            EvaluateBankruptcy();
        }

        private static void ShowNotification(string text)
        {
            if (UIGame.Instance != null)
                UIGame.AddNotification(text, otherColor: Color.yellow, otherDuration: 5f);
        }
    }
}
