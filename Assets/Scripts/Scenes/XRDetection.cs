using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// XRDetection detects whether a HMD is connected
// This in order to activate "xrRig" that allows the use of the HMD
// or activate "desktopRig" that allows the use of the scene with mouse control
public class XRDetection : MonoBehaviour
{
    public GameObject xrRig;
    public GameObject desktopRig;
    public XRUIInputModule xrInputModule;
    public InputSystemUIInputModule desktopInputModule;

    void Start()
    {
        // it uses a coroutine because the HMD sometimes is not detected at the begining of the Start execution
        StartCoroutine(CheckXRReady());
    }


    // (De)activate the rig necessary and its input module
    System.Collections.IEnumerator CheckXRReady()
    {
        // Wait a frame to be sure that systems are loaded
        yield return null;

        if (IsHMDConnected())
        {
            xrRig.SetActive(true);
            xrInputModule.enabled = true;

            desktopRig.SetActive(false);
            desktopInputModule.enabled = false;
        }
        else
        {
            xrRig.SetActive(false);
            xrInputModule.enabled = false;

            desktopRig.SetActive(true);
            desktopInputModule.enabled = true;
        }
    }

    public GameObject GetActiveRig()
    {
        if (IsHMDConnected())
        {
            return xrRig;
        }
        else
        {
            return desktopRig;
        }
    }

    // Checks whether the HMD is connected
    public bool IsHMDConnected()
    {
        List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetSubsystems(displaySubsystems);

        foreach (var subsystem in displaySubsystems)
        {
            if (subsystem.running)
            {
                return true;
            }
        }

        return false;
    }
}
