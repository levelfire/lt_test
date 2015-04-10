using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.Protorol
{
    public struct PlayerLogin
    {
        public int nLenthName;
        public int nLenthPassword;
        public string szName;
        public string szPassword;
    }
    public struct PlayerCreateStep
    {
        public int nPlayerID;
        public int nStageID;
        public int nStepID;
    }
    public struct PlayerMove
    {
        public int nPlayerID;
        public int nFromX;
        public int nFromY;
        public int nToX;
        public int nToY;
    }
    public struct PlayerStopMove
    {
        public int nPlayerID;
        public int nX;
        public int nY;
    }
    public struct PlayerKnock
    {
        public int nPlayerID;
        public int nStageID;
        public int nStepID;
        public int nSubStepID;
        public int nDiffIID;
        public int nKnockX;
        public int nKnockY;
    }

    public struct Crash
    {
        public int nPlayerID;
        public int nMonsterIndex;
        public int nPlayerX;
        public int nPlayerY;
    }
    public struct PlayerQueryTeam
    {
        public int nPlayerID;
    }
    public struct PlayerCreateTeam
    {
        public int nPlayerID;
        public int nBattleType;
        public int nMemberCount;
        public int nNameLenth;
        public int nPasswordLenth;
        public string szName;
        public string szPassword;
    }
    public struct PlayerJoinTeam
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct PlayerJoinTeamWithPassword
    {
        public int nPlayerID;
        public int nTeamIndex;
        public int nLenth;
        public string szPassword;
    }
    public struct PlayerJoinTeamRandom
    {
        public int nPlayerID;
    }
    public struct PlayerQuitTeam
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct PlayerReady
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct PlayerNotReady
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct TeamReady
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct TeamNotReady
    {
        public int nPlayerID;
        public int nTeamIndex;
    }
    public struct OpenSeat
    {
        public int nTeamIndex;
        //public int nSeat;
    }
    public struct CloseSeat
    {
        public int nTeamIndex;
        //public int nSeat;
    }
    public struct KickPlayer
    {
        //public int nTeamIndex;
        //public int nSeat;
        public int nPlayerID;
    }
    public struct KickAndClose
    {
        public int nPlayerID;
    }

    public struct SinglePlayerTimeOut
    {
        public int nPlayerID;
    }

    public struct QueryNextSubStep
    {
        public int nPlayerID;
    }

    public struct UseItem_Find
    {
        public int nPlayerID;
    }

    public struct UseItemCommon
    {
        public int nUseType;
    }

    public struct QueryStageScore
    {
        public int nStageID;
    }
}
