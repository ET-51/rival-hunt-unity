using System.Collections.Generic;
using UnityEngine;

/// The twist of this project: the Rival is SIMULATED, not a real AI agent.
/// It is a number that grows over time, shaped by three modifiers.
/// Design reason: the competition should live in the player's decisions
/// (which gems to grab, which zones to clear), not in fighting an agent.
public class RivalSimulator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XPSystem rivalXp;   // the Rival's own XPSystem
    [SerializeField] private XPSystem playerXp;  // to compare levels for rubber-band

    [Header("Base pace")]
    [Tooltip("XP per second with no modifiers. Tuned so the race stays close for 3:00.")]
    [SerializeField] private float baseXpPerSecond = 2.2f;

    [Header("Starving - player pickups slow the Rival")]
    [Tooltip("Seconds a collected gem keeps 'starving' the Rival.")]
    [SerializeField] private float starvingWindow = 10f;
    [Tooltip("Rate reduction per recent gem (0.08 = -8% each).")]
    [SerializeField] private float starvingPerGem = 0.08f;
    [Tooltip("Maximum total reduction (0.4 = -40%).")]
    [SerializeField] private float starvingMax = 0.4f;

    [Header("Rubber-band - keeps races alive")]
    [SerializeField] private int rubberBandLevelGap = 3;
    [SerializeField] private float rubberBandBoost = 1.15f;

    // Timestamps of the player's recent gem pickups, for the starving window.
    private readonly Queue<float> recentPickups = new Queue<float>();

    public XPSystem RivalXp => rivalXp;

    // True when the Rival is 3+ levels ahead. MatchController boosts the player's gems.
    public bool RivalIsFarAhead =>
        rivalXp.CurrentLevel - playerXp.CurrentLevel >= rubberBandLevelGap;

    private void OnEnable()
    {
        Gem.OnGemCollected += HandleGemCollected;
    }

    private void OnDisable()
    {
        Gem.OnGemCollected -= HandleGemCollected;
    }

    private void HandleGemCollected()
    {
        recentPickups.Enqueue(Time.time);
    }

    private void Update()
    {
        if (MatchController.Instance == null) return;
        if (MatchController.Instance.State == MatchState.Ended) return;

        // Drop pickups that fell out of the starving window.
        while (recentPickups.Count > 0 && Time.time - recentPickups.Peek() > starvingWindow)
        {
            recentPickups.Dequeue();
        }

        float rate = baseXpPerSecond;
        rate *= StarvingMultiplier();
        rate *= RubberBandMultiplier();
        rate *= MatchController.Instance.SprintMultiplier; // Final Sprint doubles both sides

        rivalXp.AddXp(rate * Time.deltaTime);
    }

    private float StarvingMultiplier()
    {
        // Each recent player pickup slows the Rival, capped at starvingMax.
        float reduction = Mathf.Min(recentPickups.Count * starvingPerGem, starvingMax);
        return 1f - reduction;
    }

    private float RubberBandMultiplier()
    {
        // Only boosts the Rival when the PLAYER is far ahead.
        bool playerFarAhead =
            playerXp.CurrentLevel - rivalXp.CurrentLevel >= rubberBandLevelGap;
        return playerFarAhead ? rubberBandBoost : 1f;
    }
}
