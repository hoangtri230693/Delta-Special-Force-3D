using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO _characterStats;
    
    private PlayerController _playerController;

    public float _currentHealth;

    private void Awake()
    {
        _currentHealth = _characterStats.health;
        _playerController = GetComponent<PlayerController>();
    }

    public void UpdateHealth(float damage, ItemType itemType)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _characterStats.health);

        if (_currentHealth <= _characterStats.health / 2)
        {
            _playerController._shouldDefend = true;
        }

        if (_currentHealth <= 0)
        {
            if (itemType == ItemType.PrimaryItem || itemType == ItemType.SecondaryItem)
            {
                _playerController._lifeState = LifeState.DeathShoot;
            }
            if (itemType == ItemType.MeleeItem)
            {
                _playerController._lifeState = LifeState.DeathMelee;
            }
            if (itemType == ItemType.ThrowItem)
            {
                _playerController._lifeState = LifeState.DeathThrow;
            }
        }
        else
        {
            _playerController._lifeState = LifeState.Hit;
        }
    }
}
