using System.Collections;
using UnityEngine;

//정적 컨텍스트에서 안전하게 사용할 수 있도록 만든 구조
//StartCoroutine()은 MonoBehaviour 인스턴스에만 호출 가능하기 때문에 정적 클래스나 일반 클래스에서는 직접 호출할 수 없음
//숨겨진 GameObject를 만들고 해당 오브젝트에 붙은 MonoBehaviour를 통해 코루틴을 프록시 방식으로 실행

//장점
//1. 정적 클래스나 싱글톤 등에서 직접 Coroutine 사용 가능
//2. 라이프 사이클 독립: 씬 전환과 무관하게 코루틴이 지속됨
//3. 불피룡한 MonoBehaviour 상속 회피: 순수한 데이터/로직 클래스에서도 코루틴 실행이 가능해짐
public class CoroutineManager : MonoBehaviour
{
    private static MonoBehaviour monoInstance;

    //게임이 시작될 때 자동으로 실행되는 메서드
    [RuntimeInitializeOnLoadMethod]
    private static void Initializer()
    {
        //코루틴 실행용 GameObject 생성 및 자신을 붙임
        monoInstance = new GameObject($"[{nameof(CoroutineManager)}]").AddComponent<CoroutineManager>();
        DontDestroyOnLoad(monoInstance.gameObject); //씬을 전환해도 살아 있도록 설정
    }

    //정적 메서드로 코루틴 시작 가능
    public new static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        //monoInstance가 아직 초기화되지 않은 경우 예외 방지
        if (monoInstance == null)
        {
            return null;
        }
        return monoInstance.StartCoroutine(coroutine);
    }

    //정적 메서드로 코루틴 중단 가능
    public new static void StopCoroutine(Coroutine coroutine)
    {
        monoInstance.StopCoroutine(coroutine);
    }
}