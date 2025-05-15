using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerTeleport : MonoBehaviour
{
    public InputActionProperty rightGripAction; // XRI RightHand/Select
    public InputActionProperty leftGripAction;  // XRI LeftHand/Select
    public Transform teleportTarget;
    public GameObject xrRig;

    void Update()
    {
        float rightTrigger = rightGripAction.action.ReadValue<float>();
        float leftTrigger = leftGripAction.action.ReadValue<float>();
        if (rightTrigger > 0.9f || leftTrigger > 0.9f)
        {
            Teleport();
        }
    }

    private void Teleport_pre()
    {
        Vector3 headOffset = xrRig.transform.position - Camera.main.transform.position;
        Vector3 newPosition = teleportTarget.position + headOffset;
        xrRig.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);
    }

    private void Teleport()
    {
        // Compute the head offset relative to the XR rig
        Transform head = Camera.main.transform;
        Vector3 headOffset = xrRig.transform.position - head.position;

        // Move the rig to the teleport target, preserving the head offset
        Vector3 newPosition = teleportTarget.position + headOffset;
        xrRig.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);

        // Compute horizontal rotation to align the user's forward with the teleport target's forward
        Vector3 forwardFlat = new Vector3(head.forward.x, 0, head.forward.z).normalized;
        Vector3 targetForwardFlat = new Vector3(teleportTarget.forward.x, 0, teleportTarget.forward.z).normalized;

        float angleDifference = Vector3.SignedAngle(forwardFlat, targetForwardFlat, Vector3.up);

        // Rotate the rig around the head's position to match the target direction
        xrRig.transform.RotateAround(head.position, Vector3.up, angleDifference);
    }

}
