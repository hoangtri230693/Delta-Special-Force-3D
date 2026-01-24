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
        if (UIGameManager_TeamDeathmatch.instance != null)
        {
            UIGameManager_TeamDeathmatch.instance.UpdateUIArmorHealth(_currentArmorHealth, this);
            UIGameManager_TeamDeathmatch.instance.UpdateUIPlayerHealth(_currentHealth, this);
        }
        
        if (UIGameManager_ZombieSurvival.instance != null)
        {
            UIGameManager_ZombieSurvival.instance.UpdateUIArmorHealth(_currentArmorHealth, this);
            UIGameManager_ZombieSurvival.instance.UpdateUIPlayerHealth(_currentHealth, this);
        }
    }

    public void UpdateHealth(float damage, ItemType itemType)
    {
        if (_isDead) return;

        if (_currentArmorHealth > 0)
        {
            _currentArmorHealth -= damage;
            _currentArmorHealth = Mathf.Clamp(_currentArmorHealth, 0, _characterStats.health);
            UIGameManager_TeamDeathmatch.instance?.UpdateUIArmorHealth(_currentArmorHealth, this);
            UIGameManager_ZombieSurvival.instance?.UpdateUIArmorHealth(_currentArmorHealth, this);
        }
        else
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _characterStats.health);
            UIGameManager_TeamDeathmatch.instance?.UpdateUIPlayerHealth(_currentHealth, this);
            UIGameManager_ZombieSurvival.instance?.UpdateUIPlayerHealth(_currentHealth, this);   
        }
            
        if (_currentHealth <= _characterStats.health / 2)
        {
            if (_playerController != null) _playerController._shouldDefend = true;
        }

        if (_currentHealth <= 0)
        {
            if (itemType == ItemType.PrimaryItem || itemType == ItemType.SecondaryItem)
            {
                if (_playerController != null) _playerController._lifeState = LifeState.DeathShoot;
            }
            if (itemType == ItemType.MeleeItem)
            {
                if (_playerController != null) _playerController._lifeState = LifeState.DeathMelee;
            }
            if (itemType == ItemType.ThrowItem)
            {
                if (_playerController != null) _playerController._lifeState = LifeState.DeathThrow;
            }

            _isDead = true;
        }
        else
        {
            if (_playerController != null) _playerController._lifeState = LifeState.Hit;
        }

        Debug.Log("Update Health: " + _currentHealth);
    }

    public void ResetHealth()
    {
        _currentHealth = _characterStats.health;
        _isDead = false;
        UIGameManager_TeamDeathmatch.instance?.UpdateUIPlayerHealth(_currentHealth, this);
        UIGameManager_ZombieSurvival.instance?.UpdateUIPlayerHealth(_currentHealth, this);
    }
}
