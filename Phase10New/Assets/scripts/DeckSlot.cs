using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class DeckSlot : CardSlot, IDropHandler
{
    
    public static OnDropEvent _onDropEvent;

    private static DeckSlot instance;
    public static DeckSlot Instance => instance;

    private void Start()
    {
        base.Start();
        instance = this;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Card dropped on Deck!");
        if(eventData.pointerDrag != null)
        {
            Debug.Log("dropped go is: " + eventData.pointerDrag.gameObject);
            if(displayCard.DisplayImage.enabled == false)
                displayCard.DisplayImage.enabled = true;
            _onDropEvent();
            //eventData.pointerDrag.transform.position = transform.position;
        }
    }
}