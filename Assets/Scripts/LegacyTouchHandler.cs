using UnityEngine;

public class LegacyTouchHandler : MonoBehaviour
{
    public GameObject prefabCatsPaw;
    
    void Update()
    {
        // Handle touch input using legacy Input system
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log($"Legacy touch detected at: {touch.position}");
                HandleTap(touch.position);
            }
        }
        
        // Handle mouse input for testing in editor
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Legacy mouse click detected at: {Input.mousePosition}");
            HandleTap(Input.mousePosition);
        }
    }
    
    private void HandleTap(Vector2 screenPosition)
    {
        Debug.Log($"Legacy HandleTap called with screen position: {screenPosition}");
        var gm = GameManager.Instance;
        bool canInteract = (gm != null && gm.IsRunning);
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (canInteract)
        {
            GameObject catsPaw = GameObject.Find("CatsPaw");
            if(catsPaw){
                Destroy(catsPaw);
            }
            catsPaw= Instantiate(prefabCatsPaw);
            catsPaw.name = "CatsPaw";
            catsPaw.transform.position = new Vector3(ray.origin.x, ray.origin.y, 0f);
        }

        Vector2 worldPos2D = Camera.main.ScreenToWorldPoint(screenPosition);
        var hit = Physics2D.OverlapPoint(worldPos2D);
        if(hit != null){
            var mouse = hit.GetComponentInParent<MouseScript>() ?? hit.GetComponent<MouseScript>();
            if(mouse != null){
                if (canInteract)
                {
                    mouse.SmackThisMouse();
                }
            }
            else
            {
                if (canInteract){
                    gm.RegisterMissTap();
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayMissSfx();
                }
            }
        }
        else
        {
            if (canInteract){
                gm.RegisterMissTap();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMissSfx();
            }
        }
    }
}
