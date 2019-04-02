
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PhotonIntro
{
    public class PhotonServer : ApplicationBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
         
        // IP : UnityClient
        private Lobby lobby;


        #region Overload of ApplicationBase
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            String IP = initRequest.PhotonPeer.GetRemoteIP();

            UnityClient newClient = new UnityClient(initRequest.Protocol, initRequest.PhotonPeer);

            Log.Debug("%%%%%%%%%THIS CONNECTING IP: " + IP);
            /*
            if (lobby.connectedClients.ContainsKey(IP))
            {
                Log.Debug("SAME IP CONNECTING ID: " + initRequest.PhotonPeer.GetConnectionID());

                UnityClient toDiscon = lobby.connectedClients[IP];
                toDiscon.deleted = true;

                toDiscon.handleDisconnection();
            }

            lobby.connectedClients[IP] = newClient;
            */
            newClient.EnterLobby(lobby);
            Log.Debug("NEW CLIENT ID: " + newClient.ClientID);

            return newClient;
        }

        protected override void Setup()
        {
            var file = new FileInfo(Path.Combine(BinaryPath, "log4net.config"));
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }


            lobby = new Lobby(this);
        }

        protected override void TearDown()
        {

        }


        #endregion

    }
}
