using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotonIntro
{
    public static class Constants
    {
        public static int MAX_PLAYERS { get { return 4; } }

        //
        public enum OPERATION_CODE
        {
            JOINROOM,    //0
            START,   //1
            MOVE,    //2
            RECON,    //3
            CREATEROOM, //4
            JOINLOBBY,   //5
            DELETEROOM,   //6
            DISABLEROOM,     //7
            LEAVEROOM          //8
        }

        // the color mapping for clients to display
        public enum COLOR
        {
            EMPTY,      //0
            P1,         //1
            P2,         //2
            P3,         //3
            P4,         //4
            FOOD        //5
        }

        // the events controller send to Clients
        public enum EVENT
        {      
            NONE,       //0
            NORMAL,     //1
            ATE,        //2
            CUT,        //3
            BECUT,      //4
            KILLED,      //5
            WIN          //6
        }

        //For Controller to talk to Model
        public enum PLAYEREVENTID
        {
            LEAVE = -2,     //-2
            NONE,           //-1
            UP,             //0
            RIGHT,          //1
            DOWN,           //2
            LEFT,           //3
            RECON           //4

        }
    }
}
