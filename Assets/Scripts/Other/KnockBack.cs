using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBack : MonoBehaviour
{
    [SerializeField] private float knockBackForce;
    [SerializeField] private float knockBackMovingTimerMax;

    private float knockBackMovingTimer;

    private Rigidbody2D rb;

    public bool IsGettingKnockedBack { get; private set; }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        knockBackMovingTimer -= Time.deltaTime;
        if (knockBackMovingTimer < 0) {
            StopKnockBackMovment();
        }
    }

    public void GetKnockBackMovment(Transform damageSource) {
        IsGettingKnockedBack = true;
        knockBackMovingTimer = knockBackMovingTimerMax;
        Vector2 difference = (transform.position - damageSource.position).normalized * knockBackForce / rb.mass;
        rb.AddForce(difference, ForceMode2D.Impulse);
    }

    public void StopKnockBackMovment() {
        rb.linearVelocity = Vector3.zero;
        IsGettingKnockedBack = false;
    }
}
