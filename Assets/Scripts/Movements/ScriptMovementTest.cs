using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptMovementTest : MonoBehaviour
{
    public Transform target;
    public float stopDistance = 0f;  // Stop walking when within 0.5 units of the target

    public float walkSpeed = 1f;
    public Vector3 direction = Vector3.forward;
    public float wallCheckDistance = 1.1f; // TODO: should be a percentage of the lenght of the asset
    public float minimumDistance = 0.1f;
    public bool alwaysNormal = true;
    public float rotationSpeed = 5f;
    public float walkAngleThreshold = 5f; // Empieza a caminar si el ngulo es menor a esto

    //private LineRenderer lineRenderer_forward;
    //private LineRenderer lineRenderer_down;
    //private LineRenderer lineRenderer_ground;


    //private LineRenderer lineRenderer_normal;

    private Animator animator;

    private const string animationIdle = "Idle";
    private const string animationAttack1 = "Attack1";
    private const string animationAttack2 = "Attack2";
    private const string animationDeath = "Death";
    private const string animationTakeDamage_002 = "TakeDamage_002";
    private const string animationWalk = "Walk";

    private const string canIdle = "canIdle";
    private const string canWalk = "canWalk";

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
        if (target == null)
        {

            Scene myScene = gameObject.scene;

            Camera mainCam = FindMainCameraInScene(myScene);

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
            return;
        }

        animator.Play(animationWalk);
        // Transition from Idle to Walk
        animator.SetBool(canIdle, false);  // Stop idle
        animator.SetBool(canWalk, true);   // Start walking
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
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget < stopDistance) // stops and just stays in IDLE animation
        {
            // TODO: pending to adapt animation time
            // Transition from Idle to Walk
            animator.SetBool(canIdle, true);  // Stop idle
            animator.SetBool(canWalk, false);   // Start walking

            return true;
        }

        return false;
    }

    // Rotate returns true if the caller funcion must stop
    bool Rotate() {

        // check if over axis x-z
        float angleX_Z = Vector3.Angle(transform.up, Vector3.up);
        if (!Mathf.Approximately(angleX_Z, 0f)) { // it means the they are not allign
            return false;
        }

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
            // Gira hacia el target (aunque est� caminando o no)
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            return true;
        }

        return false;
    }


    // Climb returns true if the caller funcion must stop
    bool Climb() {
        // ************************************
        // TO CLIMB
        // ************************************

        #region check forward - wall
        // Direccion actual (forward)

        bool raycastForward = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitForward, wallCheckDistance);
        float distanceForward = Vector3.Distance(transform.position, hitForward.point);

        // detect wall in front of gameObject
        if (raycastForward && distanceForward < minimumDistance && !hitForward.collider.CompareTag("Spider")) // TODO : CHANGE THIS LIMIT
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
        // Update the line positions
        //lineRenderer_forward.SetPosition(0, rayStart);
        //lineRenderer_forward.SetPosition(1, rayEnd);

        //#endregion

        //#region draw down line
        //raycastOriginDown = transform.position + transform.forward * 0.5f;
        //down = transform.up * -1;
        //Vector3 rayStartDown = raycastOriginDown;
        //Vector3 rayEndDown = raycastOriginDown + down * wallCheckDistance;
        // Update the line positions
        //lineRenderer_down.SetPosition(0, rayStartDown);
        //lineRenderer_down.SetPosition(1, rayEndDown);

        //#endregion



        //#region draw ground line
        //ground = transform.up * -1;
        //raycastOriginGround = transform.position + -1 * transform.forward * 0.01f;
        //Vector3 rayStartGround = raycastOriginGround;
        //Vector3 rayEndGround = raycastOriginGround + ground * wallCheckDistance;
        // Update the line positions
        //lineRenderer_ground.SetPosition(0, rayStartGround);
        //lineRenderer_ground.SetPosition(1, rayEndGround);

        //#endregion

        return false;

    }

    // Update is called once per frame
    void Update()
    {

        if (Stop())
        {
            return;
        }

        if (Rotate())
        {
            return;
        }

        if (Climb())
        {
            return;
        }

        Walk();

        
    }

    // Walk moves the gameObject forward
    void Walk()
    {
        transform.Translate(Time.deltaTime * walkSpeed * Vector3.forward);


        //Turn();
    }

    // Turn turns this current gameObject to face the target
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
}
