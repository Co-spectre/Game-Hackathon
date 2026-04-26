using System.Collections;
using NordicWilds.CameraSystems;
using NordicWilds.Combat;
using NordicWilds.Player;
using NordicWilds.UI;
using UnityEngine;

namespace NordicWilds.World
{
    public class ForestQuestController : MonoBehaviour
    {
        private enum QuestState
        {
            Waiting,
            Intro,
            FindArtifacts,
            Panic,
            Combat,
            Boss,
            TrailReveal,
            DockReady,
            Complete
        }

        [Header("Characters")]
        public Transform player;
        public Transform leaf;

        [Header("Quest Objects")]
        public ForestArtifact[] artifacts;
        public Health[] protectors;
        public Health finalBoss;
        public Transform trailFocus;
        public ForestBoatInteraction boatInteraction;
        public Vector3 yamatoDestination = new Vector3(10000f, 1.05f, 9980f);
        public Vector3 yamatoBoatLandingPoint = new Vector3(10000f, 1.05f, 9958f);
        public Vector3 yamatoBoatApproachPoint = new Vector3(10000f, 0.35f, 9936f);
        public Vector3 yamatoDockedBoatPoint = new Vector3(10000f, 0.35f, 9958f);
        public Vector3 boatSeaDirection = Vector3.back;
        public Vector3 landingActivationPoint = new Vector3(-620f, 1.05f, -628f);

        [Header("Tuning")]
        public float activationRadius = 2f;
        public int requiredProtectorDefeats = 5;
        public float leafFollowDistance = 2.35f;
        public float leafFollowSpeed = 5.5f;
        public float cameraPanDuration = 1.2f;
        public float cameraHoldDuration = 1.45f;
        public float boatCutsceneDuration = 3.25f;
        public float yamatoBoatLandingDuration = 3.25f;
        public float yamatoWalkOffBoatDuration = 2.35f;
        public float protectorSpawnRadius = 6.25f;
        public float leafHeightOffset = 0f; // Adjust this if she sinks or flies
        public float leafScale = 1.0f;     // Force her scale to this value
        public float bossSpeed = 4.5f;     // Adjustable boss movement speed
        private float lastLeafGroundY;     // Optimized caching

        [Header("Cutscene Indices")]
        public int leafMeetCutsceneIndex = 1;
        public int artifactBriefingCutsceneIndex = 2;
        public int firstArtifactCutsceneIndex = 3;
        public int secondArtifactCutsceneIndex = 4;
        public int thirdArtifactCutsceneIndex = 5;
        public int departureCutsceneIndex = 6;

        [Header("Boat Transition Image")]
        // Boat departure cutscene uses img11 so it doesn't repeat the menu's img12 title card.
        public string departureCutsceneResourcePath = "Cutscenes/img11";
        public float departureCutsceneDuration = 4.5f;

        [Header("Artifact Images")]
        // Each artifact uses a distinct image — no duplicates, no reuse with intro/start.
        public string firstArtifactResourcePath = "Cutscenes/img7";
        public string secondArtifactResourcePath = "Cutscenes/img8";
        public string thirdArtifactResourcePath = "Cutscenes/img9";
        public float artifactCutsceneDuration = 4f;

        private QuestState state = QuestState.Waiting;
        private int collectedArtifacts;
        private int defeatedProtectors;
        private string speaker;
        private string dialogueLine;
        private string promptLine;
        private string objectiveLine;
        private string revealTitle;
        private string revealBody;
        private float revealUntil;
        private float visibleChars;
        private float fadeAlpha;
        private bool waitingForDialogueInput;
        private bool dialogueOpen;
        private bool boatSequenceStarted;
        private PlayerController playerController;
        private Rigidbody playerBody;
        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle hintStyle;
        private GUIStyle objectiveStyle;
        private GUIStyle revealTitleStyle;
        private GUIStyle revealBodyStyle;
        private Texture2D darkTex;
        private Texture2D borderTex;
        private Texture2D speakerTex;

        public bool CanCollectArtifacts => state == QuestState.FindArtifacts;
        public bool CanBoardBoat => state == QuestState.DockReady && !boatSequenceStarted;

        private void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            if (player != null)
            {
                playerController = player.GetComponent<PlayerController>();
                playerBody = player.GetComponent<Rigidbody>();
            }

            if (artifacts != null)
            {
                for (int i = 0; i < artifacts.Length; i++)
                {
                    if (artifacts[i] != null)
                    {
                        artifacts[i].SetController(this, i);
                        artifacts[i].gameObject.SetActive(false);
                    }
                }
            }

            if (protectors != null)
            {
                foreach (Health protector in protectors)
                {
                    if (protector == null)
                        continue;

                    protector.OnDeath += HandleProtectorDeath;
                    protector.gameObject.SetActive(false);
                }
            }

            if (finalBoss != null)
            {
                finalBoss.OnDeath += HandleFinalBossDeath;
                finalBoss.gameObject.SetActive(false);
            }

            if (boatInteraction != null)
                boatInteraction.SetController(this);

            PrepareQuestCutscenes();

            objectiveLine = null;
            MissionTracker.Set("Chapter 1: The Drowned Shore", "Wake up. Find your bearings.");
            StartCoroutine(WaitForLandingRoutine());
        }

        private void OnDestroy()
        {
            if (protectors != null)
            {
                foreach (Health protector in protectors)
                {
                    if (protector != null)
                        protector.OnDeath -= HandleProtectorDeath;
                }
            }

            if (finalBoss != null)
                finalBoss.OnDeath -= HandleFinalBossDeath;

            if (darkTex != null)
                Destroy(darkTex);
            if (borderTex != null)
                Destroy(borderTex);
            if (speakerTex != null)
                Destroy(speakerTex);
        }

        private void Update()
        {
            visibleChars += 46f * Time.unscaledDeltaTime;

            if (leaf != null && player != null && state != QuestState.Waiting && state != QuestState.Intro && state != QuestState.Complete)
                FollowPlayerWithLeaf();
        }

        public void SetPrompt(string prompt)
        {
            promptLine = prompt;
        }

        public void ClearPrompt(string prompt)
        {
            if (promptLine == prompt)
                promptLine = null;
        }

        public void CollectArtifact(ForestArtifact artifact)
        {
            if (!CanCollectArtifacts || artifact == null || artifact.IsCollected)
                return;

            artifact.MarkCollected();
            collectedArtifacts++;

            PrepareQuestCutscenes();

            string artifactName = string.IsNullOrEmpty(artifact.ArtifactName) ? "Ancient Artifact" : artifact.ArtifactName;
            revealTitle = artifactName;
            revealBody = "The relic answers your touch and burns with a quiet forest light.";
            revealUntil = Time.time + 2.3f;

            if (collectedArtifacts >= 3)
                StartCoroutine(FinalArtifactAndPanicRoutine());
            else
                StartCoroutine(ArtifactFoundRoutine(collectedArtifacts));
        }

        public void TryStartBoatJourney(ForestBoatInteraction boat)
        {
            if (!CanBoardBoat || boatSequenceStarted)
                return;

            StartCoroutine(BoatJourneyRoutine(boat));
        }

        [ContextMenu("Skip To Goons")]
        public void DebugSkipToPanic()
        {
            if (state == QuestState.Panic || state == QuestState.Combat || state == QuestState.Complete) return;

            // Mark all artifacts as collected to avoid visual glitches
            if (artifacts != null)
            {
                foreach (var a in artifacts)
                {
                    if (a != null && !a.IsCollected) a.MarkCollected();
                }
            }

            collectedArtifacts = 3;
            StopAllCoroutines(); // Stop any pending story dialogue or cutscenes
            
            StartCoroutine(FinalArtifactAndPanicRoutine());
            Debug.Log("Fast-forwarding to Goon ambush!");
        }

        private IEnumerator WaitForLandingRoutine()
        {
            while (Object.FindFirstObjectByType<MainMenuController>() != null
                || player == null
                || Vector3.Distance(player.position, landingActivationPoint) > activationRadius)
            {
                if (player == null)
                {
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        player = playerObj.transform;
                        playerController = player.GetComponent<PlayerController>();
                        playerBody = player.GetComponent<Rigidbody>();
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }

            state = QuestState.Intro;
            objectiveLine = "Talk to Leaf.";
            MissionTracker.Set("Chapter 2: A Stranger in the Pines", "Talk to Leaf.");
            yield return IntroRoutine();
        }

        private IEnumerator IntroRoutine()
        {
            LockPlayer(true);
            yield return new WaitForSeconds(0.45f);

            yield return PlayCutsceneIfAvailable(leafMeetCutsceneIndex);

            yield return ShowLine("Leaf", "Hey. Hey, stay with me. You washed up hard, but you are breathing.");
            yield return ShowLine("Leaf", "My name is Leaf. This forest is old, scattered, and very good at hiding what matters.");

            yield return PlayCutsceneIfAvailable(artifactBriefingCutsceneIndex);

            yield return ShowLine("Leaf", "Listen closely: walk the trail, search the trees and stones, and find three faintly glowing artifacts.");
            yield return ShowLine("Leaf", "When you stand beside one, press E. I will stay with you.");

            state = QuestState.FindArtifacts;
            SetArtifactsVisible(true);
            objectiveLine = "Find 3 artifacts. Stand close to a glow and press E.";
            MissionTracker.Set("Chapter 3: Three Glowing Relics", "Find 3 artifacts hidden in the forest.");
            // Re-surface any unlearned controls — the artifact hunt is a safe moment to teach.
            ControlsTutorial tutorial = ControlsTutorial.GetOrCreate();
            if (tutorial != null) tutorial.NudgeReminder();
            LockPlayer(false);
        }

        private void SetArtifactsVisible(bool visible)
        {
            if (artifacts == null)
                return;

            foreach (ForestArtifact artifact in artifacts)
            {
                if (artifact != null && !artifact.IsCollected)
                    artifact.gameObject.SetActive(visible);
            }
        }

        private IEnumerator ArtifactFoundRoutine(int count)
        {
            yield return PlayCutsceneIfAvailable(GetArtifactCutsceneIndex(count));

            objectiveLine = "Artifacts found: " + count + " / 3.";
            string line = count == 1
                ? "That is one. The others will not shine as brightly. Look between the trees."
                : count == 2
                    ? "Second one. I can feel the ground listening now. One more, carefully."
                    : "The third relic answered. The forest heard it. Be ready.";

            yield return ShowTimedLine("Leaf", line, 3.0f);
            objectiveLine = "Find 3 artifacts. Stand close to a glow and press E.";
        }

        private IEnumerator FinalArtifactAndPanicRoutine()
        {
            yield return ArtifactFoundRoutine(3);
            yield return PanicAndCombatRoutine();
        }

        private int GetArtifactCutsceneIndex(int count)
        {
            if (count == 1)
                return firstArtifactCutsceneIndex;
            if (count == 2)
                return secondArtifactCutsceneIndex;
            return thirdArtifactCutsceneIndex;
        }

        private void PrepareQuestCutscenes()
        {
            CutsceneManager manager = CutsceneManager.GetOrCreate();
            if (manager == null)
                return;

            // Image roster (no image is reused for two different scenes):
            //   img12 - Start game title card     (set by MainMenuController)
            //   img10 - Meeting Leaf
            //   img2  - Artifact briefing (Leaf explains the relics)
            //   img7  - First artifact discovered
            //   img8  - Second artifact discovered
            //   img9  - Third artifact discovered
            //   img11 - Departure on the boat
            //   img3, img4, img5, img6 - Yamato arrival sequence (used by JapanPortal /
            //   YamatoArrivalDialogue cutscene wiring).
            manager.EnsureStartGameCutscene("Cutscenes/img12", 5f,
                "Narrator:  Long before the ice forgot the names of its dead, two shores were stitched together by a single drowning soul. This is where that soul wakes.");
            manager.EnsureCutsceneImage(leafMeetCutsceneIndex, "Cutscenes/img10",
                5.5f, "A Forest Friend",
                "Narrator:  A girl with moss in her hair leans over the wanderer.\n\"Stay with me. The forest watched you wash ashore — and the forest does not watch nothing.\"");
            manager.EnsureCutsceneImage(artifactBriefingCutsceneIndex, "Cutscenes/img2",
                5.5f, "Three Glowing Relics",
                "Leaf:  Three relics sleep among the roots and stones. Each one remembers a part of you that the sea took.\nFind them — stand close — and press E.");
            manager.EnsureCutsceneImage(firstArtifactCutsceneIndex, firstArtifactResourcePath,
                artifactCutsceneDuration, "Artifact of Roots",
                "Narrator:  The first relic answers in a language older than speech. The forest tightens around the wanderer like a held breath.");
            manager.EnsureCutsceneImage(secondArtifactCutsceneIndex, secondArtifactResourcePath,
                artifactCutsceneDuration, "Artifact of Tides",
                "Narrator:  Salt-wind rises from the second relic — a sea-wind from waters the wanderer has not yet crossed, but somehow remembers.");
            manager.EnsureCutsceneImage(thirdArtifactCutsceneIndex, thirdArtifactResourcePath,
                artifactCutsceneDuration, "Artifact of Dawn",
                "Narrator:  The third light answers, and the trees go very quiet. Somewhere in the dark, the old protectors of this forest are sitting up.");
            manager.EnsureCutsceneImage(departureCutsceneIndex, departureCutsceneResourcePath,
                departureCutsceneDuration, "Crossing the Black Water",
                "Narrator:  The longship slips from the shore. The water beneath the keel is colder than it should be, and the stars overhead are not the right stars.");
        }

        private IEnumerator PlayCutsceneIfAvailable(int index)
        {
            CutsceneManager manager = CutsceneManager.GetOrCreate();
            if (manager == null || !manager.HasCutscene(index))
                yield break;

            yield return manager.PlayCutsceneAndWait(index);
        }

        private IEnumerator PanicAndCombatRoutine()
        {
            state = QuestState.Panic;
            objectiveLine = "Leaf is panicking...";
            LockPlayer(true);

            yield return ShowTimedLine("Leaf", "No... no, that was the last seal.", 1.8f);
            yield return ShowTimedLine("Leaf", "The protectors are waking up. They are not going to let us leave.", 2.2f);
            yield return ShowTimedLine("Leaf", "Fight them! Defeat five and I will find us a way out!", 2.3f);

            LockPlayer(false);
            state = QuestState.Combat;
            objectiveLine = "Defeat the forest protectors: 0 / " + requiredProtectorDefeats + ".";
            MissionTracker.Set("Chapter 4: The Forest Wakes", "Defeat the forest protectors.");
            ControlsTutorial combatTut = ControlsTutorial.GetOrCreate();
            if (combatTut != null) combatTut.NudgeReminder();
            ActivateProtectors();
        }

        private void ActivateProtectors()
        {
            if (protectors == null)
                return;

            for (int i = 0; i < protectors.Length; i++)
            {
                Health protector = protectors[i];
                if (protector != null)
                {
                    protector.gameObject.SetActive(true);
                    MoveProtectorIntoAttackRing(protector.transform, i, protectors.Length);

                    EnemyAI ai = protector.GetComponent<EnemyAI>();
                    if (ai != null)
                    {
                        ai.SetAggression(9.8f, 260f, 2.35f);
                        ai.RushPlayer();
                    }
                }
            }
        }

        private void MoveProtectorIntoAttackRing(Transform protector, int index, int total)
        {
            if (player == null || protector == null)
                return;

            float angle = total > 0 ? (360f / total) * index : index * 72f;
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * protectorSpawnRadius;
            Vector3 target = player.position + offset;
            target.y = Mathf.Max(player.position.y + 0.05f, 1f);

            protector.position = target;

            Rigidbody body = protector.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = target;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        private void HandleProtectorDeath(GameObject protectorObject)
        {
            if (state != QuestState.Combat)
                return;

            defeatedProtectors++;
            objectiveLine = "Defeat the forest protectors: "
                + Mathf.Min(defeatedProtectors, requiredProtectorDefeats)
                + " / " + requiredProtectorDefeats + ".";

            if (defeatedProtectors >= requiredProtectorDefeats)
                StartCoroutine(BossIntroRoutine());
        }

        private IEnumerator BossIntroRoutine()
        {
            state = QuestState.Boss;
            LockPlayer(true);
            objectiveLine = "Listen to Leaf.";

            yield return ShowLine("Leaf", "No... the protectors were only the warning.");
            yield return ShowLine("Leaf", "The Ashen Jarl is here. This is the fight that decides whether we leave.");
            yield return ShowLine("Leaf", "Stay close, strike hard, and finish this.");

            ActivateFinalBoss();
            LockPlayer(false);
            objectiveLine = "Defeat the Ashen Jarl.";
            MissionTracker.Set("Chapter 5: The Ashen Jarl", "Defeat the Ashen Jarl.");
        }

        private void ActivateFinalBoss()
        {
            if (finalBoss == null)
                return;

            finalBoss.gameObject.SetActive(true);

            if (player != null)
            {
                Vector3 forward = player.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.01f)
                    forward = Vector3.forward;

                Vector3 spawn = player.position + forward.normalized * 9.5f;
                spawn.y = Mathf.Max(player.position.y + 0.05f, 1f);
                finalBoss.transform.position = spawn;

                Rigidbody body = finalBoss.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.position = spawn;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }

            EnemyAI ai = finalBoss.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.SetAggression(bossSpeed, 260f, 2.8f);
                ai.RushPlayer();
            }
        }

        private void HandleFinalBossDeath(GameObject bossObject)
        {
            if (state != QuestState.Boss)
                return;

            StartCoroutine(AfterCombatRoutine());
        }

        private IEnumerator AfterCombatRoutine()
        {
            state = QuestState.TrailReveal;
            LockPlayer(true);
            objectiveLine = "Listen to Leaf.";

            yield return ShowLine("Leaf", "Woah... the Ashen Jarl is down. You actually did it.");
            yield return ShowLine("Leaf", "The trail opens toward the dock. Follow it, and do not stop.");

            yield return PanCameraToTrail();

            state = QuestState.DockReady;
            objectiveLine = "Follow the trail to the dock. Press E beside the boat.";
            MissionTracker.Set("Chapter 6: Crossing the Black Water", "Board the longship at the dock.");
            LockPlayer(false);
        }

        private IEnumerator BoatJourneyRoutine(ForestBoatInteraction boat)
        {
            boatSequenceStarted = true;
            state = QuestState.Complete;
            promptLine = null;
            objectiveLine = "Boarding the boat...";
            LockPlayer(true);
            if (playerBody != null)
                playerBody.isKinematic = true;

            yield return ShowTimedLine("Leaf", "Into the boat. The sea road should carry us to the next place.", 2.0f);

            Transform boatTransform = boat != null ? boat.BoatRoot : null;
            Vector3 boatStart = boatTransform != null ? boatTransform.position : Vector3.zero;
            Vector3 boatForward = boatSeaDirection.sqrMagnitude > 0.01f
                ? boatSeaDirection
                : (boatTransform != null ? boatTransform.forward : Vector3.forward);
            boatForward.y = 0f;
            if (boatForward.sqrMagnitude < 0.01f)
                boatForward = Vector3.back;
            boatForward.Normalize();

            Vector3 playerStart = player != null ? player.position : Vector3.zero;
            Vector3 playerEnd = boatTransform != null ? GetBoatPassengerPosition(boatTransform, 1.0f) : playerStart;
            Vector3 leafStart = leaf != null ? leaf.position : Vector3.zero;
            Vector3 leafEnd = boatTransform != null ? GetBoatPassengerPosition(boatTransform, -1.0f) : leafStart;

            float t = 0f;
            while (t < boatCutsceneDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / boatCutsceneDuration);
                float ease = k * k * (3f - 2f * k);

                if (boatTransform != null)
                    boatTransform.position = boatStart + boatForward * (18f * ease);
                if (player != null)
                    player.position = boatTransform != null && k > 0.45f
                        ? GetBoatPassengerPosition(boatTransform, 1.0f)
                        : Vector3.Lerp(playerStart, playerEnd, ease);
                if (leaf != null)
                    leaf.position = boatTransform != null && k > 0.45f
                        ? GetBoatPassengerPosition(boatTransform, -1.0f)
                        : Vector3.Lerp(leafStart, leafEnd, ease);

                fadeAlpha = Mathf.Clamp01(Mathf.InverseLerp(0.35f, 1f, k));
                yield return null;
            }

            // NEW: Teleport and change lighting IMMEDIATELY while the screen is black (fadeAlpha = 1)
            // This hides the "vanishing map" effect.
            ApplyYamatoLighting();
            
            RegionMusicController music = RegionMusicController.Instance;
            if (music != null)
                music.PlayYamato();

            if (boatTransform != null)
                PlaceBoatAtYamatoApproach(boatTransform);
            else
                TeleportPlayerToYamatoBoatLanding();

            // Now show the transition image while the player is already at the new location
            CutsceneManager manager = CutsceneManager.GetOrCreate();
            if (manager != null)
            {
                manager.EnsureCutsceneImage(
                    departureCutsceneIndex,
                    departureCutsceneResourcePath,
                    departureCutsceneDuration,
                    "",
                    ""
                );

                if (manager.HasCutscene(departureCutsceneIndex))
                    yield return manager.PlayCutsceneAndWait(departureCutsceneIndex);
            }

            yield return new WaitForSeconds(0.35f);

            float fadeTime = 0f;
            while (fadeTime < 1.2f)
            {
                fadeTime += Time.deltaTime;
                fadeAlpha = Mathf.Lerp(1f, 0f, fadeTime / 1.2f);
                yield return null;
            }

            fadeAlpha = 0f;
            yield return YamatoBoatLandingRoutine(boatTransform);

            if (leaf != null)
            {
                leaf.gameObject.SetActive(true);
                leaf.position = yamatoDestination + new Vector3(1.7f, 0f, 2.2f);
                leaf.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            yield return WalkOffBoatInYamato();

            if (playerBody != null)
                playerBody.isKinematic = false;
            LockPlayer(false);
            objectiveLine = null;

            if (Object.FindFirstObjectByType<WorldMapOverlay>() == null)
                new GameObject("WorldMapOverlay").AddComponent<WorldMapOverlay>();
            if (Object.FindFirstObjectByType<YamatoArrivalDialogue>() == null)
                new GameObject("YamatoArrivalDialogue").AddComponent<YamatoArrivalDialogue>();
        }

        private Vector3 GetBoatPassengerPosition(Transform boatTransform, float sideOffset)
        {
            if (boatTransform == null)
                return Vector3.zero;

            return boatTransform.position + boatTransform.right * sideOffset + Vector3.up * 1.0f;
        }

        private void SetPassengerOnBoat(Transform passenger, Transform boatTransform, float sideOffset)
        {
            if (passenger == null || boatTransform == null)
                return;

            Vector3 pos = GetBoatPassengerPosition(boatTransform, sideOffset);
            passenger.position = pos;
            passenger.rotation = boatTransform.rotation;

            passenger.position = pos;
            passenger.rotation = boatTransform.rotation;

            // Optimized: Use the class-cached Rigidbody if it's the player, otherwise get it once
            Rigidbody body = (passenger == player) ? playerBody : passenger.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = pos;
                body.rotation = passenger.rotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        private void PlaceBoatAtYamatoApproach(Transform boatTransform)
        {
            Vector3 travel = yamatoDockedBoatPoint - yamatoBoatApproachPoint;
            travel.y = 0f;
            Quaternion targetRotation = travel.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(travel.normalized)
                : Quaternion.identity;

            boatTransform.position = yamatoBoatApproachPoint;
            boatTransform.rotation = targetRotation;
            SetPassengerOnBoat(player, boatTransform, 1.0f);
            SetPassengerOnBoat(leaf, boatTransform, -1.0f);

            Health health = player != null ? player.GetComponent<Health>() : null;
            if (health != null)
                health.SetRespawnPoint(yamatoDestination);

            if (Camera.main != null)
            {
                IsometricCameraFollow follow = Camera.main.GetComponent<IsometricCameraFollow>();
                if (follow != null)
                {
                    follow.target = player;
                    follow.SnapToTarget();
                }
            }
        }

        private IEnumerator YamatoBoatLandingRoutine(Transform boatTransform)
        {
            if (boatTransform == null)
            {
                TeleportPlayerToYamatoBoatLanding();
                yield break;
            }

            objectiveLine = "The boat glides toward the Yamato dock...";
            Vector3 start = yamatoBoatApproachPoint;
            Vector3 end = yamatoDockedBoatPoint;
            Vector3 travel = end - start;
            travel.y = 0f;
            Quaternion targetRotation = travel.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(travel.normalized)
                : boatTransform.rotation;

            float t = 0f;
            float duration = Mathf.Max(0.1f, yamatoBoatLandingDuration);
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                float ease = k * k * (3f - 2f * k);
                boatTransform.position = Vector3.Lerp(start, end, ease);
                boatTransform.rotation = Quaternion.Slerp(boatTransform.rotation, targetRotation, Time.deltaTime * 5f);
                SetPassengerOnBoat(player, boatTransform, 1.0f);
                SetPassengerOnBoat(leaf, boatTransform, -1.0f);
                yield return null;
            }

            boatTransform.position = end;
            boatTransform.rotation = targetRotation;

            if (player != null)
            {
                player.position = yamatoBoatLandingPoint;
                if (playerBody != null)
                    playerBody.position = yamatoBoatLandingPoint;
            }
        }

        private void TeleportPlayerToYamatoBoatLanding()
        {
            if (player == null)
                return;

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
                playerBody.position = yamatoBoatLandingPoint;
            }

            player.position = yamatoBoatLandingPoint;
            player.rotation = Quaternion.identity;

            Health health = player.GetComponent<Health>();
            if (health != null)
                health.SetRespawnPoint(yamatoDestination);

            if (Camera.main != null)
            {
                IsometricCameraFollow follow = Camera.main.GetComponent<IsometricCameraFollow>();
                if (follow != null)
                {
                    follow.target = player;
                    follow.SnapToTarget();
                }
            }
        }

        private IEnumerator WalkOffBoatInYamato()
        {
            if (player == null)
                yield break;

            Vector3 start = player.position;
            Vector3 end = yamatoDestination;
            Vector3 direction = end - start;
            direction.y = 0f;
            Quaternion targetRotation = direction.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(direction.normalized)
                : player.rotation;

            objectiveLine = "Arriving in Yamato...";

            float t = 0f;
            while (t < yamatoWalkOffBoatDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, yamatoWalkOffBoatDuration));
                float ease = k * k * (3f - 2f * k);
                Vector3 pos = Vector3.Lerp(start, end, ease);

                player.position = pos;
                player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * 8f);
                if (playerBody != null)
                {
                    playerBody.position = pos;
                    playerBody.rotation = player.rotation;
                    playerBody.linearVelocity = Vector3.zero;
                    playerBody.angularVelocity = Vector3.zero;
                }

                yield return null;
            }

            player.position = end;
            player.rotation = targetRotation;
            if (playerBody != null)
            {
                playerBody.position = end;
                playerBody.rotation = targetRotation;
            }

            Health health = player.GetComponent<Health>();
            if (health != null)
                health.SetRespawnPoint(end);

            if (Camera.main != null)
            {
                IsometricCameraFollow follow = Camera.main.GetComponent<IsometricCameraFollow>();
                if (follow != null)
                    follow.SnapToTarget();
            }
        }

        private IEnumerator PanCameraToTrail()
        {
            Camera cam = Camera.main;
            if (cam == null || trailFocus == null || player == null)
                yield break;

            IsometricCameraFollow follow = cam.GetComponent<IsometricCameraFollow>();
            if (follow != null)
                follow.enabled = false;

            Vector3 offset = cam.transform.position - player.position;
            Vector3 startPos = cam.transform.position;
            Vector3 endPos = trailFocus.position + offset;
            Quaternion startRot = cam.transform.rotation;

            float t = 0f;
            while (t < cameraPanDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / cameraPanDuration);
                float ease = k * k * (3f - 2f * k);
                cam.transform.position = Vector3.Lerp(startPos, endPos, ease);
                cam.transform.rotation = startRot;
                yield return null;
            }

            yield return new WaitForSeconds(cameraHoldDuration);

            t = 0f;
            while (t < cameraPanDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / cameraPanDuration);
                float ease = k * k * (3f - 2f * k);
                cam.transform.position = Vector3.Lerp(endPos, startPos, ease);
                cam.transform.rotation = startRot;
                yield return null;
            }

            if (follow != null)
            {
                follow.enabled = true;
                follow.SnapToTarget();
            }
        }

        private void FollowPlayerWithLeaf()
        {
            if (leaf == null) return;
            
            // Force Scale and Physics State
            leaf.localScale = Vector3.one * leafScale;
            Rigidbody leafRb = leaf.GetComponent<Rigidbody>();
            if (leafRb != null) leafRb.isKinematic = true;

            Vector3 target = player.position - player.forward * leafFollowDistance + player.right * 1.35f;
            
            // Raycast to find actual ground height for Leaf (Optimized: only raycast every 3 frames)
            if (Time.frameCount % 3 == 0)
            {
                if (Physics.Raycast(target + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
                {
                    lastLeafGroundY = hit.point.y + leafHeightOffset;
                }
                else
                {
                    lastLeafGroundY = player.position.y + leafHeightOffset;
                }
            }
            target.y = lastLeafGroundY;
            
            float dist = Vector3.Distance(leaf.position, target);
            leaf.position = Vector3.Lerp(leaf.position, target, Time.deltaTime * leafFollowSpeed);

            Vector3 look = player.position - leaf.position;
            look.y = 0f;
            if (look.sqrMagnitude > 0.05f)
                leaf.rotation = Quaternion.Slerp(leaf.rotation, Quaternion.LookRotation(look), Time.deltaTime * 9f);

            // Play walk animation if moving
            Animator anim = leaf.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                bool isMoving = dist > 0.15f;
                // Set both common param names just in case
                foreach (var p in anim.parameters)
                {
                    if (p.name == "Speed") anim.SetFloat("Speed", isMoving ? 1f : 0f);
                    if (p.name == "Walking") anim.SetBool("Walking", isMoving);
                    if (p.name == "isWalking") anim.SetBool("isWalking", isMoving);
                }
            }
        }

        private IEnumerator ShowLine(string who, string line)
        {
            speaker = who;
            dialogueLine = line;
            visibleChars = 0f;
            dialogueOpen = true;
            waitingForDialogueInput = true;

            while (waitingForDialogueInput)
            {
                bool pressed = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)
                    || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);

                if (pressed)
                {
                    if (visibleChars < dialogueLine.Length)
                    {
                        visibleChars = dialogueLine.Length;
                    }
                    else
                    {
                        waitingForDialogueInput = false;
                    }
                }

                yield return null;
            }

            dialogueOpen = false;
        }

        private IEnumerator ShowTimedLine(string who, string line, float duration)
        {
            speaker = who;
            dialogueLine = line;
            visibleChars = 0f;
            dialogueOpen = true;
            yield return new WaitForSeconds(duration);
            dialogueOpen = false;
        }

        private void LockPlayer(bool locked)
        {
            if (playerController != null)
                playerController.SetControlsLocked(locked);

            if (playerBody != null && locked)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }
        }

        private void ApplyYamatoLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.013f;
            RenderSettings.fogColor = new Color(0.46f, 0.30f, 0.32f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.50f, 0.36f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.30f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.28f);
            RenderSettings.ambientIntensity = 0.85f;
            RenderSettings.reflectionIntensity = 0.30f;

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.32f, 0.22f, 0.26f);
                Camera.main.allowHDR = false;
            }

            Light dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = new Color(0.96f, 0.58f, 0.32f);
                dirLight.intensity = 0.55f;
                dirLight.transform.rotation = Quaternion.Euler(10f, -32f, 0f);
                dirLight.shadows = LightShadows.Soft;
                dirLight.shadowStrength = 0.38f;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (!string.IsNullOrEmpty(objectiveLine))
            {
                Rect objective = new Rect(18f, 18f, Mathf.Min(650f, Screen.width - 36f), 46f);
                DrawPanel(objective, 2f);
                GUI.Label(new Rect(objective.x + 14f, objective.y + 6f, objective.width - 28f, objective.height - 12f), objectiveLine, objectiveStyle);
            }

            if (!string.IsNullOrEmpty(promptLine))
            {
                float promptWidth = Mathf.Min(560f, Screen.width - 36f);
                Rect prompt = new Rect((Screen.width - promptWidth) * 0.5f, Screen.height - 112f, promptWidth, 46f);
                DrawPanel(prompt, 2f);
                GUI.Label(new Rect(prompt.x + 16f, prompt.y + 7f, prompt.width - 32f, prompt.height - 14f), promptLine, hintStyle);
            }

            if (Time.time < revealUntil)
            {
                float cardWidth = Mathf.Min(460f, Screen.width - 36f);
                Rect card = new Rect((Screen.width - cardWidth) * 0.5f, 88f, cardWidth, 100f);
                DrawPanel(card, 2f);
                GUI.Label(new Rect(card.x + 22f, card.y + 14f, card.width - 44f, 32f), revealTitle, revealTitleStyle);
                GUI.Label(new Rect(card.x + 22f, card.y + 48f, card.width - 44f, 40f), revealBody, revealBodyStyle);
            }

            if (dialogueOpen && !string.IsNullOrEmpty(dialogueLine))
            {
                float width = Mathf.Min(780f, Screen.width - 70f);
                Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 232f, width, 182f);
                DrawPanel(panel, 3f);

                Rect speakerBox = new Rect(panel.x + 24f, panel.y + 14f, 210f, 34f);
                DrawSpeakerPanel(speakerBox);
                GUI.Label(new Rect(speakerBox.x + 12f, speakerBox.y + 5f, speakerBox.width - 24f, 24f), speaker, speakerStyle);

                int chars = Mathf.Clamp(Mathf.FloorToInt(visibleChars), 0, dialogueLine.Length);
                GUI.Label(new Rect(panel.x + 28f, panel.y + 58f, panel.width - 56f, 86f), dialogueLine.Substring(0, chars), dialogueStyle);

                Rect continueBox = new Rect(panel.x + panel.width - 176f, panel.y + panel.height - 38f, 150f, 24f);
                DrawSpeakerPanel(continueBox);
                GUI.Label(new Rect(continueBox.x + 4f, continueBox.y + 2f, continueBox.width - 8f, continueBox.height - 4f), "E / Space", hintStyle);
            }

            if (fadeAlpha > 0.001f)
            {
                Color previous = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previous;
            }
        }

        private void DrawPanel(Rect rect, float thickness)
        {
            GUI.DrawTexture(rect, darkTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), borderTex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), borderTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), borderTex);
            GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), borderTex);
        }

        private void DrawSpeakerPanel(Rect rect)
        {
            GUI.DrawTexture(rect, speakerTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f), borderTex);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 4f, rect.width, 4f), borderTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 4f, rect.height), borderTex);
            GUI.DrawTexture(new Rect(rect.xMax - 4f, rect.y, 4f, rect.height), borderTex);
        }

        private void EnsureStyles()
        {
            if (dialogueStyle != null)
                return;

            darkTex = MakeTex(new Color(0.04f, 0.025f, 0.015f, 0.96f)); // More opaque
            borderTex = MakeTex(new Color(0.86f, 0.64f, 0.28f, 0.98f)); // Thicker color
            speakerTex = MakeTex(new Color(0.12f, 0.075f, 0.035f, 0.98f));

            speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            speakerStyle.normal.textColor = new Color(0.98f, 0.82f, 0.42f, 1f);

            dialogueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            dialogueStyle.normal.textColor = new Color(0.96f, 0.90f, 0.78f, 1f);

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.95f, 0.88f, 0.68f, 0.94f);

            objectiveStyle = new GUIStyle(hintStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22
            };

            revealTitleStyle = new GUIStyle(speakerStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20
            };
            revealTitleStyle.normal.textColor = new Color(1f, 0.86f, 0.38f, 1f);

            revealBodyStyle = new GUIStyle(dialogueStyle)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 16
            };
        }

        private Texture2D MakeTex(Color color)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
    }

}
