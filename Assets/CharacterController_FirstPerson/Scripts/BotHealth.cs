using UnityEngine;
using UnityEngine.AI;

public class BotHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public float destroyDelay = 5f;

    private NavMeshAgent _agent;
    private Bot _bot;
    private Collider _collider;
    private Animator _animator;

    public bool IsDead { get; private set; }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _bot = GetComponent<Bot>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        if (_animator != null)
        {
            _animator.SetTrigger("Hit");
        }
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;

        if (_bot != null) _bot.enabled = false;
        if (_agent != null && _agent.enabled) _agent.isStopped = true;
        if (_collider != null) _collider.enabled = false;

        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

        Destroy(gameObject, destroyDelay);
    }
}