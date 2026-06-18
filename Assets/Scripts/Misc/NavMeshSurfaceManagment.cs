using NavMeshPlus.Components;
using System.Collections;
using UnityEngine;

public class NavMeshSurfaceManagment : MonoBehaviour
{

    public static NavMeshSurfaceManagment Instance { get; private set; }

    private NavMeshSurface navmeshSurface;

    private void Awake() {
        Instance = this;
        navmeshSurface = GetComponent<NavMeshSurface>();
        navmeshSurface.hideEditorLogs = true;
    }

    public void Rebake() {
        StartCoroutine(RebakeCoroutine());
    }

    private IEnumerator RebakeCoroutine() {
        yield return null;

        navmeshSurface.BuildNavMesh();
    }
}
