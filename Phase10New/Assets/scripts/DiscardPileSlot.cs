using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class DiscardPileSlot : CardSlot, IDropHandler
{
    
    //public static event Action _onDropEvent;
    public static OnDropEvent _onDropEvent;

    private static DiscardPileSlot instance;
    public static DiscardPileSlot Instance => instance;

    private void Start()
    {
        base.Start();
        instance = this;
    }

    public void OnDrop(PointerEventData eventData)
    {

        if(eventData.pointerDrag == null) return;
        //We can only drop GameObjects on the slot that have a UICard Component
        //if(!eventData.pointerDrag.TryGetComponent<UICard>(out UICard uiCard))

        if(eventData.pointerDrag.GetComponent<UICard>() == null)
        {
            Debug.Log("Cant drop gameObject on discard pile!");
            return;
        }

        //Only the current player can drop cards on the slot
        UICard uiCard = eventData.pointerDrag.GetComponent<UICard>();
        if(!uiCard.Owner.IsCurrentPlayer)
        {
            Debug.Log("You are not the current player");
            return;
        }

        Debug.Log("Card dropped on Discard Pile!");

        Debug.Log("dropped go is: " + eventData.pointerDrag.gameObject);
        //if(displayCard.DisplayImage.enabled == false)
            //displayCard.DisplayImage.enabled = true;
        _onDropEvent();
        //eventData.pointerDrag.transform.position = transform.position;
        
    }
}