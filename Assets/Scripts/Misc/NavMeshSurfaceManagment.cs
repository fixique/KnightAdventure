using NavMeshPlus.Components;
using System.Collections;
using UnityEngine;

public class NavMeshSurfaceManagment : MonoBehaviour
{

    public static NavMeshSurfaceManagment Instance { get; private set; }

    private NavMeshSurface navmeshSurface;
    private float rebuildDelay = 0.3f;

    private Coroutine rebuildCoroutine;
    private bool rebakeInProgress = false;
    private bool rebakeRequestedWhileInProgress = false;

    private void Awake() {
        Instance = this;
        navmeshSurface = GetComponent<NavMeshSurface>();
        navmeshSurface.hideEditorLogs = true;
    }

    public void Rebake() {

        if (rebuildCoroutine != null) {
            StopCoroutine(rebuildCoroutine);
        }

        rebuildCoroutine = StartCoroutine(RebakeAfterDelayCoroutine());
    }

    private IEnumerator RebakeAfterDelayCoroutine() {
        yield return new WaitForSeconds(rebuildDelay);

        rebuildCoroutine = null;

        if (rebakeInProgress) {
            rebakeRequestedWhileInProgress = true;
            yield break;
        }


        yield return RebakeNavMash();
    }

    private IEnumerator RebakeNavMash() {
        rebakeInProgress = true;

        AsyncOperation operation = navmeshSurface.BuildNavMeshAsync();

        yield return operation;

        rebakeInProgress = false;

        if (rebakeRequestedWhileInProgress) {
            rebakeRequestedWhileInProgress = false;
            Rebake();
        }
    }
}
