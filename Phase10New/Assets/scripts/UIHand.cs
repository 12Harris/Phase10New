using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;

public class UIHand : MonoBehaviour
{
    private List<UICard> uiHand;
    public List<UICard> UiHand => uiHand;
    private UICard activeCard;
    public UICard ActiveCard => activeCard;

    public event Action<int,int> _onSwapCards;
    public static event Action _test;

    public bool startCalled = false;

    private void Start()
    //public override void OnStartClient()
    {
        if(!GetComponent<Player>().hasAuthority) return;

        Debug.Log("Creating UI HAND");
        uiHand = new List<UICard>();
        activeCard = null;
        startCalled = true;
    }

    public void removeActiveCard()
    {
        uiHand.Remove(activeCard);

        //Debug.Log("Active card gameobject = " + activeCard.transform.gameObject);

        Destroy(activeCard.gameObject);
        activeCard = null;
        StartCoroutine(resetCards());
    }

    private IEnumerator resetCards()
    {
        float timer = 0;
        yield return new WaitForSeconds(0.2f);
        for(int i = 0; i < uiHand.Count; i++)
        {
            uiHand[i].InitialPosition = uiHand[i].transform.position;
        }
    }

    public int getCount()
    {
        return uiHand.Count;
    }

    public void addCard(UICard card)
    {
        uiHand.Add(card);
    }

    public UICard getCard(int index)
    {
        return uiHand[index];
    }

    public void MoveActiveCard()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<UIManager>().UiCanvas.transform as RectTransform, Input.mousePosition, Camera.main, out pos);
        activeCard.transform.position = new Vector3(GetComponent<UIManager>().UiCanvas.transform.TransformPoint(pos).x, activeCard.transform.position.y, activeCard.transform.position.z);
    }

    public void GetActiveCard(out UICard leftCard, out UICard rightCard)
    {
        leftCard = rightCard = null;
        int i;

        for(i = 0; i < uiHand.Count; i++)
        {   
            if(uiHand[i].GetComponent<DragDrop>().Dragging)
            {
                if(i > 0) leftCard = uiHand[i-1];
                if(i < uiHand.Count-1) rightCard = uiHand[i+1];
                activeCard = uiHand[i];
                GetComponent<Player>().setActiveCard(i);//method chaining will also set active card on server
                break;
            }
        }
        if(i == uiHand.Count)
        {
            if(activeCard != null)
            {
                //Reset position of active card to its initial position
                activeCard.transform.position = activeCard.InitialPosition;

                if(!DiscardPileSlot.Instance.mouseInCardSlot())   
                    activeCard = null;
                else
                    print("Card is still active!");
            }

        }
    }

    //Swaps position of active card with given card + rearranges the list of cards
    public void swapActiveCard(UICard otherCard)
    {
        otherCard.transform.position = activeCard.InitialPosition;
        Vector3 temp = otherCard.InitialPosition;
        otherCard.InitialPosition = activeCard.InitialPosition;
        activeCard.InitialPosition = temp;

        int index1 = getIndexOf(activeCard);
        int index2 = getIndexOf(otherCard);

        UICard temp2 = uiHand[index1];
        uiHand[index1] = uiHand[index2];
        uiHand[index2] = temp2;

        for(int i = 0; i < uiHand.Count; i++)
            uiHand[i].transform.SetSiblingIndex(i);

        _onSwapCards?.Invoke(index1, index2);
        GetComponent<Player>().updateHandDisplay();
    }

    /*private void swapCards(UICard first, UICard second)
    {
        first.transform.position = second.InitialPosition;
        Vector3 temp = first.InitialPosition;
        first.InitialPosition = second.InitialPosition;
        second.InitialPosition = temp;

        int index1 = getIndexOf(first);
        int index2 = getIndexOf(second);

        UICard temp2 = uiHand[index1];
        uiHand[index1] = uiHand[index2];
        uiHand[index2] = temp2;

        for(int i = 0; i < uiHand.Count; i++)
            uiHand[i].transform.SetSiblingIndex(i);

        //_onSwapCards?.Invoke(index1, index2);
        //GetComponent<Player>().updateHandDisplay();
    }*/

    private int getIndexOf(UICard card)
    {
        for(int i = 0; i < uiHand.Count; i++)
        {
            if(uiHand[i] == card)
                return i;
        }
        return -1;
    }

    public UICard leftMostCard()
    {
        UICard result = null;
        float minX = 5000;
        for(int i = 0; i < uiHand.Count; i++)
        {
            if(uiHand[i].transform.position.x < minX)
            {
                minX = uiHand[i].transform.position.x;
                result = uiHand[i];
            }
        }
        
        return result;
    }

}