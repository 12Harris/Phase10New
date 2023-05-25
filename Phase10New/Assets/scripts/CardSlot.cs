using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using Mirror;

public delegate void OnDropEvent();
public abstract class CardSlot : MonoBehaviour
{

    public List<Card> slotCards; //should be card object

    protected Image backGround;
    public Image BackGround => backGround;
    [SerializeField] protected UICard displayCard;
    public UICard DisplayCard => displayCard;
    [SerializeField] private Collider collider;

    protected void Start()
    {
        //DisplayCard.enabled = false;
        //displayCard.DisplayImage.enabled = false;
        backGround = transform.Find("Background").GetComponent<Image>();
        UICard._onUICardInitialized += disableDisplayCard;
    }

    private void Update()
    {
        //mouseInCardSlot();
    }

    public bool mouseInCardSlot()
    {
        RaycastHit hit;
        if(Physics.Raycast(GetMouseRay(), out hit))
        {
            if(hit.collider == collider)
            {
                print("Mouse in Card Slot!");
                return true;
            }
        }
        return false;
    }

    private static Ray GetMouseRay()
    {   
        return Camera.main.ScreenPointToRay(Input.mousePosition);
    }


    private void disableDisplayCard()
    {
        displayCard.DisplayImage.enabled = false;
    }

}