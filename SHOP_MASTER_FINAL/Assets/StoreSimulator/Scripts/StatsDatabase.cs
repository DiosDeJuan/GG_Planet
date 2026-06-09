//Adaptado por POMPIC 20100333
/*  This file is part of the "Store Simulator" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store, Epic FAB).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using SimpleJSON;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Caches certain values from events that happened during the day.
    /// Used in the day over scene to display statistical information to the player.
    /// </summary>
    public class StatsDatabase : MonoBehaviour
    {
        /// <summary>
        /// Returns a reference to this script instance.
        /// </summary>
        public static StatsDatabase Instance { get; private set; }

        /// <summary>
        /// Cache for money earned during the day.
        /// </summary>
        public long moneyEarned { get; private set; }

        /// <summary>
        /// Cache for money spent during the day.
        /// </summary>
        public long moneySpent { get; private set; }

        /// <summary>
        /// Cache for experience earned during the day. If only customers are taken into account,
        /// and customers give 1 XP, then this is the same as customerHappy - customersUnhappy.
        /// </summary>
        public long xpEarned { get; private set; }

        /// <summary>
        /// Cache for count of customers who left happy.
        /// </summary>
        public int customersHappy { get; private set; }

        /// <summary>
        /// Cache for count of customers who left unhappy.
        /// </summary>
        public int customersUnhappy { get; private set; }

        /// <summary>
        /// Salary expenses charged for hired employees during the current day.
        /// </summary>
        public long employeeSalarySpent { get; private set; }

        public int theftEventsCount { get; private set; }
        public long stolenValue { get; private set; }
        public long recoveredValue { get; private set; }
        public int escapedThievesCount { get; private set; }
        public int manualArrestsCount { get; private set; }
        public int automaticArrestsCount { get; private set; }
        public int securityLevelAtEndOfDay { get; private set; }
        public int securityAutoArrestAttempts { get; private set; }
        public int securityAutoArrestSuccesses { get; private set; }
        public long charismaticBonusIncome { get; private set; }


        //initialize references
        void Awake()
        {
            Instance = this;

            StoreDatabase.onMoneyUpdate += OnMoneyUpdate;
            StoreDatabase.onExperienceUpdate += OnExperienceUpdate;
            DayCycleSystem.onDayLoaded += OnDayLoaded;
            CustomerSystem.onCustomerLeft += OnCustomerLeft;
        }


        //subscribed to day event
        private void OnDayLoaded()
        {
            moneyEarned = moneySpent = 0;
            xpEarned = 0;
            customersHappy = customersUnhappy = 0;
            employeeSalarySpent = 0;
            theftEventsCount = 0;
            stolenValue = 0;
            recoveredValue = 0;
            escapedThievesCount = 0;
            manualArrestsCount = 0;
            automaticArrestsCount = 0;
            securityLevelAtEndOfDay = EntrepreneurProgress.SecurityLevel;
            securityAutoArrestAttempts = 0;
            securityAutoArrestSuccesses = 0;
            charismaticBonusIncome = 0;
        }


        //subscribed to money change
        private void OnMoneyUpdate(string current, string changeString)
        {
            long change = StoreDatabase.FromStringToLongMoney(changeString);

            if (change > 0) moneyEarned += change;
            else moneySpent += change;
        }


        //subscribed to experience change
        private void OnExperienceUpdate(long current, long change)
        {
            //if (change > 0) if only positive values should count
            xpEarned += change;
        }


        //subscribed to customer event
        private void OnCustomerLeft(bool wasHappy)
        {
            if (wasHappy) customersHappy++;
            else customersUnhappy++;
        }

        public void RegisterEmployeeSalary(long amount)
        {
            if (amount <= 0)
                return;

            employeeSalarySpent += amount;
        }

        public void RegisterTheftEscaped(long value)
        {
            theftEventsCount++;
            escapedThievesCount++;
            stolenValue += value > 0 ? value : 0;
        }

        public void RegisterManualArrest(long amount)
        {
            theftEventsCount++;
            manualArrestsCount++;
            recoveredValue += amount > 0 ? amount : 0;
        }

        public void RegisterAutomaticArrest(long amount)
        {
            theftEventsCount++;
            automaticArrestsCount++;
            recoveredValue += amount > 0 ? amount : 0;
        }

        public void RegisterSecurityAutoArrestAttempt(bool success)
        {
            securityAutoArrestAttempts++;
            if (success)
                securityAutoArrestSuccesses++;
        }

        public void RecordSecurityLevel(int level)
        {
            securityLevelAtEndOfDay = Mathf.Clamp(level, 0, 3);
        }

        public void RegisterCharismaticBonus(long amount)
        {
            if (amount <= 0)
                return;

            charismaticBonusIncome += amount;
        }


        /// <summary>
        /// Reads component data that should be persisted and returns it as a JSONNode. 
        /// </summary>
        public JSONNode SaveToJSON()
        {
            JSONNode data = new JSONObject();
                
            data["moneyEarned"] = moneyEarned;
            data["moneySpent"] = moneySpent;
            data["xpEarned"] = xpEarned;
            data["customersHappy"] = customersHappy;
            data["customersUnhappy"] = customersUnhappy;
            data["employeeSalarySpent"] = employeeSalarySpent;
            data["theftEventsCount"] = theftEventsCount;
            data["stolenValue"] = stolenValue;
            data["recoveredValue"] = recoveredValue;
            data["escapedThievesCount"] = escapedThievesCount;
            data["manualArrestsCount"] = manualArrestsCount;
            data["automaticArrestsCount"] = automaticArrestsCount;
            data["securityLevelAtEndOfDay"] = securityLevelAtEndOfDay;
            data["securityAutoArrestAttempts"] = securityAutoArrestAttempts;
            data["securityAutoArrestSuccesses"] = securityAutoArrestSuccesses;
            data["charismaticBonusIncome"] = charismaticBonusIncome;
            
            return data;
        }


        /// <summary>
        /// Applies existing data coming from a JSONNode and overwrites it on this component.
        /// </summary>
        public void LoadFromJSON(JSONNode data)
        {
            if (data == null || data.Count == 0)
                return;

            moneyEarned = data["moneyEarned"].AsLong;
            moneySpent = data["moneySpent"].AsLong;
            xpEarned = data["xpEarned"].AsLong;
            customersHappy = data["customersHappy"].AsInt;
            customersUnhappy = data["customersUnhappy"].AsInt;
            employeeSalarySpent = data["employeeSalarySpent"].AsLong;
            theftEventsCount = data["theftEventsCount"].AsInt;
            stolenValue = data["stolenValue"].AsLong;
            recoveredValue = data["recoveredValue"].AsLong;
            escapedThievesCount = data["escapedThievesCount"].AsInt;
            manualArrestsCount = data["manualArrestsCount"].AsInt;
            automaticArrestsCount = data["automaticArrestsCount"].AsInt;
            securityLevelAtEndOfDay = data["securityLevelAtEndOfDay"].AsInt;
            securityAutoArrestAttempts = data["securityAutoArrestAttempts"].AsInt;
            securityAutoArrestSuccesses = data["securityAutoArrestSuccesses"].AsInt;
            charismaticBonusIncome = data["charismaticBonusIncome"].AsLong;
        }


        //unsubscribe from events
        void OnDestroy()
        {
            StoreDatabase.onMoneyUpdate -= OnMoneyUpdate;
            StoreDatabase.onExperienceUpdate -= OnExperienceUpdate;
            DayCycleSystem.onDayLoaded -= OnDayLoaded;
            CustomerSystem.onCustomerLeft -= OnCustomerLeft;
        }
    }
}
