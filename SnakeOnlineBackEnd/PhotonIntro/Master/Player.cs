using ExitGames.Logging;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotonIntro
{
    public class Player
    {
        // the index a player in a game Room
        private readonly ILogger log = LogManager.GetCurrentClassLogger();

        private int playerID;
        public int PlayerID
        {
            get { return playerID; }
            set { playerID = value; }
        }

        private Game _game = null;
        internal Game game {
            get {
                if (_game == null) {
                    log.Error("GAME NOT START YET");
                    throw new Exception("GAME NOT START YET");
                }
                return _game;
            
            }
            set { _game = value; }
        }

        private GameRoom _gameRoom;
        public GameRoom gameRoom { get { return _gameRoom;} set { _gameRoom = value ;} }

        private UnityClient _client;
        public string RemoteIP {
            get
            {
                return _client.RemoteIP;
            }
        }
        public bool Connected {
            get
            {
                return _client.Connected;
            }
        }

        
        public Player(UnityClient client) {
            _client = client;
        }



        internal void SnakeMove(int moveID)
        {
            //only valid when _game is started ie when it's defined
            log.Debug("PLAYER: " + PlayerID + " MOVING: " + moveID);

            game.PlayerEvent(playerID, moveID);
        }


        internal int StartRequest()
        {
            log.Debug("START REQUEST FROM PLAYER: " + playerID);
            return gameRoom.StartRequest(playerID);
        }

        internal int LeaveGameRoom()
        {
 
            int left = gameRoom.RemovePlayer(this);

            return left;
        }

        internal void JoinGameRoom(GameRoom gameRoom) {
            playerID = gameRoom.AddPlayer(this);
            if (playerID == -1) {
                EnterLobbyView();
            }
        }

        internal void EnterLobbyView()
        {
            _client.EnterLobbyView();
        }
    
        internal SendResult SendEvent(EventData Event,SendParameters sendP)
        {
            return _client.SendEvent(Event, sendP);
        }
    }
}
