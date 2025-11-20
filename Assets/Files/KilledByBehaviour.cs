using System.Collections.Generic;
using UnityEngine;

public class KilledByBehaviour : MonoBehaviour
{
    [Header("Kill Rules")]
    [Tooltip("Tags that can be destroyed or that can destroy this object.")]
    public List<string> killTags = new();

    [Tooltip("Layers that can be destroyed or that can destroy this object.")]
    public List<string> killLayers = new();

    [Tooltip("If true, this object is destroyed when touched. Otherwise, it destroys others.")]
    public bool killedBy = false;

    [Header("Ignore Rules")]
    [Tooltip("Tag to ignore during collision checks.")]
    public List<string> ignoreTags = new() { "UNKILLABLE" };

    [Tooltip("Layers to ignore during collision checks.")]
    public List<string> ignoreLayers = new() { "IgnoreKill" };

    // --- 3D ---
    private void OnCollisionEnter(Collision collision) => HandleCollision(collision.collider);
    private void OnTriggerEnter(Collider other) => HandleCollision(other);

    // --- 2D ---
    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision.collider);
    private void OnTriggerEnter2D(Collider2D other) => HandleCollision(other);

    // --------------------------------------------------------------------
    // UNIFIED HANDLER
    // --------------------------------------------------------------------
    private void HandleCollision(Component other)
    {
        if (other == null) return;

        GameObject target = killedBy ? gameObject : other.gameObject;
        GameObject source = killedBy ? other.gameObject : gameObject;

        // --- Ignore by Tag ---
        foreach (string ignore in ignoreTags)
        {
            if (string.IsNullOrEmpty(ignore)) continue;
            if (source.CompareTag(ignore) || target.CompareTag(ignore))
                return;
        }

        // --- Ignore by Layer ---
        foreach (string ignoreLayer in ignoreLayers)
        {
            if (string.IsNullOrEmpty(ignoreLayer)) continue;
            int ignoreLayerIndex = LayerMask.NameToLayer(ignoreLayer);
            if (ignoreLayerIndex >= 0 &&
                (source.layer == ignoreLayerIndex || target.layer == ignoreLayerIndex))
                return;
        }

        bool hasTags = killTags != null && killTags.Count > 0;
        bool hasLayers = killLayers != null && killLayers.Count > 0;

        // --- No filters means kill anything not ignored ---
        if (!hasTags && !hasLayers)
        {
            TriggerDestroy(target);
            return;
        }

        // --- Tag check ---
        if (hasTags)
        {
            foreach (string tag in killTags)
            {
                if (string.IsNullOrEmpty(tag)) continue;

                if (source.CompareTag(tag) || other.CompareTag(tag))
                {
                    TriggerDestroy(target);
                    return;
                }
            }
        }

        // --- Layer check ---
        if (hasLayers)
        {
            foreach (string layerName in killLayers)
            {
                if (string.IsNullOrEmpty(layerName)) continue;
                int layerIndex = LayerMask.NameToLayer(layerName);
                if (layerIndex < 0) continue;

                if (source.layer == layerIndex || other.gameObject.layer == layerIndex)
                {
                    TriggerDestroy(target);
                    return;
                }
            }
        }
    }

    protected virtual void TriggerDestroy(GameObject go)
    {
        if (go != null)
        {
            Debug.Log($"[KilledByBehaviour] Destroying {go.name}");
            Destroy(go);
        }
    }
}
