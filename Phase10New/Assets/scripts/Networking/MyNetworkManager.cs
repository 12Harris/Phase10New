using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{

   //[SerializeField] private GameObject enemySpawnerPrefab = null;
   private Player player;
   [SerializeField]private GameObject uiManagerPrefab;
   //GameObject cardArea;


   public override void OnServerAddPlayer(NetworkConnectionToClient conn)
   {
        base.OnServerAddPlayer(conn);
        player = conn.identity.GetComponent<Player>();

        player.name = "Player " + numPlayers;
        if(numPlayers == 1)
            player.Color = Color.blue;
        if(numPlayers == 2)
            player.Color = Color.green;
        if(numPlayers == 3)   
            player.Color = Color.yellow;
        if(numPlayers == 4)   
            player.Color = Color.magenta;

        //For each client spawn a individual UIManager
        //GameObject uIManager = Instantiate(uiManagerPrefab, conn.identity.transform.position, conn.identity.transform.rotation);
        //NetworkServer.Spawn(uIManager, conn);

        //spawn the unitspawner at the player position
        /*GameObject cardAreaPrefabInstance = Instantiate(cardAreaPrefab
            conn.identity.transform.position, 
            conn.identity.transform.rotation);

        NetworkServer.Spawn(cardAreaPrefabInstance , conn);*/

   }

}
