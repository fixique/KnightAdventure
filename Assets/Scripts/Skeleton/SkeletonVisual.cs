using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SkeletonVisual : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyEntity enemyEntity;
    [SerializeField] private GameObject shadowObject;

    // Health UI
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject healthBarFill;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";
    private const string TAKE_HIT = "TakeHit";
    private const string CHASING_SPEED_MULTIPLIER = "ChasingSpeedMultiplier";
    private const string ATTACK = "Attack";
    private const string IS_DIE = "IsDie";

    #region Lifecycle

    private void Awake() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        enemyAI.OnEnemyAttack += EnemyAI_OnEnemyAttack;
        enemyEntity.OnTakeHit += EnemyEntity_OnTakeHit;
        enemyEntity.OnDeath += EnemyEntity_OnDeath;
    }

    private void OnDestroy() {
        enemyAI.OnEnemyAttack -= EnemyAI_OnEnemyAttack;
        enemyEntity.OnTakeHit -= EnemyEntity_OnTakeHit;
        enemyEntity.OnDeath -= EnemyEntity_OnDeath;
    }

    private void Update() {
        animator.SetBool(IS_RUNNING, enemyAI.IsRunning);
        animator.SetFloat(CHASING_SPEED_MULTIPLIER, enemyAI.GetRoamingAnimationSpeed());
    }

    #endregion

    #region Public

    public void TriggerAttackAnimationTurnOff() {
        enemyEntity.PolygonColliderTurnOff();
    }

    public void TriggerAttackAnimationTurnOn() {
        enemyEntity.PolygonColliderTurnOn();
    }

    #endregion

    #region Animation

    private void EnemyAI_OnEnemyAttack(object sender, System.EventArgs e) {
        animator.SetTrigger(ATTACK);
    }

    private void EnemyEntity_OnTakeHit(object sender, EnemyHitEventArgs e) {
        SetHealth(e.currentHealth, e.maxHealth);
        animator.SetTrigger(TAKE_HIT);
    }

    private void EnemyEntity_OnDeath(object sender, EventArgs e) {
        animator.SetBool(IS_DIE, true);
        spriteRenderer.sortingOrder = -1;
        shadowObject.SetActive(false);
    }
    #endregion

    #region HealthBar 

    private void SetHealth(float currentHealth, float maxHealth) {
        healthBarFill.transform.localScale = new Vector3(Mathf.Clamp01(currentHealth / maxHealth), 1, 1);
    }

    #endregion
}
