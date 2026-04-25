using UnityEngine;

public class CameraJuiceManager : MonoBehaviour
{
    public static CameraJuiceManager Instance { get; private set; }

    public bool IsShaking => shakeDuration > 0;
    public Vector3 CurrentShakeOffset { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float shakeFrequency = 22f;
    [SerializeField] private float shakeDamping = 1.75f;
    [SerializeField] private float minShakeThreshold = 0.005f;

    [Header("Hit Stop")]
    [SerializeField] private float maxHitStopTimeScale = 0.05f;

    private float shakeDuration = 0f;
    private float shakeDurationTotal = 0f;
    private float shakeMagnitude = 0f;
    private float hitStopRemaining = 0f;
    private float originalTimeScale = 1f;
    private Vector3 shakeSeed;
    private bool isHitStopped;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        shakeSeed = new Vector3(
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f),
            Random.Range(0f, 1000f));
    }

    void Update()
    {
        if (shakeDuration > 0f)
        {
            shakeDuration -= Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(shakeDuration / Mathf.Max(shakeDurationTotal, 0.0001f));
            float damping = Mathf.Pow(normalizedTime, shakeDamping);

            float time = Time.unscaledTime * shakeFrequency;
            float x = (Mathf.PerlinNoise(shakeSeed.x, time) * 2f - 1f) * shakeMagnitude * damping;
            float y = (Mathf.PerlinNoise(shakeSeed.y, time + 31.7f) * 2f - 1f) * shakeMagnitude * damping;
            float z = (Mathf.PerlinNoise(shakeSeed.z, time + 63.4f) * 2f - 1f) * shakeMagnitude * damping;
            CurrentShakeOffset = new Vector3(x, y, z);

            if (CurrentShakeOffset.sqrMagnitude < minShakeThreshold * minShakeThreshold)
                CurrentShakeOffset = Vector3.zero;
        }
        else
        {
            CurrentShakeOffset = Vector3.zero;
            shakeDuration = 0f;
            shakeDurationTotal = 0f;
            shakeMagnitude = 0f;
        }

        if (hitStopRemaining > 0f)
        {
            hitStopRemaining -= Time.unscaledDeltaTime;

            if (hitStopRemaining <= 0f)
            {
                Time.timeScale = originalTimeScale;
                hitStopRemaining = 0f;
                isHitStopped = false;
            }
        }
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f) return;

        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeDurationTotal = Mathf.Max(shakeDurationTotal, duration);
        shakeMagnitude = Mathf.Max(shakeMagnitude, magnitude);

        if (CurrentShakeOffset == Vector3.zero)
        {
            shakeSeed = new Vector3(
                Random.Range(0f, 1000f),
                Random.Range(0f, 1000f),
                Random.Range(0f, 1000f));
        }
    }

    public void HitStop(float duration)
    {
        if (duration <= 0f) return;

        if (!isHitStopped)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = Mathf.Min(originalTimeScale, maxHitStopTimeScale);
            isHitStopped = true;
        }

        hitStopRemaining = Mathf.Max(hitStopRemaining, duration);
    }

    public void StopAllJuice()
    {
        shakeDuration = 0f;
        shakeDurationTotal = 0f;
        shakeMagnitude = 0f;
        hitStopRemaining = 0f;
        CurrentShakeOffset = Vector3.zero;

        if (isHitStopped)
        {
            Time.timeScale = originalTimeScale;
            isHitStopped = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StopAllJuice();
            Instance = null;
        }
    }

    private void OnDisable()
    {
        StopAllJuice();
    }
}
