using Unity.VisualScripting;
using UnityEngine;

public class Scrap : MonoBehaviour
{
    public int team;

    public bool bomb;
    public bool advanced;

    public Unit owner;

    public bool used = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used)
        {
            Destroy(this.gameObject);
            return;
        }
        used = true;
        Unit temp = collision.gameObject.GetComponent<Unit>();

        if (temp != null)
        {
            if (bomb)
            {
                if (temp.team != team)
                {
                    temp.TakeDamage(2,owner);
                    Destroy(this.gameObject);
                }
            }
            else
            {
                if (temp.team == team)
                {
                    int multiplier = advanced ? 1 : 2;

                    int effect = Random.Range(0, 3); // Unity's Random.Range with int max is exclusive

                    switch (effect)
                    {
                        case 0: // Heal
                            temp.health += 5 * multiplier; // adjust amount as needed
                            TextSpawner.Instance.SpawnText("+health", Color.blue, this.gameObject.transform.position);
                            break;
                        case 1: // Increase damage
                            temp.damage += 2 * multiplier;
                            TextSpawner.Instance.SpawnText("+damage", Color.blue, this.gameObject.transform.position);
                            break;
                        case 2: // Increase shield
                            temp.shield += 3 * multiplier;
                            TextSpawner.Instance.SpawnText("+shield", Color.blue, this.gameObject.transform.position);
                            break;
                    }
                    if (owner.gameObject.GetComponent<Gabo>())
                    {
                        owner.gameObject.GetComponent<Gabo>().GainScrap();
                    }

                    Destroy(this.gameObject);
                }
            }
        }
    }


}