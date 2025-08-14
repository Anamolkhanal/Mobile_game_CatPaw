using UnityEngine;

public class MouseScript : MonoBehaviour
{
    private Vector3 endPos;
    private float speed =2.0f;
    private float smackSpeed = 15.0f;
    private int points =1 ;
    private bool isSmacked;
	private bool destroyedByHit;
    [SerializeField] private bool faceMovementDirection = true;
    [SerializeField, Tooltip("If true, orientation is set once on spawn based on travel direction (no continuous updates)")]
    private bool orientOnceOnSpawn = true;
    [SerializeField] private bool flipOnlyOnX = true;
    [SerializeField, Tooltip("Optional: if your sprite's nose faces up or another direction, set an angle offset (e.g., -90 if nose faces up)")]
    private float directionAngleOffset = 0f;
    [SerializeField, Tooltip("Optional: rotate this child instead of the root transform (drag the sprite object here)")]
    private Transform spriteRootToRotate;
    [SerializeField, Tooltip("If true, for vertical movement use SpriteRenderer.flipY instead of rotating the transform")]
    private bool preferFlipYForVertical = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;
        Vector3 prev = transform.position;
        Vector3 next = Vector3.MoveTowards(prev, endPos, step);
        transform.position = next;

        if (faceMovementDirection && !orientOnceOnSpawn)
        {
            Vector3 delta = next - prev;
            if (delta.sqrMagnitude > 0.000001f)
            {
                UpdateFacing(delta);
            }
        }
		if(Vector3.Distance(transform.position,endPos)< 0.001f)
        {
			Destroy(gameObject);
        }
    }

    public void SetEndPosition(Vector3 pos){
        this.endPos = pos;
        if (faceMovementDirection)
        {
            Vector3 delta = (endPos - transform.position);
            if (delta.sqrMagnitude > 0.000001f)
            {
                UpdateFacing(delta);
                // If we only orient once at spawn, stop further updates
                if (orientOnceOnSpawn)
                {
                    faceMovementDirection = false;
                }
            }
        }
    }

    public void SetSpeed(float speed){
        this.speed = speed;
    }

    public void SetSmackSpeed(float speed){
        this.smackSpeed = speed;
    }
    public void SetPoints(int points){
        this.points = points;
    }
    public int GetPoints(){
        return this.points;
    }
    public bool HasBeenSmacked(){
        return this.isSmacked;
    }

	public void SmackThisMouse(){
		this.isSmacked = true;
		this.destroyedByHit = true;
		if (AudioManager.Instance != null) AudioManager.Instance.PlayHitSfx();
		Destroy(gameObject);
	}

	void OnDestroy(){
		// Avoid spawning new objects while scene is unloading or game is not actively running
		if (!Application.isPlaying) return;
		var gm = GameManager.Instance;
		if (gm == null || !gm.IsRunning || gm.IsGameOver) return;
		gm.HandleMouseDestroyed(this, destroyedByHit);
	}

    private void UpdateFacing(Vector3 delta)
    {
        // For vertical-only movement: keep natural look when moving up; rotate/flip for down
        if (Mathf.Abs(delta.x) < 0.0001f)
        {
            var target = spriteRootToRotate != null ? spriteRootToRotate : transform;
            if (preferFlipYForVertical)
            {
                // Use sprite flip Y to face down/up without changing transform rotation
                var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sr.flipY = delta.y < 0f; // true when moving down
                }
                target.rotation = Quaternion.AngleAxis(directionAngleOffset, Vector3.forward);
            }
            else
            {
                if (delta.y >= 0f)
                {
                    target.rotation = Quaternion.AngleAxis(directionAngleOffset, Vector3.forward);
                }
                else
                {
                    target.rotation = Quaternion.AngleAxis(180f + directionAngleOffset, Vector3.forward);
                }
            }
            return;
        }

        // Fallback: general facing logic
        if (flipOnlyOnX)
        {
            var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = delta.x < 0f;
            }
        }
        else
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg + directionAngleOffset;
            var target = spriteRootToRotate != null ? spriteRootToRotate : transform;
            target.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    }
