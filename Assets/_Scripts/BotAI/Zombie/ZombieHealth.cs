using UnityEngine;


public class ZombieHealth : MonoBehaviour
{
    private ZombieController _zombieController;

    public float _currentHealth;

    private void Awake()
    {
        _zombieController = GetComponent<ZombieController>();
        GetHealth();
    }

    private void GetHealth()
    {
        _currentHealth = _zombieController.ZombieStats.health;
    }

    public void UpdateHealth(float damage)
    {
        if (_currentHealth <= 0) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _zombieController.ZombieStats.health);
        _zombieController.HandleHurt();

        if (_currentHealth <= 0)
            _zombieController.HandleDeath();
    }
}
