using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityOptimizer : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Settings")]
    public string targetTag = "";
    public float activationRadius = 30f;
    public float checkInterval = 0.05f;

    [Header("Auto-Detect Spawned")]
    [Tooltip("Automatically check for new tagged objects periodically")]
    public bool autoDetectNew = true;
    [Tooltip("How often to scan for new objects")]
    public float detectInterval = 2f;

    private List<GameObject> managedObjects = new List<GameObject>();
    private float sqrRadius;
    private bool isRunning = false;

    void Start()
    {
        if (target == null)
            target = this.transform;

        sqrRadius = activationRadius * activationRadius;

        FindAllTaggedObjects();

        if (managedObjects.Count > 0 || autoDetectNew)
        {
            isRunning = true;
            StartCoroutine(ProximityCheckRoutine());

            if (autoDetectNew)
                StartCoroutine(DetectNewObjectsRoutine());
        }
    }

    void FindAllTaggedObjects()
    {
        if (string.IsNullOrEmpty(targetTag)) return;

        // Find active objects
        GameObject[] activeFound = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject obj in activeFound)
        {
            if (obj != null && !managedObjects.Contains(obj))
                AddObject(obj);
        }

        // Find inactive objects
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in allTransforms)
        {
            if (t.CompareTag(targetTag) && !managedObjects.Contains(t.gameObject))
                AddObject(t.gameObject);
        }
    }

    void AddObject(GameObject obj)
    {
        managedObjects.Add(obj);
        float sqrDist = (obj.transform.position - target.position).sqrMagnitude;
        obj.SetActive(sqrDist <= sqrRadius);
    }

    private IEnumerator ProximityCheckRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (isRunning)
        {
            // Clean up destroyed
            for (int i = managedObjects.Count - 1; i >= 0; i--)
            {
                if (managedObjects[i] == null)
                    managedObjects.RemoveAt(i);
            }

            // Update all tracked objects
            foreach (GameObject obj in managedObjects)
            {
                if (obj == null) continue;

                float sqrDistance = (target.position - obj.transform.position).sqrMagnitude;
                bool shouldBeActive = sqrDistance <= sqrRadius;

                if (obj.activeSelf != shouldBeActive)
                    obj.SetActive(shouldBeActive);
            }

            yield return wait;
        }
    }

    private IEnumerator DetectNewObjectsRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectInterval);

        while (isRunning)
        {
            yield return wait;

            if (string.IsNullOrEmpty(targetTag)) continue;

            // Reuse full search (active + inactive) instead of active-only
            int before = managedObjects.Count;
            FindAllTaggedObjects();
            int added = managedObjects.Count - before;

            if (added > 0)
                Debug.Log($"[ProximityOptimizer] Auto-detected {added} new object(s).");
        }
    }

    // Call this from your spawner script for immediate registration
    public void RegisterObject(GameObject obj)
    {
        if (obj != null && obj.CompareTag(targetTag) && !managedObjects.Contains(obj))
        {
            AddObject(obj);
            Debug.Log($"[ProximityOptimizer] Manually registered: {obj.name}");
        }
    }

    // Alternative: Call this after instantiating
    public GameObject RegisterClone(GameObject original, Vector3 position, Quaternion rotation)
    {
        GameObject clone = Instantiate(original, position, rotation);
        RegisterObject(clone);
        return clone;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, activationRadius);

        if (!Application.isPlaying || !isRunning) return;

        Gizmos.color = Color.green;
        foreach (var obj in managedObjects)
        {
            if (obj != null && obj.activeSelf)
                Gizmos.DrawLine(target.position, obj.transform.position);
        }

        Gizmos.color = Color.red;
        foreach (var obj in managedObjects)
        {
            if (obj != null && !obj.activeSelf)
                Gizmos.DrawLine(target.position, obj.transform.position);
        }
    }
}