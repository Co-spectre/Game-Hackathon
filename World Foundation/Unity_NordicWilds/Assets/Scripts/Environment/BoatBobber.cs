using UnityEngine;

namespace NordicWilds.Environment
{
    // Sits a boat on top of the ocean: samples the OceanWater height at the boat's world XZ,
    // smoothly tracks Y, and rocks roll/pitch based on horizontal wave gradient. If no ocean
    // is bound, falls back to a calm sine bob so it still looks alive. Runtime-only so the
    // boat stays put when authoring scenes.
    public class BoatBobber : MonoBehaviour
    {
        public OceanWater ocean;
        public float vertOffsetFromSurface = 0.0f;
        public float positionLerp = 6f;
        public float rotationLerp = 3.5f;
        public float maxRollDegrees = 6f;
        public float maxPitchDegrees = 4f;
        public float fallbackBobAmplitude = 0.18f;
        public float fallbackBobFrequency = 0.55f;
        public float forwardSampleDistance = 4f;
        public float sideSampleDistance = 2.4f;
        public float lateralDriftAmplitude = 0f;
        public float lateralDriftFrequency = 0.05f;
        public Vector3 lateralDriftAxis = Vector3.right;

        Quaternion baseLocalRot;
        Vector3 basePos;
        Vector3 driftAxisWorld;
        bool initialized;
        float verticalVelocity;
        Rigidbody body;

        void OnEnable()
        {
            basePos = transform.position;
            baseLocalRot = transform.localRotation;
            driftAxisWorld = lateralDriftAxis.sqrMagnitude > 0.0001f
                ? transform.TransformDirection(lateralDriftAxis.normalized)
                : transform.right;
            driftAxisWorld.y = 0f;
            if (driftAxisWorld.sqrMagnitude > 0.0001f)
                driftAxisWorld.Normalize();
            body = GetComponent<Rigidbody>();
            initialized = true;
        }

        void Update()
        {
            if (!initialized) OnEnable();

            float t = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
            Vector3 pos = transform.position;
            Vector3 driftedXZ = new Vector3(pos.x, pos.y, pos.z);
            if (lateralDriftAmplitude > 0.0001f)
            {
                Vector3 drift = driftAxisWorld * (Mathf.Sin(t * lateralDriftFrequency * Mathf.PI * 2f) * lateralDriftAmplitude);
                driftedXZ = new Vector3(basePos.x + drift.x, pos.y, basePos.z + drift.z);
            }

            float targetY;
            float roll  = 0f;
            float pitch = 0f;

            if (ocean != null)
            {
                Vector3 fwd  = transform.forward * forwardSampleDistance;
                Vector3 side = transform.right   * sideSampleDistance;

                float center = ocean.SampleHeight(driftedXZ);
                float front  = ocean.SampleHeight(driftedXZ + fwd);
                float back   = ocean.SampleHeight(driftedXZ - fwd);
                float right  = ocean.SampleHeight(driftedXZ + side);
                float left   = ocean.SampleHeight(driftedXZ - side);

                targetY = center + vertOffsetFromSurface;
                pitch = Mathf.Clamp(Mathf.Atan2(back - front, forwardSampleDistance * 2f) * Mathf.Rad2Deg, -maxPitchDegrees, maxPitchDegrees);
                roll  = Mathf.Clamp(Mathf.Atan2(right - left, sideSampleDistance   * 2f) * Mathf.Rad2Deg, -maxRollDegrees,  maxRollDegrees);
            }
            else
            {
                targetY = basePos.y + Mathf.Sin(t * fallbackBobFrequency * Mathf.PI * 2f) * fallbackBobAmplitude + vertOffsetFromSurface;
                pitch = Mathf.Sin(t * 0.7f) * 2.5f;
                roll  = Mathf.Sin(t * 0.5f + 0.6f) * 3f;
            }

            float smoothY = Mathf.SmoothDamp(pos.y, targetY, ref verticalVelocity, 1f / Mathf.Max(0.01f, positionLerp));
            Vector3 nextPosition = new Vector3(driftedXZ.x, smoothY, driftedXZ.z);

            Quaternion targetRot = baseLocalRot * Quaternion.Euler(pitch, 0f, roll);
            Quaternion nextRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * rotationLerp);

            if (body != null && body.isKinematic)
            {
                body.MovePosition(nextPosition);
                body.MoveRotation(nextRotation);
            }
            else
            {
                transform.position = nextPosition;
                transform.localRotation = nextRotation;
            }
        }
    }
}
