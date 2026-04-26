using UnityEngine;

namespace NordicWilds.UI
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [Header("Cutscene")]
        public int cutsceneIndex;
        public bool triggerOnce = true;

        private bool hasTriggered;

        public void PlayConfiguredCutscene()
        {
            if (triggerOnce && hasTriggered)
                return;

            if (CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.PlayCutscene(cutsceneIndex);
                hasTriggered = true;
            }
        }

        public void PlayCutsceneByIndex(int index)
        {
            if (CutsceneManager.Instance != null)
                CutsceneManager.Instance.PlayCutscene(index);
        }
    }
}