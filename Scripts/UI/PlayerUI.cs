using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public MainMenu quickMenu;
    public MainMenu deadMenu;

    [SerializeField] private TMP_Text healthText; //text for now, it will be changed later for a bar
    [SerializeField] private TMP_Text victoryText; //also only there for the prototype

    public void SetVictoryText(int time) //delete later
    { 
        victoryText.text = "You have won in " + time + " turns!";
    }

    public void SetHealthBar(float value)
    { 
        healthText.text = "Player HP: " + value;
    }
}
