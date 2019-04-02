using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Photon.SocketServer;

namespace PhotonIntro
{
    public class Lobby
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Dictionary<String, UnityClient> connectedClients;
        

        public PhotonServer _photonServer;

        //And Room
        private ReusableList<GameRoom> gameRooms;
        public int freeRoomCount
        {
            get
            {
                int res = 0;
                foreach (GameRoom room in gameRooms)
                {
                    if (room != null && !room.gameOn)
                    {
                        res += 1;
                    }
                }
                return res;
            }
        }

        public int roomCount
        {
            get
            {
                return gameRooms.solidCount;
            }
        }

        //collection for Clients
        public ReusableList<UnityClient> Clients;
        public int clientCount
        {
            get
            {
                return Clients.solidCount;
            }
        }

        //IP : roomID
        internal Dictionary<string, int> IPTable;


        public Lobby(PhotonServer photonServer)
        {
            this._photonServer = photonServer;
            this.Clients = new ReusableList<UnityClient>(Constants.MAX_PLAYERS);
            IPTable = new Dictionary<string, int>();
            gameRooms = new ReusableList<GameRoom>(0);
            connectedClients = new Dictionary<String, UnityClient>();

        }


        internal int CreateRoom()
        {
            GameRoom newRoom = new GameRoom(this);
            // if reuse stack has a pos, use it
            // else append to a new pos
            newRoom.RoomID = gameRooms.Add(newRoom);
            Log.Debug("ROOM CREATED: " + newRoom.RoomID + " SOLID COUNT: " + gameRooms.solidCount);

            EventData createRoomEvent = new EventData((int)Constants.OPERATION_CODE.CREATEROOM, GetLobbyData());
            SendParameters sendP = new SendParameters();
            Util.BroadCastEvent(Clients, createRoomEvent, sendP);



            return newRoom.RoomID;

        }

        internal void DropRoom(GameRoom gameRoom) {
            Log.Debug("@@@@@@@@@@@@@@@DROPING ROOM@@@@@@@@@@@@@@@@: " + gameRoom.RoomID);

            gameRooms.remove(gameRoom.RoomID);
            EventData dropRoomEvent = new EventData((int)Constants.OPERATION_CODE.DELETEROOM, new Dictionary<byte, object>(){{0, gameRoom.RoomID}});
            SendParameters sendP = new SendParameters();
            Util.BroadCastEvent(Clients, dropRoomEvent, sendP);
        }


        internal void AssignRoom(UnityClient newClient, int roomID)
        {
            newClient.JoinGameRoom(gameRooms[roomID]);
        }

        
        internal int AddClient(UnityClient newClient)
        {
            //add to client list regardless
            int clientID = Clients.Add(newClient);
            newClient.ClientID = clientID;

            //if it's reconnecting, recon to the room direcly
            if (IPTable.ContainsKey(newClient.RemoteIP))
            {
                int roomID = IPTable[newClient.RemoteIP];
                newClient.JoinGameRoom(gameRooms[roomID]);

            }
            else
            {
                //TODO: entering lobby view instead of joining random room
                newClient.EnterLobbyView();
                //this.CreateRoom();
                //newClient.JoinGameRoom(gameRooms[0]);
            }
            return clientID;
        }

        internal void RemoveClient(UnityClient unityClient)
        {
            Clients.remove(unityClient.ClientID);
        }

        internal Dictionary<byte, object> GetLobbyData()
        {
            Dictionary<byte, object> result = new Dictionary<byte, object>();
            Log.Debug("GAMEROOMS: " + gameRooms.solidCount);
            foreach (GameRoom room in gameRooms)
            {
                if (room != null)
                {
                    result[(byte)room.RoomID] = room.playerNum;
                }
            }
            return result;
        }




    

    }
}
