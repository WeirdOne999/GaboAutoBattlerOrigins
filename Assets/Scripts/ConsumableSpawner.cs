using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableSpawner : MonoBehaviour
{
    public static ConsumableSpawner Instance { get; private set; }

    public TilemapManager tilemapManager; // assign in inspector
    public float spawnOffsetRange = 0.3f; // random offset so items don't stack perfectly

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Spawn a consumable on a specific tile (row, column) using the given prefab
    /// </summary>
    public void SpawnConsumable(GameObject prefab,Unit unit)
    {
        if (prefab == null || tilemapManager == null) return;

        GameObject obj = Instantiate(prefab);

        obj.GetComponent<Scrap>().team = unit.team;
        obj.GetComponent<Scrap>().owner = unit;

        // Get tile's center position
        Vector3 basePos = tilemapManager.GetRandomTileInColumn(unit).transform.position;

        // Apply small random offset
        float offsetX = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        float offsetY = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        obj.transform.position = basePos + new Vector3(offsetX, offsetY, 0);

    }

    public void SpawnConsumableOtherRandom(GameObject prefab, Unit unit)
    {
        if (prefab == null || tilemapManager == null) return;

        GameObject obj = Instantiate(prefab);

        obj.GetComponent<Scrap>().team = unit.team;
        obj.GetComponent<Scrap>().owner = unit;

        // Get tile's center position
        Vector3 basePos = tilemapManager.GetRandomOtherTileInColumn(unit).transform.position;

        // Apply small random offset
        float offsetX = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        float offsetY = Random.Range(-spawnOffsetRange, spawnOffsetRange);
        obj.transform.position = basePos + new Vector3(offsetX, offsetY, 0);

    }
}