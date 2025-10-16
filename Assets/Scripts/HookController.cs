using UnityEngine;

public enum HookState { Idle, InWater, Reeling }

public class HookController : MonoBehaviour
{
    [Header("Motion")]
    public float dropSpeed = 6f;
    public float reelSpeed = 10f;
    public float maxDepth = 10f;
    public float attachDistanceDone = 0.2f;

    [Header("Line")]
    public LineRenderer line;
    private Transform origin;

    [Header("Catch")]
    public Transform catchAnchor; // empty child on hook tip (optional)
    private Fish caughtFish;

    private HookState state = HookState.Idle;
    private float targetDepth = 0f;

    System.Action<bool> onFinished;

    Rigidbody2D rb;

    public void Init(Transform origin, System.Action<bool> onFinished)
    {
        this.origin = origin;
        this.onFinished = onFinished;
        rb = GetComponent<Rigidbody2D>();
        if (!line) line = GetComponent<LineRenderer>();
        targetDepth = 0f;
    }

    public void Cast()
    {
        state = HookState.InWater;
        targetDepth = 0f; // start at surface
    }

    void Update()
    {
        if (origin == null) return;

        // Input: control depth (Down = deeper), Up = reel
        if (state == HookState.InWater)
        {
            if (Input.GetKey(KeyCode.DownArrow))
                targetDepth = Mathf.Clamp(targetDepth + 6f * Time.deltaTime, 0f, maxDepth);

            if (Input.GetKey(KeyCode.UpArrow))
                state = HookState.Reeling;
        }

        // Motion
        Vector3 desiredPos = origin.position + Vector3.down * targetDepth;

        if (state == HookState.InWater)
            transform.position = Vector3.MoveTowards(transform.position, desiredPos, dropSpeed * Time.deltaTime);
        else if (state == HookState.Reeling)
            transform.position = Vector3.MoveTowards(transform.position, origin.position, reelSpeed * Time.deltaTime);

        // Arrived at origin?
        if (state == HookState.Reeling && Vector3.Distance(transform.position, origin.position) <= attachDistanceDone)
        {
            DeliverCatch();
            onFinished?.Invoke(caughtFish != null);
            Destroy(gameObject);
            return;
        }

        // Update line
        if (line)
        {
            line.positionCount = 2;
            line.SetPosition(0, origin.position);
            line.SetPosition(1, transform.position);
        }
    }

    private void DeliverCatch()
    {
        if (caughtFish)
        {
            GameManager.Instance.AddScore(caughtFish.points);
            GameManager.Instance.IncrementFish();
            Destroy(caughtFish.gameObject);
            caughtFish = null;
            GameManager.Instance.PlayCatchSfx();
        }
        else
        {
            GameManager.Instance.PlayReelSfx();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (caughtFish != null) return;
        Fish f = other.GetComponent<Fish>();
        if (f != null && !f.IsHooked)
        {
            caughtFish = f;
            f.OnHooked(this);
            if (catchAnchor) f.transform.SetParent(catchAnchor, true);
            else f.transform.SetParent(transform, true);
            f.transform.localPosition = Vector3.zero;
            GameManager.Instance.PlaySplashOrPop(true); // pop/chime on catch
        }
    }
}
