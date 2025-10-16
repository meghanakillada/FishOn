using UnityEngine;

public class Fish : MonoBehaviour
{
    public int points = 10;
    public float speed = 2f;
    public Vector2 verticalSway = new Vector2(0.5f, 2f); // amplitude, frequency
    public bool avoidsHook = false;

    [Header("Bounds")]
    public float minX = -10f, maxX = 10f, minY = -4f, maxY = 0f;

    public bool IsHooked { get; private set; }

    Rigidbody2D rb;
    float dir = 1f;
    float swayT;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dir = Random.value < 0.5f ? -1f : 1f;
        swayT = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (IsHooked) return;

        // Wander horizontally
        Vector3 pos = transform.position;
        pos.x += dir * speed * Time.deltaTime;

        // Vertical sway
        swayT += verticalSway.y * Time.deltaTime;
        pos.y += Mathf.Sin(swayT) * verticalSway.x * Time.deltaTime;

        transform.position = pos;

        // Flip & bounce at bounds
        if (transform.position.x < minX) { dir = 1f; Flip(); }
        if (transform.position.x > maxX) { dir = -1f; Flip(); }

        // Optional: avoid hook
        if (avoidsHook && GameManager.Instance.CurrentHookPos(out Vector3 hookPos))
        {
            float d = Vector3.Distance(hookPos, transform.position);
            if (d < 2f) dir = Mathf.Sign(transform.position.x - hookPos.x); // dart away
        }
    }

    void Flip()
    {
        var s = transform.localScale; s.x = Mathf.Abs(s.x) * (dir > 0 ? 1 : -1); transform.localScale = s;
    }

    public void OnHooked(HookController hook)
    {
        IsHooked = true;
        var col = GetComponent<Collider2D>(); if (col) col.enabled = false;
        if (rb) { rb.isKinematic = true; rb.velocity = Vector2.zero; }
    }
}
