using ExitGames.Logging;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PhotonIntro
{
    public class Game
    {
        private readonly ILogger log = LogManager.GetCurrentClassLogger();

        private SnakeMap _snakeMap;
        private Timer updateTimer;

        private List<int> eventPool;
        private Queue<int>[] eventQs;

        private GameRoom _gameRoom;
        private int playerCount;
        private static int updateInterval = 200;


        private Dictionary<int, int> playerIDtoIndex;
        public int getPlayerIndex(int playerID) {
            return playerIDtoIndex[playerID];
        }

        public void broadCastStartEvent() {
            foreach (Player p in _gameRoom.playerList)
            {
                if (p != null && p.Connected)
                {
                    //Tell everyone we are starting the game
                    EventData startEvent = new EventData((int)Constants.OPERATION_CODE.START, new Dictionary<byte, object>() { { 0, playerIDtoIndex[p.PlayerID] }, { 1, playerCount } });
                    SendParameters sendP = new SendParameters();
                    p.SendEvent(startEvent, sendP);
                }
            }
        }
        
        public Game(GameRoom gameRoom) {

            _gameRoom = gameRoom;

            playerIDtoIndex = new Dictionary<int, int>();

            playerCount = 0;
            foreach (Player p in _gameRoom.playerList)
            {
                if (p != null && p.Connected)
                {
                    playerIDtoIndex[p.PlayerID] = playerCount;

                    playerCount += 1;
                }
            }



            updateTimer = new Timer(updateInterval);
            updateTimer.Elapsed += updateEvent;

            log.Debug("PLAYER COUNT: " + playerCount);
            _snakeMap = new SnakeMap();
            //construct the default values for the datas
            //NOTE: the movePool is the size of the active player num in the game, not the gameRoom length
            //CAN MAKE EVENTPOOL A LIST OF EVENTS SO YOU JUST COUNT HOW MANY ARE NOT NULL
            eventPool = Enumerable.Repeat(-1, playerCount).ToList();
            eventQs = new Queue<int>[playerCount];
            for (int i = 0; i < playerCount; i++) 
            {
                eventQs[i] = new Queue<int>();    
            }
            

        }

        private void updateEvent(Object source, ElapsedEventArgs e)
        {
            log.Debug("*********UPDATE EVENT******* ");
            DetachEvents();
            ApplyEvents(eventPool);
            PopulateDefaultEvents();
        }

        private void DetachEvents()
        {

            for (int i = 0; i < playerCount; i++) 
            {
                if (eventQs[i].Count > 0)
                {
                    log.Debug("PLAYER: " + i + " DEQUEUEING EVENT: " + eventQs[i].First());

                    eventPool[i] = eventQs[i].Dequeue();
                }
            }
        }

        private void PopulateDefaultEvents()
        {
            for (int i = 0; i < eventPool.Count; i++)
            {
                eventPool[i] = -1;
            }
        }

        public void Start(){
            updateTimer.Start();

        }

        private bool notSameAsLastCommand(int eventID, Queue<int> Q) {
            return (!(Q.Count > 0) || eventID != Q.Last());
        }

        public void PlayerEvent(int playerID, int eventID)
        {
            int playerIndex = playerIDtoIndex[playerID];
            if (eventQs[playerIndex].Count < 2 && notSameAsLastCommand(eventID, eventQs[playerIndex]))
            {
                log.Debug("PLAYER INDEX: " + playerIndex + " ENQUEUEING EVENT: " + eventID);

                eventQs[playerIndex].Enqueue(eventID);
            }

            /*
            //update the data
            if (!playerEvented[playerID])
            {
                eventPool[playerID] = eventID;
                eventRequestCount += 1;
                playerEvented[playerID] = true;
            }else
            {
                //if it just moved
                log.Error("TTTTHIS PLAYER SENT MULTIPLE REQUESTS: " + playerID + " move " + eventID);
                return;
            }

            log.Debug("Player Event: eventRequestCount:" + eventRequestCount + " _gameRoom.playerNum: " + _gameRoom.playerNum);
            if (eventRequestCount >= _gameRoom.playerNum)
            {
                log.Debug("MAKING MOVES TOGETHER TO THE MODEL: " + playerEvented[0]);

                //service the moves together to MODEL
                ApplyEvents(eventPool);
                //get back to the defaults
                PopulateDefault();
            }
            */

        }

        public void ApplyEvents(List<int> moves)
        {

            Dictionary<byte, object> replyData = _snakeMap.Map_Update(moves);

            //Dictionary<byte, object> replyData = _snakeMap.Map_Update(moves);
            int[] map = (int[])replyData[0];
            for (int i = 0; i < 15; i++) {
                string row = "";

                for (int j = 0; j < 15; j++) {
                    row += map[15* i + j].ToString();
                }

                log.Debug(row);
            }

            int[] events = (int[])replyData[1];
            for (int i = 0; i < events.Count(); i++)
            {
                if (events[i] == (int)Constants.EVENT.WIN) {
                    log.Debug("THIS DUDE PLAYER: " + i + " JUST WON");
                    this.End();
                }
            }
            _gameRoom.BroadcastUpdate(replyData);
        }

        internal void End()
        {
            updateTimer.Stop();
        }
    }
}
