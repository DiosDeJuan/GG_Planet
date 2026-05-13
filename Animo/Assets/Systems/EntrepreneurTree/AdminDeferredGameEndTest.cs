// AdminDeferredGameEndTest — helper for deferred game-end overlay testing from Admin Mode.
// Created and destroyed at runtime; not part of any scene or prefab.

using System.Collections;
using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// One-shot MonoBehaviour that waits one frame then fires the requested admin game-end test.
    /// Automatically destroys itself after firing.
    /// </summary>
    public class AdminDeferredGameEndTest : MonoBehaviour
    {
        private bool testMonopoly;

        public void Setup(bool monopoly)
        {
            testMonopoly = monopoly;
        }

        IEnumerator Start()
        {
            yield return null; // wait one frame so systems are fully initialised

            if (testMonopoly)
                GameEndSystem.AdminTestMonopoly();
            else
                GameEndSystem.AdminTestBankruptcy();

            Destroy(gameObject);
        }
    }
}
