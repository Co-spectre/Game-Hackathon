using UnityEngine;
using NordicWilds.Combat;

namespace NordicWilds.Environment
{
    public class TemperatureSystem : MonoBehaviour
    {
        [Header("Survival Mechanics")]
        [SerializeField] private float maxTemperature = 100f;
        [SerializeField] private float currentTemperature;
        [SerializeField] private float decayRate = 1.5f;     
        [SerializeField] private float warmRate = 3.5f;     

        [Header("Thresholds")]
        [SerializeField] private float freezingThreshold = 25f; 

        [Header("VFX / Screen FX")]
        [SerializeField] private ParticleSystem breathSteam;
        [SerializeField] private float freezeDamagePerSecond = 3f;
        [SerializeField] private float freezeDamageTickInterval = 1f;
        [SerializeField] private float initialFreezeGracePeriod = 10f;

        private bool isNearWarmth = false;
        private Health health;
        private float freezeDamageTimer;
        private float survivalStartTime;

        public float CurrentTemp => currentTemperature;

        private void Start()
        {
            currentTemperature = maxTemperature;
            survivalStartTime = Time.time;
            health = GetComponent<Health>();

            if (health == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    health = playerObj.GetComponent<Health>();
            }
        }

        private void Update()
        {
            HandleTemperature();
            UpdateVisuals();
        }

        private void HandleTemperature()
        {
            float rate = isNearWarmth ? warmRate : -decayRate;
            currentTemperature = Mathf.Clamp(currentTemperature + (rate * Time.deltaTime), 0f, maxTemperature);

            if (currentTemperature <= 0f)
            {
                if (Time.time < survivalStartTime + initialFreezeGracePeriod)
                    return;

                freezeDamageTimer += Time.deltaTime;

                if (freezeDamageTimer >= freezeDamageTickInterval)
                {
                    freezeDamageTimer = 0f;

                    if (health != null)
                    {
                        float damageAmount = freezeDamagePerSecond * freezeDamageTickInterval;
                        var damageInfo = new DamageInfo(
                            damageAmount,
                            gameObject,
                            transform.position,
                            Vector3.zero,
                            DamageType.Environmental,
                            knockbackForce: 0f,
                            staggerDuration: 0f,
                            canBeBlocked: false,
                            isCritical: false);

                        health.TakeDamage(damageInfo);
                    }
                    else
                    {
                        Debug.LogWarning("Player is frozen but no Health component was found to apply damage.");
                    }
                }
            }
            else
            {
                freezeDamageTimer = 0f;
            }
        }

        private void UpdateVisuals()
        {
            if (currentTemperature < freezingThreshold)
            {
                if (breathSteam != null && !breathSteam.isPlaying) breathSteam.Play();
            }
            else
            {
                if (breathSteam != null && breathSteam.isPlaying) breathSteam.Stop();
            }
        }

        public void SetWarmth(bool nearHeatSource)
        {
            isNearWarmth = nearHeatSource;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Campfire"))
            {
                SetWarmth(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Campfire"))
            {
                SetWarmth(false);
            }
        }
    }
}