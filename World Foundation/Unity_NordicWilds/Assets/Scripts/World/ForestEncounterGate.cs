using NordicWilds.Combat;
using UnityEngine;

namespace NordicWilds.World
{
    public class ForestEncounterGate : MonoBehaviour
    {
        [SerializeField] private Health[] foes;
        [SerializeField] private GameObject portalRoot;
        [SerializeField] private GameObject[] blockersUntilCleared;

        private int remainingFoes;
        private bool unlocked;

        private void Start()
        {
            if (portalRoot != null)
                portalRoot.SetActive(false);

            remainingFoes = 0;

            if (foes != null)
            {
                foreach (Health foe in foes)
                {
                    if (foe == null || foe.IsDead)
                        continue;

                    remainingFoes++;
                    foe.OnDeath += HandleFoeDeath;
                }
            }

            if (remainingFoes <= 0)
                UnlockPortal();
        }

        private void OnDestroy()
        {
            if (foes == null)
                return;

            foreach (Health foe in foes)
            {
                if (foe != null)
                    foe.OnDeath -= HandleFoeDeath;
            }
        }

        private void HandleFoeDeath(GameObject foeObject)
        {
            Health foe = foeObject != null ? foeObject.GetComponent<Health>() : null;
            if (foe != null)
                foe.OnDeath -= HandleFoeDeath;

            remainingFoes = Mathf.Max(0, remainingFoes - 1);

            if (remainingFoes <= 0)
                UnlockPortal();
        }

        private void UnlockPortal()
        {
            if (unlocked)
                return;

            unlocked = true;

            if (portalRoot != null)
                portalRoot.SetActive(true);

            if (blockersUntilCleared == null)
                return;

            foreach (GameObject blocker in blockersUntilCleared)
            {
                if (blocker != null)
                    blocker.SetActive(false);
            }
        }
    }
}
