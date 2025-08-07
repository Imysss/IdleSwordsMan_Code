using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GameScene : BaseScene
{

    private void Awake()
    {
        Init();
        // SceneChangeAnimation_Out anim = Managers.Resource.Instantiate("SceneChangeAnimation_Out").GetOrAddComponent<SceneChangeAnimation_Out>();
        // anim.SetInfo(SceneType, () =>
        // {
        //     
        // });
    }
    
    protected override void Init()
    {
        Debug.Log("@>> GameScene Init");
        base.Init();

        SceneType = Define.Scene.GameScene;
        
        //Managers.GoogleLogin.Init();
        Managers.Guest.Init();
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
        //Managers.SleepMode.Init();
        
        UIGameScene uiGameScene = Managers.UI.ShowSceneUI<UIGameScene>();
        uiGameScene.SetInfo();

        if (!Managers.SaveLoad.hasSaveData)
        {
            Debug.Log("No Save Data");
            StartCoroutine(CoStartCutSceneAndTutorial());
        }
        else
        {
            Debug.Log("Has Save Data");
        }
        
        Managers.Sound.Play(Define.Sound.Bgm, "BGM_Game");
        
        // 게임 시작을 알림
        EventBus.Raise(new GameStartEvent());
    }

    private IEnumerator CoStartCutSceneAndTutorial()
    {
        Time.timeScale = 0f;

        UICutScene cutSceneUI = Managers.UI.ShowFirstPopupUI<UICutScene>();
        cutSceneUI.SetInfo();  
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
