using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartPanel : PanelBase
{
    public override void Show()
    {
        GetElement<Button>("StartBtn").onClick.AddListener(() => print("开始游戏"));
        GetElement<Button>("EndBtn").onClick.AddListener(() => print("结束游戏"));
        AddEntry<Button>("StartBtn", EventTriggerType.PointerEnter, (data) => print("指针进入"));
        AddEntry<Button>("EndBtn", EventTriggerType.PointerExit, (data) => print("指针离开"));
    }
    public override void Hide()
    {
        throw new System.NotImplementedException();
    }
}
