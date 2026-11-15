using UnityEngine;

public class Ball_Behaviour : MonoBehaviour
{
    public float damage = 5f;
    public bool isPlayerBullet;

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        // prüfen ob Bot getroffen wurde
        if (hitObject.TryGetComponent<BotHealth>(out BotHealth bot))
        {
            bot.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // prüfen ob player getroffen wurde
        if (hitObject.TryGetComponent<PlayerHealth>(out PlayerHealth player))
        {
            if (!isPlayerBullet)
            {
                player.TakeDamage(damage);
                Destroy(gameObject);
            }
            return;
        }
    }
}