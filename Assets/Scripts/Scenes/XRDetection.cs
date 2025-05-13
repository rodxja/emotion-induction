using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections.Generic;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRDetection : MonoBehaviour
{
    public GameObject xrRig;
    public GameObject desktopRig;
    public XRUIInputModule xrInputModule;
    public InputSystemUIInputModule desktopInputModule;

    void Start()
    {
        StartCoroutine(CheckXRReady());
    }

    System.Collections.IEnumerator CheckXRReady()
    {
        // Espera un frame para asegurarte que los subsistemas están cargados
        yield return null;

        if (IsHMDConnected())
        {
            xrRig.SetActive(true);
            desktopRig.SetActive(false);
            xrInputModule.enabled = true;
            desktopInputModule.enabled = false;
            Debug.Log("HMD detected. XR Rig enabled.");
        }
        else
        {
            xrRig.SetActive(false);
            desktopRig.SetActive(true);
            xrInputModule.enabled = false;
            desktopInputModule.enabled = true;
            Debug.Log("No HMD detected. Desktop Rig enabled.");
        }
    }
    bool IsHMDConnected()
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
