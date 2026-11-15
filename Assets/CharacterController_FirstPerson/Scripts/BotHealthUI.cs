using UnityEngine;
using UnityEngine.UI;

public class BotHealthUI : MonoBehaviour
{

    public BotHealth botHealth;
    public Image healthBarFill;
    public CanvasGroup canvasGroup; 
  
    public float smoothSpeed = 8f;
    public bool hideWhenFull = true;

    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;

        if (botHealth == null)
        {
            botHealth = GetComponentInParent<BotHealth>();
        }
    }

    void Update()
    {
        if (botHealth == null || healthBarFill == null) return;

        float targetFill = Mathf.Clamp01(botHealth.currentHealth / botHealth.maxHealth);
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);

        // einblenden der group wenn der bot nicht full hp ist /tot ist
        if (canvasGroup != null)
        {
            bool shouldShow = !botHealth.IsDead && (!hideWhenFull || botHealth.currentHealth < botHealth.maxHealth);
            float targetAlpha = shouldShow ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * 10f);
        }
    }

    // LateUpdate, dass der canvas zur spieler cam schaut, NACHDEM diese rotiert ist
    void LateUpdate()
    {
        if (_mainCamera != null)
        {
            transform.rotation = _mainCamera.transform.rotation;
        }
    }
}