using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum MovementStyle
{
    ToTarget, // ToTarget only moves to a specific target
    Wander, // Wander allows to move and wander in a specift area, now it is radius around a point
    Forward // Forward only allows to move forward, it ignores target
}


// This script handles the movement of the spider
// It moves towards a target
// It rotates if it is not facing the target between the "walkAngleThreshold"
// It climbs and descend walls 
public class ScriptSpiderMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // target is the object where the spider is moving towards, DO NOT USE FOR CALCULUS
    /*private*/
    public Vector3 innerTarget; // innerTarget it just holds "target.position"
    public float stopDistance = 0.5f;  // stopDistance indicates the distance in which the spider must stop

    // TODO : use a single speed, merge walk and rotation speed
    [Header("Speed")]
    /*private*/     public float walkSpeed = 0f; // walkSpeed
    /*private*/    public float rotationSpeed = 0f; // rotationSpeed
    public float animationSpeed = 1f;
    public float walkSpeedMultiplier = 1f; // walkSpeedMultiplier multiplies the speed
    public float rotationSpeedMultiplier = 5f; // rotationSpeedMultiplier multiplies the speed

    [Header("Movement Style")]
    public MovementStyle movementStyle = MovementStyle.ToTarget;
    public float walkAngleThreshold = 5f; // Empieza a caminar si el ngulo es menor a esto

    public float radius = 5.0f; // radius of the circle to wander around
    private Vector3 center; // center is used when MovementStyle.Wander in order to keep the center of the circle

    private Animator animator;

    private const string animationIdle = "Idle";
    private const string animationAttack1 = "Attack1";
    private const string animationAttack2 = "Attack2";
    private const string animationDeath = "Death";
    private const string animationTakeDamage_002 = "TakeDamage_002";
    private const string animationWalk = "Walk";

    private const string canIdle = "canIdle";
    private const string canWalk = "canWalk";

    // for climbing
    private float wallCheckDistance = 1.1f; // TODO: should be a percentage of the lenght of the asset
    private float minimumCheckDistance = 0.1f; // minimumDistance
    private bool alwaysNormal = true; // alwaysNormal

    //private LineRenderer lineRenderer_forward;
    //private LineRenderer lineRenderer_down;
    //private LineRenderer lineRenderer_ground;
    //private LineRenderer lineRenderer_normal;

    //void Awake()
    //{
    //    lineRenderer_forward = CreateLineRenderer(UnityEngine.Color.green);
    //    lineRenderer_down = CreateLineRenderer(UnityEngine.Color.blue);
    //    lineRenderer_ground = CreateLineRenderer(UnityEngine.Color.red);
    //    lineRenderer_normal = CreateLineRenderer(UnityEngine.Color.yellow);
    //}

    LineRenderer CreateLineRenderer(UnityEngine.Color color)
    {
        var lr = new GameObject("LineRenderer_" + color).AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startColor = color;
        lr.endColor = color;
        return lr;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null && movementStyle != MovementStyle.Forward)
        {

            Scene myScene = gameObject.scene;

            Camera mainCam = FindMainCameraInScene(myScene);

            if (mainCam != null)
            {
                target = mainCam.transform;
                innerTarget = target.position;
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
            return;
        }

        animator.Play(animationWalk);
        // Transition from Idle to Walk
        animator.SetBool(canIdle, false);  // Stop idle
        animator.SetBool(canWalk, true);   // Start walking 

        // Obtener escala global promedio del objeto
        float scale = transform.lossyScale.magnitude / Mathf.Sqrt(3);

        // Calcular velocidad ajustada inversamente a la escala
        //walkSpeed = Mathf.Clamp(4.5f / (scale + 0.05f), 0.1f, 5f) * walkSpeedMultiplier;
        //rotationSpeed = Mathf.Clamp(4.5f / (scale + 0.05f), 0.1f, 5f) * rotationSpeedMultiplier;

        //Debug.Log($"{gameObject.name}\t-\tscale: {scale}\t-\twalkSpeed: {walkSpeed}\t-\twalkSpeedMultiplier: {walkSpeedMultiplier}\t-\trotationSpeed: {rotationSpeed}\t-\trotationSpeedMultiplier: {rotationSpeedMultiplier}\t-\t{Mathf.Clamp(4.5f / (scale + 0.05f), 0.1f, 5f)}");

   


        if (movementStyle == MovementStyle.Wander)
        {
            // the first target is the center of the circle
            // then innerTarget will change but the value of center will keep without changing
            center = new Vector3(innerTarget.x, 0, innerTarget.z);
            // set new target around circle
            NewTarget();
        }


        animator.speed = animationSpeed;
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

    // Stop returns true if the caller funcion must stop
    bool Stop()
    {
        // Calculate the distance to the target
        float distanceToTarget = Vector3.Distance(transform.position, innerTarget);

        if (distanceToTarget < stopDistance) // stops and just stays in IDLE animation
        {
            // TODO: pending to adapt animation time
            // Transition from Idle to Walk
            animator.SetBool(canIdle, true);  // Stop idle
            animator.SetBool(canWalk, false);   // Start walking

            return true;
        }

        animator.Play(animationWalk);

        return false;
    }

    // Rotate returns true if the caller funcion must stop
    bool Rotate()
    {
        if (movementStyle == MovementStyle.Forward)
        {
            return false;
        }

        // check if over axis x-z
        float angleX_Z = Vector3.Angle(transform.up, Vector3.up);
        if (!Mathf.Approximately(angleX_Z, 0f))
        { // it means the they are not allign
            return false;
        }

        Vector3 directionToTarget = innerTarget - transform.position;
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
            // Gira hacia el target (aunque est� caminando o no)
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            return true;
        }

        return false;
    }


    // Climb returns true if the caller funcion must stop
    bool Climb()
    {
        // ************************************
        // TO CLIMB
        // ************************************

        #region check forward - wall
        // Direccion actual (forward)

        bool raycastForward = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitForward, wallCheckDistance);
        float distanceForward = Vector3.Distance(transform.position, hitForward.point);

        // detect wall in front of gameObject
        if (raycastForward && distanceForward < minimumCheckDistance && !hitForward.collider.CompareTag("Spider")) // TODO : CHANGE THIS LIMIT
        {
            Vector3 newForward = Vector3.Cross(transform.right, hitForward.normal);
            transform.rotation = Quaternion.LookRotation(newForward, hitForward.normal);
        }

        #endregion

        #region check down - cliff and ground
        Vector3 down = transform.up * -1;
        Vector3 raycastOriginDown = transform.position + transform.forward * 0.5f;
        bool raycastDown = Physics.Raycast(raycastOriginDown, down, out RaycastHit hitDown, /*wallCheckDistance*/0.2f);

        Vector3 ground = transform.up * -1;
        Vector3 raycastOriginGround = transform.position + -1 * transform.forward * 0.01f;
        bool raycastGround = Physics.Raycast(raycastOriginGround, ground, out RaycastHit hitGround, /*wallCheckDistance*/0.2f);

        if (!raycastGround && !raycastDown)
        {
            //transform.Rotate(Vector3.forward, 90f, Space.Self);

            transform.Rotate(90f, 0f, 0f, Space.Self);
        }

        //if (raycastGround && !hitGround.collider.CompareTag("Spider"))
        //{
        //    Vector3 start = hitGround.point;
        //    Vector3 end = start + hitGround.normal * 2.0f;

        //    lineRenderer_normal.SetPosition(0, start);
        //    lineRenderer_normal.SetPosition(1, end);
        //}

        // when gound collides with a surface i need to know if it perpendicular to it
        // if not we need to adjust it
        if (raycastGround && alwaysNormal && !hitGround.collider.CompareTag("Spider"))
        {
            Vector3 normal = hitGround.normal.normalized;
            Vector3 rayDir = ground.normalized;

            float dot = Mathf.Abs(Vector3.Dot(rayDir, normal));
            if (!Mathf.Approximately(Mathf.Abs(dot), 1f))
            {
                Vector3 newForward = Vector3.Cross(transform.right, hitGround.normal);
                transform.rotation = Quaternion.LookRotation(newForward, hitGround.normal);
            }
        }



        #endregion


        //#region draw forward line
        //Vector3 rayStart = transform.position;
        //Vector3 rayEnd = transform.position + transform.forward * wallCheckDistance;
        //// Update the line positions
        //lineRenderer_forward.SetPosition(0, rayStart);
        //lineRenderer_forward.SetPosition(1, rayEnd);

        //#endregion

        //#region draw down line
        //raycastOriginDown = transform.position + transform.forward * 0.5f;
        //down = transform.up * -1;
        //Vector3 rayStartDown = raycastOriginDown;
        //Vector3 rayEndDown = raycastOriginDown + down * wallCheckDistance;
        //// Update the line positions
        //lineRenderer_down.SetPosition(0, rayStartDown);
        //lineRenderer_down.SetPosition(1, rayEndDown);

        //#endregion



        //#region draw ground line
        //ground = transform.up * -1;
        //raycastOriginGround = transform.position + -1 * transform.forward * 0.01f;
        //Vector3 rayStartGround = raycastOriginGround;
        //Vector3 rayEndGround = raycastOriginGround + ground * wallCheckDistance;
        //// Update the line positions
        //lineRenderer_ground.SetPosition(0, rayStartGround);
        //lineRenderer_ground.SetPosition(1, rayEndGround);

        //#endregion

        return false;

    }

    // Update is called once per frame
    void Update()
    {
        ChangeTarget();

        if (Stop())
        {
            return;
        }

        if (Rotate())
        {
            return;
        }

        //if (Climb())
        //{
        //    return;
        //}

        Walk();


    }

    // Walk moves the gameObject forward
    void Walk()
    {
        transform.Translate(Time.deltaTime * walkSpeed * Vector3.forward);


        //Turn();
    }

    // Turn turns this current gameObject to face the target
    // TODO : implement as script for dogs, to do gradually
    void Turn()
    {
        // Get the direction towards the target in the X-Z plane
        Vector3 direction = new Vector3(innerTarget.x - transform.position.x, 0, innerTarget.z - transform.position.z);

        // If the direction is not zero, rotate towards the target
        if (direction != Vector3.zero)
        {
            // Create a rotation that looks in the direction of the target on the X-Z plane
            Quaternion rotation = Quaternion.LookRotation(direction);

            // Apply the new rotation to the object, while preserving its current Y rotation
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }


    // ChangeTarget changes the target in random value for x and z
    void ChangeTarget()
    {

        if (movementStyle != MovementStyle.Wander)
        {
            return;
        }

        Vector3 position = new Vector3(transform.position.x, 0, transform.position.z);
        // get distance to target
        float distanceToTarget = Vector3.Distance(transform.position, innerTarget);

        // if it is not close then do not change target
        if (distanceToTarget > stopDistance)
        {
            return;
        }


        NewTarget();
    }

    void NewTarget()
    {

        Vector2 random2D = Random.insideUnitCircle * radius;
        innerTarget = center + new Vector3(random2D.x, 0, random2D.y); // y = 0 for flat ground
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, radius);

        if (innerTarget != null)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawSphere(innerTarget, 0.2f);
        }
    }
}
