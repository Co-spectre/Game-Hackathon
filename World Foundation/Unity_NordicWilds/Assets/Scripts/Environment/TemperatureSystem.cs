using UnityEngine;

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
        private bool isNearWarmth = false;

        public float CurrentTemp => currentTemperature;

        private void Start()
        {
            currentTemperature = maxTemperature;
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
