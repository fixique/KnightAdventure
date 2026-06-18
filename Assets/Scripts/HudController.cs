using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour {

    [SerializeField] private Image healthFill; 

    public static HudController Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void SetHealth(float currentHealth, float maxHealth) {
        if (maxHealth <= 0f) {
            healthFill.fillAmount = 0f;
            return;
        }

        healthFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
