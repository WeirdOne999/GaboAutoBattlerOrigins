using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitEvolutionManager : MonoBehaviour
{
    public static UnitEvolutionManager Instance { get; private set; }

    public TilemapManager tilemapManager; // assign in inspector

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: DontDestroyOnLoad(gameObject);
    }

    public void EvolveUnit(Unit oldUnit, GameObject newUnitPrefab)
    {
        if (oldUnit == null || newUnitPrefab == null) return;

        int row = oldUnit.row;
        int column = oldUnit.column;
        int direction = oldUnit.direction;
        int team = oldUnit.team;

        tilemapManager.board[row, column].Remove(oldUnit);
        tilemapManager.allUnits.Remove(oldUnit);
        Destroy(oldUnit.gameObject);

        GameObject obj = Instantiate(newUnitPrefab);
        Unit newUnit = obj.GetComponent<Unit>();
        newUnit.row = row;
        newUnit.column = column;
        newUnit.direction = direction;
        newUnit.team = team;

        tilemapManager.board[row, column].Add(newUnit);
        tilemapManager.allUnits.Add(newUnit);

        int childIndex = tilemapManager.board[row, column].Count - 1;
        newUnit.transform.position = tilemapManager.GetTileChildPosition(row, column, childIndex);
    }

    public void SpawnUnitAtRandomTileInColumn(Unit referenceUnit, GameObject unitPrefab)
    {
        if (referenceUnit == null || unitPrefab == null) return;

        int column = referenceUnit.column;
        int team = referenceUnit.team;

        // Get a random row in the column
        int randomRow = Random.Range(0, TilemapManager.ROWS);

        // Instantiate the new unit
        GameObject obj = Instantiate(unitPrefab);
        Unit newUnit = obj.GetComponent<Unit>();
        newUnit.row = randomRow;
        newUnit.column = column;
        newUnit.direction = referenceUnit.direction; // optional: match direction
        newUnit.team = team;

        // Add to board and allUnits list
        tilemapManager.board[randomRow, column].Add(newUnit);
        tilemapManager.allUnits.Add(newUnit);

        // Position at tile child
        int childIndex = tilemapManager.board[randomRow, column].Count - 1;
        newUnit.transform.position = tilemapManager.GetTileChildPosition(randomRow, column, childIndex);
    }

    public void EvolveUnitToRandomPrefab(Unit oldUnit, List<GameObject> evolutionOptions)
    {
        if (evolutionOptions.Count == 0) return;
        GameObject chosenPrefab = evolutionOptions[Random.Range(0, evolutionOptions.Count)];
        EvolveUnit(oldUnit, chosenPrefab);
    }
}