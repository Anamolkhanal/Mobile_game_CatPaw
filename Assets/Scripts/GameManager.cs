using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

    [SerializeField, Tooltip("Assign one or more mouse prefabs here")] private GameObject[] mousePrefabs;
	[SerializeField] private float minSpeed = 2.0f;
	[SerializeField] private float maxSpeed = 4.0f;
	[SerializeField] private int scorePerHit = 10;
    [Header("Timer")]
    [SerializeField] private float startingTimeSeconds = 60f;
    [SerializeField] private float timePenaltyOnMissSeconds = 2f;
    [Header("Optional Score Penalty (used if time penalty is 0)")]
    [SerializeField] private int scorePenaltyOnMiss = 0;
    [SerializeField] private bool allowNegativeScore = false;
    [Header("Optional UI Hooks")] 
    [SerializeField] private Text scoreText;
	[SerializeField] private Text timeText;
	[Header("Game Over UI (optional)")]
	[SerializeField] private GameObject gameOverPanel;
	[SerializeField] private Text finalScoreText;
    [SerializeField] private Button playAgainButton;

    [Header("Game Flow")]
    [SerializeField] private bool autoStart = true;
    private int score;
    private float remainingTime;
    private bool isGameOver;
    private bool isRunning;

	Camera mainCam;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		// Ensure camera is available as early as possible
		if (mainCam == null) mainCam = Camera.main;
	}

	void Start()
	{
		if (mainCam == null) mainCam = Camera.main;
        remainingTime = startingTimeSeconds;
        EnsureUi();
        UpdateUi();
        if (autoStart)
        {
            StartGameLoop();
        }

        // Switch to game music when gameplay starts
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }
	}

    void Update()
    {
        if (!isRunning || isGameOver) return;
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame();
        }
        UpdateUi();
    }

	public void HandleMouseDestroyed(MouseScript mouse, bool wasHit)
	{
        if (!isRunning || isGameOver) return;
        if (wasHit)
		{
			score += scorePerHit;
            UpdateUi();
		}
        SpawnNewMouse();
	}

	void SpawnNewMouse()
	{
        if (!isRunning || isGameOver) return;
        if (mousePrefabs == null || mousePrefabs.Length == 0)
		{
			Debug.LogWarning("Mouse prefabs not assigned on GameManager.");
			return;
		}

        // choose from non-null prefabs only
        var validPrefabs = System.Array.FindAll(mousePrefabs, p => p != null);
        if (validPrefabs.Length == 0)
        {
            Debug.LogWarning("Mouse prefabs array has no valid entries assigned.");
            return;
        }

        // Choose a random path across the screen edges
		Vector3 start;
		Vector3 end;
		GetRandomEdgePath(out start, out end);

        var chosen = validPrefabs[Random.Range(0, validPrefabs.Length)];
		var go = Instantiate(chosen);
		go.name = "Mouse";
		go.transform.position = start;
		var mouse = go.GetComponent<MouseScript>();
		if (mouse != null)
		{
			float speed = Random.Range(minSpeed, maxSpeed);
			mouse.SetSpeed(speed);
			mouse.SetEndPosition(end);
		}
	}

	void GetRandomEdgePath(out Vector3 start, out Vector3 end)
	{
		if (mainCam == null) mainCam = Camera.main;
		if (mainCam == null)
		{
			start = end = Vector3.zero;
			Debug.LogError("GameManager: No Main Camera found. Ensure your scene has a camera tagged MainCamera.");
			return;
		}
		// Get screen world bounds at z=0
		var min = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
		var max = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // only vertical paths: bottom -> top OR top -> bottom
        float x = Random.Range(min.x, max.x);
        bool bottomToTop = Random.value < 0.5f;
        if (bottomToTop)
        {
            start = new Vector3(x, min.y - 1f, 0);
            end = new Vector3(x, max.y + 1f, 0); // keep x constant for strict vertical
        }
        else
        {
            start = new Vector3(x, max.y + 1f, 0);
            end = new Vector3(x, min.y - 1f, 0); // keep x constant for strict vertical
        }

    }
    public void RegisterMissTap()
    {
        if (!isRunning || isGameOver) return;
        if (timePenaltyOnMissSeconds > 0f)
        {
            remainingTime -= timePenaltyOnMissSeconds;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                EndGame();
            }
        }
        else if (scorePenaltyOnMiss != 0)
        {
            score -= scorePenaltyOnMiss;
            if (!allowNegativeScore && score < 0) score = 0;
        }
        UpdateUi();
    }

    public bool IsGameOver => isGameOver;
    public bool IsRunning => isRunning;

    public void StartGameLoop()
    {
        // reset and begin
        score = 0;
        remainingTime = startingTimeSeconds;
        isGameOver = false;
        isRunning = true;
        UpdateUi();
        SpawnNewMouse();
    }

    private void EndGame()
    {
        isGameOver = true;
        UpdateUi();
        Debug.Log("Game Over");
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null)
        {
            finalScoreText.alignment = TextAnchor.MiddleCenter;
            finalScoreText.horizontalOverflow = HorizontalWrapMode.Wrap;
            finalScoreText.verticalOverflow = VerticalWrapMode.Overflow;
            finalScoreText.lineSpacing = 1.0f;
            finalScoreText.text = $"Game Over\nScore: {score}";
        }
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(RestartGame);
        }
    }

    private void UpdateUi()
    {
        if (scoreText != null) scoreText.text = $"Score {score}";
        if (timeText != null) timeText.text = $"Time {Mathf.CeilToInt(remainingTime)}";
    }

    private void EnsureUi()
	{
		if (scoreText != null && timeText != null) return;

		Canvas canvas = Object.FindFirstObjectByType<Canvas>();
		if (canvas == null)
		{
			var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
			canvas = canvasGo.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			var scaler = canvasGo.GetComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920, 1080);
		}

		Font font = null;
		try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}

		if (scoreText == null)
		{
			var go = new GameObject("Score Text", typeof(Text));
			go.transform.SetParent(canvas.transform, false);
			scoreText = go.GetComponent<Text>();
			if (font != null) scoreText.font = font;
			scoreText.fontSize = 48; // Increased from 20 to 48 for mobile
			scoreText.fontStyle = FontStyle.Bold;
			scoreText.color = Color.white;
			scoreText.alignment = TextAnchor.UpperLeft;
			scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
			scoreText.verticalOverflow = VerticalWrapMode.Overflow;
			var rt = scoreText.rectTransform;
			rt.anchorMin = new Vector2(0, 1);
			rt.anchorMax = new Vector2(0, 1);
			rt.pivot = new Vector2(0, 1);
			rt.anchoredPosition = new Vector2(80, -80);
			rt.sizeDelta = new Vector2(300, 100); // Give it proper size
		}

		if (timeText == null)
		{
			var go = new GameObject("Time Text", typeof(Text));
			go.transform.SetParent(canvas.transform, false);
			timeText = go.GetComponent<Text>();
			if (font != null) timeText.font = font;
			timeText.fontSize = 48; // Increased from 20 to 48 for mobile
			timeText.fontStyle = FontStyle.Bold;
			timeText.color = Color.white;
			timeText.alignment = TextAnchor.UpperRight;
			timeText.horizontalOverflow = HorizontalWrapMode.Overflow;
			timeText.verticalOverflow = VerticalWrapMode.Overflow;
			var rt = timeText.rectTransform;
			rt.anchorMin = new Vector2(1, 1);
			rt.anchorMax = new Vector2(1, 1);
			rt.pivot = new Vector2(1, 1);
			rt.anchoredPosition = new Vector2(-80, -80);
			rt.sizeDelta = new Vector2(300, 100); // Give it proper size
    }

		// Create a simple Game Over overlay if not provided
		if (gameOverPanel == null)
		{
			var panelGo = new GameObject("GameOver Panel", typeof(Image));
			panelGo.transform.SetParent(canvas.transform, false);
			var img = panelGo.GetComponent<Image>();
			img.color = new Color(0, 0, 0, 0.6f);
			var prt = img.rectTransform;
			prt.anchorMin = new Vector2(0, 0);
			prt.anchorMax = new Vector2(1, 1);
			prt.offsetMin = Vector2.zero;
			prt.offsetMax = Vector2.zero;
			panelGo.SetActive(false);
			gameOverPanel = panelGo;

			var textGo = new GameObject("Final Score Text", typeof(Text));
			textGo.transform.SetParent(panelGo.transform, false);
			finalScoreText = textGo.GetComponent<Text>();
			if (font != null) finalScoreText.font = font;
			finalScoreText.fontSize = 72; // Increased from 20 to 72 for mobile
			finalScoreText.fontStyle = FontStyle.Bold;
			finalScoreText.color = Color.white;
            finalScoreText.alignment = TextAnchor.MiddleCenter;
            finalScoreText.horizontalOverflow = HorizontalWrapMode.Wrap;
            finalScoreText.verticalOverflow = VerticalWrapMode.Overflow;
            finalScoreText.lineSpacing = 1.2f; // Increased line spacing for better readability
			var trt = finalScoreText.rectTransform;
			trt.anchorMin = new Vector2(0.5f, 0.5f);
			trt.anchorMax = new Vector2(0.5f, 0.5f);
			trt.pivot = new Vector2(0.5f, 0.5f);
			trt.anchoredPosition = new Vector2(0, 50); // Move up a bit
			trt.sizeDelta = new Vector2(800, 200); // Give it proper size
			finalScoreText.text = "";

			// Create Play Again button
			var btnGo = new GameObject("Play Again Button", typeof(Image), typeof(Button));
			btnGo.transform.SetParent(panelGo.transform, false);
			var btnImg = btnGo.GetComponent<Image>();
			btnImg.color = new Color(1, 1, 1, 0.15f);
			playAgainButton = btnGo.GetComponent<Button>();
			var brt = btnGo.GetComponent<RectTransform>();
			brt.anchorMin = new Vector2(0.5f, 0.5f);
			brt.anchorMax = new Vector2(0.5f, 0.5f);
			brt.pivot = new Vector2(0.5f, 0.5f);
			brt.anchoredPosition = new Vector2(0, -200); // Moved down for larger text
			brt.sizeDelta = new Vector2(400, 100); // Increased from 220x60 to 400x100
			playAgainButton.onClick.RemoveAllListeners();
			playAgainButton.onClick.AddListener(RestartGame);

			var btnTextGo = new GameObject("Text", typeof(Text));
			btnTextGo.transform.SetParent(btnGo.transform, false);
			var btnText = btnTextGo.GetComponent<Text>();
			if (font != null) btnText.font = font;
			btnText.text = "Play Again";
			btnText.fontSize = 48; // Increased from 20 to 48 for mobile
			btnText.fontStyle = FontStyle.Bold;
			btnText.alignment = TextAnchor.MiddleCenter;
			btnText.color = Color.white;
			btnText.horizontalOverflow = HorizontalWrapMode.Overflow;
			btnText.verticalOverflow = VerticalWrapMode.Overflow;
			var btrt = btnText.rectTransform;
			btrt.anchorMin = new Vector2(0, 0);
			btrt.anchorMax = new Vector2(1, 1);
			btrt.offsetMin = Vector2.zero;
			btrt.offsetMax = Vector2.zero;
		}
	}

	private void RestartGame()
	{
		Scene current = SceneManager.GetActiveScene();
		SceneManager.LoadScene(current.name);
	}
    }
