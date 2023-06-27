using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventName
{
    public const string ENTER_BATTLE_MODE = "EnterBattleMode";  // 进入战斗魔术 无参数
    public const string LEAVE_BATTLE_MODE = "LeaveBattleMode";  // 返回非战斗模式 无参数

    public const string MOVE_PLAYER_ISLAND = "MovePlayerIsland";    // 玩家岛已经被重新编辑 无参数

    public const string MOVE_ON_MAP_HOR = "MoveOnMap";  // 玩家在地图上横向移动 参数：Vector2Int（原位置，新位置）
    public const string MERGE_ROOM = "MergeRoom";  // 合并房间 参数：int（被合并的房间原序号）
    public const string MOVE_Land = "MoveLand";  //岛屿移动 参数：MoveDir(移动方向)

    public const string UI_UpdateMagicPower = "UpdateMagicPower";   // 魔力点数变化 参数：int（新魔力点数）
    public const string UI_ShowBattleMagicianDetailInfo = "ShowBattleMagicianDetailInfo";//展示魔术师详细面板 参数 魔术师类
    public const string UI_ExitBattleMagicianDetailInfo = "ExitBattleMagicianDetailInfo";//消失魔术师详细面板 参数 魔术师类
    public const string CORE_HP_CHANGE = "CoreHPChange";    // 核心生命值变化 参数：int（新HP）
    public const string UI_ShowUITip = "ShowUITip";//鼠标悬浮显示提示文字，参数：字典<标题,内容>
    public const string UI_HideUITip = "HideUITip";//鼠标移出隐藏提示

    public const string BATTLE_HUMAN_CREATE = "BattleHumanCreate";  // 战斗时创建单位 参数：CreateCommand（创建指令，单位已初始化）
    public const string BATTLE_HUMAN_MOVED = "BattleHumanMoved";  // 战斗时单位移动 参数：MoveCommand（移动指令，单位数据尚未更新）
    public const string BATTLE_HUMAN_DESTROY = "BattleHumanDestroy";  // 战斗时销毁单位 参数：DestroyCommand（销毁指令，单位尚未销毁）
}
