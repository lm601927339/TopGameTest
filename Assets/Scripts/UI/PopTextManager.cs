using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopTextManager : MonoBehaviour
{
    public void PopUpNumberOnCharacter(GameObject cha, int value, bool asHeal = false, bool asCritical = false)
    {
        if (!cha) return;
        Camera cam = GameRuntime.MainCamera;
        if (!cam) return;

        Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(cam, cha.transform.position);
        GameObject popTextPrefab = GameRuntime.LoadPrefab("UI/PopText");
        if (!popTextPrefab) return;

        GameObject text = Instantiate(popTextPrefab, mScreenPos, Quaternion.identity, transform);
        text.GetComponent<UnitPopText>().target = cha;

        Text txt = text.GetComponent<Text>();
        txt.text = "<color=" + (asHeal ? "green" : "red") + ">" + (asHeal ? "+" : "-") + value + (asCritical ? "!" : "") + "</color>";
        txt.fontSize = asCritical ? 40 : 30;
    }

    public void PopUpStringOnCharacter(GameObject cha, string text, int size = 30)
    {
        if (!cha) return;
        Camera cam = GameRuntime.MainCamera;
        if (!cam) return;

        Vector2 mScreenPos = RectTransformUtility.WorldToScreenPoint(cam, cha.transform.position);
        GameObject popTextPrefab = GameRuntime.LoadPrefab("UI/PopText");
        if (!popTextPrefab) return;

        GameObject textObj = Instantiate(popTextPrefab, mScreenPos, Quaternion.identity, transform);
        textObj.GetComponent<UnitPopText>().target = cha;

        Text txt = textObj.GetComponent<Text>();
        txt.text = text;
        txt.fontSize = size;
    }
}
