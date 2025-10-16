using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float minX = -8f, maxX = 8f;

    [Header("Hooking")]
    public Transform hookOrigin;
    public HookController hookPrefab;
    private HookController currentHook;

    void Update()
    {
        // Left/Right
        float dir = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) dir -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) dir += 1f;

        transform.position += Vector3.right * dir * moveSpeed * Time.deltaTime;
        var p = transform.position; p.x = Mathf.Clamp(p.x, minX, maxX); transform.position = p;

        // Cast (Space) – only if no active hook
        if (Input.GetKeyDown(KeyCode.Space) && currentHook == null)
        {
            currentHook = Instantiate(hookPrefab, hookOrigin.position, Quaternion.identity);
            currentHook.Init(hookOrigin, OnHookFinished);
            currentHook.Cast();
        }
    }

    private void OnHookFinished(bool deliveredCatch)
    {
        currentHook = null; // Ready for next cast
    }
}
