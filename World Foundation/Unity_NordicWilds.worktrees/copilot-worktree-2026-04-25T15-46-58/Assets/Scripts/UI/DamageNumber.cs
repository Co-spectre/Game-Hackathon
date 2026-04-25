using UnityEngine;

namespace NordicWilds.UI
{
    [RequireComponent(typeof(TextMesh))]
    public class DamageNumber : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private float lifetime = 1.2f;
        [SerializeField] private float riseSpeed = 2.3f;
        [SerializeField] private float driftStrength = 0.45f;

        private float age;
        private string text = "0";
        private TextMesh textMesh;
        private Color baseColor = Color.white;
        private Vector3 velocity;
        private Camera cachedCamera;

        private void Awake()
        {
            textMesh = GetComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.12f;
            textMesh.fontSize = 48;
            textMesh.color = Color.white;

            cachedCamera = Camera.main;
            velocity = new Vector3(Random.Range(-driftStrength, driftStrength), riseSpeed, Random.Range(-driftStrength, driftStrength));
        }

        private void Start()
        {
            if (textMesh != null)
                textMesh.text = text;
        }

        private void Update()
        {
            age += Time.deltaTime;

            transform.position += velocity * Time.deltaTime;

            if (cachedCamera == null)
                cachedCamera = Camera.main;

            if (cachedCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - cachedCamera.transform.position, Vector3.up);
            }

            float fadeT = Mathf.InverseLerp(lifetime * 0.6f, lifetime, age);
            if (textMesh != null)
            {
                Color faded = baseColor;
                faded.a = Mathf.Lerp(baseColor.a, 0f, fadeT);
                textMesh.color = faded;
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        public void Setup(float damage, bool isHeavy)
        {
            text = Mathf.RoundToInt(damage).ToString();

            if (isHeavy)
            {
                baseColor = new Color(1f, 0.45f, 0.15f, 1f);
                velocity = new Vector3(Random.Range(-0.2f, 0.2f), riseSpeed * 1.1f, Random.Range(-0.2f, 0.2f));
            }
            else
            {
                baseColor = Color.white;
            }

            if (textMesh == null)
            {
                textMesh = GetComponent<TextMesh>();
            }

            if (textMesh != null)
            {
                textMesh.text = text;
                textMesh.fontSize = isHeavy ? 64 : 48;
                textMesh.color = baseColor;
            }
        }
    }
}
