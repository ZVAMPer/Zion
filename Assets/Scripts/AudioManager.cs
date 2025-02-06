using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Bank")]
    public AudioBank audioBank; // assign in Inspector
    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();

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
        
        // Load the AudioBank clips into a dictionary for quick lookup
        if (audioBank != null)
        {
            foreach(SoundDefinition s in audioBank.sounds)
            {
                if(!clipDictionary.ContainsKey(s.key))
                    clipDictionary.Add(s.key, s.clip);
            }
        }

        
    }
    
    // Utility method for retrieving an AudioClip
    public AudioClip GetClip(string key)
    {
        if (clipDictionary.TryGetValue(key, out AudioClip clip))
            return clip;
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