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

public class LoadSubScenes : MonoBehaviour
{
    public Toggle mediaToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown secondsOptions; // Asigna el Dropdown desde el Inspector
    public TMPro.TMP_Dropdown stimuliOptions; // Asigna el Dropdown desde el Inspector
    public Button startButton; // Asigna el bot�n desde el Inspector

    public Renderer myRenderer;

    public Material materialScene_0;
    public Material materialScene_1;
    public Material materialScene_2;
    public Material materialBlack;
    public Material materialScene_360;

    public Transform rightController;     // El controlador derecho XR
    private GameObject cameraPivot;         // La cámara montada fija (su transform)
    public float sensitivityY = 100f;      // Sensibilidad vertical (mirar arriba/abajo)
    public float sensitivityZ = 100f;      // Sensibilidad horizontal (mirar a los lados)

    private Vector3 lastControllerPosition;

    public GameObject XRRig;


    private bool embodiment = false;
    private int secondsOption = 0;
    private string scene = "";
    private bool start = false;

    private const string labMenuStr = "LabMenu";

    private GameObject labMenu;
    private Vector3 labMenuPosition;

    private Scene secundaryScene;

    private VideoPlayer videoPlayer;

    private Quaternion originalRotation;

    public enum SceneNames
    {
        PositiveAnimal_forLab,
        NegativeScene_forLab,
        Lab_forLab,
        ThreeSixtyVideo
    }

    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        if (XRRig)
        {
            Debug.LogWarning("there is no XRRig set");
        }

        if (myRenderer)
        {
            Debug.LogWarning("there is no myRenderer set");
        }

        if (materialScene_0)
        {
            Debug.LogWarning("there is no materialScene_0 set");
        }

        if (materialScene_1)
        {
            Debug.LogWarning("there is no materialScene_1 set");
        }

        if (materialScene_2)
        {
            Debug.LogWarning("there is no materialScene_2 set");
        }

        if (rightController == null)
        {
            Debug.LogWarning("there is no rightController set");
        }
        else
        {
            lastControllerPosition = rightController.position;
        }

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

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished playing!");
        ActivateLabMenu();
        myRenderer.material = materialBlack;
    }

    private void Update()
    {
        MoveCameraByTiltNew();
    }

    private void MoveCamera()
    {

        if (rightController == null || cameraPivot == null)
            return;

        Vector3 currentPos = rightController.position;
        Vector3 delta = currentPos - lastControllerPosition;

        // Movimiento en Y → rotación en X (pitch)
        float pitch = -delta.y * sensitivityY * Time.deltaTime;

        // Movimiento en Z → rotación en Y (yaw)
        float yaw = delta.z * sensitivityZ * Time.deltaTime;

        Transform previous = cameraPivot.transform;

        Debug.Log($"currentPos {currentPos} - lastControllerPosition {lastControllerPosition} - delta {delta} - pitch {pitch} - yaw {yaw}");


        // Aplica rotación acumulativa
        cameraPivot.transform.Rotate(pitch, yaw, 0f, Space.Self);

        if (previous != cameraPivot.transform)
        {
            Debug.Log("la posición ha cambiado");
        }

        lastControllerPosition = currentPos;
    }

    private void MoveCameraByTilt() // this was the previous working
    {
        if (rightController == null || cameraPivot == null)
            return;

        // Obtenemos la rotación actual del control
        Quaternion controllerRotation = rightController.rotation;

        // Convertimos la rotación a Euler para trabajar con pitch y yaw
        Vector3 euler = controllerRotation.eulerAngles;

        // Si quieres, puedes ajustar los valores para que estén centrados en cero
        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);

        // Aplicamos sensibilidad (puedes ajustar los factores)
        float adjustedPitch = pitch * sensitivityY;
        float adjustedYaw = yaw * sensitivityZ;

        // Solo aplicamos rotación en los ejes deseados (X = pitch, Y = yaw)
        cameraPivot.transform.localRotation = Quaternion.Euler(adjustedPitch, adjustedYaw, 0f);
    }

    private void MoveCameraByTiltNew()
    {
        if (rightController == null || cameraPivot == null)
            return;

        // Obtenemos la rotación actual del control
        Quaternion controllerRotation = rightController.rotation;

        // Convertimos la rotación a Euler para trabajar con pitch y yaw
        Vector3 euler = controllerRotation.eulerAngles;

        // Centramos los ángulos para que oscilen alrededor de 0
        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);

        // Aplicamos sensibilidad
        float adjustedPitch = pitch * sensitivityY;
        float adjustedYaw = yaw * sensitivityZ;

        // Creamos una rotación de compensación a partir del movimiento del control
        Quaternion tiltOffset = Quaternion.Euler(adjustedPitch, adjustedYaw, 0f);

        // Aplicamos la rotación relativa a la original
        cameraPivot.transform.localRotation = originalRotation * tiltOffset;
    }

    private float NormalizeAngle(float angle)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return angle;
    }

    void OnSceneUnloaded(Scene inScene)
    {
        if (inScene.name != scene)
        {
            Debug.Log($"unloaded scene is not the same inScene.name {inScene.name} scene {scene}");
        }

        cameraPivot = null;

        ActivateLabMenu();
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
            SetSceneMaterial(stimuliOptions.value);

            DeactivateLabMenu();
            StartCoroutine(LoadSceneAndCall(scene));
        }
    }

    private void Load360Video()
    {
        SetScene(3);
        SetSceneMaterial(3);

        DeactivateLabMenu();
        StartCoroutine(Load360SceneAndCall(scene));
    }

    private void LoadFlatVideo()
    {
        // Step 1: Create a unique RenderTexture
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);

        // Step 2: Create a new material (Unlit so lighting doesn't affect it)
        Material screenMaterial = new Material(Shader.Find("Unlit/Texture"));
        screenMaterial.mainTexture = renderTexture;

        // Step 3: Assign the material to the screen
        myRenderer.material = screenMaterial;

        // Create or reuse an AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Reset video player audio configuration
        videoPlayer.Stop(); // stop before modifying
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
        videoPlayer.EnableAudioTrack(0, true); // assuming 1 track

        // Optional: Reset time/frame in case
        videoPlayer.frame = 0;


        // Step 4: Add and configure the VideoPlayer
        VideoClip clip = GetVideoClip();
        if (clip == null)
        {
            Debug.LogWarning("could not load clip");
            return;
        }
        videoPlayer.clip = clip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Or AudioSource if needed
        DeactivateLabMenu();
        videoPlayer.Play();
    }

    // name of video : "Assets/Videos/{{name}}-{{time}}.mp4"
    // e.g. : Assets/Videos/spanglerLawn-5seconds.mp4
    // Assets/Videos/spanglerLawn-5seconds.mp4
    private VideoClip GetVideoClip()
    {
        Dictionary<int, string> name = new Dictionary<int, string>();
        name.Add(0, "1");
        name.Add(1, "2");
        name.Add(2, "3");
        name.Add(3, "4");
        name.Add(4, "5");

        Dictionary<int, string> time = new Dictionary<int, string>();
        time.Add(0, "A");
        time.Add(1, "B");

        string videoPath = "Videos/" + name[stimuliOptions.value] + "-" + time[secondsOptions.value];

        Debug.Log($"video to load {videoPath}");
        VideoClip clip = Resources.Load<VideoClip>(videoPath);
        return clip;
    }

    public void SetSceneMaterial(int option)
    {
        switch (option)
        {
            case 0:
                myRenderer.material = materialScene_0;
                break;
            case 1:
                myRenderer.material = materialScene_1;
                break;
            case 2:
                myRenderer.material = materialScene_2;
                break;
            case 3:
                myRenderer.material = materialScene_360;
                break;
            default:
                myRenderer.material = materialScene_0;
                break;
        }

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

        labMenuPosition = labMenu.transform.position;

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

        labMenu.transform.position = labMenuPosition;
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


        foreach (GameObject rootObj in secundaryScene.GetRootGameObjects())
        {
            cameraPivot = FindWithTagRecursive(rootObj.transform, "RenderCamera");
            if (cameraPivot != null)
            {
                originalRotation = cameraPivot.transform.localRotation;
                Debug.Log("Found RenderCamera by tag");
                break;
            }
        }


        // Buscar el script SceneManager en esa escena
        foreach (GameObject rootObj in secundaryScene.GetRootGameObjects())
        {
            GameObject sceneManager_Obj = FindWithTagRecursive(rootObj.transform, "SceneManager");
            if (sceneManager_Obj.TryGetComponent<MySceneManager>(out var sceneManager))
            {
                Debug.Log("encontro sceneManager");
                sceneManager.StartsWith(embodiment, secondsOption, true); // Llamar a tu método
                break;
            }
        }
    }

    IEnumerator Load360SceneAndCall(string scene)
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


        foreach (GameObject rootObj in secundaryScene.GetRootGameObjects())
        {
            cameraPivot = FindWithTagRecursive(rootObj.transform, "RenderCamera");
            if (cameraPivot != null)
            {
                originalRotation = cameraPivot.transform.localRotation;
                Debug.Log("Found RenderCamera by tag");
                break;
            }
        }

        foreach (GameObject rootObj in secundaryScene.GetRootGameObjects())
        {
            GameObject which_Obj = FindWithTagRecursive(rootObj.transform, "SceneManager");
            if (which_Obj != null && which_Obj.TryGetComponent<WhichVideoToPlay>(out var whichVideoToPlay))
            {
                Debug.Log("encontro sceneManager");
                whichVideoToPlay.PlayVideo(stimuliOptions.value, secondsOptions.value); // Llamar a tu método
                break;
            }
        }
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
