using UnityEngine;

public class Knight : Unit
{
    public bool thornsType;
    public bool shiedlType;

    public int blockAmount=0;
    public bool killedEnemy;

    public bool blocked;

    public bool canEvolve = false;

    public GameObject ThornsKnight;
    public GameObject ShieldKnight;

    public override void StartOfRound()
    {
        base.StartOfRound();
        blocked = false;
    }

    public override void TakeDamage(int amount,Unit source)
    {
        if (!blocked)
        {
            blockAmount++;
            if(blockAmount >= 3)
            {
                CheckEvolution();
            }

            blocked = true;

            bool gotText = false;
            

            if (thornsType)
            {
                TextSpawner.Instance.SpawnText("DEFLECT", Color.red, this.gameObject.transform.position);
                source.TakeDamage(Mathf.FloorToInt(amount / 2), this);
            }

            if(shiedlType)
            {
                TextSpawner.Instance.SpawnText("+SHIELD", Color.blue, this.gameObject.transform.position);
                damage += Mathf.FloorToInt(amount / 2);
            }

            if(!gotText) TextSpawner.Instance.SpawnText("BLOCKED", Color.blue, this.gameObject.transform.position);

            return;
        }

        base.TakeDamage(amount, source);
    }

    public override void OnKill(Unit killedUnit)
    {
        base.OnKill(killedUnit);
        killedEnemy = true;
        CheckEvolution();
    }

    public override void CheckEvolution()
    {
        base.CheckEvolution();
        if (!canEvolve) return;

        if (killedEnemy)
        {
            EvolutionText();
            UnitEvolutionManager.Instance.EvolveUnit(this, ThornsKnight);
        }
        
        if(blockAmount >= 3)
        {
            EvolutionText();
            UnitEvolutionManager.Instance.EvolveUnit(this, ShieldKnight);
        }

        
    }
}
