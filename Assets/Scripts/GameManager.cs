using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private static bool skipTitleOnNextLoad;

    [Header("Refs")]
    [SerializeField] private PlayerMove player;
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private GameObject heartItemPrefab;
    [SerializeField] private GameObject playerDeathFxPrefab;


    [Header("Message UI")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI rankingText;

    [Header("Settings")]
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float invincibilityDuration = 1.0f;
    [SerializeField] private float heartSpawnInterval = 15f;
    [SerializeField] private float heartLifetime = 6f;
    [SerializeField] private float encouragementDuration = 2.5f;
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private AudioSource titleBgm;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioClip deathSfx;


    private bool hasGameStarted;

    [SerializeField]
    private string[] encouragementMessages =
    {
        "좋아요! 계속 버텨봐요!",
        "집중력 유지!",
        "지금 페이스 좋아요!",
        "조금만 더 버티면 돼요!",
        "리듬을 잃지 마세요!"
    };

    private const string LeaderboardKey = "GM_LEADERBOARD";
    private const int MaxLeaderboardEntries = 5;

    private readonly List<float> leaderboard = new List<float>();

    private int lives;
    private float elapsedTime;
    private bool isGameOver;

    private float heartTimer;
    private bool isInvincible;
    private float invincibleTimer;

    private float nextEncouragementTime = 10f;
    private float encouragementTimer;

    public bool IsGameRunning => hasGameStarted && !isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadLeaderboard();
    }

    private void Start()
    {
        if (skipTitleOnNextLoad)
        {
            skipTitleOnNextLoad = false;
            StartGame();
            return;
        }

        EnterTitleState();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
            return;
        }

        if (!hasGameStarted)
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                StartGame();
            }
            return;
        }

        if (!IsGameRunning)
        {
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                RestartGame();
            }
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimeUI();

        HandleHeartSpawn();
        UpdateInvincibility();
        HandleEncouragement();
    }

    private void StartGame()
    {
        hasGameStarted = true;
        if (titlePanel != null) titlePanel.SetActive(false);
        {
        titleBgm.time = 0f;   // 매번 처음부터 재생
        titleBgm.Play();
        }

        Time.timeScale = 1f;
        isGameOver = false;
        elapsedTime = 0f;
        lives = Mathf.Max(1, startingLives);

        heartTimer = 0f;
        isInvincible = false;
        invincibleTimer = 0f;

        nextEncouragementTime = 10f;
        encouragementTimer = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);

        UpdateLifeUI();
        UpdateTimeUI();
        UpdateLeaderboardUI();
    }

    private void EnterTitleState()
    {
        Time.timeScale = 1f;
        hasGameStarted = false;
        isGameOver = false;

        if (titlePanel != null) titlePanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (messagePanel != null) messagePanel.SetActive(false);

        if (titleBgm != null && !titleBgm.isPlaying) titleBgm.Play();
    }

    public void DamagePlayer()
{
    if (!IsGameRunning) return;
    if (isInvincible) return;

    lives--;
    PlaySfx(hitSfx, 1f); // 여기: 실제 데미지가 들어간 직후 피격음

    UpdateLifeUI();

    isInvincible = true;
    invincibleTimer = invincibilityDuration;

    if (lives <= 0) HandleGameOver();
}

    public void AddLife(int amount = 1)
    {
        if (!IsGameRunning) return;
        lives += Mathf.Max(1, amount);
        UpdateLifeUI();
    }

    private void HandleGameOver()
    {
        isGameOver = true;
        if (titleBgm != null && titleBgm.isPlaying)
        titleBgm.Stop();

        if (playerDeathFxPrefab != null && player != null)
        Instantiate(playerDeathFxPrefab, player.transform.position, Quaternion.identity);

        if (player != null)
        player.gameObject.SetActive(false);

        PlaySfx(deathSfx, 1f); // 여기: 게임오버 순간 1회 재생
        Time.timeScale = 0f;

        TryAddScore(elapsedTime);

        if (messagePanel != null) messagePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverScoreText != null)
            gameOverScoreText.text = $"생존 시간: {elapsedTime:F1}초\nEnter: 재시작\nESC: 종료";

    }

    private void HandleHeartSpawn()
    {
        if (heartItemPrefab == null) return;

        heartTimer += Time.deltaTime;
        if (heartTimer < heartSpawnInterval) return;
        heartTimer = 0f;

        Camera cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector2 pos = new Vector2(
            Random.Range(-halfWidth * 0.8f, halfWidth * 0.8f),
            Random.Range(-halfHeight * 0.8f, halfHeight * 0.8f)
        );

        GameObject heart = Instantiate(heartItemPrefab, pos, Quaternion.identity);

        if (heart.TryGetComponent(out HeartItem item))
            item.Configure(heartLifetime);
        else
            Destroy(heart, heartLifetime);
    }

    private void UpdateInvincibility()
    {
        if (!isInvincible) return;
        invincibleTimer -= Time.deltaTime;
        if (invincibleTimer <= 0f) isInvincible = false;
    }

    private void HandleEncouragement()
    {
        if (elapsedTime >= nextEncouragementTime)
        {
            ShowEncouragement();
            nextEncouragementTime += 10f;
        }

        if (encouragementTimer > 0f)
        {
            encouragementTimer -= Time.deltaTime;
            if (encouragementTimer <= 0f && messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }
    }

    private void ShowEncouragement()
    {
        if (messagePanel == null || messageText == null || encouragementMessages == null || encouragementMessages.Length == 0)
            return;

        messageText.text = encouragementMessages[Random.Range(0, encouragementMessages.Length)];
        messagePanel.SetActive(true);
        encouragementTimer = encouragementDuration;
    }

    private void UpdateLifeUI()
    {
        if (lifeText != null) lifeText.text = $"Life: {lives}";
    }

    private void UpdateTimeUI()
    {
        if (timeText != null) timeText.text = $"Time: {elapsedTime:F1}s";
    }

    private void TryAddScore(float score)
    {
        leaderboard.Add(score);
        leaderboard.Sort((a, b) => b.CompareTo(a));
        if (leaderboard.Count > MaxLeaderboardEntries)
            leaderboard.RemoveRange(MaxLeaderboardEntries, leaderboard.Count - MaxLeaderboardEntries);

        SaveLeaderboard();
        UpdateLeaderboardUI();
    }

    private void LoadLeaderboard()
    {
        leaderboard.Clear();
        string saved = PlayerPrefs.GetString(LeaderboardKey, string.Empty);
        if (string.IsNullOrEmpty(saved)) return;

        string[] parts = saved.Split('|');
        foreach (string part in parts)
        {
            if (float.TryParse(part, out float value))
                leaderboard.Add(value);
        }

        leaderboard.Sort((a, b) => b.CompareTo(a));
        if (leaderboard.Count > MaxLeaderboardEntries)
            leaderboard.RemoveRange(MaxLeaderboardEntries, leaderboard.Count - MaxLeaderboardEntries);
    }

    private void SaveLeaderboard()
    {
        string data = string.Join("|", leaderboard.Select(v => v.ToString("F3")));
        PlayerPrefs.SetString(LeaderboardKey, data);
        PlayerPrefs.Save();
    }

    private void UpdateLeaderboardUI()
    {
        if (rankingText == null) return;

        var builder = new StringBuilder();
        builder.AppendLine("TOP 5");
        for (int index = 0; index < MaxLeaderboardEntries; index++)
        {
            if (index < leaderboard.Count) builder.AppendLine($"{index + 1}. {leaderboard[index]:F1}s");
            else builder.AppendLine($"{index + 1}. ---");
        }

        rankingText.text = builder.ToString();
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        skipTitleOnNextLoad = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }


    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
