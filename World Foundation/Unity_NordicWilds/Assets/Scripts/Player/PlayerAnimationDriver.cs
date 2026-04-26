using UnityEngine;

namespace NordicWilds.Player
{
    public class PlayerAnimationDriver : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Animator component on the Meshy child model.")]
        public Animator animator;
        [Tooltip("The parent Rigidbody.")]
        public Rigidbody rb;
        [Tooltip("The parent PlayerController.")]
        public PlayerController playerController;

        // Animator parameter hashes for performance
        private static readonly int HashSpeed   = Animator.StringToHash("Speed");
        private static readonly int HashSprint  = Animator.StringToHash("IsSprinting");
        private static readonly int HashDash    = Animator.StringToHash("IsDashing");
        private static readonly int HashAttack  = Animator.StringToHash("Attack");
        private static readonly int HashHeavyAttack = Animator.StringToHash("HeavyAttack");
        private static readonly int HashHit     = Animator.StringToHash("Hit");
        private static readonly int HashDead    = Animator.StringToHash("IsDead");
        private static readonly int HashGrounded = Animator.StringToHash("IsGrounded");

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (playerController == null) playerController = GetComponent<PlayerController>();
            
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            if (animator == null || rb == null || playerController == null) return;

            // Use the intended move direction instead of physical velocity to prevent 
            // animations from stuttering or dropping to Idle when rubbing against walls
            // or when physics interactions cause momentary velocity drops.
            float horizontalSpeed = playerController.moveDirection.magnitude;

            // 2. Update Animator every frame
            animator.SetFloat(HashSpeed, horizontalSpeed, 0.05f, Time.deltaTime);
            
            // 3. True only when sprinting
            animator.SetBool(HashSprint, playerController.IsSprinting);
            
            // 4. True only during dash
            animator.SetBool(HashDash, playerController.CurrentState == PlayerController.PlayerState.Dashing);

            // 5. Grounded state
            animator.SetBool(HashGrounded, playerController.IsGrounded);
        }

        // 5. Called by PlayerCombat when attacking
        public void PlayAttack(bool isHeavy = false)
        {
            if (animator != null)
            {
                if (isHeavy) animator.SetTrigger(HashHeavyAttack);
                else         animator.SetTrigger(HashAttack);
            }
        }

        // 6. Called by Health/Combat when taking damage
        public void PlayHit()
        {
            if (animator != null) animator.SetTrigger(HashHit);
        }

        // 7. Called by Health when dying
        public void SetDead(bool isDead)
        {
            if (animator != null) animator.SetBool(HashDead, isDead);
        }
    }
}
