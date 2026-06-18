using System.Collections;
using UnityEngine;

public class TransparencyDetection : MonoBehaviour
{

    [SerializeField] private SpriteRenderer spriteRenderer;
    [Range(0f, 1f)]
    [SerializeField] private float transparencyValue = 0.8f;
    [SerializeField] private float fadeTime = 0.5f;

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.gameObject.GetComponent<Player>() && collider is CapsuleCollider2D) {
            StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, transparencyValue));
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.gameObject.GetComponent<Player>() && collider is CapsuleCollider2D) {
            StartCoroutine(FadeRoutine(spriteRenderer, fadeTime, spriteRenderer.color.a, 1.0f));
        }
    }

    private IEnumerator FadeRoutine(SpriteRenderer spriteRenderer, float fadeTime, float sourceTransparencyValue, float targetTransparencyValue) {
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime) {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(sourceTransparencyValue, targetTransparencyValue, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);

            yield return null;
        }
    }
}
