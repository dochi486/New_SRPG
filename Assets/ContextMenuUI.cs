using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContextMenuUI : BaseUI<ContextMenuUI>
{
    public GameObject baseItem;

    protected override void OnInit()
    {
        baseItem = transform.Find("BG/Button").gameObject;

        Dictionary<string, UnityAction> menus = new Dictionary<string, UnityAction>();
        menus.Add("턴 종료(F12)", EndTurnPlayer);
        menus.Add("테스트", TestMenu);
        menus.Add("테스트메뉴", () => { print("무명함수는 뭘까"); OnClick(); });

        foreach (var item in menus)
        {
            GameObject go = Instantiate(baseItem, baseItem.transform.parent);
            go.GetComponentInChildren<Text>().text = item.Key;
            go.GetComponent<Button>().AddListener(this, item.Value);
        }

        baseItem.SetActive(false);
    }

    private void OnClick()
    {
        Close();
    }

    private void TestMenu()
    {
        print("테스트메뉴");
        OnClick();
    }

    private void EndTurnPlayer()
    {
        print("EndTurnPlayer");

        StageManager.Instance.EndTurnPlayer();

        OnClick();
    }

    internal void Show(Vector3 uiPosition)
    {
        base.Show();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), 
            uiPosition, null, out Vector2 localPoint);

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = localPoint;
    }
}
