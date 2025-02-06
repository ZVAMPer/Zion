using UnityEngine;

[System.Serializable]
public class SoundDefinition
{
    public string key;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "AudioBank", menuName = "Audio/AudioBank")]
public class AudioBank : ScriptableObject
{
    public SoundDefinition[] sounds;
    
    // Optional: Utility method to lookup a clip by key
    public AudioClip GetClip(string key)
    {
        foreach (SoundDefinition s in sounds)
        {
            if (s.key == key)
                return s.clip;
        }
        return null;
    }
}