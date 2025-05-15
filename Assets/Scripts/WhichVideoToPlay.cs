using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class WhichVideoToPlay : MonoBehaviour
{
    public VideoPlayer videoPlayer;     // Assign in Inspector or dynamically

    private Dictionary<int, string> name = new Dictionary<int, string>();
    private Dictionary<int, string> time = new Dictionary<int, string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        name.Add(0, "1");
        name.Add(1, "2");
        name.Add(2, "3");
        name.Add(3, "4");
        name.Add(4, "5");

        time.Add(0, "A");
        time.Add(1, "B");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayVideo(int videoOption, int secondsOption) {
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
        VideoClip clip = GetVideoClip(videoOption, secondsOption);
        if (clip == null)
        {
            Debug.LogWarning("could not load clip");
            return;
        }
        videoPlayer.clip = clip;
        

        videoPlayer.Play();
    }

    private VideoClip GetVideoClip(int videoOption, int secondsOption)
    {
        string videoPath = "Videos/" + name[videoOption] + "-" + time[secondsOption];

        Debug.Log($"video to load {videoPath}");
        VideoClip clip = Resources.Load<VideoClip>(videoPath);
        return clip;
    }
}
