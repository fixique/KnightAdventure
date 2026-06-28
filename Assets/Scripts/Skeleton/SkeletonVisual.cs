using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    [SerializeField] private GameObject healthBarDelayed;
    [SerializeField] private float delayBeforeAnimation = 0.25f;
    [SerializeField] private float animationDuration = 0.35f;
    [SerializeField] private float visibleDuration = 2.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";
    private const string TAKE_HIT = "TakeHit";
    private const string CHASING_SPEED_MULTIPLIER = "ChasingSpeedMultiplier";
    private const string ATTACK = "Attack";
    private const string IS_DIE = "IsDie";

    private Coroutine delayedDamageCoroutine;
    private Coroutine hideHealthBarCoroutine;

    #region Lifecycle

    private void Awake() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthBar.SetActive(false);
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

    private void LateUpdate() {
        healthBar.transform.rotation = Quaternion.identity;
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
        ShowHealthBar();
        SetHealth(e.currentHealth, e.maxHealth);
        animator.SetTrigger(TAKE_HIT);
    }

    private void EnemyEntity_OnDeath(object sender, EventArgs e) {
        healthBar.SetActive(false);

        animator.SetBool(IS_DIE, true);
        spriteRenderer.sortingOrder = -1;
        shadowObject.SetActive(false);
    }
    #endregion

    #region HealthBar 

    private void SetHealth(float currentHealth, float maxHealth) {
        float targetXScale = Mathf.Clamp01(currentHealth / maxHealth);
        healthBarFill.transform.localScale = new Vector3(targetXScale, 1, 1);

        if (delayedDamageCoroutine != null) {
            StopCoroutine(delayedDamageCoroutine);
        }

        delayedDamageCoroutine = StartCoroutine(AnimateDelayedDamage(targetXScale));
    }

    private IEnumerator AnimateDelayedDamage(float targetXScale) {
        yield return new WaitForSeconds(delayBeforeAnimation);

        float startedXScale = healthBarDelayed.transform.localScale.x;
        float elapsed = 0f;

        while (elapsed < animationDuration) {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / animationDuration);
            float scale = Mathf.Lerp(startedXScale, targetXScale, progress);

            healthBarDelayed.transform.localScale = new Vector3(scale, 1, 1);

            yield return null;
        }

        healthBarDelayed.transform.localScale = new Vector3(targetXScale, 1, 1);
        delayedDamageCoroutine = null;
    }

    private void ShowHealthBar() {
        healthBar.SetActive(true);

        if (hideHealthBarCoroutine != null) {
            StopCoroutine(hideHealthBarCoroutine);
        }

        hideHealthBarCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay() {
        yield return new WaitForSeconds(visibleDuration);

        healthBar.SetActive(false);
        hideHealthBarCoroutine = null;
    }

    #endregion
}
