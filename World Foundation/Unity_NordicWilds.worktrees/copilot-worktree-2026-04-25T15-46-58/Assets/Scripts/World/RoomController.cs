using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NordicWilds.Combat;

namespace NordicWilds.World
{
    /// <summary>
    /// Replicates the core Hades gameplay loop:
    /// Enter Room -> Doors Lock -> Waves Spawn -> Enemies Defeated -> Reward Appears -> Doors Unlock
    /// </summary>
    public class RoomController : MonoBehaviour
    {
        [Header("Room State")]
        public bool isCleared = false;
        private bool hasEntered = false;
        private bool hasSubscribedToWaveSpawner = false;

        [Header("Exits & Entrances")]
        [SerializeField] private GameObject[] entranceDoors;
        [SerializeField] private GameObject[] exitDoors;
        [SerializeField] private GameObject rewardSpawnPoint;

        [Header("Combat Settings")]
        [SerializeField] private WaveSpawner waveSpawner;

        [Header("Events")]
        public UnityEvent onRoomEntered;
        public UnityEvent onRoomCleared;

        private void Start()
        {
            // Ensure exits are locked initially unless it's a safe room (like Charon/NPC rooms in Hades)
            SetDoorsLocked(exitDoors, true);
        }

        private void OnDisable()
        {
            UnsubscribeFromWaveSpawner();
        }

        private void OnDestroy()
        {
            UnsubscribeFromWaveSpawner();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!hasEntered && !isCleared && other.CompareTag("Player"))
            {
                hasEntered = true;
                StartRoomEncounter();
            }
        }

        private void StartRoomEncounter()
        {
            // Lock the player in (Hades style)
            SetDoorsLocked(entranceDoors, true);
            
            // Notify other systems (e.g., play combat music)
            onRoomEntered?.Invoke();

            if (waveSpawner != null)
            {
                // Hook into the wave spawner to know when combat ends
                if (!hasSubscribedToWaveSpawner)
                {
                    waveSpawner.OnAllWavesCleared += HandleRoomCleared;
                    hasSubscribedToWaveSpawner = true;
                }
                waveSpawner.StartWaves();
            }
            else
            {
                // If it's a peaceful room, clear immediately
                HandleRoomCleared();
            }
        }

        private void HandleRoomCleared()
        {
            UnsubscribeFromWaveSpawner();

            isCleared = true;

            // Spawn Room Reward (Boon, Max Health, Gold, etc.)
            SpawnReward();

            // Open exits to let player choose next path
            SetDoorsLocked(exitDoors, false);

            onRoomCleared?.Invoke();
        }

        private void SpawnReward()
        {
            // In a real Hades clone, this determines what drops based on the door you chose previously
            Debug.Log("Room Cleared! Spawning Reward...");
            // Instantiate(rewardPrefab, rewardSpawnPoint.transform.position, Quaternion.identity);
        }

        private void SetDoorsLocked(GameObject[] doors, bool isLocked)
        {
            if (doors == null)
                return;

            foreach (var door in doors)
            {
                if (door == null)
                    continue;

                // In a full implementation, trigger door animation/particle effects here
                door.SetActive(isLocked); // Simply toggle the physical barrier for now
            }
        }

        private void UnsubscribeFromWaveSpawner()
        {
            if (waveSpawner != null && hasSubscribedToWaveSpawner)
            {
                waveSpawner.OnAllWavesCleared -= HandleRoomCleared;
                hasSubscribedToWaveSpawner = false;
            }
        }
    }
}