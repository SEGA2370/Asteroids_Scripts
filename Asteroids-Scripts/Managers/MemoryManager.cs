using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    public float checkInterval = 60f; // Time interval to clear cache (in seconds)
    public float screenBuffer = 1f;   // Buffer area outside the screen where objects are considered off-screen

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        InvokeRepeating(nameof(ClearCache), checkInterval, checkInterval);
    }

    void ClearCache()
    {
        // Unload unused assets
        Resources.UnloadUnusedAssets();

        // Find all game objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Skip if the object is active
            if (obj.activeInHierarchy) continue;

            // Destroy inactive objects
            Destroy(obj);
        }

        // Check and destroy off-screen objects
        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && IsOffScreen(obj.transform))
            {
                Destroy(obj);
            }
        }
    }

    bool IsOffScreen(Transform objTransform)
    {
        if (!mainCamera) return false;

        Vector3 screenPoint = mainCamera.WorldToViewportPoint(objTransform.position);
        return screenPoint.x < -screenBuffer || screenPoint.x > 1 + screenBuffer ||
               screenPoint.y < -screenBuffer || screenPoint.y > 1 + screenBuffer;
    }
}
