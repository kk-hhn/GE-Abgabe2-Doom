using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject respawnPanel;
    void Start()
    {
        // erstmal deaktivieren
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(false);
        }
    }
    public void ShowDeathScreen()
    {
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(true);
        }

        // maus freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // timestop
        Time.timeScale = 0f;
    }

    public void RestartScene()
    {
        // timestop aus
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}