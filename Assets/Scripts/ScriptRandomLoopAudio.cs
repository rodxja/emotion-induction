using UnityEngine;
using System.Collections;

public class ScriptRandomLoopAudio : MonoBehaviour
{

    public AudioSource audioSource;
    public bool keepPlaying = true;
    public int minimunSeconds = 3;
    public int maximunSeconds = 10;
    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError($"current property 'audioSource' is null");
        }

        if (audioSource.clip == null)
        {
            Debug.LogError($"current property 'audioSource.clip' is null");
        }

        minimunSeconds += (int)audioSource.clip.length;
        maximunSeconds += (int)audioSource.clip.length;

        //Debug.Log($"new minimun : '{minimunSeconds}', new maximun : '{maximunSeconds}', current clip seconds : '{audioSource.clip.length}'");

        StartCoroutine(RandomLoop());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator RandomLoop() {
        while (keepPlaying)
        {
            audioSource.PlayOneShot(audioSource.clip);
            int seconds = Random.Range(minimunSeconds, maximunSeconds);
            //Debug.Log($"repeat in '{seconds}'");
            yield return new WaitForSeconds(seconds);
        }
    }
}
