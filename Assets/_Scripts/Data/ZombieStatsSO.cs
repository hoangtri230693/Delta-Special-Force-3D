using UnityEngine;

[CreateAssetMenu(fileName = "ZombieStatsSO", menuName = "Scriptable Objects/ZombieStatsSO")]
public class ZombieStatsSO : ScriptableObject
{
    public float health = 100f;
    public float damage = 5f;
    public float runSpeed = 4f;
    public AudioClip[] footStepSounds;
    public AudioClip[] growlSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] hurtSounds;
    public AudioClip[] fallSounds;
}
