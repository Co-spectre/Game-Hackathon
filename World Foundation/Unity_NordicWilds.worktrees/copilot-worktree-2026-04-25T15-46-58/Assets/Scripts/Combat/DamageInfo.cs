using UnityEngine;

namespace NordicWilds.Combat
{
    public enum DamageType
    {
        Physical,
        Fire,
        Frost,
        Arcane,
        Environmental
    }

    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly GameObject Source;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitDirection;
        public readonly DamageType Type;
        public readonly float KnockbackForce;
        public readonly float StaggerDuration;
        public readonly bool CanBeBlocked;
        public readonly bool IsCritical;

        public DamageInfo(
            float amount,
            GameObject source,
            Vector3 hitPoint,
            Vector3 hitDirection,
            DamageType type = DamageType.Physical,
            float knockbackForce = 0f,
            float staggerDuration = 0f,
            bool canBeBlocked = true,
            bool isCritical = false)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            HitDirection = hitDirection;
            Type = type;
            KnockbackForce = knockbackForce;
            StaggerDuration = staggerDuration;
            CanBeBlocked = canBeBlocked;
            IsCritical = isCritical;
        }
    }
}