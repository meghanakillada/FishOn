using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator on your PlayerSprite (the object that renders the player).")]
    public Animator animator;

    [Tooltip("Where the hook spawns from (rod tip / hand).")]
    public Transform hookOrigin;

    [Tooltip("Hook prefab with your HookController.")]
    public HookController hookPrefab;

    private HookController currentHook;

    [Header("Movement (optional)")]
    public float moveSpeed = 6f;
    public float minX = -8f, maxX = 8f;

    void Update()
    {
        HandleMovement();

        // CAST: press Space once if no active hook
        if (Input.GetKeyDown(KeyCode.Space) && currentHook == null)
        {
            StartCast();
        }

        // REEL while UpArrow is held (only if a hook exists)
        if (currentHook != null)
        {
            bool reelHeld = Input.GetKey(KeyCode.UpArrow);

            if (reelHeld)
            {
                // Transition Cast -> Reel
                animator.SetBool("IsCasting", false);  // leave Cast
                animator.SetBool("IsReeling", true);   // enter Reel
                currentHook.StartReel();               // tell hook to reel (method on your HookController)
            }
            else
            {
                // Stop Reel (will go Reel -> Idle)
                animator.SetBool("IsReeling", false);
                currentHook.StopReel();                // optional, if your hook supports pausing reel
            }
        }
        else
        {
            // No hook: ensure we’re not stuck in Reel/Cast
            animator.SetBool("IsReeling", false);
            animator.SetBool("IsCasting", false);
        }
    }

    private void StartCast()
    {
        // Flip animator to Cast and spawn hook
        animator.ResetTrigger("Cast"); // harmless if you previously tried triggers
        animator.SetBool("IsReeling", false);
        animator.SetBool("IsCasting", true);  // Idle -> Cast (and stays there)

        currentHook = Instantiate(hookPrefab, hookOrigin.position, Quaternion.identity);
        currentHook.Init(hookOrigin, OnHookFinished); // assumes your HookController supports this
        currentHook.Cast();                            // hook begins dropping / moving into water
    }

    private void OnHookFinished(bool deliveredCatch)
    {
        // Hook returned or despawned: go back to Idle
        animator.SetBool("IsReeling", false);
        animator.SetBool("IsCasting", false);
        currentHook = null;
    }

    private void HandleMovement()
    {
        float dir = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) dir -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) dir += 1f;

        if (Mathf.Abs(dir) > 0.001f)
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x + dir * moveSpeed * Time.deltaTime, minX, maxX);
            transform.position = p;
        }
    }
}
