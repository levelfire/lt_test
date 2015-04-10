using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.Protorol
{
    public class ProtorlEnum
    {
        public struct FrameHead
        {
            public ulong nFrameLenth;
        }
        public struct FrameInfo
        {
            public byte nFrameGroup;
            public byte nFrameType;
        }

        public enum FrameGroup
        {
            FrameGroup_UserToServer = 0x00, //客户端到服务端
            FrameGroup_ServerResult = 0x01, //服务端返回客户端
            FrameGroup_ServerToUser = 0x02, //服务端到客户端
        }

        public enum FrameType_UserToServer
        {
            FrameType_Login = 0x00,             //玩家登陆
            FrameType_SingleEnterStep,          //单人开启关卡
            FrameType_PlayerMove,               //移动请求
            FrameType_PlayerKnock,              //敲击请求
            FrameType_PlayerStopMove ,          //停止移动请求
            FrameType_Crash,                    //碰撞
            FrameType_QueryTeam,                //询问队伍列表   
            FrameType_CreateTeam ,              //创建队伍
            FrameType_JoinTeam,                 //加入队伍(界面点击或指定Index，不带密码)
            FrameType_JoinTeamWithPassword,     //加入队伍(指定密码)
            FrameType_JoinTeamRandom,           //加入队伍(快速加入)
            FrameType_QuitTeam,                 //退队
            FrameType_PlayerReady,              //玩家单人准备
            FrameType_PlayerNotReady,           //玩家单人取消准备
            FrameType_TeamReady             = 14, //小队准备（进行排队）
            FrameType_TeamNotReady,             //小队取消准备（取消排队）
            FrameType_OpenSeat,                 //开启座位
            FrameType_CloseSeat,                //关闭座位
            FrameType_KickPlayer,               //踢人
            FrameType_KickAndClose,             //踢人并关闭座位

            FrameType_SingleTimeOut         = 21,//单人超时
            FrameType_QueryNextSubStep      = 22,//请求下一关

            //道具操作
            FrameType_UseItem               = 40,//使用道具统一协议

            FrameType_UseItem_Find          = 41,//放大镜
            FrameType_UseItem_Speed         = 42,//速度变化
            FrameType_UseItem_Invincible    = 43,//无敌
            FrameType_UseItem_AOE           = 44,//导弹
            FrameType_UseItem_Stun          = 45,//飞弹（跟踪）
            FrameType_UseItem_TimeSlow      = 46,//时间变慢
            FrameType_UseItem_AT            = 47,//保护罩
            FrameType_UseItem_Hammer        = 48,//巨锤（敲击范围）
            FrameType_UseItem_Trans         = 49,//传送

            //请求
            FrameType_QueryStageScore       = 51,//请求获取某大关分数

            FrameType_Keep = 100,            //玩家心跳          
        }
        public enum FrameType_ServerResult
        {
            FrameType_Result_Login = 0x00,              //登陆返回
            FrameType_Result_SingleEnterStep,           //开启关卡返回
            FrameType_Result_Move,                      //移动请求返回
            FrameType_Result_Knock,                     //敲击请求返回
            FrameType_Result_StopMove,                  //停止移动返回
            FrameType_Result_Crash,                     //碰撞返回
            FrameType_Result_QueryTeam,                 //询问队伍列表
            FrameType_Result_CreateTeam,                //创建队伍
            FrameType_Result_JoinTeam,                  //加入队伍(界面点击或指定Index，不带密码)
            FrameType_Result_JoinTeamWithPassword,      //加入队伍(指定密码)
            FrameType_Result_JoinTeamRandom,            //加入队伍(快速加入)
            FrameType_Result_QuitTeam,                  //退队
            FrameType_Result_PlayerReady,               //玩家单人准备返回
            FrameType_Result_PlayerNotReady,            //玩家单人取消准备返回
            FrameType_Result_TeamReady = 14,            //小队准备返回
            FrameType_Result_TeamNotReady,              //小队取消准备返回
            FrameType_Result_OpenSeat,                  //开启座位返回  
            FrameType_Result_CloseSeat,                 //关闭座位返回    
            FrameType_Result_KickPlayer,                //踢人返回
            FrameType_Result_KickAndClose,              //踢人并关闭座位返回

            //道具操作
            FrameType_Result_UseItem = 40,//使用道具统一协议

            FrameType_Result_UseItem_Find       = 41,         //使用放大镜返回
            FrameType_Result_UseItem_Speed      = 42,//速度变化
            FrameType_Result_UseItem_Invincible = 43,//无敌
            FrameType_Result_UseItem_AOE        = 44,//导弹
            FrameType_Result_UseItem_Stun       = 45,//飞弹（跟踪）
            FrameType_Result_UseItem_TimeSlow   = 46,//时间变慢
            FrameType_Result_UseItem_AT         = 47,//保护罩
            FrameType_Result_UseItem_Hammer     = 48,//巨锤（敲击范围）
            FrameType_Result_UseItem_Trans      = 49,//传送
        }
        public enum FrameType_ServerToUser
        {
            FrameType_PlayStepEnd               = 00, //关卡结束
            FrameType_PlayEnterSubStep          = 01, //玩家进入子场景
            FrameType_TeamEnterStep             = 02, //队伍进入战斗场景
            FrameType_PlayerMove                = 03, //玩家移动
            FrameType_PlayerStopMove            = 04, //玩家停止移动
            FrameType_PlayerKnock               = 05, //玩家敲击
            FrameType_RobortMove                = 06, //机器人移动
            FrameType_PlayBattleEnd             = 07, //战斗关卡结束

            FrameType_TeamDisband               = 10, //队伍解散
            FrameType_MonsterChangeMoveDir      = 11, //怪物移动方向变化
            FrameType_BossFire                  = 12, //boss发子弹

            //房间相关
            FrameType_OtherPlayJoinTeam             = 21, //房间有人加入
            FrameType_OtherPlayRoomStateChange      = 22, //房间内其他玩家准备
            FrameType_TeamSeatCountChange           = 23, //房间座位变化(开关)
            FrameType_PlayerBeKicked                = 24, //玩家被踢出
            FrameType_OtherPlayerQuitTeam           = 25, //其他玩家离队
            FrameType_TeamStateChange               = 26, //队伍状态变化
            FrameType_PlayerBeKickAndClose          = 27, //踢人并关闭座位返回

            //关卡相关
            FrameType_StageStepUpdate               = 31, //关卡进度更新
            FrameType_SubStepClear                  = 32, //本图所有点被找到
            FrameType_StepGrade                     = 33, //关卡评分

            //道具相关
            FrameType_OtherUseItem                  = 40,//使用道具统一协议

            FrameType_OtherUseItem_Find             = 41, //使用放大镜
            FrameType_OtherUseItem_Speed            = 42,//速度变化
            FrameType_OtherUseItem_Invincible       = 43,//无敌
            FrameType_OtherUseItem_AOE              = 44,//导弹
            FrameType_OtherUseItem_Stun             = 45,//飞弹（跟踪）
            FrameType_OtherUseItem_TimeSlow         = 46,//时间变慢
            FrameType_OtherUseItem_AT               = 47,//保护罩
            FrameType_OtherUseItem_Hammer           = 48,//巨锤（敲击范围）
            FrameType_OtherUseItem_Trans            = 49,//传送

            //请求
            FrameType_StageScore                    = 51,//响应请求获取某大关分数
        }

        public enum ResultEnum
        {
            TimeOut = -1,   //超时
            End = 1,        //正常结束
        }

        public enum OPResult
        {
            OP_TARGETPLAYERISNULL = -6,//操作对象不存在
            OP_TEAMMEMBERCOUNTERROE = -5,//队伍人数不和操作要求（过多或过少）
            OP_ISNOTLEADER = -4,//不是队长
            OP_TEAMSTATEERROE = -3,//队伍状态不和要求
            OP_TEAMNOMATCH = -2,//队伍不匹配
            OP_TEAMISNULL = -1,//队伍不存在
            OP_FAILURE = 0,//失败
            OP_OK = 1,  //成功
        }

        
    }
}
