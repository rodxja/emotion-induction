using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class UnloadSceneOnVideoEnd : MonoBehaviour
{
    public VideoPlayer videoPlayer;     // Assign in Inspector or dynamically

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("VideoPlayer not assigned or found.");
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Unsubscribe to avoid duplicate events
        vp.loopPointReached -= OnVideoEnd;

        UnloadMyScene();
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
}
