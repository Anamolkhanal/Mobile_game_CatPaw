using UnityEngine;

public class StartGameOnLoad : MonoBehaviour
{
	void Start()
	{
		var gm = GameManager.Instance;
		if (gm != null && !gm.IsRunning)
		{
			gm.StartGameLoop();
		}
	}
}


