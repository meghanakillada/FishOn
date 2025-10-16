using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }

    [Header("UI")]
    public TMP_Text scoreText, timerText, fishText;
    public UnityEngine.UI.Slider depthSlider;

    [Header("Round")]
    public float roundSeconds = 60f;
    float timeLeft;
    int score;
    int fishCount;

    [Header("Audio")]
    public AudioSource sfx;
    public AudioClip castSfx, reelSfx, splashSfx, catchChimeSfx, gameOverSfx, gameOverHighSfx;
    public AudioSource music; // loop upbeat bgm

    Transform lastHookOrigin;
    HookController currentHook;

    void Start()
    {
        timeLeft = roundSeconds;
        UpdateUI();
        if (music && !music.isPlaying) music.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();

        timeLeft -= Time.unscaledDeltaTime * (Time.timeScale > 0f ? 1f : 0f);
        if (timeLeft <= 0f) EndGame();
        UpdateUI();

        // Depth meter follows current hook (0..maxDepth)
        if (currentHook && depthSlider)
        {
            // Approximation: distance from origin in Y
            float depth = Mathf.Max(0f, currentHook.transform.position.y - currentHook.transform.position.y); // placeholder to keep slider optional
        }
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
        if (fishText) fishText.text = $"Fish: {fishCount}";
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(Mathf.Max(0f, timeLeft))}";
    }

    public void AddScore(int pts) { score += pts; UpdateUI(); }
    public void IncrementFish() { fishCount++; UpdateUI(); }

    public void RegisterHook(Transform origin, HookController hook)
    {
        lastHookOrigin = origin; currentHook = hook;
    }

    public bool CurrentHookPos(out Vector3 pos)
    {
        if (currentHook) { pos = currentHook.transform.position; return true; }
        pos = Vector3.zero; return false;
    }

    void EndGame()
    {
        Time.timeScale = 0f;
        int hi = PlayerPrefs.GetInt("HI", 0);
        bool newHigh = score > hi;
        if (newHigh) PlayerPrefs.SetInt("HI", score);

        if (sfx) sfx.PlayOneShot(newHigh ? gameOverHighSfx : gameOverSfx);

        // Pass score to End scene
        PlayerPrefs.SetInt("LAST_SCORE", score);
        PlayerPrefs.SetInt("LAST_HIGH", Mathf.Max(hi, score));
        Time.timeScale = 1f;
        SceneManager.LoadScene("End");
    }

    void TogglePause()
    {
        Time.timeScale = (Time.timeScale > 0f) ? 0f : 1f;
    }

    // Convenience SFX calls
    public void PlaySplashOrPop(bool caught) { if (sfx) sfx.PlayOneShot(caught ? catchChimeSfx : splashSfx); }
    public void PlayCatchSfx() { if (sfx) sfx.PlayOneShot(catchChimeSfx); }
    public void PlayReelSfx() { if (sfx) sfx.PlayOneShot(reelSfx); }
}
