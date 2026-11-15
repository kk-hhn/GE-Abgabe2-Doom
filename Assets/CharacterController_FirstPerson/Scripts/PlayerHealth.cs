using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public RespawnUI respawnUI;

    private FP_CharacterController _controller;

    public bool IsDead { get; private set; }

    void Start()
    {
        currentHealth = maxHealth;
        _controller = GetComponent<FP_CharacterController>();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth);
        Debug.Log($"player took dmg: {amount}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("player died");
        
        respawnUI.ShowDeathScreen();
    }
}