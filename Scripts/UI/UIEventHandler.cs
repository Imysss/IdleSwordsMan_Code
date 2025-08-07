using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Unity 이벤트 인터페이스 통합 처리: UI 요소의 이벤트 처리를 단일 지점에서 중앙집중적으로 관리함
//Action 델리게이트를 통한 유연한 바인딩: Action, Action<BaseEventData>를 외부에서 바인딩할 수 있게 하여 다양한 UI 이벤트를 별도 스크립트 없이 동적으로 연결할 수 있게 설계
//Update에서 _pressed 상태 감지: 버튼이 눌린 동안 지속적으로 수행할 로직을 구현

//장점
//1. 통합 이벤트 핸들링: 클릭, 드래그, 포인터 등 다양한 이벤트를 하나의 컴포넌트에서 처리
//2. Action 방식의 바인딩: 인스펙터에 의존하지 않고 코드로 런타임 바인딩 가능
//3. Button의 interactable 체크 내장: 비활성화된 버튼 무시 가능
//4. Update로 지속 처리 가능: _pressed를 활용한 길게 누르기/충전형 버튼 등 응용 가능
public class UIEventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //외부에서 연결할 수 있는 이벤트 핸들러
    public Action OnClickHandler = null;
    public Action OnPressedHandler = null;      
    public Action OnPointerDownHandler = null; 
    public Action OnPointerUpHandler = null;
    public Action<BaseEventData> OnDragHandler = null;
    public Action<BaseEventData> OnBeginDragHandler = null;
    public Action<BaseEventData> OnEndDragHandler = null;

    private bool _pressed = false;
    private bool _isDragging = false;
    
    private float _basePressedInterval = 0.1f;     // 초기 호출 주기
    private float _pressedElapsed = 0f;
    private float _pressedDuration = 0f;

    private const float _maxSpeedMultiplier = 10f;  // 최대 10배 빠르게
    private float _minInterval => _basePressedInterval / _maxSpeedMultiplier;

    [SerializeField] private ScrollRect _parentScrollRect;

    private static UIEventHandler _currentPressedHandler;

    private Button button;
    private Toggle toggle;
    
    

    private void OnDisable()
    {
        if(_currentPressedHandler == this)
            ReleaseCurrentPressed();
    }

    private void Start()
    {
        _parentScrollRect = GetComponentInParent<ScrollRect>();
        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();
    }

    private void Update()
    {
        if (_pressed)
        {
            if ((button != null && !button.interactable) || (toggle != null && !toggle.interactable))
            {
                _pressed = false;
                return;
            }

            _pressedElapsed += Time.unscaledDeltaTime;
            _pressedDuration += Time.unscaledDeltaTime;

            // 선형 가속: 2초 이상 누르면 최대 속도 도달
            float t = Mathf.Clamp01(_pressedDuration / 2f); // 0~2초 사이 비율
            float dynamicInterval = Mathf.Lerp(_basePressedInterval, _minInterval, t);

            if (_pressedElapsed >= dynamicInterval)
            {
                _pressedElapsed = 0f;
                OnPressedHandler?.Invoke();
            }
        }
    }

    public static void ReleaseCurrentPressed()
    {
        if (_currentPressedHandler != null)
        {
            _currentPressedHandler._pressed = false;
            _currentPressedHandler._pressedElapsed = 0f;
            _currentPressedHandler.OnPointerUpHandler?.Invoke();
            _currentPressedHandler = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //드래그 중에는 클릭 무시
        if (_isDragging)
            return;
        
        //버튼이 비활성화된 상태면 무시
        if (button != null && !button.interactable) 
            return;
        
        //토글이 비활성화된 상태면 무시
        if (toggle != null && !toggle.interactable)
            return;
        
        OnClickHandler?.Invoke();   //클릭 이벤트 호출
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //다른 눌림 상태가 있으면 강제로 해제
        if (_currentPressedHandler != null && _currentPressedHandler != this)
        {
            ReleaseCurrentPressed();
        }
        
        _currentPressedHandler = this;
        
        if (button != null && !button.interactable) 
            return;
        
        //토글이 비활성화된 상태면 무시
        if (toggle != null && !toggle.interactable)
            return;
        
        _pressed = true;
        OnPointerDownHandler?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_currentPressedHandler == this)
            _currentPressedHandler = null;

        _pressed = false;
        _pressedElapsed = 0f;
        _pressedDuration = 0f;  // 누른 시간 초기화
        OnPointerUpHandler?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragHandler?.Invoke(eventData);

        if (_parentScrollRect != null && (_parentScrollRect.horizontal || _parentScrollRect.vertical) && _parentScrollRect.IsActive()) 
        {
            _parentScrollRect?.OnDrag(eventData);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _pressed = true;
        _isDragging = true;
        OnBeginDragHandler?.Invoke(eventData);

        if (_parentScrollRect == null || !_parentScrollRect || !_parentScrollRect.gameObject.activeInHierarchy)
            return;
        
        _parentScrollRect?.OnBeginDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_currentPressedHandler == this)
            _currentPressedHandler = null;
        
        _pressed = false;
        _isDragging = false;
        _pressedElapsed = 0f;
        OnEndDragHandler?.Invoke(eventData);
        
        _parentScrollRect?.OnEndDrag(eventData);
    }
}
