using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
    public Unit unit;

    public TextMeshProUGUI health, shield, damage;

    private void Update()
    {
        health.text = "<3: " + unit.health.ToString();
        shield.text = "<}: " + unit.shield.ToString();
        damage.text = "-}---: " + unit.damage.ToString();
    }
}
