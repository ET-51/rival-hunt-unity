using System;
using UnityEngine;

/// Reusable XP + leveling component.
/// Used by both the player and the Rival.
public class XPSystem : MonoBehaviour
{
    [Header("Leveling")]
    [SerializeField] private float baseXpToLevel = 10f;   // XP needed for level 2
    [SerializeField] private float xpGrowthPerLevel = 5f; // each level needs this much more

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXp { get; private set; } = 0f;

    // Fired when a level is gained. RaceUI listens to this.
    public event Action<XPSystem, int> OnLevelUp;

    public float XpToNextLevel => baseXpToLevel + xpGrowthPerLevel * (CurrentLevel - 1);

    // 0..1 progress toward the next level, for UI bars.
    public float Progress01 => Mathf.Clamp01(CurrentXp / XpToNextLevel);

    public void AddXp(float amount)
    {
        if (amount <= 0f) return;

        CurrentXp += amount;

        // While-loop so one big gain can grant multiple levels.
        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            CurrentLevel++;
            OnLevelUp?.Invoke(this, CurrentLevel);
        }
    }
}
