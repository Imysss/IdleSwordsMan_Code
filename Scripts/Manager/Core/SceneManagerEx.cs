using UnityEngine;
using UnityEngine.SceneManagement;

//Unity의 SceneManager는 정적 API이므로 게임의 상태에 따라 로딩 전에 처리해야 할 공통 작업 (타입 스케일 초기화, 현재 씬 클리어 등)을 넣기 어려움
//해당 클래스는 씬 전환 전 로직을 커스터마이징하기 위해 만들어짐
//BaseScene을 기준으로 현재 씬 상태를 파악하고 씬별로 전환 전 작업을 조건부 실행하도록 함
//씬 이름을 Enum을 문자열로 변환하여 하드 코딩 없이 유연하게 관리

//장점
//1. BaseScene 중심 구조: 씬 간 공통 인터페이스 제공 -> 일관된 Clear() 처리 가능
//2. Define.Scene enum 사용: 씬 이름 하드 코딩 제거 -> 유지보수 관리
//3. Time.timeScale = 1 처리 내장: 게임 중 일시 정지/2배속 등이 적용된 상태를 씬 전환 시 자동 초기화
public class SceneManagerEx
{
    //현재 씬을 BaseScene 타입으로 캐스팅하여 참조
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

    //씬 전환 함수
    public void LoadScene(Define.Scene type, Transform parents = null)
    {
        Debug.Log($"LoadScene - currentscene {CurrentScene.SceneType.ToString()}");
        switch (CurrentScene.SceneType)
        {
            case Define.Scene.TitleScene:
                SceneManager.LoadScene(GetSceneName(type));
                
                // SceneChangeAnimation_In anim1 = Managers.Resource.Instantiate("SceneChangeAnimation_In").GetComponent<SceneChangeAnimation_In>();
                // anim1.transform.SetParent(parents);
                // anim1.transform.localPosition = Vector3.zero;
                //
                // anim1.SetInfo(type, () =>
                // {
                //     SceneManager.LoadScene(GetSceneName(type));
                // });
                break;
            case Define.Scene.GameScene:
                //게임 씬은 보통 일시 정지나 배속 상태일 수 있으므로 원상 복구
                Time.timeScale = 1;
                Managers.Sound.Stop(Define.Sound.Bgm);
                Managers.Resource.Destroy(Managers.UI.SceneUI.gameObject);
                Managers.Clear();
                SceneManager.LoadScene(GetSceneName(type));
                break;
            case Define.Scene.DevGameScene:
                Time.timeScale = 1;
                SceneManager.LoadScene(GetSceneName(type));
                break;
        }
    }

    //Enum 값을 문자열로 변환하여 씬 이름 획득 -> enum 기반 관리의 핵심
    string GetSceneName(Define.Scene type)
    {
        return System.Enum.GetName(typeof(Define.Scene), type);
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}

