using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadSubScenes : MonoBehaviour
{
    public Toggle embodimentToggle; // Asigna el Toggle desde el Inspector
    public TMPro.TMP_Dropdown secondsOptions; // Asigna el Dropdown desde el Inspector
    public TMPro.TMP_Dropdown scenesOptions; // Asigna el Dropdown desde el Inspector
    public Button startButton; // Asigna el bot�n desde el Inspector


    private bool embodiment = false;
    private int seconds = 30;
    private string scene = "";
    private bool start = false;


    public enum SceneNames
    {
        PositiveAnimal,
        NegativeScene_2
    }

    void Start()
    {
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

    // selects embodiment
    private void OnToggleChanged(bool value) {
        embodiment = value;
    }

    private void OnSecondsChanged(int value)
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
                seconds = 30;
                break;
        }
    }
    private void OnScenesChanged(int value)
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
        StartCoroutine(LoadSceneAndCall(scene));
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
        Scene loadedScene = SceneManager.GetSceneByName(scene);

        // Asegurarse de que esté activa (opcional, depende de tu diseño)
        // SceneManager.SetActiveScene(loadedScene);

        // Buscar el script SceneManager en esa escena
        foreach (GameObject obj in loadedScene.GetRootGameObjects())
        {
            if (obj.TryGetComponent<MySceneManager>(out var sceneManager))
            {
                sceneManager.StartsWith(embodiment, seconds); // Llamar a tu método
                break;
            }
        }
    }

}
