using UnityEngine;
using UnityEngine.InputSystem;

public class ClickScript : MonoBehaviour
{
    public GameObject prefabCatsPaw;
    
    private InputAction attackAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Create and configure the attack action
        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Touchscreen>/primaryTouch/tap");
        attackAction.Enable();
        
        // Subscribe to the performed event
        attackAction.performed += OnAttackPerformed;
    }
    
    void OnDestroy()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.Disable();
            attackAction.Dispose();
        }
    }
    
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPosition;
        
        // Get the position based on the control that triggered the action
        if (context.control.device is Mouse mouse)
        {
            screenPosition = mouse.position.ReadValue();
            Debug.Log($"Mouse attack performed at: {screenPosition}");
        }
        else if (context.control.device is Touchscreen touchscreen)
        {
            screenPosition = touchscreen.primaryTouch.position.ReadValue();
            Debug.Log($"Touch attack performed at: {screenPosition}");
        }
        else
        {
            // Fallback to mouse position if we can't determine the device
            screenPosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Debug.Log($"Unknown device attack performed at: {screenPosition}");
        }
        
        HandleTap(screenPosition);
    }

    // Update is called once per frame
    void Update()
    {
        // Input handling is now done through the InputAction callback
    }

	private void HandleTap(Vector2 screenPosition)
	{
		Debug.Log($"HandleTap called with screen position: {screenPosition}");
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
