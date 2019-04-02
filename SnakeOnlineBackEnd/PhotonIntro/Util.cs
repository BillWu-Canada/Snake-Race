using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using Photon.SocketServer;



namespace PhotonIntro
{
    class Util
    {
        private readonly static ILogger log = LogManager.GetCurrentClassLogger();

        public static void BroadCastEvent(ReusableList<UnityClient> list, EventData startEvent, SendParameters sendP)
        {
            log.Debug("BROADCASTING: " + startEvent.Code + " PARAMETER COUNT: " + startEvent.Parameters.Count);
            //send event to everyone that is connected
            foreach (UnityClient p in list)
            {
                if (p != null && p.Connected) {
                    SendResult res = p.SendEvent(startEvent, sendP);
                    log.Debug("BROADCASTING TO IP: " + p.RemoteIP);
                    if (res != SendResult.Ok)
                    {
                        log.Error("SEND EVENT FAILED: " + res);
                    }
                }
            }
        }
    }
}
