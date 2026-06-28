using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private EnemySO enemySO;

    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.transform.TryGetComponent(out Player player)) {
            player.TakeDamage(transform, enemySO.enemyDamageAmount);
        }
    }
}
