using UnityEngine;

public class FlashBlink : MonoBehaviour
{
    [SerializeField] private MonoBehaviour damagableObject;
    [SerializeField] private Material blinkMaterial;
    [SerializeField] private float blinkDuration = 0.2f;

    private float blinkTimer;
    private Material defaultMaterial;
    private SpriteRenderer spriteRenderer;
    private bool isBlinking;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;

        isBlinking = false;
    }

    private void Start() {
        if (damagableObject is Player player) {
            player.OnFlashBlink += FlashBlink_OnFlashBlink;
        }
    }

    private void Update() {
        if (isBlinking) {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer < 0) {
                StopBlinking();
            }
        }
    }

    private void OnDestroy() {
        if (damagableObject is Player player) {
            player.OnFlashBlink -= FlashBlink_OnFlashBlink;
        }
    }

    private void FlashBlink_OnFlashBlink(object sender, System.EventArgs e) {
        SetBlinkingMaterial();
    }

    public void StopBlinking() {
        SetDefaultMaterial();
        isBlinking = false;
    }

    private void SetBlinkingMaterial() {
        blinkTimer = blinkDuration;
        spriteRenderer.material = blinkMaterial;
        isBlinking = true;
    }

    private void SetDefaultMaterial() {
        spriteRenderer.material = defaultMaterial;
    }
}
