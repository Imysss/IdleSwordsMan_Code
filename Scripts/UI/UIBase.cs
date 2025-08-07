using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//UI 요소를 Enum 기반으로 바인딩: 하드 코딩 없이 UI 요소들을 Enum 값으로 매핑함으로써 코드 안전성과 유지보수성을 높임
//유형별 바인딩/조회 분리: BindImage, GetText처럼 각 타입별로 명확하게 역할을 분리하여 UI 이벤트 처리의 중복 제거
//UI 이벤트 처리 추상화: 클릭, 드래그, 눌림 등을 UIEventHandler로 통합 처리하여 UI 이벤트 처리의 중복 제거
//DOTween 애니메이션 재사용: 팝업 등에서 사용할 수 있는 애니메이션 함수를 따로 제공하여 모듈화와 일관된 연출 유지

//장점
//1. Enum 기반 바인딩: 타입 안정성, UI 이름 오탈자 방지, 리팩토링 시 자동 추적 가능
//2. 한 번의 Init으로 바인딩 제한: 중복 초기화 방지
//3. 유형별 Get/Bind 제공: 실수 방지, 가독성 향상
//4. 정적 이벤트 바인딩 메서드: UI 이벤트 처리 일관성 확보
//5. Popup 애니메이션 내장: 코드 재사용 + 애니메이션 연출 통일
public class UIBase : MonoBehaviour
{
    //Enum으로 바인딩된 UI 요소를 타입별로 저장 (ex. Text, Image)
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
    protected bool _init = false;

    public virtual bool Init()
    {
        if (_init)
            return false;
        _init = true;   //중복 초기화 방지
        return true;
    }

    private void Awake()
    {
        //UI 생성 시 자동 초기화
        Init();
    }

    #region Bind UI
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(type);   //enum 이름을 UI 이름으로 가정
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        _objects.Add(typeof(T), objects);

        for (int i = 0; i < names.Length; i++)
        {
            //GameObject면 그대로 찾고 나머지는 제너릭으로  타입 변환하여 찾기
            if (typeof(T) == typeof(GameObject))
            {
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);
            }

            if (objects[i] == null)
            {
                Debug.Log($"Failed to bind {names[i]}");
            }
        }
    }

    protected void BindObject(Type type) { Bind<GameObject>(type); }
    protected void BindImage(Type type) { Bind<Image>(type); }
    protected void BindText(Type type) { Bind<TextMeshProUGUI>(type); }
    protected void BindButton(Type type) { Bind<Button>(type); }
    protected void BindToggle(Type type) { Bind<Toggle>(type); }

    #endregion

    #region Get UI
    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        //index로 해당 UI 요소 반환
        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected TextMeshProUGUI GetText(int idx) { return Get<TextMeshProUGUI>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }

    #endregion

    #region Bind Event
    public static void BindEvent(GameObject go, Action action = null, Action<BaseEventData> dragAction = null,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        //Handler 컴포넌트 확보
        UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(go);

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.UIEvent.Pressed:
                evt.OnPressedHandler -= action;
                evt.OnPressedHandler += action;
                break;
            case Define.UIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.UIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= dragAction;
                evt.OnDragHandler += dragAction;
                break;
            case Define.UIEvent.BeginDrag:
                evt.OnBeginDragHandler -= dragAction;
                evt.OnBeginDragHandler += dragAction;
                break;
            case Define.UIEvent.EndDrag:
                evt.OnEndDragHandler -= dragAction;
                evt.OnEndDragHandler += dragAction;
                break;
        }
    }

    public static void UnbindEvent(GameObject go, Action action = null, Action<BaseEventData> dragAction = null,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UIEventHandler evt = Util.GetOrAddComponent<UIEventHandler>(go);
        if (evt == null)
            return;

        switch (type)
        {
            case Define.UIEvent.Click:
                evt.OnClickHandler -= action;
                break;
            case Define.UIEvent.Pressed:
                evt.OnPressedHandler -= action;
                break;
            case Define.UIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                break;
            case Define.UIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                break;
            case Define.UIEvent.Drag:
                evt.OnDragHandler -= dragAction;
                break;
            case Define.UIEvent.BeginDrag:
                evt.OnBeginDragHandler -= dragAction;
                break;
            case Define.UIEvent.EndDrag:
                evt.OnEndDragHandler -= dragAction;
                break;
        }
    }
    #endregion

    public void PopupOpenAnimation(GameObject contentObject)
    {
        contentObject.transform.localScale = Vector3.zero;
        contentObject.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        // contentObject.transform.localScale = Vector3.one;
        // contentObject.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutQuad).OnComplete(() => contentObject.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad));
    }

    public void PopupCloseAnimation(GameObject contentObject, Action onComplete)
    {
        //onComplete: 콜백 방식으로 후처리 연결
        contentObject.transform.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void PopupSlideIn(GameObject contentObject, float fromY = -870f, float duration = 0.25f, float overshoot = 30f)
    {
        RectTransform rt = contentObject.GetComponent<RectTransform>();

        Vector2 endPos = rt.anchoredPosition;
        Vector2 startPos = new Vector2(endPos.x, fromY);
        Vector2 overshootPos = new Vector2(endPos.x, endPos.y + overshoot);

        rt.anchoredPosition = startPos;

        DOTween.Sequence()
            .Append(rt.DOAnchorPos(overshootPos, duration).SetEase(Ease.OutCubic).SetUpdate(true))  //올라가기
            .Append(rt.DOAnchorPos(endPos, 0.1f).SetEase(Ease.InCubic).SetUpdate(true));    //튕기듯 내려감
    }

    public void PopupSlideOut(GameObject contentObject, float toY = -870f, float duration = 0.25f, Action onComplete = null)
    {
        RectTransform rt = contentObject.GetComponent<RectTransform>();
        
        Vector2 startPos = rt.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, toY);

        rt.DOAnchorPos(endPos, duration).SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() => onComplete?.Invoke());
    }
}
