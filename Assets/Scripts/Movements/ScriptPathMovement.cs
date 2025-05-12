using UnityEngine;

public class PathFollower : MonoBehaviour
{
    public Transform[] pathPoints;
    public float speed = 2f;
    public bool loop = true; // TODO : pending to implement
    public DogType dogType; // This will show as a dropdown in the Inspector

    public enum DogType
    {
        corgi,
        cur,
        none
    }

    private bool hasStopped = false;

    private float distanceTravelled = 0f;

    private Animator animator;

    private string animationWalking02 = "Walking02";
    void Start()
    {

        if (pathPoints.Length < 2)
        {
            Debug.LogError("pathPoints has less than two points");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"current gameObject '{gameObject.name}' does not have an 'Animator' component");
        }

        animator.Play(GetDogTypeName() + animationWalking02);
    }

    void Update()
    {
        MoveThroughPath();
        if (hasStopped)
        {
            animator.SetBool("isBreathing", true);
            animator.SetBool("isWalking02", false);
            animator.SetBool("isSitting", true);
        }
    }

    public string GetDogTypeName()
    {
        switch (dogType)
        {
            case DogType.cur: return "cur_";
            case DogType.corgi: return "corgi_";
            default: return "";
        }
    }

    void MoveThroughPath()
    {
        if (hasStopped)
        {
            return;
        }
        // this only works when there is a segment, more than 1 point
        if (pathPoints.Length < 2) return;

        // we increase the distance travelled
        distanceTravelled += Time.deltaTime * speed;

        // we get the segment in which the asset is travelling
        int segmentIndex = GetSegmentIndex(distanceTravelled);

        // if the index is -1 it means that it did not find the segment
        // second condition should not happen
        if (segmentIndex == -1 || segmentIndex == pathPoints.Length - 1)
        {
            hasStopped = true;
            Debug.LogError($"{GetDogTypeName()} segmentIndex {segmentIndex} distanceTravelled {distanceTravelled}");
            return;
        }

        float totalFullSegmentDistance = GetTotalPreviousSegments(segmentIndex);

        // get start and end of segment
        Vector3 start = pathPoints[segmentIndex].position;
        Vector3 end = pathPoints[segmentIndex + 1].position;

        float previousDistanceSegment = 0f;
        if (segmentIndex != 0)
        {
            previousDistanceSegment = Vector3.Distance(pathPoints[segmentIndex - 1].position, start);
        }

        // distance between points
        float distanceSegment = Vector3.Distance(start, end);



        // get the percentage travelled in the segment
        float t = (distanceTravelled - totalFullSegmentDistance) / distanceSegment; // percentage along this segment

        // move the asset
        Vector3 newPosition = Vector3.Lerp(start, end, t);

        //float distanceInJump = Vector3.Distance(newPosition, transform.position);
        //if (distanceInJump > 1)
        //{
        //    Debug.LogError($"distanceInJump {distanceInJump} - segmentIndex {segmentIndex} distanceTravelled {distanceTravelled} totalFullSegmentDistance {totalFullSegmentDistance} distanceSegment {distanceSegment} previousDistanceSegment {previousDistanceSegment} - start {start} end {end}");
        //}

        transform.position = newPosition;

        turn(segmentIndex);
    }

    // TODO: this must be stored in a variable and not be calculated each iteration

    float GetTotalPreviousSegments(int index)
    {

        float totalSegmentDistance = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            if (i == index)
            {
                return totalSegmentDistance;
            }
            totalSegmentDistance += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
        }

        return totalSegmentDistance;
    }


    // points = [a, b, c, d]
    // between a - b, there is the segment 0
    // b - c : 1
    // c - d : 2
    // TODO : store distance in a table just to query then and not calculate in each iteration
    int GetSegmentIndex(float distance)
    {
        float sumDistance = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            sumDistance += Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            if (distance <= sumDistance)
            {
                return i;
            }
        }

        return -1;
    }

    void printSegments()
    {
        float sumDistance = 0f;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {

            float totalPreviousSegments = GetTotalPreviousSegments(i);

            float segmentDistance = Vector3.Distance(pathPoints[i].position, pathPoints[i + 1].position);
            sumDistance += segmentDistance;
            //Debug.Log($"segment({i})\tstart\t{i} -\tend\t{i + 1}\t: segmentDistance\t{segmentDistance}\tsumDistance\t{sumDistance}\ttotalPreviousSegments\t{totalPreviousSegments}");
        }

    }


    // rotate
    // it needs a final direction to face, a vector
    // TODO : to smooth turning
    // - calculate angle between following point and the third
    // - according to distance move gradually towards it
    // - if angle is 30 grades, and distance is 60, we turn 0.5 grades towards target
    // - NOT OPTIONAL FOR SHARP TURNS
    void turn(int segmentIndex)
    {

        // get start and end of segment
        Vector3 start = pathPoints[segmentIndex].position;
        Vector3 end = pathPoints[segmentIndex + 1].position;
        Vector3 target = Vector3.zero;

        // target is to rotate the asset
        // the idea is to take the next point after the current segment
        // otherwise it takes the end point of the current segment
        if (segmentIndex < pathPoints.Length - 2)
        {
            target = pathPoints[segmentIndex + 2].position;
        }
        else
        {
            target = end;
        }

        // TODO :  first time the direction will not be correct
        Vector3 currentDirection = transform.forward; // the direction your GameObject is facing
        //Vector3 currentDirection = (end - start).normalized;
        Vector3 targetDirection = (target - transform.position).normalized; // direction to target
        Vector3 up = transform.up; // rotation axis (usually Y for upright characters)

        float angle = Vector3.Angle(currentDirection, targetDirection);
        float signedAngle = angle;

        // Cross product to know the rotation axis
        Vector3 rotationAxis = Vector3.Cross(currentDirection, targetDirection).normalized;


        float distanceCurrentToEnd = Vector3.Distance(transform.position, end);

        float stepAngle = signedAngle / distanceCurrentToEnd * Time.deltaTime;

        //Debug.Log($"signedAngle\t{signedAngle}\tdistanceCurrentToEnd\t{distanceCurrentToEnd}\tstepAngle\t{stepAngle}");

        transform.rotation = Quaternion.AngleAxis(stepAngle, rotationAxis) * transform.rotation;
    }

}