using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using static MySceneManager;
using Scene = UnityEngine.SceneManagement.Scene;
using System.Collections.Generic;
using UnityEngine.Video;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.Audio;

// It is similar to LoadSubScenes
public class LauncherManager : MonoBehaviour
{
    public Toggle mediaToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown secondsOptions; // Asigna el Dropdown desde el Inspector
    public TMPro.TMP_Dropdown stimuliOptions; // Asigna el Dropdown desde el Inspector
    public Button startButton; // Asigna el bot�n desde el Inspector

    private bool embodiment = false;
    private int secondsOption = 0;
    private string scene = "";

    private const string labMenuStr = "LabMenu";

    private GameObject labMenu;

    private Scene secundaryScene;

    private XRDetection xrDetection;

    public enum SceneNames
    {
        PositiveAnimal_forLab,
        NegativeScene_forLab,
        Lab_forLab,
        ThreeSixtyVideo
    }

    void Awake()
    {
        xrDetection = GetComponent<XRDetection>();
        if (xrDetection == null) {
            Debug.LogError($"there is no 'XRDetection' component in gameObject {gameObject.name}");
        }
    }

    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        if (mediaToggle != null)
        {
            mediaToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        if (secondsOptions != null)
        {
            secondsOptions.onValueChanged.AddListener(OnSecondsChanged);
        }

        if (stimuliOptions != null)
        {
            stimuliOptions.onValueChanged.AddListener(OnScenesChanged);
        }


        if (startButton != null)
        {
            startButton.onClick.AddListener(OnButtonClick);
        }

        SetCGIOptions();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing!");
        ActivateLabMenu();
    }

    void OnSceneUnloaded(Scene inScene)
    {
        if (inScene.name != scene)
        {
            Debug.Log($"unloaded scene is not the same inScene.name {inScene.name} scene {scene}");
        }


        ActivateLabMenu();
        xrDetection.GetActiveRig().SetActive(true);
        //SceneManager.sceneUnloaded -= OnSceneUnloaded; // Clean up
    }

    // selects embodiment
    private void OnToggleChanged(bool value)
    {
        if (value)
        {
            SetVideoOptions();
        }
        else
        {
            SetCGIOptions();
        }
    }

    private void SetCGIOptions()
    {
        // Add new options
        List<string> options = new List<string> { "A", "B", "C" };
        stimuliOptions.ClearOptions();
        stimuliOptions.AddOptions(options);

        // Set default value
        stimuliOptions.value = 0;
        Debug.Log("Options count: " + stimuliOptions.options.Count);
    }

    private void SetVideoOptions()
    {
        // Add new options
        List<string> options = new List<string> { "1", "2", "3", "4", "5" };
        stimuliOptions.ClearOptions();
        stimuliOptions.AddOptions(options);

        // Set default value
        stimuliOptions.value = 0;

        Debug.Log("Options count: " + stimuliOptions.options.Count);

    }

    private void OnSecondsChanged(int value)
    {
        secondsOption = value; // the scene maps values 0 or 1 to the correct amount of seconds
    }
    private void OnScenesChanged(int value)
    {
        SetScene(value);
    }

    private void SetScene(int value)
    {
        switch (value)
        {
            case 0:
                scene = SceneNames.PositiveAnimal_forLab.ToString();
                break;
            case 1:
                scene = SceneNames.NegativeScene_forLab.ToString();
                break;
            case 2:
                scene = SceneNames.Lab_forLab.ToString();
                break;
            case 3:
                scene = SceneNames.ThreeSixtyVideo.ToString();
                break;
            default:
                scene = SceneNames.PositiveAnimal_forLab.ToString();
                break;
        }

    }

    // this starts and loads the scene
    private void OnButtonClick()
    {
        Debug.Log("hubo click");
        if (mediaToggle.isOn)
        {
            // LoadFlatVideo ();
            Load360Video(); // TODO : pending to add options to select the video to play

        }
        else
        {
            Debug.Log("CGI");
            SetScene(stimuliOptions.value);

            DeactivateLabMenu();
            StartCoroutine(LoadSceneAndCall(scene));
        }
    }

    private void Load360Video()
    {
        SetScene(3);

        DeactivateLabMenu();
        StartCoroutine(Load360SceneAndCall(scene));
    }


    void DeactivateLabMenu()
    {
        // if it is null then i need to get it, otherwise it is loaded on labMenu variable
        if (labMenu == null)
        {
            labMenu = GetLabMenu();
        }

        // if it is still null, there should not call SetActive
        if (labMenu == null)
        {
            UnityEngine.Debug.LogError("labMenu is null");
            return;
        }
        labMenu.SetActive(false);
    }

    private GameObject GetLabMenu()
    {
        string tag = TagsOrNames.Menu.ToString();
        string componentName = labMenuStr;

        GameObject[] menues = GameObject.FindGameObjectsWithTag(tag);
        if (menues.Length == 0)
        {
            UnityEngine.Debug.LogError($"no gameObject was found with the tag '{tag}'");
            return null;
        }

        labMenu = System.Array.Find(menues, x => x.name == componentName);
        if (labMenu == null)
        {
            UnityEngine.Debug.LogError($"no gameObject was found with the name '{componentName}'");
            return null;
        }


        return labMenu;
    }


    void ActivateLabMenu()
    {
        if (labMenu == null)
        {
            UnityEngine.Debug.LogError("labMenu is null");
            return;
        }

        labMenu.SetActive(true);

    }


    IEnumerator LoadSceneAndCall(string scene)
    {
        if (SceneManager.GetSceneByName(scene).isLoaded)
            yield break;

        Debug.Log("Additive");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // Esperar hasta que se cargue completamente
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("cargo");

        // Buscar la escena recién cargada
        secundaryScene = SceneManager.GetSceneByName(scene);

        GameObject obj = GetGameObjectFromSceneByTag(secundaryScene, "SceneManager");
        if (obj != null && obj.TryGetComponent<MySceneManager>(out var sceneManager))
        {
            sceneManager.StartsWith(embodiment, secondsOption, true);
        }

        xrDetection.GetActiveRig().SetActive(false);
        GameObject xrRig = GetGameObjectFromSceneByTag(secundaryScene, "XRRig");
        if (xrRig != null)
        {
            xrRig.SetActive(true);
        }

    }

    IEnumerator Load360SceneAndCall(string scene)
    {
        if (SceneManager.GetSceneByName(scene).isLoaded)
            yield break;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        // Esperar hasta que se cargue completamente
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Buscar la escena recién cargada
        secundaryScene = SceneManager.GetSceneByName(scene);

        GameObject obj = GetGameObjectFromSceneByTag(secundaryScene, "SceneManager");
        if (obj != null && obj.TryGetComponent<WhichVideoToPlay>(out var whichVideoToPlay))
        {
            whichVideoToPlay.PlayVideo(stimuliOptions.value, secondsOptions.value); 
        }

        //xrDetection.GetActiveRig().SetActive(false);
        //GameObject xrRig = GetGameObjectFromSceneByTag(secundaryScene, "XRRig");
        //if (xrRig != null)
        //{
        //    xrRig.SetActive(true);
        //}
    }

    private GameObject GetGameObjectFromSceneByTag(Scene scene, string tag) {

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            GameObject which_Obj = FindWithTagRecursive(rootObj.transform, tag);
            if (which_Obj != null)
            {
                return which_Obj;
            }
        }

        return null;
    }

    // Recursive search function
    GameObject FindWithTagRecursive(Transform parent, string tag)
    {
        if (parent.CompareTag(tag))
            return parent.gameObject;

        foreach (Transform child in parent)
        {
            GameObject result = FindWithTagRecursive(child, tag);
            if (result != null)
                return result;
        }

        return null;
    }

}
