using UnityEngine.Video;
using UnityEngine;

public class LaptopScreen : MonoBehaviour
{
    public Renderer screenRenderer;      // Assign this in the Inspector
    public VideoClip videoToPlay;        // Assign a unique video for each laptop

    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;

    void Start()
    {
        // Step 1: Create a unique RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);

        // Step 2: Create a new material (Unlit so lighting doesn't affect it)
        Material screenMaterial = new Material(Shader.Find("Unlit/Texture"));
        screenMaterial.mainTexture = renderTexture;

        // Step 3: Assign the material to the screen
        screenRenderer.material = screenMaterial;

        // Step 4: Add and configure the VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.clip = videoToPlay;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Or AudioSource if needed
        videoPlayer.Play();
    }
}
