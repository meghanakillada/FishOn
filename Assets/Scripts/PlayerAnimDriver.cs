using UnityEngine;

public class PlayerAnimDriver : MonoBehaviour
{
    public Animator anim;

    public void PlayCast()
    {
        anim.ResetTrigger("Cast");
        anim.SetTrigger("Cast");
        anim.SetBool("IsReeling", false);
    }

    public void StartReel()
    {
        anim.SetBool("IsReeling", true);
    }

    public void StopReel()
    {
        anim.SetBool("IsReeling", false);
    }
}
