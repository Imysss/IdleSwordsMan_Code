using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DevGameScene : BaseScene
{
    protected override void Init()
    {
        Debug.Log("@>> DevGameScene Init");
        base.Init();

        SceneType = Define.Scene.DevGameScene;

        #if UNITY_EDITOR
        if (!Managers.Resource.IsPreloadComplete)
        {
            //최소 리소스만 로드
            Managers.Resource.Preload();
            StartCoroutine(WaitUntilPreloadComplete());
        }
        else
        {
            ContinueInit();
        }
        #else
        ContinueInit();
        #endif
    }
    
    private IEnumerator WaitUntilPreloadComplete()
    {
        yield return new WaitUntil(() => Managers.Resource.IsPreloadComplete);
        ContinueInit();
    }
    
    private void ContinueInit()
    {
        Managers.Init();
        //Managers.GoogleLogin.Init();
        Managers.Data.Init();
        Managers.Game.Init();
        Managers.Spawn.Init();
        Managers.Quest.Init();
        Managers.ItemDatabase.Init();
        Managers.Inventory.Init();
        Managers.Player.Init();
        Managers.Equipment.Init();
        UITutorial uiTutorial = Managers.UI.ShowFirstPopupUI<UITutorial>();
        Managers.Tutorial.Init(uiTutorial);
        Managers.Level.Init();
        Managers.Gacha.Init();
        Managers.StatUpgrade.Init();
        Managers.Dungeon.Init();
        Managers.Time.Init();
        Managers.Profile.Init();
        
        UIGameScene uiGameScene = Managers.UI.ShowSceneUI<UIGameScene>();
        uiGameScene.SetInfo();

        if (!Managers.SaveLoad.hasSaveData)
        {
            StartCoroutine(CoStartCutSceneAndTutorial());
        }
        else
        {
            //Managers.Sound.Play(Define.Sound.Bgm, "Adventure");
        }
    }
    
    private IEnumerator CoStartCutSceneAndTutorial()
    {
        Time.timeScale = 0f;

        UICutScene cutSceneUI = Managers.UI.ShowFirstPopupUI<UICutScene>();
        bool isCutSceneFinished = false;
        cutSceneUI.SetOnComplete(() =>
        {
            isCutSceneFinished = true;
        });
        
        //컷신 끝날 때까지 대기
        yield return new WaitUntil(() => isCutSceneFinished);
        Managers.UI.CloseFirstPopupUI(cutSceneUI);
        
        Time.timeScale = 1f;
        
        //튜토리얼 시작
        Managers.Tutorial.StartTutorial(9901);
    }
    
    public override void Clear()
    {
        
    }
}
