using UnityEngine;

public class ScriptCorgiMovement : MonoBehaviour
{

    public bool isBreathing = true;
    public bool isWalking01 = false;
    public bool isWalking02 = false;
    public bool isWiggling = false;
    public bool isSitting = false;

    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have an 'Animator' component");
        }

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isBreathing", isBreathing);
        animator.SetBool("isWalking01", isWalking01);
        animator.SetBool("isWalking02", isWalking02);
        animator.SetBool("isWiggling", isWiggling);
        animator.SetBool("isSitting", isSitting);
    }
}
