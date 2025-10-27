using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Horizontal move speed in world units/sec")]
    public float moveSpeed = 6f;
    [Tooltip("Clamp player X within these bounds")]
    public float minX = -8f, maxX = 8f;

    [Header("Hook Setup")]
    [Tooltip("Where the hook spawns from (tip of the rod)")]
    public Transform hookOrigin;
    [Tooltip("Hook prefab with HookController")]
    public HookController hookPrefab;

    [Header("Animation")]
    [Tooltip("Animator driver for player (Idle/Cast/Reel)")]
    public PlayerAnimDriver playerAnim;

    [Header("SFX (optional)")]
    public AudioSource sfx;
    public AudioClip castSfx;

    // runtime
    private HookController currentHook;

    void Update()
    {
        HandleMovement();
        HandleCastingAndReelAnim();
    }

    private void HandleMovement()
    {
        float dir = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) dir -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) dir += 1f;

        if (Mathf.Abs(dir) > 0f)
        {
            transform.position += Vector3.right * dir * moveSpeed * Time.deltaTime;

            // Clamp X bounds
            var p = transform.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            transform.position = p;
        }
    }

    private void HandleCastingAndReelAnim()
    {
        // CAST: Space, only if we don't already have a hook
        if (Input.GetKeyDown(KeyCode.Space) && currentHook == null)
        {
            // Play cast anim & sound
            if (playerAnim) playerAnim.PlayCast();
            if (sfx && castSfx) sfx.PlayOneShot(castSfx);

            // Spawn and init hook
            currentHook = Instantiate(hookPrefab, hookOrigin.position, Quaternion.identity);
            currentHook.Init(hookOrigin, OnHookFinished);
            currentHook.Cast();

            // (Optional) let a GameManager know which hook is active
            if (GameManager.Instance) GameManager.Instance.RegisterHook(hookOrigin, currentHook);
        }

        // REEL ANIM: While UpArrow is held, show reeling; otherwise stop
        if (currentHook != null)
        {
            if (Input.GetKey(KeyCode.UpArrow))
                playerAnim?.StartReel();
            else
                playerAnim?.StopReel();
        }
        else
        {
            // ensure we return to Idle when no hook active
            playerAnim?.StopReel();
        }
    }

    /// <summary>
    /// Callback from HookController when the hook returns to the origin.
    /// deliveredCatch = true if a fish was delivered.
    /// </summary>
    private void OnHookFinished(bool deliveredCatch)
    {
        currentHook = null;
        playerAnim?.StopReel();

        // (Optional) any post-catch feedback could go here
        // e.g., small bump, UI flash, etc.
    }
}
