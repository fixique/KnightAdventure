using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(EnemyAI))]
public class EnemyEntity : MonoBehaviour
{

    public event EventHandler OnTakeHit;
    public event EventHandler OnDeath;

    [SerializeField] private EnemySO enemySO;
    private int currentHealth;

    private PolygonCollider2D polygonColider2D;
    private BoxCollider2D boxCollider2D;
    private EnemyAI enemyAi;

    private void Awake() {
        polygonColider2D = GetComponent<PolygonCollider2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        enemyAi = GetComponent<EnemyAI>();
    }

    private void Start() {
        currentHealth = enemySO.enemyHealth;
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.transform.TryGetComponent(out Player player)) {
            player.TakeDamage(transform, enemySO.enemyDamageAmount);
        }
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        OnTakeHit?.Invoke(this, EventArgs.Empty);
        DetectDeath();
    }

    public void PolygonColliderTurnOff() {
        polygonColider2D.enabled = false;
    }

    public void PolygonColliderTurnOn() {
        polygonColider2D.enabled = true;
    }

    private void DetectDeath() {
        if (currentHealth <= 0) {
            boxCollider2D.enabled = false;
            polygonColider2D.enabled = false;
            enemyAi.SetDeathState();
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }
}
