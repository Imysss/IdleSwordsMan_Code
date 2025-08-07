using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using Component = UnityEngine.Component;

public static class Extension 
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    //UI Event 연결 (OnClick 등)
    public static void BindEvent(this GameObject go, Action action = null, Action<BaseEventData> dragAction = null,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        
        UIBase.BindEvent(go, action, dragAction, type);
    }

    public static void UnbindEvent(this GameObject go, Action action = null, Action<BaseEventData> dragAction = null,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UIBase.UnbindEvent(go, action, dragAction, type);
    }

    //자식 오브젝트 전부 삭제
    public static void DestroyChilds(this GameObject go)
    {
        Transform[] children = new Transform[go.transform.childCount];
        for (int i = 0; i < go.transform.childCount; i++)
        {
            children[i] = go.transform.GetChild(i);
        }

        foreach (Transform child in children)
        {
            // 풀에 반환하기 전, 상태를 깨끗하게 초기화하는 로직 추가
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            
            Managers.Resource.Destroy(child.gameObject);
        }
    }
    
    // Enum 확장메소드, Description 읽어오기
    public static string ToDescription(this Enum source)
    {
        FieldInfo fi = source.GetType().GetField(source.ToString());
        var att = (DescriptionAttribute)fi.GetCustomAttribute(typeof(DescriptionAttribute));
        if (att != null)
        {
            return att.Description;
        }
        else
        {
            return source.ToString();
        }
    }
}
