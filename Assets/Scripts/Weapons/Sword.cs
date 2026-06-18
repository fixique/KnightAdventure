using System;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;

    private PolygonCollider2D polygonCollider;

    public event EventHandler OnSwordSwing;

    private void Awake() {
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start() {
        polygonCollider.enabled = false;
    }

    public void Attack() {
        AttackColliderTurnOffOn();
        OnSwordSwing?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.TryGetComponent(out EnemyEntity enemyEntity)) {
            enemyEntity.TakeDamage(damageAmount);
        }
    }

    public void AttackColliderTurnOff() {
        polygonCollider.enabled = false;
    }

    private void AttackColliderTurnOn() {
        polygonCollider.enabled = true;
    }

    private void AttackColliderTurnOffOn() {
        AttackColliderTurnOff();
        AttackColliderTurnOn();
    }
}
