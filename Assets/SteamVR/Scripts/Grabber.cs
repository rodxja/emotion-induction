using UnityEngine;
using Valve.VR;

public class Grabber : MonoBehaviour
{
    public SteamVR_Input_Sources handType; // Left or Right
    public SteamVR_Action_Boolean grabAction;
    private GameObject collidingObject;
    private GameObject objectInHand;
    public SteamVR_Action_Vibration hapticAction;

    private void SetCollidingObject(Collider col)
    {
        if (collidingObject || !col.GetComponent<Rigidbody>())
            return;

        collidingObject = col.gameObject;
    }

    public void OnTriggerEnter(Collider other) => SetCollidingObject(other);
    public void OnTriggerStay(Collider other) => SetCollidingObject(other);
    public void OnTriggerExit(Collider other)
    {
        if (!collidingObject) return;
        collidingObject = null;

        // Haptic feedback on touch
        hapticAction.Execute(0, 0.05f, 100, 0.5f, handType);
    }

    void Update()
    {
        //if (grabAction != null && grabAction.GetState(handType))
        //{
        //    Debug.Log($"{handType} grip is pressed");
        //}        //if (grabAction != null && grabAction.GetState(handType))
        //{
        //    Debug.Log($"{handType} grip is pressed");
        //}

        if (grabAction.GetStateDown(handType))
        {
            if (collidingObject)
            {
                objectInHand = collidingObject;
                collidingObject = null;
                var joint = gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = objectInHand.GetComponent<Rigidbody>();

                // Haptic feedback on grab
                hapticAction.Execute(0, 0.1f, 200, 0.8f, handType);
            }
        }

        if (grabAction.GetStateUp(handType))
        {
            if (objectInHand)
            {
                if (GetComponent<FixedJoint>())
                {
                    GetComponent<FixedJoint>().connectedBody = null;
                    Destroy(GetComponent<FixedJoint>());
                    Rigidbody rb = objectInHand.GetComponent<Rigidbody>();
                    rb.linearVelocity = GetComponent<Rigidbody>().linearVelocity;
                    rb.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
                }

                objectInHand = null;
            }
        }

        if (objectInHand)
        {
            Debug.DrawLine(transform.position, objectInHand.transform.position, Color.red);
        }
    }

    //void LateUpdate()
    //{
    //    if (objectInHand)
    //    {
    //        Vector3 pos = objectInHand.transform.position;
    //        objectInHand.transform.position = new Vector3(pos.x, 0.5f, pos.z); // 0.5f o la altura deseada
    //    }
    //}
}
