using UnityEngine;

public class RayButtonClick : MonoBehaviour
{
    // Detect if the ray hits the button
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Something Collided with Button");

        // Check if the collider that's interacting is the ray from the hand (XR Ray Interactor)
        if (other.CompareTag("XRRay"))
        {
            Debug.Log("Ray Collided with Button");
            // Perform action, for example, invoke a button click
            OnRayHitButton();
        }
    }

    // Method called when the ray hits the button
    private void OnRayHitButton()
    {
        // Example action: Simulate button click or invoke any behavior
        Debug.Log("Button Clicked via Ray!");
        // You can also trigger an event or other logic here
    }
}
