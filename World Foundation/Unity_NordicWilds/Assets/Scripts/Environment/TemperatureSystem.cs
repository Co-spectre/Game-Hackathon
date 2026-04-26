using UnityEngine;

namespace NordicWilds.Environment
{
    /// <summary>
    /// Temperature / freeze system — DISABLED.
    /// The class is kept as an empty stub so any GameObjects that already have
    /// this component attached do not show "Missing Script" errors in Unity.
    /// All temperature decay, freeze damage, and breath-steam VFX have been removed.
    /// </summary>
    public class TemperatureSystem : MonoBehaviour
    {
        // No fields, no Update, no damage — feature fully removed.
        // Safe to also remove this component from any Player GameObject via the Inspector.
    }
}