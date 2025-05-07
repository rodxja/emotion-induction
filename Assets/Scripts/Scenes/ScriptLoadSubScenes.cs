using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using static MySceneManager;
using UnityEditor.SearchService;
using Scene = UnityEngine.SceneManagement.Scene;

public class LoadSubScenes : MonoBehaviour
{
    public Toggle embodimentToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown secondsOptions; // Asigna el Dropdown desde el Inspector
    public TMPro.TMP_Dropdown scenesOptions; // Asigna el Dropdown desde el Inspector
    public Button startButton; // Asigna el bot�n desde el Inspector

    public Renderer myRenderer;

    public Material materialScene_0;
    public Material materialScene_1;

    public Transform rightController;     // El controlador derecho XR
    private GameObject cameraPivot;         // La cámara montada fija (su transform)
    public float sensitivityY = 100f;      // Sensibilidad vertical (mirar arriba/abajo)
    public float sensitivityZ = 100f;      // Sensibilidad horizontal (mirar a los lados)

    private Vector3 lastControllerPosition;


    private bool embodiment = false;
    private int secondsOption = 0;
    private string scene = "";
    private bool start = false;

    private const string labMenuStr = "LabMenu";

    private GameObject labMenu;

    private Scene secundaryScene;

    public enum SceneNames
    {
        PositiveAnimal,
        NegativeScene_2
    }

    void Start()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;

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

        if (rightController == null)
        {
            Debug.LogWarning("there is no rightController set");
        }
        else {
            lastControllerPosition = rightController.position;
        }

        if (embodimentToggle != null)
        {
            embodimentToggle.onValueChanged.AddListener(OnToggleChanged);
        }

        if (secondsOptions != null)
        {
            secondsOptions.onValueChanged.AddListener(OnSecondsChanged);
        }

        if (scenesOptions != null)
        {
            scenesOptions.onValueChanged.AddListener(OnScenesChanged);
        }


        if (startButton != null)
        {
            startButton.onClick.AddListener(OnButtonClick);
        }
    }

    private void Update()
    {
        MoveCameraByTilt();
    }

    private void MoveCamera() {

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

        if (previous != cameraPivot.transform) {
            Debug.Log("la posición ha cambiado");
        }

        lastControllerPosition = currentPos;
    }

    private void MoveCameraByTilt()
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
        embodiment = value;
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
                scene = SceneNames.PositiveAnimal.ToString();
                break;
            case 1:
                scene = SceneNames.NegativeScene_2.ToString();
                break;
            default:
                scene = SceneNames.PositiveAnimal.ToString();
                break;
        }

    }

    // this starts and loads the scene
    private void OnButtonClick()
    {
        SetScene(scenesOptions.value);
        SetSceneMaterial();

        DeactivateLabMenu();

        StartCoroutine(LoadSceneAndCall(scene));
    }

    public void SetSceneMaterial ()
    {
        switch (scenesOptions.value)
        {
            case 0:
                myRenderer.material = materialScene_0;
                break;
            case 1:
                myRenderer.material = materialScene_1;
                break;
            default:
                myRenderer.material = materialScene_0;
                break;
        }

    }


    void DeactivateLabMenu()
    {
        // if it is null then i need to get it, otherwise it is loaded on labMenu variable
        if (labMenu == null) {
            labMenu = GetLabMenu();
        }

        // if it is still null, there should not call SetActive
        if (labMenu == null) {
            UnityEngine.Debug.LogError("labMenu is null");
            return;
        }
        labMenu.SetActive(false);
    }

    private GameObject GetLabMenu() {
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
        if (labMenu == null) {
            UnityEngine.Debug.LogError("labMenu is null");
            return;
        }

        labMenu.SetActive(true);
    }


    IEnumerator LoadSceneAndCall(string scene)
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

        foreach (GameObject obj in secundaryScene.GetRootGameObjects())
        {
            if (obj.name == "RenderCamera") // Assuming your camera pivot is named "CameraRigPivot"
            {
                cameraPivot = obj;
                break;
            }
        }


        // Buscar el script SceneManager en esa escena
        foreach (GameObject obj in secundaryScene.GetRootGameObjects())
        {
            if (obj.TryGetComponent<MySceneManager>(out var sceneManager))
            {
                sceneManager.StartsWith(embodiment, secondsOption, true); // Llamar a tu método
                break;
            }
        }
    }

}
