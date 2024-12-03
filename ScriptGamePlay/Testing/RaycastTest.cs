using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastExample : MonoBehaviour
{
    public LayerMask targetLayer; // Layer for the colliders you want to hit

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, targetLayer);

            foreach (var hit in hits)
            {
                Debug.Log($"Hit: {hit.collider.name} at {hit.point} with normal {hit.normal}");
                // Instantiate(effectPrefab, hit.point, Quaternion.identity);
            }

            if (hits.Length == 0)
            {
                Debug.Log("No hit detected");
            }
        }
    }
}
