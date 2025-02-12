using UnityEngine;

[System.Serializable]
public class SoundDefinition
{
    public string key;
    // Either assign a single clip or multiple variations.
    public AudioClip[] clips;
    
    // Optional helper: if only one clip is assigned, return it.
    public AudioClip GetRandomClip()
    {
        if (clips != null && clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);
            return clips[index];
        }
        return null;
    }
}

[CreateAssetMenu(fileName = "AudioBank", menuName = "Audio/AudioBank")]
public class AudioBank : ScriptableObject
{
    public SoundDefinition[] sounds;
    
    // Returns a random clip for the given key
    public AudioClip GetRandomClip(string key)
    {
        foreach (SoundDefinition s in sounds)
        {
            if (s.key == key)
                return s.GetRandomClip();
        }
        return null;
    }
}