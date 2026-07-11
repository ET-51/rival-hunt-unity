using System;
using UnityEngine;

public enum MatchState { Running, FinalSprint, Ended }

public enum MatchResult { PlayerWins, RivalWins, Tie }

/// Owns the match: timer, state transitions, and the final comparison.
/// Also owns the player-side multipliers so Gem has one place to ask.
public class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private XPSystem playerXp;
    [SerializeField] private RivalSimulator rival;

    [Header("Timing")]
    [SerializeField] private float matchLengthSeconds = 60f;
    [SerializeField] private float finalSprintSeconds = 15f;

    [Header("Final Sprint")]
    [Tooltip("Both sides' XP gain is multiplied by this during the last seconds.")]
    [SerializeField] private float sprintBoost = 2f;

    [Header("Rubber-band (player side)")]
    [SerializeField] private float playerRubberBandBoost = 1.15f;

    public MatchState State { get; private set; } = MatchState.Running;
    public float TimeRemaining { get; private set; }

    public event Action OnFinalSprintStarted;
    public event Action<MatchResult, int, int> OnMatchEnded; // result, playerLevel, rivalLevel

    // Gems multiply their XP value by this. Combines sprint + rubber-band.
    public float PlayerGemMultiplier
    {
        get
        {
            float m = SprintMultiplier;
            if (rival.RivalIsFarAhead) m *= playerRubberBandBoost;
            return m;
        }
    }

    public float SprintMultiplier => State == MatchState.FinalSprint ? sprintBoost : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        TimeRemaining = matchLengthSeconds;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (State == MatchState.Ended) return;

        TimeRemaining -= Time.deltaTime;

        // Running -> FinalSprint at the threshold.
        if (State == MatchState.Running && TimeRemaining <= finalSprintSeconds)
        {
            State = MatchState.FinalSprint;
            OnFinalSprintStarted?.Invoke();
        }

        // Any state -> Ended at zero.
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            EndMatch();
        }
    }

    private void EndMatch()
    {
        State = MatchState.Ended;

        int playerLevel = playerXp.CurrentLevel;
        int rivalLevel = rival.RivalXp.CurrentLevel;

        MatchResult result = MatchResult.Tie;

        if (playerLevel > rivalLevel)
        {
            result = MatchResult.PlayerWins;
        }
        else if (rivalLevel > playerLevel)
        {
            result = MatchResult.RivalWins;
        }

        OnMatchEnded?.Invoke(result, playerLevel, rivalLevel);
    }
}
