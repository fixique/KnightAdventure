using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyHitEventArgs : EventArgs { 
    public float currentHealth { get; set; }
    public float maxHealth { get; set; }
}

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(EnemyAI))]
public class EnemyEntity : MonoBehaviour
{

    public event EventHandler<EnemyHitEventArgs> OnTakeHit;
    public event EventHandler OnDeath;

    [SerializeField] private EnemySO enemySO;
    [SerializeField] private PolygonCollider2D attackHitbox;
    private int currentHealth;
    private int maxHealth;

    private BoxCollider2D boxCollider2D;
    private EnemyAI enemyAi;

    private void Awake() {
        boxCollider2D = GetComponent<BoxCollider2D>();
        enemyAi = GetComponent<EnemyAI>();
    }

    private void Start() {
        currentHealth = enemySO.enemyHealth;
        maxHealth = enemySO.enemyHealth;
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        EnemyHitEventArgs args = new EnemyHitEventArgs { 
            currentHealth = currentHealth,
            maxHealth = maxHealth,
        };

        OnTakeHit?.Invoke(this, args);
        DetectDeath();
    }

    public void PolygonColliderTurnOff() {
        attackHitbox.enabled = false;
    }

    public void PolygonColliderTurnOn() {
        attackHitbox.enabled = true;
    }

    private void DetectDeath() {
        if (currentHealth <= 0) {
            boxCollider2D.enabled = false;
            attackHitbox.enabled = false;
            enemyAi.SetDeathState();
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }
}
