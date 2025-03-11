using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor;

    public ulong ClientID { get; private set; }
    private FixedString32Bytes playerName;
    public int Coins { get; private set; }

    public void Initialise(ulong clientID, FixedString32Bytes playerName, int coins)
    {
        ClientID = clientID;
        this.playerName = playerName;

        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor;
        }

        UpdateCoins(coins);
        UpdateDisplayText();
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;
        UpdateDisplayText();
    }

    public void UpdateDisplayText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {playerName} ({Coins})";
    }
}
