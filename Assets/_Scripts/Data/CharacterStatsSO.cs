using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsSO", menuName = "Scriptable Objects/CharacterStatsSO")]
public class CharacterStatsSO : ScriptableObject
{
    public float health = 100f;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 5f;
    public AudioClip[] footStepSound;
    public AudioClip landStepSound;
    public AudioClip switchItemSound;
    public AudioClip zoomSound;
    public AudioClip[] hurtSound;
    public AudioClip deathSound;
}
