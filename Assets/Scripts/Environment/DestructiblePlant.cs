using System;
using UnityEngine;

public class DestructiblePlant : MonoBehaviour
{

    [SerializeField] private Boolean isDistructible = true;

    public event EventHandler OnDestructibleTakeDamage;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (isDistructible != true) return;

        if (collision.gameObject.GetComponent<Sword>()) {
            OnDestructibleTakeDamage?.Invoke(this, EventArgs.Empty);
            Destroy(gameObject);

            NavMeshSurfaceManagment.Instance.Rebake();
        }
    }
}
