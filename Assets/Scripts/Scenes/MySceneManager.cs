using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Linq;



// script that deactivate gameObjects and activate them when it is necessary
// the idea is to add a button to start the simulation, or a counter with black background
// it also verifies if the scene was loaded as additive, if it is it should deactivate the xr rig
public class MySceneManager : MonoBehaviour
{
    public Toggle embodimentToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown secondsOptions; // Asigna el Dropdown desde el Inspector
    public Button startButton; // Asigna el bot�n desde el Inspector

    private bool start = false;
    private bool embodiment = false;
    private int seconds = 30;
    private bool shutDown = false;


    // targetTag indicats the tag of the gameObjects to deactivate until start is true
    public string[] targetTags;

    private bool isBlackoutTriggered = false;
    private bool isActivated = false;

    private List<GameObject> targetObjects = new List<GameObject>();

    private Color originalBackgroundColor;

    public enum TagsOrNames
    {
        Menu,
        Body,
        Spider,
        MainCamera,
        StartMenu,
        XRRig
    }
    void Awake()
    {

        // deactivate tags until start is set to true
        DeactivateTags();

        // sets the screen in black
        Blackout();

        // Deactivate XROrigin in case the scene is Additive
        if (IsAdittiveScene())
        {
            XROrigin rig = FindXRRig();

            if (rig != null)
            {
                rig.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No XR Rig found in scene.");
            }
        }
    }

    public void StartsWith(bool embodimentOption, int secondsOption, bool shutDownOption)
    {
        shutDown = shutDownOption;

        embodiment = embodimentOption;

        DeactivateStartMenu();

        InitStimuli(secondsOption);
    }


    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnButtonClick);
        }

        if (secondsOptions != null)
        {
            secondsOptions.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (embodimentToggle != null)
        {
            embodimentToggle.onValueChanged.AddListener(OnToggleChanged);
        }

    }

    private void OnButtonClick()
    {
        InitStimuli(secondsOptions.value);
    }

    private void InitStimuli(int secondsOptions)
    {
        UnityEngine.Debug.Log("El bot�n ha sido presionado");
        start = true;

        // Deactivate body
        if (!embodiment)
        {
            DeactivateBody();
        }

        // this is done in case that the person press Start immediatly
        SetSeconds(secondsOptions);

        ResetSpiders();

        DeactivateStartMenu();
    }

    // this will be called by both scenes but spiders only exists in the negative scene
    private void ResetSpiders()
    {
        // sets gameObject to initial positiond
        if (seconds == 5)
        {
            string tag = "MainCamera";
            GameObject[] tmp_targetObjects = GameObject.FindGameObjectsWithTag(tag).Where(go => go.activeInHierarchy).ToArray();

            if (tmp_targetObjects.Length != 1)
            {
                UnityEngine.Debug.LogError($"no active gameObject was found with the tag '{tag}'");
                return;
            }
            GameObject mainCamera = tmp_targetObjects[0];

            GameObject[] spiders = Array.FindAll<GameObject>(targetObjects.ToArray(), x => x.CompareTag(TagsOrNames.Spider.ToString()));
            foreach (GameObject spider in spiders)
            {
                //MoveAXYtoB(spider.transform, mainCamera.transform);

            }
        }
    }


    void MoveAXYtoB(Transform A, Transform B)
    {
        Transform aParent = A.parent;   // A’s coordinate frame

        // --- 1. get B’s position in world space ------------
        Vector3 bWorld = B.position;

        // --- 2. convert that world point into A-parent space
        Vector3 bInAParent = aParent.InverseTransformPoint(bWorld);

        // --- 3. keep A’s original z, but copy the x & y ----
        Vector3 newLocal = A.localPosition;   // start with the current local pos
        newLocal.x = bInAParent.x;
        newLocal.y = bInAParent.y;

        // --- 4. apply --------------------------------------
        A.localPosition = newLocal;
    }

    private void OnDropdownChanged(int value)
    {
        SetSeconds(value);
    }

    private void SetSeconds(int value)
    {
        switch (value)
        {
            case 0:
                seconds = 5;
                break;
            case 1:
                seconds = 30;
                break;
            default:
                seconds = 5;
                break;
        }
    }

    private void OnToggleChanged(bool value)
    {
        embodiment = value;
    }
    // Opcional: Si el bot�n puede desuscribirse (por ejemplo, cuando se destruye el objeto)
    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnButtonClick);
        }

        if (secondsOptions != null)
        {
            secondsOptions.onValueChanged.RemoveListener(OnDropdownChanged);
        }

        if (embodimentToggle != null)
        {
            embodimentToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    private void Update()
    {
        if (start && !isActivated)
        {
            ActivateTags();
            RestoreBlackout();
            isActivated = true;
        }

        if (start && !isBlackoutTriggered)
        {
            StartCoroutine(BlackoutAfterDelayCoroutine());
            isBlackoutTriggered = true;
        }
    }

    private XROrigin FindXRRig()
    {
        UnityEngine.SceneManagement.Scene scene = gameObject.scene;

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            XROrigin rig = rootObj.GetComponentInChildren<XROrigin>(true);
            if (rig != null)
            {
                Debug.Log("Found XR Rig in scene: " + scene.name + " - " + rig.gameObject.name);
                // Puedes guardar una referencia si quieres usarla
                return rig;
            }
        }

        return null; // XR Rig not found
    }

    // isAdittiveScene returns true if the scene is loaded as an Additive Scene
    // or returns false if it is loaded as the Main Scene
    bool IsAdittiveScene()
    {
        UnityEngine.SceneManagement.Scene myScene = gameObject.scene;
        UnityEngine.SceneManagement.Scene activeScene = SceneManager.GetActiveScene();

        return !(myScene == activeScene);
    }

    void DeactivateBody()
    {

        string tag = TagsOrNames.Body.ToString();

        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);
        if (bodies.Length == 0)
        {
            UnityEngine.Debug.LogError($"no gameObject was found with the tag '{tag}'");
            return;
        }
        foreach (GameObject body in bodies)
        {
            body.SetActive(false);
        }

    }

    void DeactivateStartMenu()
    {
        string tag = TagsOrNames.Menu.ToString();
        string componentName = TagsOrNames.StartMenu.ToString();

        GameObject[] menues = GameObject.FindGameObjectsWithTag(tag);
        if (menues.Length == 0)
        {
            UnityEngine.Debug.LogError($"no gameObject was found with the tag '{tag}'");
            return;
        }

        GameObject startMenu = System.Array.Find(menues, x => x.name == componentName);
        if (startMenu == null)
        {
            UnityEngine.Debug.LogError($"no gameObject was found with the name '{componentName}'");
            return;
        }

        startMenu.SetActive(false);
    }


    void DeactivateTags()
    {
        foreach (string tag in targetTags)
        {
            // Find and initially deactivate the target GameObjects
            GameObject[] tmp_targetObjects = GameObject.FindGameObjectsWithTag(tag);

            if (tmp_targetObjects.Length == 0)
            {
                UnityEngine.Debug.LogError($"no gameObject was found with the tag '{tag}'");
                return;
            }

            foreach (GameObject obj in tmp_targetObjects)
            {
                obj.SetActive(false);
                targetObjects.Add(obj);
            }
        }
    }

    void ActivateTags()
    {
        if (targetObjects.Count == 0)
        {
            UnityEngine.Debug.LogError("there are no objects in 'targetObjects'");
        }

        foreach (GameObject obj in targetObjects)
        {
            obj.SetActive(true);
        }
    }

    // BlackoutAfterDelayCoroutine displays a black screen after N amount of seconds
    private IEnumerator BlackoutAfterDelayCoroutine()
    {
        // Wait for 5 seconds
        yield return new WaitForSeconds((float)seconds);

        // ??? i think these two methods should not be called anymore
        // because if i am in the scene after n minutes i reloads everything, - pending to test
        // and if it is from lab, it unloads itself
        DeactivateTags();
        Blackout();

        if (shutDown)
        {
            UnloadMyScene();
        }
        else
        {
            ReloadSceneA();
        }

    }

    void Blackout()
    {
        UnityEngine.SceneManagement.Scene myScene = gameObject.scene;

        // Disable all renderers in the scene
        foreach (var renderer in FindObjectsByType<Renderer>(UnityEngine.FindObjectsSortMode.None))
        {
            if (renderer.gameObject.scene == myScene)
            {
                renderer.enabled = false;
            }
        }

        // Change background color only if the camera is from Scene B
        Camera[] allCameras = Camera.allCameras;
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.scene == myScene)
            {
                originalBackgroundColor = cam.backgroundColor;
                cam.backgroundColor = Color.black;
            }
        }
    }

    void RestoreBlackout()
    {
        UnityEngine.SceneManagement.Scene myScene = gameObject.scene;
        //if (!sceneB.IsValid()) return;

        // Enable renderers in Scene B only
        foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (renderer.gameObject.scene == myScene)
            {
                renderer.enabled = true;
            }
        }

        // Restore background color of cameras in Scene B only
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam.gameObject.scene == myScene)
            {
                cam.backgroundColor = originalBackgroundColor;
            }
        }
    }

    private void UnloadMyScene()
    {
        Scene scene = gameObject.scene; // the scene this GameObject belongs to
        if (scene.IsValid())
        {
            SceneManager.UnloadSceneAsync(scene);
            Debug.Log("Unloading scene: " + scene.name);
        }
    }


    void ReloadSceneA()
    {
        string currentScene = gameObject.scene.name;
        SceneManager.LoadScene(currentScene);
    }

}
