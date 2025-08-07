using System;
using System.Collections;
using UnityEngine;

public class SceneChangeAnimation_In : UIPopup
{
    private Animator _anim;
    private Action _action;
    private Define.Scene _nextScene;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void SetInfo(Define.Scene nextScene, Action callback)
    {
        transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        _action = callback;
        _nextScene = nextScene;
        StartCoroutine(OnAnimationComplete());
    }

    private IEnumerator OnAnimationComplete()
    {
        yield return new WaitForSeconds(1f);
        _action.Invoke();
    }
}
