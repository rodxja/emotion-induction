using UnityEngine;

public class Crawl : MonoBehaviour
{

    private LineRenderer lineRenderer_wall;
    private LineRenderer lineRenderer_cliff;
    private LineRenderer lineRenderer_ground;

    public float rayCastLength = 1f;

    void Awake()
    {
        lineRenderer_wall = CreateLineRenderer(UnityEngine.Color.green);
        lineRenderer_cliff = CreateLineRenderer(UnityEngine.Color.blue);
        lineRenderer_ground = CreateLineRenderer(UnityEngine.Color.red);
    }

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
        
    }

    // Update is called once per frame
    void Update()
    {
        DrawRaycast();
    }

    void DrawRaycast() {

        #region draw forward line
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = transform.position + transform.forward * rayCastLength;
        // Update the line positions
        lineRenderer_wall.SetPosition(0, rayStart);
        lineRenderer_wall.SetPosition(1, rayEnd);
        #endregion

        #region draw down line
        Vector3 raycastOriginDown = transform.position + transform.forward * 0.5f;
        Vector3 down = transform.up * -1;
        Vector3 rayStartDown = raycastOriginDown;
        Vector3 rayEndDown = raycastOriginDown + down * rayCastLength;
        // Update the line positions
        lineRenderer_cliff.SetPosition(0, rayStartDown);
        lineRenderer_cliff.SetPosition(1, rayEndDown);
        #endregion



        #region draw ground line
        Vector3 ground = transform.up * -1;
        Vector3 raycastOriginGround = transform.position + -1 * transform.forward * 0.01f;
        Vector3 rayStartGround = raycastOriginGround;
        Vector3 rayEndGround = raycastOriginGround + ground * rayCastLength;
        // Update the line positions
        lineRenderer_ground.SetPosition(0, rayStartGround);
        lineRenderer_ground.SetPosition(1, rayEndGround);
        #endregion
    }
}
