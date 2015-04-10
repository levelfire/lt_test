using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.Protorol
{
    public struct PlayerLoginResult
    {
        public int nResult;
        public int nPlayerID;
        public int nResourceID;
        public int nBody;
        public int nLogStageID;
        public int nLogStepID;
    }

    public struct PlayerStepResult
    {
        public int nResult;
    }

    public struct PlayerMoveResult
    {
        public int nResult;
    }
    public struct PlayerStopMoveResult
    {
        public int nPlayerID;
        public int nX;
        public int nY;
    }

    public struct PlayerKnockResult
    {
        //没有敲中返回0，中了返回不同点ID
        public int nResult;
    }
    public struct PlayerQueryTeamResult
    {
        public int nPlayerID;
        public int nTeamCount;
    }
    struct PlayerTeamListInfo
    {
        public int nBattleType;
        public int nTeamIndex;
        public int nMaxMemberCount;
        public int nMember;
        public int nHasPassword;
        //public int nNameLenth;
        //public string szName;

        public void Make(Team pTeam)
        {
            nTeamIndex = pTeam.nTeamIndex;
            nMaxMemberCount = pTeam.nMaxMemberCount;
            nMember = pTeam.cdicMembers.Count;
            if (null != pTeam.szPassword)
            {
                nHasPassword = 1;
            } 
            else
            {
                nHasPassword = 0;
            }
            ////nNameLenth = pTeam.szName.Length;
            //byte[] name = System.Text.Encoding.UTF8.GetBytes(pTeam.szName);
            //nNameLenth = name.Length;
            //szName = pTeam.szName;

            // 目前只有对抗，之后改 [12/2/2011 test]
            nBattleType = (int)Team.BattleType.Opposition;
        }
    }

    struct PlayerTeamNameInfo
    {
        public int nNameLenth;
        public byte[] btName;

        public void Make(Team pTeam)
        {
            //nNameLenth = pTeam.szName.Length;
            byte[] name = System.Text.Encoding.UTF8.GetBytes(pTeam.szName);
            nNameLenth = name.Length;
            btName = name;
        }
    }

    public struct PlayerCreateTeamResult
    {
        public int nPlayerID;
        public int nBattleType;
        public int nTeamIndex;
        public int nMemberCount;
    }
    public struct PlayerJoinTeamResult
    {
        // 不传玩家ID了，改为返回结果 
        //-6.在bandList中
        //-5.人数已满 
        //-4.指定队伍不存在 
        //-3.没有队伍 
        //-2.需要密码 
        //-1.密码错误 
        //0.失败 
        //1.成功  
        //[12/2/2011 test]
        public int nResult;
        //public int nPlayerID;
        public int nTeamIndex;
        public int nLeaderID;
        public int nMemberCount;
        public int nCurMaxCount;
    }

    public struct PlayerReadyResult
    {
        public int nResult;
    }
    public struct TeamReadyResult
    {
        public int nResult;
    }

    public struct OpenSeatResult
    {
        public int nResult;
        //public int nSeat;
    }
    public struct CloseSeatResult
    {
        public int nResult;
        //public int nSeat;
    }
    public struct KickPlayerResult
    {
        public int nResult;
        //public int nSeat;
    }

    public struct QuitTeamResult
    {
        public int nResult;
    }
    public struct KickAndCloseResult
    {
        public int nResult;
    }

    public struct UseItemResultCommon
    {
        public int nResult;
        public int nUseType;
        public int nParam;
    }
}
