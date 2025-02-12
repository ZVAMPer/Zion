using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Bank")]
    public AudioBank audioBank; // assign in Inspector
    // Store the SoundDefinition by key so we can retrieve multiple clip variations
    private Dictionary<string, SoundDefinition> soundDictionary = new Dictionary<string, SoundDefinition>();

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup gunGroup;
    public AudioMixerGroup playerStepsGroup;
    public AudioMixerGroup explosionsGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup voiceGroup;
    public AudioMixerGroup ambientGroup;

    [Header("Audio Sources (Non-Spatial)")]
    public AudioSource musicSource;
    public AudioSource voiceSource;
    public AudioSource ambientSource;
    // For local playback (non-spatial) of own sounds
    public AudioSource localGunSource;
    public AudioSource localPlayerStepsSource;

    [Header("3D Audio Prefab (Spatial)")]
    public GameObject spatialAudioPrefab; // Should have an AudioSource with spatialBlend = 1

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configure non-spatial sources
        if (musicSource) musicSource.outputAudioMixerGroup = musicGroup;
        if (voiceSource) voiceSource.outputAudioMixerGroup = voiceGroup;
        if (localGunSource) localGunSource.outputAudioMixerGroup = gunGroup;
        if (localPlayerStepsSource) localPlayerStepsSource.outputAudioMixerGroup = playerStepsGroup;
        if (ambientSource) ambientSource.outputAudioMixerGroup = ambientGroup;

        // Load the AudioBank sounds into a dictionary for quick lookup
        if (audioBank != null)
        {
            foreach (SoundDefinition s in audioBank.sounds)
            {
                if (!soundDictionary.ContainsKey(s.key))
                    soundDictionary.Add(s.key, s);
            }
        }
    }

    void Start()
    {
        // Play background music
        PlayMusic("FinalsMusic");
    }

    // Utility method for retrieving a default AudioClip (first clip) for a key
    public AudioClip GetClip(string key)
    {
        if (soundDictionary.TryGetValue(key, out SoundDefinition soundDef))
        {
            if (soundDef.clips != null && soundDef.clips.Length > 0)
                return soundDef.clips[0];
        }
        return null;
    }

    // Returns a random AudioClip variation from AudioBank for the specified key.
    public AudioClip GetRandomClip(string key)
    {
        if (soundDictionary.TryGetValue(key, out SoundDefinition soundDef))
        {
            return soundDef.GetRandomClip();
        }
        return null;
    }

    // Play a music track
    public void PlayMusic(string key)
    {
        AudioClip clip = GetClip(key);
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    // Local (non-spatial) gun sound for self
    public void PlayGunSFXLocal(string key)
    {
        AudioClip clip = GetClip(key);
        if (clip != null && localGunSource != null)
        {
            localGunSource.PlayOneShot(clip);
        }
    }

    // Remote (spatial) gun sound for other players
    public void PlayGunSFXRemote(string key, Vector3 position)
    {
        AudioClip clip = GetClip(key);
        if (clip != null)
        {
            SpawnSpatialAudio(clip, gunGroup, position);
        }
    }

    public void PlayPlayerDieSFXLocal(string key)
    {
        AudioClip clip = GetClip(key);
        if (clip != null)
        {
            // You can add pitch variation here if desired.
            AudioSource localSource = localGunSource;
            localSource.pitch = Random.Range(0.95f, 1.05f);
            localSource.PlayOneShot(clip);
        }
    }

    // Local (non-spatial) footstep sound for self
    public void PlayPlayerStepsSFXLocal(string key)
    {
        // Get a random variant from the audio bank
        AudioClip clip = GetRandomClip(key);
        if (clip != null && localPlayerStepsSource != null)
        {
            // Apply slight random pitch variation for natural variety.
            localPlayerStepsSource.pitch = Random.Range(0.95f, 1.05f);
            localPlayerStepsSource.PlayOneShot(clip);
        }
    }

    // Remote (spatial) footstep sound for other players
    public void PlayPlayerStepsSFXRemote(string key, Vector3 position)
    {
        // Get a random variant from the audio bank
        AudioClip clip = GetRandomClip(key);
        if (clip != null)
        {
            SpawnSpatialAudio(clip, playerStepsGroup, position);
        }
    }

    // Instantiates a spatial audio source for remote playback
    private void SpawnSpatialAudio(AudioClip clip, AudioMixerGroup group, Vector3 position)
    {
        GameObject go = Instantiate(spatialAudioPrefab, position, Quaternion.identity);
        AudioSource source = go.GetComponent<AudioSource>();
        if (source != null)
        {
            Debug.Log("Spawning spatial audio at " + position);
            source.outputAudioMixerGroup = group;
            source.spatialBlend = 1.0f;
            // Randomize pitch for variety.
            source.pitch = Random.Range(0.95f, 1.05f);
            source.clip = clip;
            source.Play();
            Destroy(go, clip.length);
        }
        else
        {
            Destroy(go);
        }
    }
}