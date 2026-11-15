using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float radius = 5f;
    public float force = 20f;
    public LayerMask interactionMask;
    public GameObject exploPrefab;
    private Renderer rend;
    private Collider col;
    private Rigidbody bombRb;

    void Start()
    {
        Invoke(nameof(Explode), 1f);
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        bombRb = GetComponent<Rigidbody>();
    }

    void Explode()
    {
        Collider[] objs = Physics.OverlapSphere(transform.position, radius, interactionMask);
        foreach (Collider obj in objs)
        {
            if (obj.TryGetComponent<BotHealth>(out BotHealth bot))
            {
                bot.TakeDamage(25f);
            }

            if (obj.TryGetComponent<PlayerHealth>(out PlayerHealth player))
            {
                player.TakeDamage(25f);
            }
        }

        if (exploPrefab != null)
        {
            Instantiate(exploPrefab, transform.position, Quaternion.identity);
        }

        rend.enabled = false;
        col.enabled = false;
        bombRb.isKinematic = true;

        Invoke(nameof(End), 3f);
    }

    void End()
    {
        Destroy(gameObject);
    }
}