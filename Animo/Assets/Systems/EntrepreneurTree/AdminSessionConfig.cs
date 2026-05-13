// Admin Mode — AdminSessionConfig
// Static data carrier that passes admin configuration from the Intro scene into the Game scene.
// No MonoBehaviour — survives scene transitions as static state.

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Holds configuration values set by the ADMIN panel in the Intro scene.
    /// Read by AdminModeApplier in the Game scene after systems have loaded.
    /// </summary>
    public static class AdminSessionConfig
    {
        private const string LogPrefix = "[AdminMode] ";

        /// <summary>Whether an admin session was requested.</summary>
        public static bool isActive;

        /// <summary>Starting money in cents (e.g. 500000 = $5,000).</summary>
        public static long startMoney = 500000;

        /// <summary>Starting tree points to award.</summary>
        public static int treePoints = 0;

        /// <summary>Unlock every product node in the Entrepreneur Tree.</summary>
        public static bool unlockAllProducts;

        /// <summary>Unlock every employee node in the Entrepreneur Tree.</summary>
        public static bool unlockAllEmployees;

        /// <summary>Unlock every node in the Entrepreneur Tree (overrides the above).</summary>
        public static bool unlockEntireTree;

        /// <summary>
        /// Reset to safe defaults so accidental re-use doesn't bleed into a real session.
        /// Call this after applying the config in the Game scene.
        /// </summary>
        public static void Reset()
        {
            isActive = false;
            startMoney = 500000;
            treePoints = 0;
            unlockAllProducts = false;
            unlockAllEmployees = false;
            unlockEntireTree = false;
            UnityEngine.Debug.Log(LogPrefix + "AdminSessionConfig reset to defaults.");
        }
    }
}
