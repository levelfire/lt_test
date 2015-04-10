using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server_Form.GameInse;
using Server_Form.GameData;

namespace Server_Form.Protorol
{
    public struct PlayerBaseInfo
    {
        public int nPlayerID;
        public int nResourceID;
        // 名字之后考虑 [12/2/2011 test]
        //public int nLenthName;
        //public string szName;
    }
    public struct PlayEnterSubStep
    {
        public int nStageID;
        public int nStepID;
        public int nSubStepID;
    }
    public struct DiffMake
    {
        public int nDiffID;
        public int nRightOrLeft;

        public void Make(Diff pDiff)
        {
            if (null != pDiff)
            {
                nDiffID = pDiff.ID;
                nRightOrLeft = pDiff.RightOrLeft;
            }
        }
    }

    public struct MonsterMake
    {
        public int nIndex;
        public int nID;
        public int nBody;
        public int nSpeed;
        public int nMoveWay;
        public int nHitEffect;
        public int nHitTime;
        public int nX;
        public int nY;
        public int nDirection;

        public void Make(Monster pMonster)
        {
            if (null != pMonster)
            {
                nIndex = pMonster.Index;
                nID = pMonster.ID;
                nBody = pMonster.Body;
                nSpeed = pMonster.Speed;
                nMoveWay = pMonster.MoveWay;
                nHitEffect = pMonster.HitEffect;
                nHitTime = pMonster.HitTime;
                nX = pMonster.X;
                nY = pMonster.Y;
                nDirection = pMonster.Direction;
            } 
        }
    }

    public struct PlayerMake
    {
        public int nID;
        public int nTeamIndex;
        public int nResourceID;
        public int nX;
        public int nY;

        public void Make(Player p)
        {
            if (null != p)
            {
                nID = p.NPlayerID;
                if (null != p.CurTeam)
                {
                    nTeamIndex = p.CurTeam.nTeamIndex;
                }
                else
                {
                    nTeamIndex = 0;
                }
                nResourceID = p.NResourceID;

                Random r = new Random(Guid.NewGuid().GetHashCode());
                nX = r.Next(100, SubStepData.width);
                nY = r.Next(100, SubStepData.height);
            }
        }
    }

    public struct BossMake
    {
        public int nID;
        public int nLoop;

        public void Make(BossData p)
        {
            nID = p.m_nID;
            nLoop = p.m_nFireLoop;
        }
    }

    public struct MonsterMoveChange
    {
        public int nMonsterIndex;
        public int nAngle; 
    }

    public struct PlayStepEnd
    {
        public int nResult;
        public int nStageID;
        public int nStepID;
    }

    public struct BattleEndInfo
    {
        public int nPointSelf;
        public int nPointEnemy;
    }

    public struct PlayerMoveInfo
    {
        public int nResult;
        public int nPlayerID;
        public int nFromX;
        public int nFromY;
        public int nToX;
        public int nToY;
    }
    public struct PlayerStopMoveInfo
    {
        public int nPlayerID;
        public int nX;
        public int nY;
    }
    public struct PlayerKnockInfo
    {
        public int nPlayerID;
        //没有敲中返回0，中了返回不同点ID
        public int nResult;
    }
    public struct RobortMoveInfo
    {
        public int nPlayerID;
        public int nX;
        public int nY;
    }

    public struct PlayerRoomStateChange
    {
        public int nPlayerID;
        public int nState;
    }

    public struct TeamSeatCountChange
    {
        public int nCount;
    }
    public struct PlayerBeKicked
    {
        public int nPlayerID;
    }
    public struct TeamDisband
    {
        //解散时状态 1.普通状态下 2.排队状态下 3.比赛后
        public int nResult;
    }
    public struct TeamStateChange
    {
        public int nState;
    }
    public struct KickAndCloseBroadCast
    {
        public int nPlayerID;
        public int nSeatCount;
    }

    public struct PlayerStageStepUpdate
    {
        public int nStageID;
        public int nStepID;
        public int nTime;
    }

    public struct PlayerStepGrade
    {
        public int nStageID;
        public int nStepID;
        public int nGrade;
    }

    public struct SubStepClear
    {
        public int nResult;
    }

    public struct OtherUserItemCommon
    {
        public int nPlayerID;
        public int nUseType;
        public int nParam;
    }

    public struct BossFire
    {
        public int nBossID;
    }
}
