using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//[Serializable]
public class Card
{
    //[field: SerializeField] public InputReader InputReader {get; private set;}
    private int number;
    public int Number{get => number; set => number = value;}
    private string color;
    public string Color{get => color; set =>  color = value;}
    private bool isJoker;
    public bool IsJoker{get => isJoker; set => isJoker = value;}
    private bool isSkipCard;
    public bool IsSkipCard{get => isSkipCard; set => isSkipCard = value;}
    private int deckIndex;
    public int DeckIndex{get => deckIndex; set => deckIndex = value;}

    public Card(int number, string color)
    {
        this.number = number;
        this.color = color;
        isJoker = false;
        isSkipCard = false;
        deckIndex = 0;
    }

    public Card(int number, string color, bool isJoker, bool isSkipCard)
    {
        this.number = number;
        this.color = color;
        this.isJoker = isJoker;
        this.isSkipCard = isSkipCard;
        deckIndex = 0;
    }

    public Card()
    {
        number = 0;
    }
}
