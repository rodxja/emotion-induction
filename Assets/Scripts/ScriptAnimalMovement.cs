using UnityEngine;
using System.Collections;

public class ScriptAnimalMovement : MonoBehaviour
{
    private Animator animator;

    private const string animationNone = "None";

    private const string animationBreathing = "Breathing";
    private const string animationWigglingTail = "WigglingTail";
    private const string animationWalking01 = "Walking01";
    private const string animationWalking02 = "Walking02";
    private const string animationRunning = "Running";

    private const string animationEatingStart = "EatingStart";
    private const string animationEatingCycle = "EatingCycle";
    private const string animationEatingEnd = "EatingEnd";

    private const string animationAngryStart = "AngryStart";
    private const string animationAngryCycle = "AngryCycle";
    private const string animationAngryEnd = "AngryEnd";

    private const string animationSittingStart = "SittingStart";
    private const string animationSittingCycle = "SittingCycle";
    private const string animationSittingEnd = "SittingEnd";

    private float moveSpeed = 5f;  // Movement speed while running
    private float moveRadius = 3f; // Radius of the circle

    private Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have a 'Animator' component");
        }

        startPosition = transform.position;
        animator = GetComponent<Animator>();

        // Start the sequence
        StartCoroutine(AnimationSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator AnimationSequence()
    {
        animator.Play(animationSittingCycle);
        yield return new WaitForSeconds(5f);

        animator.Play(animationBreathing);
        yield return new WaitForSeconds(5f);

        // Step 1: Play Animation A for 0.5 seconds
        animator.Play(animationWigglingTail);
        yield return new WaitForSeconds(5f);

        animator.Play(animationBreathing);
        yield return new WaitForSeconds(5f);

        // Step 2: Play Animation B while moving in a circle for 2 seconds
        animator.Play(animationRunning);
        yield return new WaitForSeconds(5f);

        animator.Play(animationBreathing);
        yield return new WaitForSeconds(5f);


        //float timePassed = 0f;
        //while (timePassed < 8f)
        //{
        //    MoveInCircle(timePassed / 3f);  // Normalize time for smooth movement
        //    timePassed += Time.deltaTime;
        //    yield return null;
        //}

        // Step 3: Play Jump animation while running for 5 seconds
        //animator.Play(jumpAnimation);
        //timePassed = 0f;
        //while (timePassed < 5f)
        //{
        //    MoveInCircle(1f); // Keep moving in a circle while jumping
        //    timePassed += Time.deltaTime;
        //    yield return null;
        //}

        // After 5 seconds, the running continues (no more jump animation)
        animator.Play(animationRunning);
    }

    // Moves the object in a circle
    void MoveInCircle(float normalizedTime)
    {
        float angle = normalizedTime * 2 * Mathf.PI; // Full rotation
        float x = Mathf.Cos(angle) * moveRadius;
        float z = Mathf.Sin(angle) * moveRadius;

        transform.position = startPosition + new Vector3(x, 0, z);
    }
}
