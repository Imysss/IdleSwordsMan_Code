using System;
using System.Collections;
using UnityEngine;

public class SceneChangeAnimation_Out : UIPopup
{
    private Animator _anim;
    private Action _action;
    private Define.Scene _prevScene;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void SetInfo(Define.Scene prevScene, Action callback)
    {
        transform.localScale = Vector3.one;
        _action = callback;
        _prevScene = prevScene;
        StartCoroutine(OnAnimationComplete());
    }

    private IEnumerator OnAnimationComplete()
    {
        yield return new WaitForSeconds(1f);
        _action.Invoke();
    }
}
