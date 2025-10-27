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
    [SerializeField] private string lineSortingLayer = "Hook";
    [SerializeField] private int lineSortingOrder = 100;

    [Header("Catch")]
    public Transform catchAnchor;
    private Fish caughtFish;

    private Transform origin;
    private HookState state = HookState.Idle;
    private float targetDepth = 0f;
    private System.Action<bool> onFinished;
    private Rigidbody2D rb;

    public float CurrentDepth => origin ? Mathf.Max(0f, origin.position.y - transform.position.y) : 0f;

    public void Init(Transform origin, System.Action<bool> onFinished)
    {
        this.origin = origin;
        this.onFinished = onFinished;

        rb = GetComponent<Rigidbody2D>();
        if (!line) line = GetComponent<LineRenderer>();
        SetupLineRenderer();

        GameManager.Instance.RegisterHook(origin, this);
        targetDepth = 0f;
    }

    private void SetupLineRenderer()
    {
        if (!line) return;

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.startColor = Color.red;
        line.endColor = Color.red;
        line.sortingLayerName = lineSortingLayer;
        line.sortingOrder = lineSortingOrder;

        // Ensure visible color (white, opaque)
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        line.colorGradient = grad;
    }

    public void Cast()
    {
        state = HookState.InWater;
        targetDepth = 0f;
    }

    void Update()
    {
        if (origin == null) return;

        // --- Input ---
        if (state == HookState.InWater)
        {
            if (Input.GetKey(KeyCode.DownArrow))
                targetDepth = Mathf.Clamp(targetDepth + 6f * Time.deltaTime, 0f, maxDepth);

            if (Input.GetKey(KeyCode.UpArrow))
                state = HookState.Reeling;
        }

        // --- Motion ---
        Vector3 desiredPos = origin.position + Vector3.down * targetDepth;

        if (state == HookState.InWater)
            transform.position = Vector3.MoveTowards(transform.position, desiredPos, dropSpeed * Time.deltaTime);
        else if (state == HookState.Reeling)
            transform.position = Vector3.MoveTowards(transform.position, origin.position, reelSpeed * Time.deltaTime);

        // --- Done reeling ---
        if (state == HookState.Reeling && Vector3.Distance(transform.position, origin.position) <= attachDistanceDone)
        {
            DeliverCatch();
            onFinished?.Invoke(caughtFish != null);
            Destroy(gameObject);
            return;
        }

        // --- Draw the line every frame ---
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

    public void StartReel()
    {
        // Begin reeling phase — set state or flag if needed
        state = HookState.Reeling; // only if you use a HookState enum
    }

    public void StopReel()
    {
        // Stop or pause reeling
        if (state == HookState.Reeling)
            state = HookState.InWater; // or Idle, depending on your setup
    }

}
