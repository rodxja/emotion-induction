using UnityEngine;
using UnityEngine.SceneManagement;


// TODO:
// [X] make a transition between idle and walk indefitively
// [X] when walk make the spider to move
// [X] make the spider to face the front direction when its walking
// [X] make the spider rotate over itself
// [X] make the spider to walk towards the camera
// [ ] adapt animation time

// This script controls the spider movements
// When the spider is not facing the target it will rotate to do it
// Once it is facing the target it will walk until 0.5 meters from it, and the idle animation will activate
// NOTE:
// * It does not hanfle collition, so it will go through trees and other obstacules

public class ScriptSpiderMovement_original : MonoBehaviour
{

    public Transform target;

    public float walkSpeed = 2f;  // Speed at which the spider moves
    public Vector3 direction = Vector3.forward;  // Direction of movement (forward by default
    public float stopDistance = 0f;  // Stop walking when within 0.5 units of the target
    public float animationSpeed = 1f;
    public float rotationSpeed = 5f;
    public float walkAngleThreshold = 15f; // Empieza a caminar si el �ngulo es menor a esto


    private Animator animator;

    private const string animationIdle = "Idle";
    private const string animationAttack1 = "Attack1";
    private const string animationAttack2 = "Attack2";
    private const string animationDeath = "Death";
    private const string animationTakeDamage_002 = "TakeDamage_002";
    private const string animationWalk = "Walk";

    private const string canIdle = "canIdle";
    private const string canWalk = "canWalk";

    private Camera mainCam;


    public float wallCheckDistance = 0.1f; // TODO: should be a percentage of the lenght of the asset

    private Rigidbody rb;
    private bool isClimbing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // validate needed variables
        // TODO : fix this when is loaded in LAB scene
        if (target == null)
        {

            Scene myScene = gameObject.scene;

            mainCam = FindMainCameraInScene(myScene);

            if (mainCam != null)
            {
                target = mainCam.transform;
            }
            else
            {
                Debug.LogWarning("No Main Camera found!");
            }
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have an 'Animator' component");
        }

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have an 'Rigidbody' component");
        }

        rb.useGravity = true;

        animator.speed = animationSpeed;
        walkSpeed = walkSpeed * animationSpeed;
        rotationSpeed = rotationSpeed * animationSpeed * 8;

    }

    private Camera FindMainCameraInScene(Scene scene)
    {
        if (!scene.isLoaded)
            return null;

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            Camera cam = obj.GetComponentInChildren<Camera>(true);
            if (cam != null && cam.CompareTag("MainCamera"))
            {
                return cam;
            }
        }

        return null;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the distance to the target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < stopDistance) // stops and just stays in IDLE animation
        {
            // TODO: pending to adapt animation time
            // Transition from Idle to Walk
            animator.SetBool(canIdle, true);  // Stop idle
            animator.SetBool(canWalk, false);   // Start walking
            return;
        }


        // ************************************
        // TO ROTATE OVER ITSELF
        // ************************************

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; // ignorar altura

        // Direcci�n actual (forward)
        Vector3 forward = transform.forward;
        forward.y = 0;

        // get angle that the current game object is toward in relation to the target
        float angle = Vector3.Angle(forward, directionToTarget);

        // if angle between forward and target is greater than the threshold, then the gameobject must rotate until get a valid angle
        //TODO : if the targets moves when it is in a wall there might be an issue
        if (angle > walkAngleThreshold)
        { // it needs to rotate over itself, so it stops moving forward

            animator.Play(animationWalk);
            // Gira hacia el target (aunque est� caminando o no)
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            return;
        }

        // ************************************
        // TO CLIMB
        // ************************************
        if (!isClimbing)
        {
            bool raycast = Physics.Raycast(transform.position, forward, out RaycastHit hit, wallCheckDistance);
            Debug.Log($"raycast - {raycast}");
            // detect wall in front of gameObject
            if (raycast && hit.collider != null && !hit.collider.CompareTag("Spider"))
            {
                //hit.normal
                Debug.Log($"climb");
                if (!isClimbing)
                {
                    isClimbing = true;
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                }
                return;
            }

        }
        else
        {

            // Subir verticalmente más lento
            float delay = 1f;
            rb.MovePosition(rb.position + Time.deltaTime * walkSpeed * delay * Vector3.up);
            //transform.Translate(Vector3.up * walkSpeed * Time.deltaTime);

            bool inWallRaycast = Physics.Raycast(transform.position, transform.forward, out RaycastHit topHit, 2f);

            Debug.Log($"inWallRaycast mientras false seguir escalando - {inWallRaycast}");

            // Detectar si ya llegó a la parte de arriba con raycast hacia adelante y abajo
            if (inWallRaycast)
            {
                // Si hay suelo arriba, "salir" del modo escalada
                isClimbing = false;
                rb.useGravity = true;

                // Ajustar posición sobre el suelo
                Vector3 finalPosition = new Vector3(rb.position.x, topHit.point.y + 0.5f, rb.position.z);
                rb.MovePosition(finalPosition);
            }
            return;
        }

        // ************************************
        // TO WALK
        // ************************************

        Walk();


    }

    // TODO : to not walk in straight line
    void Walk()
    {


        animator.Play(animationWalk);

        // Transition from Idle to Walk
        animator.SetBool(canIdle, false);  // Stop idle
        animator.SetBool(canWalk, true);   // Start walking

        // Move the spider in the specified direction with the given speed

        transform.Translate(Time.deltaTime * walkSpeed * direction);


        Turn();
    }

    // rotate
    // it needs a final direction to face, a vector
    void Turn()
    {
        // Get the direction towards the target in the X-Z plane
        Vector3 direction = new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z);

        // If the direction is not zero, rotate towards the target
        if (direction != Vector3.zero)
        {
            // Create a rotation that looks in the direction of the target on the X-Z plane
            Quaternion rotation = Quaternion.LookRotation(direction);

            // Apply the new rotation to the object, while preserving its current Y rotation
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }


    void Rotate()
    {


    }


}
