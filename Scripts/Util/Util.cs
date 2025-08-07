using System;
using System.Reflection;
using UnityEngine;
using static Define;

public class Util : MonoBehaviour
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        return transform.gameObject;
    }
    
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>()) 
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                {
                    return component;
                }
            }
        }

        return null;
    }
    
    public static Color HexToColor(string color)
    {
        Color parsedColor;
        ColorUtility.TryParseHtmlString("#" + color, out parsedColor);

        return parsedColor;
    }

    public static Color GetBackgroundColor(RarityType type)
    {
        switch (type)
        {
            case RarityType.Normal:
                return UIColors.Normal;
            case RarityType.Rare:
                return UIColors.Rare;
            case RarityType.Epic:
                return UIColors.Epic;
            case RarityType.Unique:
                return UIColors.Unique;
            case RarityType.Legendary:
                return UIColors.Legendary;
            default:
                return UIColors.Normal;
        }
    }

    public static string FormatTo2DecimalsNoRounding(float value)
    {
        float trimmed = Mathf.Floor(value * 1000.0f) / 1000.0f;
        return trimmed.ToString("0.###");
    }

    public static string GetGradeStringEngToKor(RarityType type)
    {
        switch (type)
        {
            case RarityType.Normal:
                return "노멀";
            case RarityType.Rare:
                return "레어";
            case RarityType.Epic:
                return "에픽";
            case RarityType.Unique:
                return "유니크";
            case RarityType.Legendary:
                return "레전더리";
            default:
                return "";
        }
    }
    

}
