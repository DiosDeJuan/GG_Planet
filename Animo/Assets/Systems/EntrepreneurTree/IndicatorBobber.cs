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
        // 2π — full circle in radians (6.2831853...)
        private const float TwoPi = 6.2831853f;

        private float timeOffset;
        // Cached position struct to avoid re-querying localPosition each frame.
        private Vector3 cachedPos;

        void Start()
        {
            // Randomise phase so multiple thieves don't bob in unison.
            timeOffset = Random.Range(0f, TwoPi);
            cachedPos  = transform.localPosition;
        }

        void Update()
        {
            cachedPos.y = baseLocalY + Mathf.Sin((Time.time * Frequency) + timeOffset) * Amplitude;
            transform.localPosition = cachedPos;
        }
    }
}
