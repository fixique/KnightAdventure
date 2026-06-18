using UnityEngine;

public class DestructiblePlantVisual : MonoBehaviour
{
    [SerializeField] private DestructiblePlant destructiblePlant;
    [SerializeField] private GameObject flowerDeathVFXPrefab;

    private void Start() {
        destructiblePlant.OnDestructibleTakeDamage += DestructiblePlant_OnDestructibleTakeDamage;
    }

    private void OnDestroy() {
        destructiblePlant.OnDestructibleTakeDamage -= DestructiblePlant_OnDestructibleTakeDamage;
    }

    private void DestructiblePlant_OnDestructibleTakeDamage(object sender, System.EventArgs e) {
        ShowDeathVFX();
    }

    private void ShowDeathVFX() {
        Instantiate(flowerDeathVFXPrefab, destructiblePlant.transform.position, Quaternion.identity);
    }
}
