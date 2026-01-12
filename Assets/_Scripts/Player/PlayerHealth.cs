using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO _characterStats;
    
    private PlayerController _playerController;

    public float _currentHealth;
    public float _currentArmorHealth;
    public bool _isDead = false;


    private void Awake()
    {
        _currentHealth = _characterStats.health;
        _currentArmorHealth = 0;
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        UIGameManager.instance.UpdateUIArmorHealth(_currentArmorHealth, this);
        UIGameManager.instance.UpdateUIPlayerHealth(_currentHealth, this);
    }

    public void UpdateHealth(float damage, ItemType itemType)
    {
        if (_isDead) return;

        if (_currentArmorHealth > 0)
        {
            _currentArmorHealth -= damage;
            _currentArmorHealth = Mathf.Clamp(_currentArmorHealth, 0, _characterStats.health);
            UIGameManager.instance.UpdateUIArmorHealth(_currentArmorHealth, this);
        }
        else
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _characterStats.health);
            UIGameManager.instance.UpdateUIPlayerHealth(_currentHealth, this);
        }
            
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

            _isDead = true;
        }
        else
        {
            _playerController._lifeState = LifeState.Hit;
        }
   
        
    }

    public void ResetHealth()
    {
        _currentHealth = _characterStats.health;
        _isDead = false;
        UIGameManager.instance.UpdateUIPlayerHealth(_currentHealth, this);
    }
}
