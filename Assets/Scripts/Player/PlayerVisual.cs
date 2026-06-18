using UnityEngine;
using UnityEngine.Rendering;

public class PlayerVisual : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private const string IS_RUNNING = "IsRunning";
    private const string IS_DIE = "IsDie";

    private void Awake() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        Player.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
    }

    private void Instance_OnPlayerDeath(object sender, System.EventArgs e) {
        animator.SetBool(IS_DIE, true);
    }

    private void Update() {
        animator.SetBool(IS_RUNNING, Player.Instance.IsRunning());
        AdjustPlayerFacingDirection();
    }

    private void OnDestroy() {
        Player.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
    }

    private void AdjustPlayerFacingDirection() {
        if (!Player.Instance.isAlive)
            return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 playerPos = Player.Instance.GetPlayerScreenPosition();

        if (mousePos.x < playerPos.x) {
            spriteRenderer.flipX = true;
        } else {
            spriteRenderer.flipX = false;
        }
    }
}
