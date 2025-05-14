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
            Vector3 headOffset = xrRig.transform.position - Camera.main.transform.position;
            xrRig.transform.position = teleportTarget.position + headOffset;
        }
    }
}
