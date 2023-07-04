using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartPanel : PanelBase
{
    public override void Show()
    {
        GetElement<Button>("StartBtn").onClick.AddListener(() => print("��ʼ��Ϸ"));
        GetElement<Button>("EndBtn").onClick.AddListener(() => print("������Ϸ"));
        AddEntry<Button>("StartBtn", EventTriggerType.PointerEnter, (data) => print("ָ�����"));
        AddEntry<Button>("EndBtn", EventTriggerType.PointerExit, (data) => print("ָ���뿪"));
    }
    public override void Hide()
    {
        throw new System.NotImplementedException();
    }
}
