using UnityEngine;

public class Slime : Unit
{
    int dodgeAmt;

    public GameObject AssAsslime;
    public GameObject miniSlime;

    public override void StartOfRound()
    {
        base.StartOfRound();
        dodgeAmt = 0;
    }

    public override void CheckEvolution()
    {
        base.CheckEvolution();

        if( dodgeAmt >= 3)
        {
            EvolutionText();
            UnitEvolutionManager.Instance.EvolveUnit(this, AssAsslime);
        }
    }

    public override bool TryDodge()
    {
        if(Random.Range(0, 100) < dodge_chance)
        {
            dodgeAmt++;
            if(dodgeAmt >= 3)
            {
                CheckEvolution();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void Die()
    {
        TextSpawner.Instance.SpawnText("SPLIT", Color.green, this.gameObject.transform.position);
        UnitEvolutionManager.Instance.SpawnUnitAtRandomTileInColumn(this, miniSlime);
        UnitEvolutionManager.Instance.SpawnUnitAtRandomTileInColumn(this, miniSlime);
        base.Die();
    }
}
