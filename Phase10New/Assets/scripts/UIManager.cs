using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField] private GameObject uiCanvasPrefab;
    [SerializeField] private GameObject uiCardPrefab;
    private DiscardPileSlot discardPileSlot; //should be instance instead
    private GameObject uiCanvas;
    public GameObject UiCanvas => uiCanvas;
    private Player player;
    private bool _spawnUI;
    public bool startCalled = false;
    private bool uiSpawned = false;
    public bool UISpawned => uiSpawned;
    private List<PlayerSlot> playerSlots;


    public void Start()
    {
        if(!GetComponent<Player>().hasAuthority) return;
        startCalled = true;
        player = GetComponent<Player>();
        player._onSpawnUI += createUI;
        //CardManager._onDiscardPileUpdated += UpdateDiscardPileSlot;
    }

    public void UpdateDiscardPileSlot(Card card)
    {
        if(discardPileSlot.DisplayCard.enabled == false)
            Debug.Log("discardPileSlot.DisplayCard NOT enabled!");

        discardPileSlot.DisplayCard.DisplayText.text =  "" + card.Number + "";

        if(card.IsJoker)
            discardPileSlot.DisplayCard.DisplayText.text = "JK";

        else if(card.IsSkipCard)
            discardPileSlot.DisplayCard.DisplayText.text = "S";
                
        switch(card.Color)
        {
            case "red":
               
                discardPileSlot.DisplayCard.DisplayImage.color = new Color(255,0,0);
                break;
            case "blue":
                discardPileSlot.DisplayCard.DisplayImage.color = new Color(0,0,255);
                break;
            case "green":
                discardPileSlot.DisplayCard.DisplayImage.color = new Color(0,255,0);
                break;
            case "yellow":
                discardPileSlot.DisplayCard.DisplayImage.color = new Color(255,255,0);
                break;
            default:
                discardPileSlot.DisplayCard.DisplayImage.color = new Color(0,0,0);
                break;
        }
    }

    /*public override void OnStartClient()
    {
        player = GetComponent<Player>();
        player._onSpawnUI += createUI;
    }*/

    private void Update()
    {
        if(_spawnUI == true && GetComponent<UIHand>().startCalled)
        {
            spawnUI();
            _spawnUI = false;
        }
    }

    private void createUI()
    {
        _spawnUI = true;
    }


    //Displays and updates all information about the other players
    public void displayOpponentInfo(int opponent)
    {    
        //Debug.Log("Opponent Index = " + opponent);
        //Debug.Log("Opponent name = " + GetComponent<Player>().Opponents[opponent].name);
        //Debug.Log(GetComponent<Player>().Opponents[opponent].name + " card count = " + GetComponent<Player>().Opponents[opponent].cardCount);

        string playerSlot = "Player Slot " + (opponent+1).ToString();

        PlayerSlot slot = uiCanvas.transform.Find(playerSlot).GetComponent<PlayerSlot>();
        slot.setDisplayColor(GetComponent<Player>().Color);
        slot.setNameDisplay(GetComponent<Player>().Opponents[opponent].name);
        slot.setCardCountDisplay(GetComponent<Player>().Opponents[opponent].cardCount);

    }

    public void spawnUI()
    {
        uiCanvas = Instantiate(uiCanvasPrefab, new Vector3(530.2489f,585.004f,0), Quaternion.identity);
        var cardArea = uiCanvas.transform.Find("Card Area");

        cardArea.transform.position = new Vector3(331,255,0);

        for(int i = 0; i < player.Hand.Count; i++)
        {
            var uiCard = Instantiate(uiCardPrefab, player.transform.position, player.transform.rotation); 

            uiCard.AddComponent<UICard>();

            uiCard.GetComponent<UICard>().Owner = player;

            uiCard.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "" + player.Hand[i].Number + "";

            if(player.Hand[i].IsJoker)
                 uiCard.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "JK";

            else if(player.Hand[i].IsSkipCard)
                uiCard.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "S";

            switch(player.Hand[i].Color)
            {
                case "red":
                    uiCard.transform.Find("Image").GetComponent<Image>().color = new Color(255,0,0);
                    break;
                case "blue":
                    uiCard.transform.Find("Image").GetComponent<Image>().color = new Color(0,0,255);
                    break;
                case "green":
                    uiCard.transform.Find("Image").GetComponent<Image>().color = new Color(0,255,0);
                    break;
                case "yellow":
                    uiCard.transform.Find("Image").GetComponent<Image>().color = new Color(255,255,0);
                    break;
                default:
                     uiCard.transform.Find("Image").GetComponent<Image>().color = new Color(0,0,0);
                     break;
            }

            uiCard.transform.parent = cardArea;
            uiCard.AddComponent<DragDrop>();
            //uiCard.AddComponent<UICard>();
            GetComponent<UIHand>().addCard(uiCard.GetComponent<UICard>());
            uiCard.transform.SetSiblingIndex(i);
        }

        discardPileSlot = uiCanvas.transform.Find("Card Slots").transform.Find("Discard Pile Slot").GetComponent<DiscardPileSlot>();
        Debug.Log("Discard pile slot is: " + discardPileSlot );
        uiSpawned = true;
        GetComponent<Player>().SetInitialized();

        /*for(int i = 0; i < player.Hand.Count; i++)
        {
            GetComponent<UIHand>().getCard(i).Initialize();
        }*/
    }

}


//UIManager1(Server)                    UIManager1(client)

//spawning cards for player 1           //-


//UIManager2(Server)                    UIManager2(client)

//-                                     //spawning cards for player 2