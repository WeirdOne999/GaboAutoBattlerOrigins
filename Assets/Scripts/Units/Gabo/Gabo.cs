using UnityEngine;

public class Gabo : Unit
{
    int ScrapCollectedByOthers = 0;
    public GameObject scrap;
    public int amountOfScrapDrop;
    public GameObject ScrapBo;
    public GameObject Bombo;
    public bool canEvolve = false;
    public bool AllyDied = false;
    public override void StartOfRound()
    {
        base.StartOfRound();
        for(int i = 0;i < amountOfScrapDrop; i++)
        {
            ConsumableSpawner.Instance.SpawnConsumableOtherRandom(scrap, this);
        }
    }

    public override void CheckEvolution()
    {
        base.CheckEvolution();
        if (!canEvolve) return;

        if (AllyDied)
        {
            EvolutionText();
            UnitEvolutionManager.Instance.EvolveUnit(this, Bombo);
        }
        if (ScrapCollectedByOthers >= 3)
        {
            EvolutionText();
            UnitEvolutionManager.Instance.EvolveUnit(this, ScrapBo);
        }
    }

    public void GainScrap()
    {
        ScrapCollectedByOthers++;
        if(ScrapCollectedByOthers >= 3)
        {
            CheckEvolution();
        }
    }

    public override void OnAllyDied(int deadRow, int deadColumn)
    {
        base.OnAllyDied(deadRow, deadColumn);
        if(deadRow == row)
        {
            AllyDied = true;
            CheckEvolution();
        }
    }
}
