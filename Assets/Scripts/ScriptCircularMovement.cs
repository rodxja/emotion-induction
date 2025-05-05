using UnityEngine;

public class ScriptCircularMovement : MonoBehaviour
{
    public float radius = 2f;           // Radius of the circle
    public float speed = 0.375f;            // Speed of the rotation
    public Vector3 center = Vector3.zero; // Center of the circle
    public float curveStartPercentage = 0.875f; // Represents 7/8
    public Transform targetPoint; // Assign an empty GameObject at the center
    private Vector3 targetPositionWithoutY; // Assign an empty GameObject at the center

    private Animator animator;

    private float angle = 0f;
    private Vector3 lastPosition;

    private string animationWalking02 = "corgi_Walking02";

    private float timeElapsed = 0f;
    private Vector3 initialPosition;

    private bool stop = false;

    private const float C = 2 * Mathf.PI;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have an 'Animator' component");
        }

        // Set the initial position
        angle = 0f;
        lastPosition = transform.position;

        animator.Play(animationWalking02);
        animator.SetBool("isBreathing", false);
        animator.SetBool("isWalking02", true);


        targetPositionWithoutY = new Vector3(targetPoint.position.x, 0, targetPoint.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime * speed;
        float cyclePosition = timeElapsed % (C); // Loop the circle

        Debug.Log($"cyclePosition {cyclePosition} - {C}");

        Vector3 newPosition;

        if (cyclePosition / (C) < curveStartPercentage && !stop)
        {
            // Circular movement
            float x = targetPoint.position.x + radius * Mathf.Cos(cyclePosition);
            float z = targetPoint.position.z + radius * Mathf.Sin(cyclePosition);

            newPosition = new Vector3(x, transform.position.y, z);
            // this makes the asset to turn to the correct direction
            transform.position = newPosition;
            turn(newPosition);

        }
        else if (stop)
        {
            // dont move
        }
        else if (cyclePosition / (C) >= curveStartPercentage)
        {
            // Curve to the center
            float remainingPercentage = (cyclePosition / (2 * Mathf.PI) - curveStartPercentage) / (1f - curveStartPercentage);
            newPosition = Vector3.Lerp(transform.position.normalized * radius, targetPositionWithoutY, remainingPercentage);
            // this makes the asset to turn to the correct direction
            transform.position = newPosition;
            turn(newPosition);
            //stop = true;
        }
        circularMove();


    }

    void circularMove()
    {
        angle += speed * Time.deltaTime; // Increment the angle



        Debug.Log($"angle {angle} - {C}");

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        Vector3 newPosition = center + new Vector3(x, 0, z);
        transform.position = newPosition;



        // Calculate movement direction
        Vector3 direction = newPosition - lastPosition;

        turn(newPosition);
    }

    void turn(Vector3 newPosition)
    {

        // Calculate movement direction
        Vector3 direction = newPosition - lastPosition;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        lastPosition = newPosition;
    }
}
