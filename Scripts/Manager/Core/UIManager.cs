using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//SceneUI, PopupUI, ToastUI를 통합적으로 관리

//장점
//1. 명확한 UI 분리
//2. 스택 기반 Popup 관리: 중첩된 팝업의 순서 및 닫힘 로직을 직관적으로 관리
//3. Canvas 정렬 일관성 유지: 정해진 기준으로 CanvasScaler, SortingOrder를 통일 적용
//4. 풀링 및 재사용 고려: ShowToast, MakeSubItem
public class UIManager 
{
    private int _order = 10;
    private int _toastOrder = 500;
    
    Stack<UIPopup> _popupStack = new Stack<UIPopup>();
    Stack<UIToast> _toastStack = new Stack<UIToast>();
    private UIScene _sceneUI = null;
    public UIScene SceneUI { get { return _sceneUI; } }
    
    private UIGameScene _gameSceneUI = null;
    public UIGameScene GameSceneUI { get { return _gameSceneUI; } }

    public Action<int> OnTimeScaleChanged;
    public Action RefreshCombatPowerUI;
    
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UIRoot");  
            if (root == null)
                root = new GameObject { name = "@UIRoot" };
            return root;
        }
    }
    
    //Popup, Scene, Toast 등 UI 용도에 따라 Canvas Scale과 Order 조정
    public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0, bool isToast = false)
    {
        //UI GameObject에 Canvas 설정을 적용하고 정렬 순서 결정
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        if (canvas == null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
        }

        //화면 해상도 대응 UI 스케일 조정
        CanvasScaler cs = go.GetOrAddComponent<CanvasScaler>();
        if (cs != null)
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1440, 3040);   //기준 해상도 적용
        }

        go.GetOrAddComponent<GraphicRaycaster>();

        //UI 종류별 정렬 우선순위 설정
        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = sortOrder;
        }

        if (isToast)
        {
            _toastOrder++;
            canvas.sortingOrder = _toastOrder;
        }
    }

    public void RefreshTimeScale()
    {
        //GameScene이 아닐 경우 TimeScale을 기본값으로 복구
        if(SceneManager.GetActiveScene().name != Define.Scene.GameScene.ToString())
        {
            Time.timeScale = 1;
            return;
        }

        DOTween.timeScale = 1;
        OnTimeScaleChanged?.Invoke((int)Time.timeScale);
    }

    public void Clear()
    {
        CloseAllPopupUI();
        Time.timeScale = 1;
        _sceneUI = null;
    }

    #region Scene UI

    public T ShowSceneUI<T>(string name = null) where T : UIScene
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }
        

        //Scene UI는 한 개만 존재해야 하므로 _sceneUI에 할당
        GameObject go = Managers.Resource.Instantiate(name);
        go.transform.SetParent(Root.transform);

        T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;
        
        if (sceneUI.TryGetComponent<UIGameScene>(out UIGameScene gameScene))
        {
            _gameSceneUI = gameScene;
        }
        
        return sceneUI;
    }

    #endregion

    #region SubItem UI

    public T MakeSubItem<T>(Transform parent = null, string name = null, bool pooling = true) where T : UIBase
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }
        
        //pooling true로 만들어야 함
        GameObject go = Managers.Resource.Instantiate(name, parent, pooling);
        go.transform.SetParent(parent, false);  //설정된 본인 크기에 맞춰서 생성
        //go.transform.localScale = Vector3.one;

        return Util.GetOrAddComponent<T>(go);
    }

    #endregion
    
    #region Popup UI

    public T ShowFirstPopupUI<T>(string name = null) where T : UIPopup
    {
        // 절전모드에는 실행 x
        //if (Managers.SleepMode.IsSleepMode) return null;
        
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        
        GameObject go = Managers.Resource.Instantiate(name);
        go.transform.SetParent(Root.transform);

        T popup = Util.GetOrAddComponent<T>(go);

        return popup;
    }

    public void CloseFirstPopupUI(UIPopup popup)
    {
        Managers.Resource.Destroy(popup.gameObject);
    }
    
    
    public T ShowPopupUI<T>(string name = null) where T : UIPopup
    {
        // 절전모드에는 실행 x
        //if (Managers.SleepMode.IsSleepMode) return null;
        
        if (_popupStack.Count > 0 && _popupStack.Peek() is T)
        {
            Debug.Log("Open Popup Failed");
            return null;
        }

        if (_popupStack.Count > 0 && (_popupStack.Peek() is UIGachaResultPopup || _popupStack.Peek() is UIUpgradeResultPopup))
        {
            Debug.Log("Gacha result or upgrade popup ing... popup  failed");
            return null;
        }
        
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        
        //팝업 UI 생성 및 스택에 푸시
        GameObject go = Managers.Resource.Instantiate(name);
        go.transform.SetParent(Root.transform);

        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        RefreshTimeScale();
        
        return popup;
    }
    
    public void ClosePopupUI(UIPopup popup)
    {
        if (_popupStack.Count == 0)
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed");
            return;
        }
        
        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UIPopup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        _order--;
        RefreshTimeScale();
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > Define.ESSENTIAL_POPUP_COUNT)
            ClosePopupUI();
    }
    
    #endregion

    #region Toast UI

    public UIToast ShowToast(string msg)
    {
        string name = typeof(UIToast).Name;
        
        GameObject go = Managers.Resource.Instantiate($"{name}", pooling: true);
        go.transform.SetParent(Root.transform);
        UIToast toast = Util.GetOrAddComponent<UIToast>(go);
        toast.SetInfo(msg);
        
        _toastStack.Push(toast);

        CoroutineManager.StartCoroutine(CoCloseToastUI());
        return toast;
    }

    private IEnumerator CoCloseToastUI()
    {
        yield return new WaitForSeconds(1f);
        CloseToastUI();
    }

    public void CloseToastUI()
    {
        if (_toastStack.Count == 0)
            return;
        
        UIToast toast = _toastStack.Pop();
        Managers.Resource.Destroy(toast.gameObject);
        _toastOrder--;
    }

    #endregion
}