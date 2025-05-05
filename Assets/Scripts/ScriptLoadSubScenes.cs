using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSubScenes : MonoBehaviour
{
    public string[] scenesToLoad;

    void Start()
    {
        foreach (var scene in scenesToLoad)
        {
            if (!SceneManager.GetSceneByName(scene).isLoaded)
            {
                SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            }
        }
    }
}
