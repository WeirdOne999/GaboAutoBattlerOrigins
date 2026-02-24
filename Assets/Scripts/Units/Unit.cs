using UnityEngine;

public class Unit : MonoBehaviour
{
    public int attack_speed;
    public int movement_speed = 1;

    public int health;
    public int damage;
    public int shield;
    public int dodge_chance;

    public int direction;
    public int row;
    public int column;
    public int team; // 0 = Left, 1 = Right
    public bool died = false;
    public int speed_left;

    public virtual void StartOfRound()
    {
        speed_left = movement_speed;
    }

    public virtual int GetSpeed()
    {
        if (speed_left <= 0)
            return 0; // No speed left

        speed_left--; // Consume 1 speed
        return 1;     // Normal speed per tick
    }

    public virtual void Move() { }

    public virtual void CheckEvolution()
    {

    }

    public virtual void TakeDamage(int amount,Unit source)
    {
        if (TryDodge())
        {
            TextSpawner.Instance.SpawnText("Dodge", Color.blue, this.gameObject.transform.position);
            return;
        }

        int amountleft = amount - shield;

        shield -= amount;
        if(shield <= 0)
        {
            shield = 0;
        }

        TextSpawner.Instance.SpawnText("-" + amount.ToString(), Color.red, this.gameObject.transform.position);
        health -= amountleft;

        if (health <= 0)
        {
            
            Die();
            // Notify the source that they killed this unit
            if (source != null)
            {
                source.OnKill(this);
            }
        }
    }

    public virtual void Heal(int amount)
    {
        health += amount;
    }

    public virtual bool TryDodge()
    {
        return Random.Range(0, 100) < dodge_chance;
    }

    public virtual void Die()
    {
        died = true;
    }

    public void EvolutionText()
    {

        TextSpawner.Instance.SpawnText("EVOLUTION", Color.green, this.gameObject.transform.position);
    }

    public virtual void OnAllyDied(int deadRow, int deadColumn)
    {
        Debug.Log($"{name} noticed an ally died at row {deadRow}, column {deadColumn}!");
        // Optional: react to ally death (e.g., gain morale, buff, or trigger AI behavior)
    }

    public virtual void OnKill(Unit killedUnit)
    {
        // Example: show a text above the source unit
        TextSpawner.Instance.SpawnText("Kill!", Color.yellow, this.gameObject.transform.position);

        // You can also do other logic like gaining XP, gold, or buffs
    }
}