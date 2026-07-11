using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// All UI in one place. Listens to events; contains zero game logic.
public class RaceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XPSystem playerXp;
    [SerializeField] private RivalSimulator rival;
    [SerializeField] private MatchController match;

    [Header("Bars")]
    [SerializeField] private Slider playerBar;
    [SerializeField] private Slider rivalBar;
    [SerializeField] private Text playerLevelText;
    [SerializeField] private Text rivalLevelText;
    [SerializeField] private Image rivalBarFill;
    [SerializeField] private RectTransform rivalMarker;

    [Header("Timer & Sprint")]
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject sprintBanner;

    [Header("Results")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private Text resultsText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rivalLevelUpPing;

    [Header("Lead colors")]
    [SerializeField] private Color aheadColor = new Color(0.35f, 0.8f, 0.4f);
    [SerializeField] private Color behindColor = new Color(0.85f, 0.3f, 0.3f);
    [SerializeField] private Color tiedColor = new Color(0.95f, 0.75f, 0.25f);
    [SerializeField] private Color sprintColor = new Color(1f, 0.72f, 0.2f);

    private Color normalTimerColor;
    private Vector3 bannerBaseScale;

    private void OnEnable()
    {
        rival.RivalXp.OnLevelUp += HandleRivalLevelUp;
        match.OnFinalSprintStarted += HandleFinalSprint;
        match.OnMatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        rival.RivalXp.OnLevelUp -= HandleRivalLevelUp;
        match.OnFinalSprintStarted -= HandleFinalSprint;
        match.OnMatchEnded -= HandleMatchEnded;
    }

    private void Start()
    {
        normalTimerColor = timerText.color;
        bannerBaseScale = sprintBanner.transform.localScale;
        sprintBanner.SetActive(false);
        resultsPanel.SetActive(false);
    }

    private void Update()
    {
        // Bars show progress toward each side's next level.
        playerBar.value = playerXp.Progress01;
        rivalBar.value = rival.RivalXp.Progress01;
        playerLevelText.text = "LV " + playerXp.CurrentLevel;
        rivalLevelText.text = "LV " + rival.RivalXp.CurrentLevel;

        // The Rival portrait follows its progress along the thin race strip.
        float markerPosition = Mathf.Lerp(0.02f, 0.98f, rivalBar.value);
        rivalMarker.anchorMin = new Vector2(markerPosition, 0.5f);
        rivalMarker.anchorMax = new Vector2(markerPosition, 0.5f);

        // Lead is shown with color, not numbers, so it is readable during combat.
        if (playerXp.CurrentLevel > rival.RivalXp.CurrentLevel)
        {
            rivalBarFill.color = aheadColor;
        }
        else if (playerXp.CurrentLevel < rival.RivalXp.CurrentLevel)
        {
            rivalBarFill.color = behindColor;
        }
        else
        {
            rivalBarFill.color = tiedColor;
        }

        int displayedTime = Mathf.CeilToInt(match.TimeRemaining);
        int minutes = displayedTime / 60;
        int seconds = displayedTime % 60;
        timerText.text = minutes + ":" + seconds.ToString("00");

        if (match.State == MatchState.FinalSprint)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 7f) * 0.05f;
            sprintBanner.transform.localScale = bannerBaseScale * pulse;
        }
    }

    private void HandleRivalLevelUp(XPSystem system, int newLevel)
    {
        // Audio cue so the player's eyes can stay on the arena.
        if (audioSource != null && rivalLevelUpPing != null)
        {
            audioSource.PlayOneShot(rivalLevelUpPing);
        }
    }

    private void HandleFinalSprint()
    {
        sprintBanner.SetActive(true);
        timerText.color = sprintColor;
    }

    private void HandleMatchEnded(MatchResult result, int playerLevel, int rivalLevel)
    {
        resultsPanel.SetActive(true);
        sprintBanner.SetActive(false);
        timerText.color = normalTimerColor;

        string headline = "TIE";

        if (result == MatchResult.PlayerWins)
        {
            headline = "YOU WIN";
        }
        else if (result == MatchResult.RivalWins)
        {
            headline = "RIVAL WINS";
        }

        resultsText.text = headline + "\nYou: LV " + playerLevel + "   Rival: LV " + rivalLevel;
    }

    // Hooked to the Play Again button in the Inspector.
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
