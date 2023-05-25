using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PhaseChecker
{
    public List<Card> firstSet;
    public List<Card> secondSet;

    public bool has2Objectives;

    public abstract bool CheckFirstSet();
    public abstract bool CheckFirstSetEndGame();

    public abstract bool CheckSecondSet();
    public abstract bool CheckSecondSetEndGame();

    public string Objective1, Objective2;

    private void Start()
    {
        //DiscardPileSlot._onDropEvent += CheckFirstSet;
    }

    public void DeleteFirstSet()
    {
        while(firstSet.Count > 0)
            firstSet.Remove(firstSet[0]);
    }

    public void DeleteSecondSet()
    {
        while(secondSet.Count > 0)
            secondSet.Remove(secondSet[0]);
    }

    public PhaseChecker nextPhaseChecker;

    public bool Objective1Completed, Objective2Completed;

    public PhaseChecker()
    {
        has2Objectives = false;
        firstSet = new List<Card>();
        secondSet = new List<Card>();
        Objective1Completed = false;
        Objective2Completed = false;
    } 
}
