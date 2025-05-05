
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum SecondsAllowedDogs
{
    Five = 5,
    Thirty = 30
}


// script that deactivate gameObjects and activate them when it is necessary
// the idea is to add a button to start the simulation, or a counter with black background
// it also verifies if the scene was loaded as additive, if it is it should deactivate the xr rig
public class SceneManagerDogs : MonoBehaviour
{
    // Instance for singleton. It might not be used. Consider to delete it.
    public static SceneManager Instance { get; private set; }

    [Header("Scene Settings")]
    // embodiment indicates whether we will have a body/bench to sit in the scene, and deactivate the gameObjects if the value is false.
    public bool embodiment = true;
    // seconds indicates the amount of seconds that scene will be running.
    public SecondsAllowedDogs seconds = SecondsAllowedDogs.Five;


    [Header("Start Scene")]
    // start indicates whether the scene start
    public bool start = false;
    // targetTag indicats the tag of the gameObjects to deactivate until start is true
    public string[] targetTags;
    // startButton button to start. But it might change to a regresive count
    public Button startButton; // TODO : pending to add Start, and selection of time options

    private bool isBlackoutTriggered = false;
    private bool isActivated = false;

    private List<GameObject> targetObjects = new List<GameObject>();

    private Color originalBackgroundColor;

    void Awake()
    {

        //// deactivate tags until start is set to true
        //DeactivateTags();

        //// sets the screen in black
        //Blackout();

        // Deactivate XROrigin in case the scene is Additive
        if (IsAdittiveScene())
        {
            GameObject viveRig = FindViveXRRig();

            if (viveRig != null)
            {
                viveRig.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No XR Rig found in scene.");
            }
        }

        // Deactivate body
        if (!embodiment)
        {
            DeactivateBody();
        }

        // sets gameObject to initial position
        // THIS DOES NOT APPLY TO DOGS
        //if (seconds == SecondsAllowedDogs.Five)
        //{
        //    foreach (GameObject obj in targetObjects)
        //    {
        //        // TODO : this should only be done for spider, and not moving the sounds
        //        obj.transform.position = new Vector3(0, obj.transform.position.y, 0);
        //    }
        //}
    }

    void Start()
    { 
        
        

    }

    private void Update()
    {
        if (start && !isActivated) {
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

    // isAdittiveScene returns true if the scene is loaded as an Additive Scene
    // or returns false if it is loaded as the Main Scene
    bool IsAdittiveScene()
    {
        Scene myScene = gameObject.scene;
        Scene activeScene = SceneManager.GetActiveScene();

        return !(myScene == activeScene);
    }

    void DeactivateBody() {

        string tag = "Body";

        GameObject[] bodies = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject body in bodies)
        {
            body.SetActive(false);
        }

    }

    void DeactivateTags() {
        foreach (string tag in targetTags)
        {
            // Find and initially deactivate the target GameObjects
            GameObject[] tmp_targetObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in tmp_targetObjects)
            {
                obj.SetActive(false);
                targetObjects.Add(obj);
            }
        }
    }

    void ActivateTags() {
        if (targetObjects.Count == 0) {
            Debug.LogError("there are no objects in 'targetObjects'");
        }

        foreach (GameObject obj in targetObjects) {
            obj.SetActive(true);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
        {
            Debug.Log($"Scene {scene.name} was loaded additively.");
        }
        else
        {
            Debug.Log($"Scene {scene.name} was loaded normally (Single).");
        }
    }

    // FindXROriginInScene find the XROrigin in the scene
    private GameObject FindViveXRRig()
    {
        Scene scene = gameObject.scene;

        string tag = "MainCamera";

        GameObject[] tmp_targetObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in tmp_targetObjects)
        {

            if (obj.scene == scene)
            {
                return obj;
            }
        }

        return null; // XR Rig not found
    }

    // BlackoutAfterDelayCoroutine displays a black screen after N amount of seconds
    private IEnumerator BlackoutAfterDelayCoroutine()
    {
        // Wait for 5 seconds
        yield return new WaitForSeconds((float)seconds);
        DeactivateTags();
        Blackout();

    }

    void Blackout() {
        // Disable all renderers in the scene
        foreach (var renderer in FindObjectsByType<Renderer>(UnityEngine.FindObjectsSortMode.None))
        {
            renderer.enabled = false;
        }

        originalBackgroundColor = Camera.main.backgroundColor;
        // Set the camera background color to black
        Camera.main.backgroundColor = Color.black;
    }

    void RestoreBlackout() {
        // Disable all renderers in the scene
        foreach (var renderer in FindObjectsByType<Renderer>(UnityEngine.FindObjectsSortMode.None))
        {
            renderer.enabled = true;
        }

        //// Set the camera background color to black
        Camera.main.backgroundColor = originalBackgroundColor;
    }
}
