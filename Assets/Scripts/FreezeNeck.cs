using UnityEngine;

public class FreezeNeck : MonoBehaviour
{
    public Transform neckBone;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    void Start()
    {
        initialLocalPosition = neckBone.localPosition;
        initialLocalRotation = neckBone.localRotation;
    }

    void LateUpdate()
    {
        //Debug.Log($"neckBone.localPosition {neckBone.localPosition} - initialLocalPosition {initialLocalPosition}");
        neckBone.localPosition = initialLocalPosition;
        neckBone.localRotation = initialLocalRotation;
    }
}
