using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Sound sound)
    {
        audioSource.clip = sound.clip;
        //audioSource.outputAudioMixerGroup = sound.mixerGroup;
        audioSource.loop = sound.loop;
        audioSource.playOnAwake = sound.playOnAwake;
    }

    public void Play()
    {
        audioSource.Play();
    }
}
