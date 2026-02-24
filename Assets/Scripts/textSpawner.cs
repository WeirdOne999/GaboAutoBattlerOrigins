using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextSpawner : MonoBehaviour
{
    // Singleton instance
    public static TextSpawner Instance;

    [Header("Settings")]
    public GameObject textPrefab;       // Prefab with TextMeshPro component
    public Transform parentTransform;   // Optional parent for spawned text
    public float lifeTime = 2f;         // How long the text stays

    [Header("Object Pool")]
    public int poolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(textPrefab, parentTransform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Spawn text at a given position with a color
    /// </summary>
    public void SpawnText(string text, Color color, Vector3 position)
    {
        GameObject obj;

        // Take from pool if available
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(textPrefab, parentTransform);
        }

        obj.transform.position = position;
        obj.SetActive(true);

        // Set text and color
        TMP_Text tmp = obj.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        // Return to pool after lifetime
        StartCoroutine(ReturnToPool(obj, lifeTime));
    }

    private System.Collections.IEnumerator ReturnToPool(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}