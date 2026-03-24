using DeltaSpecialForce3D.Enums;
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
    }

    private void Start()
    {
        GetHealth();
        UpdateUIArmorHealth();
        UpdateUIHealth();
    }

    private void GetHealth()
    {
        _currentHealth = _playerController.CharacterStats.health;
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

            UpdateUIArmorHealth();
        }

        if (damage > 0)
        {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _playerController.CharacterStats.health);

            UpdateUIHealth();
        }

        if (_currentHealth <= _playerController.CharacterStats.health / 2)
        {
            if (_botAIController != null) _botAIController.SetShouldDefend(true);
        }

        UpdateLifeState(itemType);
    }

    public void UpdateLifeState(ItemType itemType)
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

    public void UpdateUIHealth()
    {
        if (_botAIController != null) return;

        if (UIGameManager_TeamDeathmatch.instance != null)
            UIGameManager_TeamDeathmatch.instance.UpdateUIPlayerHealth(_currentHealth);
        if (UIGameManager_ZombieSurvival.instance != null)
            UIGameManager_ZombieSurvival.instance?.UpdateUIPlayerHealth(_currentHealth);

    }

    public void UpdateUIArmorHealth()
    {
        if (_botAIController != null) return;

        if (UIGameManager_TeamDeathmatch.instance != null)
            UIGameManager_TeamDeathmatch.instance.UpdateUIArmorHealth(_currentArmorHealth);
        if (UIGameManager_ZombieSurvival.instance != null)
            UIGameManager_ZombieSurvival.instance?.UpdateUIArmorHealth(_currentArmorHealth);
    }

    public void ResetHealth()
    {
        _currentHealth = _playerController.CharacterStats.health;
        UpdateUIHealth();
    }
}
