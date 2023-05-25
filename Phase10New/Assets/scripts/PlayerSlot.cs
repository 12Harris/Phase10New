using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PlayerSlot : MonoBehaviour
{
    private TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI  NameDisplay => nameDisplay;

    private TextMeshProUGUI cardCountDisplay;
    public TextMeshProUGUI  CardCountDisplay => cardCountDisplay;

    private Color displayColor;

    private bool initialized = false;
    public bool Initialized => initialized;

    public void Initialize()
    {      
        nameDisplay = transform.Find("Name").GetComponent<TextMeshProUGUI>();
        cardCountDisplay = transform.Find("Card Count").GetComponent<TextMeshProUGUI>();
    }

    public void Update()
    {
        if(!initialized)
        {
            Initialize();
            
            initialized = true;
        }
    }

    public void setNameDisplay(string name)
    {
        nameDisplay.text = name;
    }

    public void setCardCountDisplay(int cardCount)
    {
        cardCountDisplay.text = "Cards: " + cardCount.ToString();
    }

    public void setDisplayColor(Color color)
    {
        displayColor = color;
        nameDisplay.color = displayColor;
        cardCountDisplay.color = displayColor;
    }
}