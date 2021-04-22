using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    // Static var
    public static AudioController aC;

    // Audio Sources
    public AudioSource musicSource;
    public AudioSource sFXSource;
    public AudioSource rainSource1;
    public AudioSource rainSource2;
    public AudioSource rainSource3;

    // Music Clips
    public AudioClip firstLevelMusic;
    public AudioClip secondLevelMusic;
    public AudioClip thirdLevelMusic;

    // Sound Effects
    public AudioClip bottlePickUp;
    public AudioClip[] boatCreak;
    public AudioClip[] hitHardDebris;
    public AudioClip[] hitEnemy;
    public AudioClip rainSoft;
    public AudioClip rainHard;
    public AudioClip rainThunder;
    public AudioClip[] thunderOneShot;
    public AudioClip playerDeath;
    public AudioClip openNotes;
    public AudioClip closeNotes;
    public AudioClip[] changeNotes;
    public AudioClip[] cutsceneBarks;

    void OnEnable()
    {
        if (aC == null) aC = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // Persist between scenes
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    // Default music for each scene
    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "2_Level1")
        {
            PlayMusic(firstLevelMusic, 0f);
            StartCoroutine(FadeAudioSource.StartFade(musicSource, 3f, 0.55f));
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    public void PlayMusic(AudioClip clip, float volume) // Plays clip on music audiosource
    {
        musicSource.volume = volume;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void Play(AudioSource audioSource, AudioClip[] audioClips, float volume) // Plays random clip from array on specified audiosource
    {
        AudioClip audioClip = GetRandomClip(audioClips);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public void Play(AudioSource audioSource, AudioClip audioClip, float volume) // Plays one shot clip on specified audiosource
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    public AudioSource PlayRandomSFXAtPoint(AudioClip[] audioClips, Vector3 position, float volume)
    {
        GameObject tempAudioClip = new GameObject("TempAudio");
        tempAudioClip.transform.position = position;
        AudioSource aSource = tempAudioClip.AddComponent<AudioSource>();
        AudioClip audioClip = GetRandomClip(audioClips);
        aSource.clip = audioClip;
        aSource.volume = volume;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.Play();
        Destroy(tempAudioClip, audioClip.length);
        return aSource;
    }

    public AudioSource PlaySFXAtPoint(AudioClip audioClip, Vector3 position, float volume)
    {
        GameObject tempAudioClip = new GameObject("TempAudio");
        tempAudioClip.transform.position = position;
        AudioSource aSource = tempAudioClip.AddComponent<AudioSource>();
        aSource.clip = audioClip;
        aSource.volume = volume;
        aSource.rolloffMode = AudioRolloffMode.Linear;
        aSource.Play();
        Destroy(tempAudioClip, audioClip.length);
        return aSource;
    }

    public AudioClip GetRandomClip(AudioClip[] audioClips) // Gets a random audioclip from an audioclip array
    {
        return audioClips[Random.Range(0, audioClips.Length)];
    }

    public void PauseActiveSources()
    {
        PauseIfActive(sFXSource);
        PauseIfActive(rainSource1);
        PauseIfActive(rainSource2);
        PauseIfActive(rainSource3);
    }

    private void PauseIfActive(AudioSource audioSource)
    {
        if (audioSource.isPlaying == true)
        {
            audioSource.Pause();
        }
    }

    public void UnPauseAudioSources()
    {
        sFXSource.UnPause();
        rainSource1.UnPause();
        rainSource2.UnPause();
        rainSource3.UnPause();
    }
}
