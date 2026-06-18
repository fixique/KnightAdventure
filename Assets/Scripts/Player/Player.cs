using System;
using System.Collections;
using UnityEngine;

[SelectionBase]

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDeath;
    public event EventHandler OnFlashBlink;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float damageRecoveryTime = 0.5f;
    [Header("Dash Settings")]
    [SerializeField] private int dashSpeedMultiplier = 4;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float dashCooldownTime = 0.25f;
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private KnockBack knockBack;

    private float minMovingSpeed = 0.1f;
    private bool canTakeDamage;
    private int currentHealth;
    private bool isRunning = false;
    private Vector2 inputVector;
    private float initialMovingSpeed;
    private bool isDashing;

    public bool isAlive { get; private set; }

    private void Awake() {
        Instance = this;
        initialMovingSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        knockBack = GetComponent<KnockBack>();
    }

    private void Start() {
        currentHealth = maxHealth;
        canTakeDamage = true;
        isAlive = true;
        GameInput.Instance.OnPlayerAttack += Player_OnPlayerAttack;
        GameInput.Instance.OnPlayerDash += Player_OnPlayerDash;
    }

    private void FixedUpdate() {
        if (knockBack.IsGettingKnockedBack)
            return;

        HandleMovment();
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerAttack -= Player_OnPlayerAttack;
        GameInput.Instance.OnPlayerDash -= Player_OnPlayerDash;
    }

    public bool IsRunning() {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition() {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }

    public void TakeDamage(Transform damageSource, int damage) {
        if (canTakeDamage && isAlive) {
            canTakeDamage = false;
            currentHealth = Mathf.Max(0, currentHealth -= damage);
            knockBack.GetKnockBackMovment(damageSource);
            HudController.Instance.SetHealth(currentHealth, maxHealth);

            OnFlashBlink?.Invoke(this, EventArgs.Empty);

            StartCoroutine(DamageRecoveryRoutine());
        }
        DetectDeath();
    }

    private void DetectDeath() {
        if (currentHealth == 0) {
            isAlive = false;
            capsuleCollider.enabled = false;
            knockBack.StopKnockBackMovment();
            GameInput.Instance.DisableMovement();
            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerator DamageRecoveryRoutine() {
        yield return new WaitForSeconds(damageRecoveryTime);
        canTakeDamage = true;
    }

    private void Player_OnPlayerAttack(object sender, System.EventArgs e) {
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void Update() {
        inputVector = GameInput.Instance.GetMovmentVector();
    }

    private void HandleMovment() {
        
        rb.MovePosition(rb.position + inputVector * (moveSpeed * Time.fixedDeltaTime));

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed) {
            isRunning = true;
        } else {
            isRunning = false;
        }
    }

    private void Player_OnPlayerDash(object sender, EventArgs e) {
        Dash();
    }

    private void Dash() {
        if (!isDashing)
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine() {
        isDashing = true;
        moveSpeed *= dashSpeedMultiplier;
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashTime);
        trailRenderer.emitting = false;
        moveSpeed = initialMovingSpeed;

        yield return new WaitForSeconds(dashCooldownTime);
        isDashing = false;
    }
}
