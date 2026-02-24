using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TilemapManager : MonoBehaviour
{
    public const int ROWS = 3;
    public const int COLUMNS = 11;
    public const int UNITS_PER_TILE = 5;

    public GameObject tile;      // Tile prefab
                                 // Example list of test units
    public List<GameObject> testUnitsPrefabs; // assign different unit prefabs in inspector

    // Number of units to spawn on each side
    public int unitsPerSide = 5;

    public List<Unit>[,] board = new List<Unit>[ROWS, COLUMNS];
    public GameObject[,] tileObjects = new GameObject[ROWS, COLUMNS];
    public List<Unit> allUnits = new List<Unit>();

    public float tileSpacing = 2f;

    private void Awake()
    {
        InitializeBoard();
        GenerateVisualTiles();
        SpawnTestUnits();
    }

    void SpawnTestUnits()
    {
        // Spawn left side units
        for (int i = 0; i < unitsPerSide; i++)
        {
            int row = UnityEngine.Random.Range(0, ROWS);
            int column = UnityEngine.Random.Range(0, 3); // leftmost 3 columns
            GameObject prefab = testUnitsPrefabs[UnityEngine.Random.Range(0, testUnitsPrefabs.Count)];
            SpawnUnitFromPrefab(prefab, row, column, 1);
        }

        // Spawn right side units
        for (int i = 0; i < unitsPerSide; i++)
        {
            int row = UnityEngine.Random.Range(0, ROWS);
            int column = UnityEngine.Random.Range(COLUMNS - 3, COLUMNS); // rightmost 3 columns
            GameObject prefab = testUnitsPrefabs[UnityEngine.Random.Range(0, testUnitsPrefabs.Count)];
            SpawnUnitFromPrefab(prefab, row, column, -1);
        }
    }

    void SpawnUnitFromPrefab(GameObject prefab, int row, int column, int direction)
    {
        if (board[row, column].Count >= UNITS_PER_TILE)
        {
            Debug.LogWarning($"Tile [{row},{column}] is full!");
            return;
        }

        GameObject obj = Instantiate(prefab);
        Unit unit = obj.GetComponent<Unit>();
        unit.direction = direction;
        unit.team = -direction;
        if(unit.team < 0) unit.team = 0;

        AddUnit(unit, row, column);
        allUnits.Add(unit);
    }

    void SpawnUnit(int row, int column, int direction)
    {
        //if (board[row, column].Count >= UNITS_PER_TILE)
        //{
        //    Debug.LogWarning($"Tile [{row},{column}] is full!");
        //    return;
        //}

        //GameObject obj = Instantiate(test_unit);
        //Unit unit = obj.GetComponent<Unit>();
        //unit.direction = direction;

        //AddUnit(unit, row, column);
        //allUnits.Add(unit);
    }

    public void RunRound()
    {
        StartCoroutine(RunRoundCoroutine());
    }

    private IEnumerator RunRoundCoroutine()
    {
        // Step 1: Start of round for all units
        foreach (Unit unit in allUnits)
        {
            unit.StartOfRound();
        }

        // Step 2: Repeat movement 10 times with delay
        int repeat = 10;
        float delay = 0.25f;

        for (int i = 0; i < repeat; i++)
        {
            MoveAllUnits();
            RepackAllTiles(); // Snap units to tile children after moving
            CheckDeadUnits();
            yield return new WaitForSeconds(delay);
        }

        // Step 3: Handle attacks based on attack speed
        // 1. Sort units by attack speed (fastest first)
        var sortedUnits = allUnits.OrderByDescending(u => u.attack_speed).ToList();

        // 2. Group units by attack speed to handle simultaneous attacks
        var groupsBySpeed = sortedUnits.GroupBy(u => u.attack_speed);

        foreach (var group in groupsBySpeed)
        {
            // Dictionary to track damage to each target in this group
            Dictionary<Unit, int> damageToApply = new Dictionary<Unit, int>();

            // Calculate preliminary damage
            foreach (Unit unit in group)
            {
                if (unit.died) continue;

                List<Unit> targets = GetUnitsInFront(unit);
                if (targets != null && targets.Count > 0)
                {
                    // Pick the first target that is on the opposing team
                    Unit target = targets.FirstOrDefault(t => t.team != unit.team && !t.died);
                    if (target == null) continue;

                    if (damageToApply.ContainsKey(target))
                        damageToApply[target] = Math.Max(damageToApply[target], unit.damage);
                    else
                        damageToApply[target] = unit.damage;
                }
            }

            // Apply damage simultaneously
            foreach (var kvp in damageToApply)
            {
                Unit target = kvp.Key;
                if (target.died) continue;

                int incomingDamage = kvp.Value;

                // Counter-damage if target is also attacking in this speed group
                int counterDamage = 0;
                if (group.Contains(target))
                {
                    List<Unit> targetsOfTarget = GetUnitsInFront(target);
                    Unit opposingTarget = targetsOfTarget?.FirstOrDefault(t => t.team != target.team && !t.died);
                    if (opposingTarget != null)
                        counterDamage = target.damage;
                }

                int netDamage = incomingDamage - counterDamage;

                if (netDamage > 0)
                {
                    target.TakeDamage(netDamage, kvp.Key);
                }
                else // minimal mutual damage
                {
                    target.TakeDamage(1, kvp.Key);

                    // Apply 1 damage to all units in this group that attacked this target
                    foreach (Unit attacker in group)
                    {
                        List<Unit> attackerTargets = GetUnitsInFront(attacker);
                        if (attackerTargets != null && attackerTargets.Contains(target))
                        {
                            attacker.TakeDamage(1,target);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(delay);
        }

        // Step 4: Remove dead units from board
        CheckDeadUnits();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunRound();

            //MoveAllUnits();
            //RepackAllTiles();
        }
    }

    void InitializeBoard()
    {
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLUMNS; c++)
                board[r, c] = new List<Unit>();
    }

    void GenerateVisualTiles()
    {
        float totalWidth = (COLUMNS - 1) * tileSpacing;
        float totalHeight = (ROWS - 1) * tileSpacing;
        Vector3 offset = new Vector3(totalWidth / 2f, totalHeight / 2f, 0);

        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLUMNS; c++)
            {
                Vector3 position = new Vector3(c * tileSpacing, r * tileSpacing, 0) - offset;
                GameObject spawnedTile = Instantiate(tile, position, Quaternion.identity, transform);
                tileObjects[r, c] = spawnedTile;
            }
    }

    public Vector3 GetTileChildPosition(int row, int column, int childIndex)
    {
        Transform tileTransform = tileObjects[row, column].transform;
        childIndex = Mathf.Clamp(childIndex, 0, UNITS_PER_TILE - 1);
        return tileTransform.GetChild(childIndex).position;
    }

    public void AddUnit(Unit unit, int row, int column)
    {
        if (board[row, column].Count >= UNITS_PER_TILE)
        {
            Debug.LogWarning($"Cannot add unit to full tile [{row},{column}]");
            return;
        }

        unit.row = row;
        unit.column = column;
        board[row, column].Add(unit);

        int childIndex = board[row, column].Count - 1;
        unit.transform.position = GetTileChildPosition(row, column, childIndex);
    }

    // ========================================
    // Move units with strict team separation
    // ========================================
    public void MoveAllUnits()
    {
        // Step 1: store desired tile for each unit
        Dictionary<Unit, int> desiredColumns = new Dictionary<Unit, int>();
        foreach (Unit unit in allUnits)
        {
            int targetColumn = Mathf.Clamp(unit.column + unit.direction * unit.GetSpeed(), 0, COLUMNS - 1);
            desiredColumns[unit] = targetColumn;
        }

        // ===== New Step 1.5: block tiles already occupied by enemies =====
        foreach (Unit unit in allUnits)
        {
            int targetColumn = desiredColumns[unit];
            if (targetColumn == unit.column) continue;

            bool enemyPresent = false;
            foreach (Unit u in board[unit.row, targetColumn])
            {
                if (IsEnemy(unit, u))
                {
                    enemyPresent = true;
                    break;
                }
            }

            if (enemyPresent)
            {
                // enemy already occupies the target tile, stay in place
                desiredColumns[unit] = unit.column;
            }
        }

        // Step 2: resolve opposing team conflicts (simultaneous moves)
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLUMNS; c++)
            {
                List<Unit> wantingTile = new List<Unit>();
                foreach (var kvp in desiredColumns)
                    if (kvp.Key.row == r && kvp.Value == c)
                        wantingTile.Add(kvp.Key);

                // Check if multiple teams want this tile
                List<Unit> team1 = new List<Unit>();
                List<Unit> team2 = new List<Unit>();
                foreach (var u in wantingTile)
                {
                    if (u.direction == 1) team1.Add(u);
                    else team2.Add(u);
                }

                if (team1.Count > 0 && team2.Count > 0)
                {
                    // pick winner between teams
                    Unit winner;
                    int speed1 = team1[0].GetSpeed();
                    int speed2 = team2[0].GetSpeed();
                    if (speed1 > speed2) winner = team1[0];
                    else if (speed2 > speed1) winner = team2[0];
                    else winner = UnityEngine.Random.value < 0.5f ? team1[0] : team2[0];

                    // only winner moves, others stay
                    foreach (var u in wantingTile)
                        if (u != winner)
                            desiredColumns[u] = u.column;
                }
            }
        }

        // Step 3: resolve same-team conflicts (too many allies)
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLUMNS; c++)
            {
                List<Unit> wantingTile = new List<Unit>();
                foreach (var kvp in desiredColumns)
                    if (kvp.Key.row == r && kvp.Value == c)
                        wantingTile.Add(kvp.Key);

                if (wantingTile.Count <= UNITS_PER_TILE) continue;

                // sort by speed descending, RNG tie-break
                wantingTile.Sort((a, b) =>
                {
                    int cmp = b.GetSpeed().CompareTo(a.GetSpeed());
                    if (cmp != 0) return cmp;
                    return UnityEngine.Random.value < 0.5f ? -1 : 1;
                });

                // top N get the tile, rest stay
                for (int i = UNITS_PER_TILE; i < wantingTile.Count; i++)
                    desiredColumns[wantingTile[i]] = wantingTile[i].column;
            }
        }

        // Step 4: apply moves
        foreach (Unit unit in allUnits)
        {
            int targetColumn = desiredColumns[unit];
            if (targetColumn == unit.column) continue;

            board[unit.row, unit.column].Remove(unit);
            board[unit.row, targetColumn].Add(unit);
            unit.column = targetColumn;
        }
    }

    bool IsEnemy(Unit a, Unit b)
    {
        return a.direction != b.direction;
    }

    // ========================================
    // Smoothly reposition units
    // ========================================
    void RepackAllTiles()
    {
        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLUMNS; c++)
            {
                List<Unit> units = board[r, c];
                for (int i = 0; i < units.Count; i++)
                    units[i].transform.position = GetTileChildPosition(r, c, i);
            }
        }
    }

    // ========================================
    // Utility
    // ========================================
    public int GetColumn(Unit unit)
    {
        int targetColumn = unit.column + unit.direction;
        if (targetColumn < 0 || targetColumn >= COLUMNS) return -1;
        return targetColumn;
    }

    public List<Unit> GetOtherUnitsInColumn(Unit unit)
    {
        List<Unit> result = new List<Unit>();
        for (int r = 0; r < ROWS; r++)
            foreach (Unit u in board[r, unit.column])
                if (u != unit) result.Add(u);
        return result;
    }

    /// <summary>
    /// Returns a random tile GameObject in the same column as the unit
    /// </summary>
    public GameObject GetRandomTileInColumn(Unit unit)
    {
        int randomRow = UnityEngine.Random.Range(0, ROWS);
        return tileObjects[randomRow, unit.column];
    }

    /// <summary>
    /// Returns a random tile GameObject in the same column but a different row than the unit
    /// </summary>
    public GameObject GetRandomOtherTileInColumn(Unit unit)
    {
        List<int> possibleRows = new List<int>();
        for (int r = 0; r < ROWS; r++)
            if (r != unit.row) possibleRows.Add(r);

        if (possibleRows.Count == 0) return null;
        int randomRow = possibleRows[UnityEngine.Random.Range(0, possibleRows.Count)];
        return tileObjects[randomRow, unit.column];
    }

    public List<Unit> GetUnitsInFront(Unit unit)
    {
        int targetColumn = unit.column + unit.direction;
        if (targetColumn < 0 || targetColumn >= COLUMNS) return null;
        return board[unit.row, targetColumn];
    }

    public void CheckDeadUnits()
    {
        List<Unit> deadUnits = new List<Unit>();

        // Step 1: find dead units
        foreach (Unit unit in allUnits)
        {
            if (unit == null || unit.died)
                deadUnits.Add(unit);
        }

        // Step 2: notify allies and remove dead units
        foreach (Unit dead in deadUnits)
        {
            // Notify allies in the same row and same team
            for (int c = 0; c < COLUMNS; c++)
            {
                foreach (Unit ally in board[dead.row, c])
                {
                    if (ally != dead && ally.team == dead.team)
                    {
                        ally.OnAllyDied(dead.row, dead.column);
                    }
                }
            }

            // Remove dead from allUnits
            allUnits.Remove(dead);

            // Remove dead from all tiles
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLUMNS; c++)
                {
                    if (board[r, c].Contains(dead))
                        board[r, c].Remove(dead);
                }
            }

            // Destroy dead unit GameObject
            if (dead != null && dead.gameObject != null)
                Destroy(dead.gameObject);
        }
    }
}