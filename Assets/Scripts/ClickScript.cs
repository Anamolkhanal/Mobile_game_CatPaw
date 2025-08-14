using UnityEngine;
using UnityEngine.InputSystem;

public class ClickScript : MonoBehaviour
{
    public GameObject prefabCatsPaw;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		// New Input System: mouse
		var mouse = Mouse.current;
		if (mouse != null && mouse.leftButton.wasPressedThisFrame)
		{
			HandleTap(mouse.position.ReadValue());
		}

		// New Input System: touch
		var touchscreen = Touchscreen.current;
		if (touchscreen != null)
		{
			foreach (var touch in touchscreen.touches)
			{
				if (touch.press.wasPressedThisFrame)
				{
					HandleTap(touch.position.ReadValue());
				}
			}
		}
	}

	private void HandleTap(Vector2 screenPosition)
	{
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
