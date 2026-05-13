// IndicatorBobber — lightweight MonoBehaviour that makes a floating indicator bob up and down.

using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Animates a floating world-space indicator with a simple sine-wave bob.
    /// Attached by ShoplifterAgent to the thief warning sphere.
    /// </summary>
    public class IndicatorBobber : MonoBehaviour
    {
        /// <summary>Starting local Y position (set by ShoplifterAgent before Start runs).</summary>
        public float baseLocalY = 2.2f;

        private const float Amplitude = 0.14f;
        private const float Frequency = 1.8f;

        private float timeOffset;

        void Start()
        {
            // Randomise phase so multiple thieves don't bob in unison.
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            Vector3 pos = transform.localPosition;
            pos.y = baseLocalY + Mathf.Sin((Time.time * Frequency) + timeOffset) * Amplitude;
            transform.localPosition = pos;
        }
    }
}
