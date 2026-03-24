using UnityEngine;

[CreateAssetMenu(fileName = "ZombieStatsSO", menuName = "Scriptable Objects/ZombieStatsSO")]
public class ZombieStatsSO : ScriptableObject
{
    public float health;
    public float damage;
    public float runSpeed;
    public AudioClip[] footStepSounds;
    public AudioClip[] growlSounds;
    public AudioClip[] attackSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] hurtSounds;
    public AudioClip[] fallSounds;
}
