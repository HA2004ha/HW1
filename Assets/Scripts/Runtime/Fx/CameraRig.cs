using UnityEngine;

namespace Runner.Fx
{
    /// <summary>
    /// Keeps the camera framed on the runner and adds the kicks that sell impacts. The base
    /// position is restored every frame so shake never drifts the view.
    /// </summary>
    public sealed class CameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followLerp = 6f;
        [SerializeField] private float yOffset = 0.9f;
        [SerializeField] private Vector3 basePosition;
        [SerializeField] private bool captureBaseOnStart = true;

        private float shakeAmount;
        private float shakeTime;
        private float shakeDuration;
        private Vector3 lastOffset;

        private void Awake()
        {
            if (captureBaseOnStart) basePosition = transform.position;
        }

        private void Update()
        {
            var desired = basePosition;

            if (target != null)
            {
                desired.y = Mathf.Lerp(desired.y, target.position.y + yOffset, followLerp * Time.deltaTime);
            }

            desired += DecayShake();
            transform.position = desired;
        }

        private Vector3 DecayShake()
        {
            if (shakeTime <= 0f)
            {
                lastOffset = Vector3.zero;
                return Vector3.zero;
            }

            shakeTime -= Time.deltaTime;
            var strength = shakeAmount * Mathf.Clamp01(shakeTime / Mathf.Max(0.0001f, shakeDuration));

            lastOffset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength) * 0.7f,
                0f);

            return lastOffset;
        }

        public void Shake(float amount, float duration)
        {
            if (amount <= 0f || duration <= 0f) return;

            if (amount >= shakeAmount || shakeTime <= 0f)
            {
                shakeAmount = amount;
                shakeDuration = duration;
            }

            shakeTime = Mathf.Max(shakeTime, duration);
        }

        public void SetTarget(Transform value) => target = value;
    }
}
