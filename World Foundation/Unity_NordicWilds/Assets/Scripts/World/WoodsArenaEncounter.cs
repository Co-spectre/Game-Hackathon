using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NordicWilds.Combat;

namespace NordicWilds.World
{
    [RequireComponent(typeof(BoxCollider))]
    public class WoodsArenaEncounter : MonoBehaviour
    {
        [Header("Encounter")]
        [SerializeField] private int requiredDefeats = 4;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject exitGate;

        [Header("Spawn Tuning")]
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private float enemyScale = 1.2f;

        private int defeatedCount;
        private bool started;
        private bool cleared;

        private string activeMessage;
        private float activeMessageUntil;
        private GUIStyle messageStyle;
        private Texture2D messageBackground;

        private readonly List<Health> aliveEnemies = new List<Health>();

        private void Awake()
        {
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;

            if (enemyMaterial == null)
            {
                enemyMaterial = new Material(Shader.Find("Standard"));
                enemyMaterial.color = new Color(0.68f, 0.10f, 0.12f);
            }

            if (exitGate != null)
                exitGate.SetActive(true);
        }

        private void OnDisable()
        {
            CleanupEnemySubscriptions();
        }

        private void OnDestroy()
        {
            CleanupEnemySubscriptions();

            if (messageBackground != null)
                Destroy(messageBackground);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (started || !other.CompareTag("Player"))
                return;

            StartEncounter();
        }

        private void StartEncounter()
        {
            started = true;
            defeatedCount = 0;

            if (exitGate != null)
                exitGate.SetActive(true);

            ShowMessage("Defeat 4 foes to clear the woods arena.", 3.0f);
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("WoodsArenaEncounter has no spawn points configured.");
                return;
            }

            int enemyCount = Mathf.Max(requiredDefeats, 1);

            for (int i = 0; i < enemyCount; i++)
            {
                Transform spawn = spawnPoints[i % spawnPoints.Length];
                if (spawn == null)
                    continue;

                GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = i == 0 ? "Woods Villain" : $"Woods Foe {i}";
                enemy.tag = "Enemy";
                enemy.transform.position = spawn.position;
                enemy.transform.rotation = spawn.rotation;
                enemy.transform.localScale = Vector3.one * enemyScale;

                Renderer renderer = enemy.GetComponent<Renderer>();
                if (renderer != null && enemyMaterial != null)
                    renderer.sharedMaterial = enemyMaterial;

                Rigidbody rb = enemy.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                rb.interpolation = RigidbodyInterpolation.Interpolate;

                Health health = enemy.AddComponent<Health>();
                health.OnDeath += HandleEnemyDeath;
                aliveEnemies.Add(health);

                enemy.AddComponent<EnemyAI>();
                enemy.AddComponent<TouchDefeatOnPlayerContact>();
            }
        }

        private void HandleEnemyDeath(GameObject enemyObj)
        {
            defeatedCount++;

            Health enemyHealth = enemyObj != null ? enemyObj.GetComponent<Health>() : null;
            if (enemyHealth != null)
            {
                enemyHealth.OnDeath -= HandleEnemyDeath;
                aliveEnemies.Remove(enemyHealth);
            }

            int remaining = Mathf.Max(requiredDefeats - defeatedCount, 0);
            if (remaining > 0)
                ShowMessage($"Foes remaining: {remaining}", 1.4f);

            if (defeatedCount >= requiredDefeats && !cleared)
                StartCoroutine(ClearEncounterRoutine());
        }

        private IEnumerator ClearEncounterRoutine()
        {
            cleared = true;

            if (exitGate != null)
                exitGate.SetActive(false);

            ShowMessage("That was hard...", 2.0f);
            yield return new WaitForSeconds(2.0f);

            ShowMessage("Path is clear. You can move forward.", 3.5f);

            BoxCollider trigger = GetComponent<BoxCollider>();
            if (trigger != null)
                trigger.enabled = false;
        }

        private void ShowMessage(string message, float duration)
        {
            activeMessage = message;
            activeMessageUntil = Time.time + duration;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(activeMessage) || Time.time > activeMessageUntil)
                return;

            EnsureStyles();

            float width = Mathf.Min(620f, Screen.width - 40f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 140f, width, 56f);
            GUI.DrawTexture(panel, messageBackground);
            GUI.Label(panel, activeMessage, messageStyle);
        }

        private void EnsureStyles()
        {
            if (messageStyle != null)
                return;

            messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
            messageStyle.normal.textColor = new Color(0.94f, 0.90f, 0.78f, 1f);

            messageBackground = new Texture2D(2, 2);
            Color tint = new Color(0.05f, 0.05f, 0.06f, 0.84f);
            messageBackground.SetPixels(new[] { tint, tint, tint, tint });
            messageBackground.Apply();
            messageBackground.hideFlags = HideFlags.HideAndDontSave;
        }

        private void CleanupEnemySubscriptions()
        {
            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                if (aliveEnemies[i] != null)
                    aliveEnemies[i].OnDeath -= HandleEnemyDeath;
            }

            aliveEnemies.Clear();
        }
    }

    [RequireComponent(typeof(Health))]
    public class TouchDefeatOnPlayerContact : MonoBehaviour
    {
        [SerializeField] private float touchDamage = 9999f;

        private Health health;
        private bool defeated;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryDefeat(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDefeat(other);
        }

        private void TryDefeat(Collider other)
        {
            if (defeated || other == null || !other.CompareTag("Player"))
                return;

            if (health == null)
                health = GetComponent<Health>();

            if (health == null)
                return;

            defeated = true;
            health.TakeDamage(new DamageInfo(
                touchDamage,
                other.gameObject,
                transform.position,
                (transform.position - other.transform.position).normalized,
                DamageType.Physical,
                knockbackForce: 0f,
                staggerDuration: 0f,
                canBeBlocked: false,
                isCritical: true));
        }
    }
}
