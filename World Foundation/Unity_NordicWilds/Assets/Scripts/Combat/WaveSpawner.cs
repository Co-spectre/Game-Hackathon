using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NordicWilds.Combat
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public int enemyCount;
        public float spawnDelay; // Delay before each enemy in a wave spawns
        public GameObject[] enemyPrefabs; // Array of possible enemies in this wave
    }

    /// <summary>
    /// Handles sequentially spawning waves of enemies like Hades' encounter system.
    /// It waits until all enemies are dead before starting the next wave.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        public Action OnAllWavesCleared;
        
        [Header("Wave Settings")]
        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform[] spawnPoints;
        
        [Header("Timing")]
        [SerializeField] private float timeBetweenWaves = 2.0f; // Brief pause before the next wave spawns

        private int currentWaveIndex = 0;
        private int currentEnemiesAlive = 0;
        private bool encounterRunning = false;

        public void StartWaves()
        {
            if (encounterRunning)
                return;

            if (waves == null || waves.Length == 0)
            {
                OnAllWavesCleared?.Invoke();
                return;
            }

            currentWaveIndex = Mathf.Clamp(currentWaveIndex, 0, waves.Length - 1);
            encounterRunning = true;
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }

        private IEnumerator SpawnWave(Wave wave)
        {
            if (wave == null)
            {
                yield return null;
                HandleWaveFinished();
                yield break;
            }

            Debug.Log($"Starting Wave: {wave.waveName}");
            currentEnemiesAlive = 0;

            if (wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning($"Wave '{wave.waveName}' cannot start because enemy prefabs or spawn points are missing.");
                HandleWaveFinished();
                yield break;
            }

            for (int i = 0; i < wave.enemyCount; i++)
            {
                // Delay between each enemy spawning
                yield return new WaitForSeconds(wave.spawnDelay);
                
                // Select random enemy & random spawn point
                GameObject enemyPrefab = wave.enemyPrefabs[UnityEngine.Random.Range(0, wave.enemyPrefabs.Length)];
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

                if (enemyPrefab == null || spawnPoint == null)
                    continue;

                // Pre-Spawn FX (like the red circles in Hades that appear before an enemy spawns)
                // Instantiate(spawnVFX, spawnPoint.position, Quaternion.identity);
                yield return new WaitForSeconds(0.5f); // "Warning" time before they actually appear

                GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
                currentEnemiesAlive++;
                
                // Hook into enemy death
                var healthCompo = enemyObj.GetComponent<Health>();
                if(healthCompo != null)
                {
                    healthCompo.OnDeath += HandleEnemyDeath;
                }
                else
                    Debug.LogWarning("Spawned enemy lacks a Health component! Wave spawner won't progress.");
            }

            if (currentEnemiesAlive <= 0)
            {
                HandleWaveFinished();
            }
        }

        private void HandleEnemyDeath(GameObject deceasedEnemy)
        {
            var healthCompo = deceasedEnemy.GetComponent<Health>();
            if(healthCompo != null) healthCompo.OnDeath -= HandleEnemyDeath;

            currentEnemiesAlive--;

            if (currentEnemiesAlive <= 0)
            {
                // Wave is clear!
                currentWaveIndex++;

                if (currentWaveIndex < waves.Length)
                {
                    // Start next wave
                    StartCoroutine(StartNextWaveTimer());
                }
                else
                {
                    // All waves passed
                    Debug.Log("Encounter Complete!");
                    OnAllWavesCleared?.Invoke();
                    encounterRunning = false;
                }
            }
        }

        private void HandleWaveFinished()
        {
            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                StartCoroutine(StartNextWaveTimer());
            }
            else
            {
                Debug.Log("Encounter Complete!");
                OnAllWavesCleared?.Invoke();
                encounterRunning = false;
            }
        }

        private IEnumerator StartNextWaveTimer()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
        }

        private void OnDisable()
        {
            encounterRunning = false;
        }
    }
}