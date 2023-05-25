using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;
using TMPro;

public class Player : NetworkBehaviour
{
#region Variables
    public enum GameStage {NONE, START, END};

    [SyncVar]public GameStage gameStage;

    [SerializeField] [SyncVar] public string name = "";
    public string Name {get {return name;} set {name = value;}}

    [SyncVar(hook = nameof(cardCountUpdated))]public int cardCount;
    public int CardCount => cardCount;

    [SyncVar] private int points;
    public int Points => points;

    [SyncVar] private string currentPlayer;
    public string CurrentPlayer => currentPlayer;

    [SyncVar] private Color color;
    public Color Color  {get => color; set => color = value;}

    private List<Card> hand;

    public List<Card> Hand => hand;

    //private List<GameObject> uiHand;

    //public List<GameObject> UiHand => uiHand;

    public GameObject handDisplayPrefab;

    private GameObject handDisplay;

    public bool cardsDelivered = false;

    private Card activeCard;

    public event Action _onSpawnUI;

    public static event Action _onChangePlayer;//called after player dropped a card on the discard pile

    private bool _spawnUI = false;

    public bool startCalled = false;

    [SyncVar] private bool isCurrentPlayer;
    public bool IsCurrentPlayer {get => isCurrentPlayer; set => isCurrentPlayer = value;}

    private List<Player> opponents;

    public List<Player> Opponents => opponents;

    [SyncVar(hook = nameof(onPlayerInitialized))] private bool initialized;
    public bool Initialized {get => initialized; set => initialized = value;}

    //[SyncVar] private bool active = false;
#endregion

    public void Start()
    {
        startCalled = true;
    }

    public void SetInitialized()
    {
        cmdInitialized();
    }

    [Command]
    private void cmdInitialized()
    {
        initialized = true;
    }

    //Only called on the server
    public override void OnStartServer()
    {
        CardManager.instance.Players.Add(this);
        CardManager._onCardsDelivered += handleCardsDelivered;
    }

    //Called on all clients
    public override void OnStartClient()
    {
        //if(!hasAuthority) return;

        Debug.Log("Client started");

        opponents = new List<Player>();

        hand = new List<Card>();
        //uiHand = new List<GameObject>();
        activeCard = null;

        gameStage = GameStage.NONE;

        GetComponent<UIHand>()._onSwapCards += swapCards;

        //Drop card on discard pile slot
        DiscardPileSlot._onDropEvent += addActiveCardToDiscardPile;

        CardManager._onDiscardPileUpdated += UpdateDiscardPileSlot;
        CardManager._onUpdatePlayerHand += RemoveActiveCardFromHand;
        CardManager._onPlayerAddOpponent += addOpponent;

        if(!hasAuthority) return;
        cmdSpawnHandDisplay();
        //Problem cards are delivered after call to updateHandDisplay()...
        //That means Start() is called after OnStartClient()...
        //updateHandDisplay();
    }

    [Server]//CardManager => isServer check THEN: to check if player can play: if(!hasAuthority) return; if(!active) return;
    public void setActive(bool active)
    {
        //this.active = active;
    }

    private void addOpponent()
    {
        if(!hasAuthority) return;

        Player[] Players = (Player[]) GameObject.FindObjectsOfType (typeof(Player));

        foreach(Player player in Players)
        {
            if(player == this) continue;

            if(!opponents.Contains(player))
                opponents.Add(player);
        }
                
        Debug.Log("opponent added");
    }

    private void RemoveActiveCardFromHand()
    {
        if(!hasAuthority) return;
        Debug.Log("Update Player Hand => remove active card");
        GetComponent<UIHand>().removeActiveCard();
        hand.Remove(activeCard);
        updateHandDisplay();
        //Debug.Log("HAND COUNT = " + hand.Count);
        cmdSetCardCount(hand.Count);
    }

    private void UpdateDiscardPileSlot(Card card)
    {
        if(!hasAuthority) return;

        if(DiscardPileSlot.Instance.DisplayCard.DisplayImage.enabled == false)
            DiscardPileSlot.Instance.DisplayCard.DisplayImage.enabled = true;

        GetComponent<UIManager>().UpdateDiscardPileSlot(card);
        //swapPlayers(); THIS IS WRONG => GETS CALLED ONCE FOR EACH CLIENT

    }

    [Command]//Executed only on server
    private void cmdSpawnHandDisplay()
    {
        //if(!hasAuthority) return;
        Debug.Log("Spawn Hand Display!");
        handDisplay = Instantiate(handDisplayPrefab,transform.position, transform.rotation);
        NetworkServer.Spawn(handDisplay, connectionToClient);
        rpcSetHandDisplay(handDisplay);
    }

    [ClientRpc]
    private void rpcSetHandDisplay(GameObject _handDisplay)
    {
        handDisplay = _handDisplay;
    }

    private void swapCards(int index1, int index2)
    {
        if(!hasAuthority) return;
        Debug.Log(name + "=>swap cards");
        Card temp = hand[index1];
        hand[index1] = hand[index2];
        hand[index2] = temp;
    }

    public void setActiveCard(int index)//Called from GetComponent<UIHand>
    {
        Debug.Log("setting active card!");
        activeCard = hand[index];
        cmdSetActiveCard(activeCard.Number, activeCard.Color, activeCard.IsJoker, activeCard.IsSkipCard);
    }

    [Command]
    private void cmdSetActiveCard(int number, string color, bool isJoker, bool isSkipCard)
    {
        Debug.Log("cmdSetActiveCard!");
        CardManager.instance.setActiveCard(number, color, isJoker, isSkipCard);
    }

    private void addActiveCardToDiscardPile()
    {
        if(!hasAuthority) return;
        cmdAddActiveCardToDiscardPile();
    }

    [Command]
    private void cmdAddActiveCardToDiscardPile()
    {
        CardManager.instance.PushActiveCardToDiscardPile();
    }

    private void addActiveCardToDeck()
    {
        if(!hasAuthority) return;
        cmdAddActiveCardToDeck();
    }

    [Command]
    private void cmdAddActiveCardToDeck()
    {
        CardManager.instance.AddActiveCardToDeck();
    }

    private void Update()
    {
        //SERVER CODE

        //This condition is only true on the server(cardsDelivered only true on server)
        if(startCalled && cardsDelivered && handDisplay != null && gameStage == GameStage.NONE)
        {
            Debug.Log("UPDATING HAND DISPLAY!");
            rpcupdateHandDisplay();
            gameStage = GameStage.START;
            //UIManager.instance.SpawnUICards(this);
        }

        //CLIENT CODE
        if(!hasAuthority) return;

        Debug.Log("Update called for " + name);

        UICard activeCard, leftCard, rightCard;

        if(!GetComponent<UIManager>().startCalled) return;

        if(_spawnUI == true) {_onSpawnUI.Invoke(); _spawnUI = false;}
    
        if(!GetComponent<UIManager>().UISpawned) return;

#region Moving Cards in the UI

        GetComponent<UIHand>().GetActiveCard(out leftCard, out rightCard);

        //Clear the highlights of all highlighted cards
        for(int i = 0; i < GetComponent<UIHand>().getCount(); i++)
        {
            if(GetComponent<UIHand>().getCard(i).Highlighted())
                GetComponent<UIHand>().getCard(i).enableHighLight(false);
        }

        if(GetComponent<UIHand>().ActiveCard == null) 
        {
            //Debug.Log("Active card is null!");
            return;
        }

        Debug.Log("Card activated!");

        GetComponent<UIHand>().ActiveCard.enableHighLight(true);
        GetComponent<UIHand>().MoveActiveCard();

        //Maybe we can move this code into MoveActiveCard()
        if(leftCard != null)
        {
            //leftCard.enableHighLight(true);
            if(GetComponent<UIHand>().ActiveCard.transform.position.x < leftCard.transform.position.x)
            {
                GetComponent<UIHand>().swapActiveCard(leftCard);
                leftCard.enableHighLight(false);
            }
        }

        if(rightCard != null)
        {
            //rightCard.enableHighLight(true);
            if(GetComponent<UIHand>().ActiveCard.transform.position.x > rightCard.transform.position.x)
            {
                GetComponent<UIHand>().swapActiveCard(rightCard);
                rightCard.enableHighLight(false);
            }
        } 

#endregion
    }

    //Only called on server
    [Server]
    void handleCardsDelivered(Player player)
    {
        if(player != this) return;
        Debug.Log(name + " has  received cards!");
        cardsDelivered = true;
        rpcSetCardCount(player.connectionToClient);
        rpcSpawnUI();//Can be target rpc instead
    }

    [ClientRpc]
    void rpcSpawnUI()
    {
        if(!hasAuthority) return;
        Debug.Log("Spawning Ui");
        _spawnUI = true;
        //_onSpawnUI.Invoke();
    }

    [TargetRpc]
    private void rpcSetCardCount(NetworkConnection conn)
    {
        if(!hasAuthority) return;
        cmdSetCardCount(hand.Count);
    }

    [Command]
    private void cmdSetCardCount(int count)
    {  
        cardCount = count;
    }

    [ClientRpc]
    public void addCardToHand(int number, string color, bool joker, bool skipCard)
    {
        if(!hasAuthority) return;
        hand.Add(new Card(number, color, joker, skipCard));
        //Debug.Log("card details(card): " + hand[hand.Count-1].Number + " " + hand[hand.Count-1].Color);
       
    }

    private void onPlayerInitialized(bool oldInitialized, bool newInitialized)
    {
        cmdPlayerInitialized();
    }

    [Command(requiresAuthority = false)]
    private void cmdPlayerInitialized()
    {
        CardManager.instance.playerInitialized();
    }

    private void cardCountUpdated(int oldCardCount, int newCardCount)//SyncVar callback
    {
        if(newCardCount < 10)
            cmdCardCountUpdatedOnClient();    
    }

    [Command(requiresAuthority = false)]
    private void cmdCardCountUpdatedOnClient()
    {
        CardManager.instance.cardCountUpdatedOnClient();
    }

    [ClientRpc]
    public void rpcDisplayOpponentInfo(string otherPlayer)
    {
        if(!hasAuthority) return;

        int opponentIndex = findOpponentIndexByName(otherPlayer);

        //Debug.Log("Display Other Player Info for Player: " + otherPlayer);
        GetComponent<UIManager>().displayOpponentInfo(opponentIndex);

    }

    private int findOpponentIndexByName(string  name)
    {
        for(int i = 0; i < opponents.Count; i++)
        {
            if(opponents[i].Name == name)
                return i;
        }
        return -1;
    }

    [ClientRpc]
    private void rpcupdateHandDisplay()
    {
        updateHandDisplay();
    }

    public void updateHandDisplay()
    {
        string text;

        Debug.Log("Updating Hand Display");

        //if(!hasAuthority) return;

        handDisplay.GetComponent<TextMeshPro>().text = "";

        for(int i = 0; i < hand.Count; i++)
        {
            if(hand[i].IsJoker) text = i + " JOKER";
            else if (hand[i].IsSkipCard) text = i + " SKIP CARD";
            else text = i + " " + hand[i].Number + " " + hand[i].Color;

            handDisplay.GetComponent<TextMeshPro>().text += text + "\n\n";
        }

        if(hand.Count == 0)
            handDisplay.GetComponent<TextMeshPro>().text = "NO CARDS!";
    }

    public int getIndexOf(Card card)
    {
        int i;
        for(i = 0; i < hand.Count; i++)
        {
            if(hand[i] == card)
                return i;
        }

        if(i == hand.Count)
            Debug.Log("i is invalid!");
        return -1;
    }
}