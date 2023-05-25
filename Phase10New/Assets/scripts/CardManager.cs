using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

public class CardManager : NetworkBehaviour
{

    public List<Card> cards;
    public Stack<Card> discardPile = new Stack<Card>();
    public Stack<Card> DiscardPile => discardPile;
    //public readonly SyncList<Card> cards = new SyncList<Card>();

    private List<int> cardIndices;

    public static CardManager instance;

    //List of players stored on the server
    public List<Player> Players;

    private bool cardsDelivered = false;

    public static event Action<Player> _onCardsDelivered;
    public static event Action OnSpawnCards;
    public static event Action<Card> _onDiscardPileUpdated;
    public static event Action _onUpdatePlayerHand;
    public static event Action _onPlayerAddOpponent;
    public Player currentPlayer;

    public bool updateObjectives = false;

    private int oldPlayerCount = 0;

    private int cardCountSyncVarCallBackCounter = 0;
    private int playerInitializedCounter = 0;

    private bool gameStarted = false;
    public bool GameStarted => gameStarted;

    [SyncVar] private Card activeCard; //The active card is synced from server to client

    public void Awake()
    {
        instance = this;
        updateObjectives = false;
    }

    public void Start()
    {
        cards = new List<Card>();
        cardIndices = new List<int>();

        if(isServer)
        {
            InitializeCards();
            ShuffleCards();
            SortDeck();
            activeCard = new Card();
            //DiscardPileSlot._onDropEvent += PushActiveCardToDiscardPile;
        }
        //DiscardPileSlot._onDropEvent += PushActiveCardToDiscardPile;
    }


    public bool PlayersAreValid()
    {
        foreach(Player player in Players)
        {
            if(player.name == "")
                return false;
        }
        return true;
    }

    //[Server]
    public void Update()
    {   

        if(isServer)
        {
            if(Players.Count == 1)
            {
                currentPlayer = Players[0];
                currentPlayer.IsCurrentPlayer = true;
                GetComponent<NetworkIdentity>().AssignClientAuthority(currentPlayer.connectionToClient);
            }

            if((Players.Count > oldPlayerCount) && PlayersAreValid() && Players[oldPlayerCount].startCalled)
            {   
                
                if(Players.Count > 1)
                    rpcPlayerAddOpponent();
                    
                Debug.Log("Players[" + oldPlayerCount + "].startCalled = " + Players[oldPlayerCount].startCalled);
                Debug.Log("Cards delivered to Player " + Players.Count);
                Debug.Log("oldPlayerCount =  " +  oldPlayerCount);
                DeliverCards(oldPlayerCount);//This can also be a targetrpc, we only call it on a specific client
                //Players[oldPlayerCount].Initialized = true;

                //rpcDeliverCards(Players[oldPlayerCount].connectionToClient);//Deliver cards only on new client who joined

                //UpdateHandDisplay(oldPlayerCount);
                //Players[oldPlayerCount].updateHandDisplay();  

                /*if(Players.Count == 2)
                {   
                    displayOpponentInfoForPlayer(0, 1);
                    displayOpponentInfoForPlayer(1, 0);
                }

                else if(Players.Count > 2)
                {
                    for(int i = 0; i < Players.Count-1; i++)
                    {
                        displayOpponentInfoForPlayer(i, Players.Count-1);
                        displayOpponentInfoForPlayer(Players.Count-1, i);
                    }
                }*/

                if(Players.Count == 2)
                    gameStarted = true;

                oldPlayerCount = Players.Count;  
            }
            //currentPlayer = Players[0];
        }


    }

    [ClientRpc]
    private void rpcPlayerAddOpponent()
    {
        _onPlayerAddOpponent.Invoke();
    }

    [Server]
    public void updatePlayerInfo()
    {
        for(int i = 0; i < Players.Count; i++)
        {
            //Players[i].updateOtherPlayerInfo();
        }
    }

    private void UpdateHandDisplay(int player)
    {
        //if(!Players[player].hasAuthority) return;
        Players[player].updateHandDisplay();
    }

    [Server]
    public void swapPlayers()
    {
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
        currentPlayer.IsCurrentPlayer = false;

        if(currentPlayer == Players[0])
            currentPlayer = Players[1];
        else
            currentPlayer = Players[0];

         Debug.Log("Current Player is " + currentPlayer.Name);
         currentPlayer.IsCurrentPlayer = true;
        
        GetComponent<NetworkIdentity>().AssignClientAuthority(currentPlayer.connectionToClient);
    }

    public void setActiveCard(int number, string color, bool isJoker, bool isSkipCard)
    {
        Debug.Log("CM => setting active card");
        rpcSetActiveCard(number,color,isJoker,isSkipCard);
    }

    [ClientRpc]
    private void rpcSetActiveCard(int number, string color, bool isJoker, bool isSkipCard)
    {
        activeCard.Number = number;
        activeCard.Color = color;
        activeCard.IsJoker = isJoker;
        activeCard.IsSkipCard = isSkipCard;
    }

    public void PushActiveCardToDiscardPile()//Add [Server] attribute
    {

        discardPile.Push(activeCard);

        //VERY IMPORTANT TO SYNC UI:
        //rpcUpdateDiscardPile(); //Since this is called on a specific client, it can be TargetRPC
        rpcUpdateDiscardPile();
        rpcUpdatePlayerHand(currentPlayer.connectionToClient);
        swapPlayers();

        Debug.Log("Upper card from discard pile => " + discardPile.Peek().Number);
       
    }

    [ClientRpc]
    private void rpcUpdateDiscardPile()
    {
         _onDiscardPileUpdated.Invoke(activeCard);
         //_onUpdatePlayerHand.Invoke();

        //_onDiscardPileUpdated.Invoke(activeCard);
        //_onUpdatePlayerHand.Invoke();
    }

    [TargetRpc]
    private void rpcUpdatePlayerHand(NetworkConnection conn)
    {

        Debug.Log("rpc Update Player Hand");
        _onUpdatePlayerHand.Invoke();
    }

    //[Server]
    public void AddActiveCardToDeck()
    {
        Debug.Log("Card: " + activeCard.Number + ", " + activeCard.Color + " added to deck!");
        cards.Add(activeCard);
    }

    //private void cmdPushActiveCardToDiscardPile()
    //Deliver 10 cards to the player and remove them from the deck
    //[ClientRpc]
    //Problem: The cards only exist on the server and not on the client, Solution: Remove ClientRpc
    public void DeliverCards(int player)
    {
        for(int j = 0; j < 10; j++)
        {
            //Debug.Log("card details(card): " + cards[0].Number + " " + cards[0].Color);
            Players[player].addCardToHand(cards[0].Number, cards[0].Color, cards[0].IsJoker, cards[0].IsSkipCard);
            cards.Remove(cards[0]);
        }

        Debug.Log("There are " + cards.Count  + " cards left");   
        _onCardsDelivered.Invoke(Players[player]);

    }

    [Server]
    public void playerInitialized()
    {
        playerInitializedCounter++;
        Debug.Log("Player initialized!");

        if(Players.Count > 1 && playerInitializedCounter == Players.Count*Players.Count)
        {
            if(Players.Count == 2)
            {   
                displayOpponentInfoForPlayer(0, 1);
                displayOpponentInfoForPlayer(1, 0);
            }

            else if(Players.Count > 2)
            {
                for(int i = 0; i < Players.Count-1; i++)
                {
                    displayOpponentInfoForPlayer(i, Players.Count-1);
                    displayOpponentInfoForPlayer(Players.Count-1, i);
                }
            }
        }
    }

    [Server]
    public void cardCountUpdatedOnClient()
    {
        //Debug.Log("cardCountUpdatedOnClient");
        cardCountSyncVarCallBackCounter++;
        if(Players.Count > 1 && cardCountSyncVarCallBackCounter == Players.Count)
        {
            //updatePlayerInfo();
            if(Players.Count < 3)
            {   
                //displayOtherPlayerInfoForPlayer(Players[0].connectionToClient, 0, 1);
	            //displayOtherPlayerInfoForPlayer(Players[1].connectionToClient, 1, 0);
            }
            cardCountSyncVarCallBackCounter = 0;
        }
    }

    [Server]
    private void displayOpponentInfoForPlayer(int player, int otherPlayer)
    {
        Players[player].rpcDisplayOpponentInfo(Players[otherPlayer].Name);
    }

    //Sort the cards on the deck so that the card with deckindex 1 is followed by the card with deckindex 2 and so on
    //Algorithm: bubble sort
    private void SortDeck()
    {
        Card temp;

        for (int i= 0; i < cards.Count; i++) {
            for (int j = 0; j < cards.Count - 1; j++) {
                if (cards[j].DeckIndex > cards[j + 1].DeckIndex) {
                    temp = cards[j + 1];
                    cards[j + 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }
    }

    //shuffle the cards whenever a player won his phase or at game start
    private void ShuffleCards()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            int deckIndex = UnityEngine.Random.Range(0, cards.Count);

            while(cardIndices.Contains(deckIndex))
            {
                deckIndex = UnityEngine.Random.Range(0, cards.Count);
            }
            cards[i].DeckIndex = deckIndex;
            cardIndices.Add(deckIndex);
        }
        
    }

    //The game consists of 108 cards
    private void InitializeCards()
    {

        for(int i = 0; i < 12; i++)
        {
            cards.Add(new Card(i+1,"red"));
            cards.Add(new Card(i+1,"red"));
            cards.Add(new Card(i+1,"blue"));
            cards.Add(new Card(i+1,"blue"));
            cards.Add(new Card(i+1,"green"));
            cards.Add(new Card(i+1,"green"));
            cards.Add(new Card(i+1,"yellow"));
            cards.Add(new Card(i+1,"yellow"));
        }

        //Add all jokers
        for(int i = 0; i < 8; i++)
        {   
            Card card = new Card();
            card.IsJoker = true;
            cards.Add(card);
        }

        //Add all skip cards
        for(int i = 0; i < 4; i++)
        {
            Card card = new Card();
            card.IsSkipCard = true;
            cards.Add(card);
        }
    }


}
