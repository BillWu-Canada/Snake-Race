using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotonIntro
{
    public class GameRoom
    {
        private readonly ILogger log = LogManager.GetCurrentClassLogger();

        public ReusableList<Player> playerList;
        public int playerNum { get { return playerList.solidCount; } }

        public List<bool> startRequestTable;

        //local copy of photoServer App
        private Lobby _lobby;
        // local copy of Game
        protected Game _game;

        private int roomID;
        public int RoomID
        {
            get { return roomID; }
            set { roomID = value; }
        }
        
        //how many start requests received
        private int startRequests;

        //IP  :  PlayerID( index in the playerList )
        protected Dictionary<string, int> IPTable;

        //if the game has begun
        public bool gameOn;

        public GameRoom(Lobby lobby) {
            gameOn = false;
            IPTable = new Dictionary<string, int>();
            _lobby = lobby;
            playerList = new ReusableList<Player>(Constants.MAX_PLAYERS, Constants.MAX_PLAYERS);
            startRequests = 0;
            startRequestTable = Enumerable.Repeat(false, Constants.MAX_PLAYERS).ToList();
        }

        /*called when client send a start game request.
        Return:
         * 0: if game starts
         * 1: startRequests 
        */
        public int StartRequest(int playerID) {
            if (gameOn) {
                log.Error("DONT SEND REQUEST,GAME ALREADY STARTED");
                return -2;
            }
            if (startRequestTable[playerID]) {
                log.Error("YOU CAN ONLY SEND ONE START GAME REQUEST");
                return -1;    
            }


            startRequestTable[playerID] = true;

            this.startRequests += 1;
            log.Debug("REQUEST: " + startRequests + " VS " + playerNum);
            if (startRequests >= playerList.solidCount / 2) {
                startRequests = 0;

                this.StartGame();
                log.Debug("GAME STARTED");
            }

            return startRequests;
        }


        public int AddPlayer(Player player){
            // if in the IPTable
            int playerID;

            //if game already on and 
            if (IPTable.ContainsKey(player.RemoteIP))
            {
                if (!gameOn) {
                    log.Error("CONNECT TO A GAME THAT's NOT ON!!!!");
                    return -2;
                }
                //get the old playerID
                playerID = IPTable[player.RemoteIP];

                //reattach it to the list
                playerList.Add(player, playerID);
                player.game = _game;                
                //tell the model plz
                _game.PlayerEvent(player.PlayerID, (int)Constants.PLAYEREVENTID.RECON);
                log.Debug("PLAYER: " + player.PlayerID + "RECONNECTED TO GAME: " + roomID);

                _game.broadCastStartEvent();
            }
            else
            {
                playerID = playerList.Add(player);
                //if full, back to lobby
                if (playerID == -1) 
                {
                    log.Error("THIS GAME ROOM ID: " + RoomID + " IS FULL");
                    player.EnterLobbyView();
                    return -1;
                }
                log.Debug("JOINED THIS ROOMID: " + RoomID + " SENT PLAYER NUMBER : " + playerList.solidCount);

                //Tell everyone's client, new guy joined. Display it
                EventData joinEvent = new EventData((int)Constants.OPERATION_CODE.JOINROOM, new Dictionary<byte, object> { { 0, roomID },  {1, playerList.solidCount} });
                SendParameters sendP = new SendParameters();
                this.BroadCastEvent(joinEvent, sendP);
            }

            player.gameRoom = this;

            return playerID;
        }


        private int StartGame() {

            gameOn = true;

            _game = new Game(this);

            //assign the game
            //populate the IPTables
            foreach (Player p in playerList) {
                if (p != null) {
                    p.game = _game;

                    //POPULATE IPTABLE KEY
                    /*
                    _lobby.IPTable[p.RemoteIP] = RoomID;
                    IPTable[p.RemoteIP] = p.PlayerID;
                    */
                }
            }

            _game.Start();

            //broadCast start event
            _game.broadCastStartEvent();

            //Tell other client to delete this room from the lobby
            EventData disableRoomEvent = new EventData((int)Constants.OPERATION_CODE.DISABLEROOM, new Dictionary<byte, object>(){{0, roomID}});
            SendParameters disableSendP = new SendParameters();
            Util.BroadCastEvent(_lobby.Clients, disableRoomEvent, disableSendP);


            log.Debug("GUYS GAME STARTED, # PLAYER: " + playerList.solidCount);
            return playerList.solidCount;
        }

        public void BroadCastEvent(EventData startEvent, SendParameters sendP)
        {
            //send event to everyone that is connected
            foreach (Player p in playerList) {
                if (p != null && p.Connected) {
                    SendResult res = p.SendEvent(startEvent, sendP);
                    if (res != SendResult.Ok)
                    {
                        log.Error("SEND EVENT FAILED: " + res);
                    }
                }
            }
        }

        /*
         * remove a player from the reusabe list
         * Return :
         *      # of player left in the list
         */
        public int RemovePlayer(Player player)
        {
            // if game is On, Don't reuse the spot
            // to left the old client reconnect
            log.Debug("REMOVING PLAYER: " + player.PlayerID);
            playerList.remove(player.PlayerID, !gameOn);
            player.gameRoom = null;
            
            if (gameOn)
            {
                // tell the model that he left
                _game.PlayerEvent(player.PlayerID, (int)Constants.PLAYEREVENTID.LEAVE);
            }
            int playerLeft = playerList.solidCount;
            if (playerLeft == 0) {
                if (gameOn)
                {
                    this.endGame();
                }
                else
                {
                    this.DropRoom();
                }
            }


            EventData leaveRoomEvent = new EventData((int)Constants.OPERATION_CODE.LEAVEROOM, new Dictionary<byte, object> { { 0, roomID }, { 1, playerList.solidCount } });
            SendParameters sendP = new SendParameters();
            this.BroadCastEvent(leaveRoomEvent, sendP);

            return playerLeft;
        }

        private void endGame()
        {

            log.Debug("##########GAME ENDING############");
            log.Debug("LOBBY IPTABLE COUNT: " + _lobby.IPTable.Count + " THIS IPTABLE COUNT: " + IPTable.Count);

            _game.End();
            this.DropRoom();
        }

        private void DropRoom()
        {
            //REMOVING KEYS FROM IPTABLE
            /*
            foreach (string key in IPTable.Keys.ToArray())
            {
                
                _lobby.IPTable.Remove(key);
                IPTable.Remove(key);
            }
            */

            _lobby.DropRoom(this);
        }




        internal void BroadcastUpdate(Dictionary<byte, object> replyData)
        {
            //Send everyone the map
            EventData mapUpdateEvent = new EventData((int)Constants.OPERATION_CODE.MOVE, replyData);
            SendParameters sendP = new SendParameters();
            this.BroadCastEvent(mapUpdateEvent, sendP);
            log.Debug("SENT THE MAP AND EVENT TO EVERYONE");
        }
    }
}
