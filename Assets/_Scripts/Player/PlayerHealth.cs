using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private PlayerController _playerController;
    private BotAIController _botAIController;

    public float _currentHealth;
    public float _currentArmorHealth;


    private void Awake()
    {     
        _playerController = GetComponent<PlayerController>();
        _botAIController = GetComponent<BotAIController>();
        GetHealth();

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

    private void GetHealth()
    {
        _currentHealth = _playerController.CharacterStas.health;
        _currentArmorHealth = 0;
    }

    public void UpdateHealth(float damage, ItemType itemType)
    {
        if (_currentHealth <= 0) return;

        if (_currentArmorHealth > 0)
        {
            if (damage <= _currentArmorHealth)
            {
                _currentArmorHealth -= damage;
                damage = 0;
            }
            else
            {
                damage -= _currentArmorHealth;
                _currentArmorHealth = 0;
            }

            UIGameManager_TeamDeathmatch.instance?.UpdateUIArmorHealth(_currentArmorHealth, this);
            UIGameManager_ZombieSurvival.instance?.UpdateUIArmorHealth(_currentArmorHealth, this);
        }

        if (damage > 0)
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _playerController.CharacterStas.health);

            UIGameManager_TeamDeathmatch.instance?.UpdateUIPlayerHealth(_currentHealth, this);
            UIGameManager_ZombieSurvival.instance?.UpdateUIPlayerHealth(_currentHealth, this);
        }

        if (_currentHealth <= _playerController.CharacterStas.health / 2)
        {
            if (_botAIController != null) _botAIController.SetShouldDefend(true);
        }

        UpdateLifeState(itemType);
    }

    private void UpdateLifeState(ItemType itemType)
    {
        if (_currentHealth <= 0)
        {
            switch (itemType)
            {
                case ItemType.PrimaryItem:
                case ItemType.SecondaryItem:
                    _playerController._lifeState = LifeState.DeathShoot;
                    break;
                case ItemType.ThrowItem:
                    _playerController._lifeState = LifeState.DeathThrow;
                    break;
                case ItemType.MeleeItem:
                case ItemType.None:
                default:
                    _playerController._lifeState = LifeState.DeathMelee;
                    break;
            }
        }
        else
        {
            if (_playerController != null) _playerController._lifeState = LifeState.Hurt;
        }
    }

    public void ResetHealth()
    {
        _currentHealth = _playerController.CharacterStas.health;
        UIGameManager_TeamDeathmatch.instance?.UpdateUIPlayerHealth(_currentHealth, this);
        UIGameManager_ZombieSurvival.instance?.UpdateUIPlayerHealth(_currentHealth, this);
    }
}
