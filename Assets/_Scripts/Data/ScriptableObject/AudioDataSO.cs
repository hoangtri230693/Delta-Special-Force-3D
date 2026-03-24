using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataSO", menuName = "Scriptable Objects/AudioDataSO")]
public class AudioDataSO : ScriptableObject
{
    [Header("Music Clips")]
    public AudioClip[] musicTracks;
    public AudioClip musicBackground;

    [Header("Lore Clips")]
    public AudioClip loreClips;

    [Header("SFX Clips")]
    public AudioClip[] sfxClips;

    [Header("Radio - TeamDeathmatch")]
    public RadioClips teamRadio;

    [Header("Radio - ZombieSurvival")]
    public RadioClips zombieRadio;
}

[System.Serializable]
public struct RadioClips
{
    public AudioClip[] ready;
    public AudioClip[] start;
    public AudioClip[] win;
    public AudioClip[] lose;
}
