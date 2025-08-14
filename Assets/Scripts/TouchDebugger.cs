using UnityEngine;
using UnityEngine.UI;

public class TouchDebugger : MonoBehaviour
{
    [SerializeField] private Text debugText;
    [SerializeField] private bool showDebugInfo = true;
    
    private int touchCount = 0;
    private Vector2 lastTouchPosition = Vector2.zero;
    
    void Start()
    {
        if (debugText == null)
        {
            CreateDebugUI();
        }
    }
    
    void Update()
    {
        if (!showDebugInfo) return;
        
        // Check for touch input using legacy Input system as backup
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            lastTouchPosition = touch.position;
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchCount++;
                    Debug.Log($"Touch began at: {touch.position}");
                    break;
                case TouchPhase.Moved:
                    Debug.Log($"Touch moved to: {touch.position}");
                    break;
                case TouchPhase.Ended:
                    Debug.Log($"Touch ended at: {touch.position}");
                    break;
            }
        }
        
        // Check for mouse input (for testing in editor)
        if (Input.GetMouseButtonDown(0))
        {
            touchCount++;
            lastTouchPosition = Input.mousePosition;
            Debug.Log($"Mouse click at: {Input.mousePosition}");
        }
        
        UpdateDebugText();
    }
    
    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            string info = $"Touch Count: {touchCount}\n";
            info += $"Last Touch: {lastTouchPosition}\n";
            info += $"Input.touchCount: {Input.touchCount}\n";
            info += $"Screen Size: {Screen.width}x{Screen.height}\n";
            info += $"Platform: {Application.platform}";
            
            debugText.text = info;
        }
    }
    
    private void CreateDebugUI()
    {
        // Find or create canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Debug Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create debug text
        GameObject textGO = new GameObject("Debug Text");
        textGO.transform.SetParent(canvas.transform, false);
        
        debugText = textGO.AddComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugText.fontSize = 16;
        debugText.color = Color.yellow;
        debugText.alignment = TextAnchor.UpperLeft;
        
        RectTransform rectTransform = debugText.rectTransform;
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(300, 200);
    }
}
