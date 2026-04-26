using UnityEngine;

namespace NordicWilds.UI
{
    // Cinematic camera dolly for the start menu. While idle it slowly pushes in toward the
    // boat with a subtle crane-down arc; when StartLaunch() is called it accelerates forward
    // along the boat's heading for a "we're sailing into the world" reveal. Disable on launch
    // completion and hand control back to IsometricCameraFollow. Runtime-only so the
    // editor scene view doesn't constantly drift the camera while authoring.
    public class MenuCameraDirector : MonoBehaviour
    {
        public Transform target;            // The boat
        public Vector3 idleOffset       = new Vector3(-24f, 12f, -30f);
        public Vector3 idleEndOffset    = new Vector3(-24f, 12f, -30f);
        public float idleDriftSpeed     = 0f;     // steady menu camera
        public float idleSwayAmplitude  = 0f;
        public float lookHeightOffset   = 3.2f;

        [Header("Launch")]
        public float launchPushInAmount = 7f;         // dolly into boat during launch
        public float launchAscend       = 4f;
        public float launchDuration     = 3.0f;

        bool launching;
        float launchT;
        float idleT;
        Vector3 launchStartOffset;

        void Start()
        {
            SnapToIdle();
        }

        void LateUpdate()
        {
            if (target == null) return;

            float dt = Application.isPlaying ? Time.deltaTime : 0.016f;

            Vector3 offset;
            if (!launching)
            {
                idleT = Mathf.Clamp01(idleT + dt * idleDriftSpeed);
                float ease = SmoothStep01(idleT);
                offset = Vector3.Lerp(idleOffset, idleEndOffset, ease);

                float t = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
                offset += new Vector3(
                    Mathf.Sin(t * 0.35f)         * idleSwayAmplitude,
                    Mathf.Sin(t * 0.27f + 1.1f)  * idleSwayAmplitude * 0.45f,
                    Mathf.Cos(t * 0.31f + 0.6f)  * idleSwayAmplitude * 0.6f);
            }
            else
            {
                launchT += dt / Mathf.Max(0.0001f, launchDuration);
                float k = SmoothStep01(Mathf.Clamp01(launchT));
                Vector3 forward = target.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
                forward.Normalize();

                offset = launchStartOffset
                    + forward * launchPushInAmount * k
                    + Vector3.up * launchAscend    * k
                    - launchStartOffset            * k * 0.55f;
            }

            Vector3 camPos = target.position + offset;
            transform.position = camPos;
            Vector3 lookAt = target.position + Vector3.up * lookHeightOffset;
            transform.rotation = Quaternion.LookRotation(lookAt - camPos, Vector3.up);
        }

        public void StartLaunch()
        {
            if (target == null) return;
            launching = true;
            launchT = 0f;
            launchStartOffset = transform.position - target.position;
        }

        public void SnapToIdle()
        {
            if (target == null) return;

            launching = false;
            launchT = 0f;
            idleT = 0f;

            Vector3 camPos = target.position + idleOffset;
            transform.position = camPos;
            Vector3 lookAt = target.position + Vector3.up * lookHeightOffset;
            transform.rotation = Quaternion.LookRotation(lookAt - camPos, Vector3.up);
        }

        static float SmoothStep01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
