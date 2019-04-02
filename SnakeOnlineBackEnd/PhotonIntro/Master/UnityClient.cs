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
    public class UnityClient : PeerBase
    {
        private readonly ILogger log = LogManager.GetCurrentClassLogger();
        

        protected int clientID;
        public int ClientID{
            get { return clientID; }
            set { clientID = value; }
        }

        public bool deleted;
        //copy of lobby
        protected Lobby _lobby;
        public Lobby lobby { get { return _lobby; } set { _lobby = value; } }

        public Player _player;
        public UnityClient(IRpcProtocol protocol, IPhotonPeer peer) : base(protocol, peer) 
        {
            log.Debug("Connection Received from: " + peer.GetRemoteIP());
            deleted = false;
            _player = null;
        }

        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {

            if (deleted) {
                return;
            }

            this.handleDisconnection();
            
        }

        public void handleDisconnection()
        {
            log.Debug("DISCOON _player is null?: " + (_player == null));
            if (this._player != null)
            {   
                int playerNumLeft = this.LeaveGameRoom();
                log.Debug("PLAYER LEFT NOW WITH: " + playerNumLeft);

            }

            lobby.RemoveClient(this);
            log.Debug("client disconnencgtted Now with: " + lobby.clientCount);
            lobby.connectedClients.Remove(this.RemoteIP);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch (operationRequest.OperationCode) {
                case (int)Constants.OPERATION_CODE.JOINROOM:
                    if (operationRequest.Parameters.ContainsKey(0))
                    {
                        log.Debug("JOINROOM REQUEST RECEIVED: " + operationRequest.Parameters[0]);

                        int roomID = (int)operationRequest.Parameters[0];
                        lobby.AssignRoom(this, roomID);
                   
                    }
                    else
                    {
                        log.Error("JOINROOM REQUEST WITHOUT PARAMETER 0");
                    }
                    break;
                case (int)Constants.OPERATION_CODE.START:
                    if (operationRequest.Parameters.ContainsKey(1)) 
                    {
                        log.Debug("START REQUEST RECEIVED: " + operationRequest.Parameters[1]);

                        int startRequests = _player.StartRequest();

                        //start packaging the response
                        OperationResponse response = new OperationResponse(operationRequest.OperationCode);
                        response.Parameters = new Dictionary<byte, object> { { (int)Constants.OPERATION_CODE.START, startRequests } };
                        SendOperationResponse(response, sendParameters);
                    }
                    else
                    {
                        log.Error("START REQUEST WITHOUT PARAMETER 1");
                    }
                    break;

                case (int)Constants.OPERATION_CODE.MOVE:
                    if (operationRequest.Parameters.ContainsKey(2))
                    {
                        log.Debug("PLAYER: " + _player.PlayerID + " SENT ME: " + (int)operationRequest.Parameters[2]);
                        _player.SnakeMove((int)operationRequest.Parameters[2]);
                    }
                    else 
                    {
                        log.Error("MOVE REQUEST WITHOUT PARAMETER 2");
                    }

                    break;
                case (int)Constants.OPERATION_CODE.CREATEROOM:
                    if (operationRequest.Parameters.ContainsKey((int)Constants.OPERATION_CODE.CREATEROOM))
                    {
                        log.Debug("CREATEROOM REQUEST RECEIVED: " + operationRequest.Parameters[(int)Constants.OPERATION_CODE.CREATEROOM]);

                        int newRoomID = lobby.CreateRoom();
                        lobby.AssignRoom(this, newRoomID);
                      
                    }
                    else
                    {
                        log.Error("CREATEROOM REQUEST WITHOUT PARAMETER 4");
                    }
                    break;
                default:
                    log.Error("unknown operational request fk you");
                    break;
            }
        }


        public void JoinGameRoom(GameRoom gameRoom)
        {
            if (_player != null)
            {
                _player.LeaveGameRoom();
            }
            //bind the local copy
            _player = new Player(this);
            _player.JoinGameRoom(gameRoom);
        }

        internal int LeaveGameRoom()
        {

            int result = _player.LeaveGameRoom();
            _player = null;
            return result;
        }

        internal void EnterLobby(Lobby lobby)
        {
            _lobby = lobby;

            ClientID = lobby.AddClient(this);
        }



        internal void EnterLobbyView() 
        {
            Dictionary<byte, object> lobbyData = lobby.GetLobbyData();
            log.Debug("LOBBYDATA: " + lobbyData.Count);
            EventData joinLobbyEvent = new EventData((int)Constants.OPERATION_CODE.JOINLOBBY, lobbyData);
            SendParameters sendP = new SendParameters();
            SendResult res = this.SendEvent(joinLobbyEvent, sendP);
        }
    }
}
