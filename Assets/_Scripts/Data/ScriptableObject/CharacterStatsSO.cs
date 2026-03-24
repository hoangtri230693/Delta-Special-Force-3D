using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsSO", menuName = "Scriptable Objects/CharacterStatsSO")]
public class CharacterStatsSO : ScriptableObject
{
    public float health;
    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;
    public float rotationSpeed;
    public AudioClip[] footStepSound;
    public AudioClip landStepSound;
    public AudioClip switchItemSound;
    public AudioClip zoomSound;
    public AudioClip[] hurtSound;
    public AudioClip deathSound;
}
