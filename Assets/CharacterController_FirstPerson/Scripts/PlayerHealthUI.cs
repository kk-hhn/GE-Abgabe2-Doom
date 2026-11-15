using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;  // playerhealth script
    public Image healthBarFill;

    private float _targetFill;

    void Start()
    {
    }

    void Update()
    {
        if (playerHealth == null || healthBarFill == null) return;

        // verhältnis berechnen
        _targetFill = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);

        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, _targetFill, Time.deltaTime);
    }
}