using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 10f;

    private Transform _target;
    private StatManager _stat;
    private Canvas _parentCanvas;

    private BigInteger _maxHp;
    private BigInteger _currentHp;
    private float _targetHP;

    private void Awake()
    {
        _parentCanvas = GetComponentInParent<Canvas>();

        if (_parentCanvas.renderMode == RenderMode.WorldSpace && _parentCanvas.worldCamera == null)
        {
            _parentCanvas.worldCamera = Camera.main;
        }
    }

    public void Init(Transform target, StatManager stat)
    {
        _target = target;
        _stat = stat;

        //slider.maxValue = (float)stat.GetBigIntValue(Define.StatType.MaxHp);
        _maxHp = stat.GetBigIntValue(Define.StatType.MaxHp);
        _currentHp = stat.CurrentHp;
        CalculateTargetValue();

        _stat.OnBigIntStatChanged += OnStatChanged;
        _stat.OnHpChanged += OnHpChanged;
    }

    private void LateUpdate()
    {
        if (_target == null || _parentCanvas == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(_target.position + offset);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            screenPos,
            _parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            (transform as RectTransform).localPosition = localPoint;
        }

        if (Mathf.Abs(slider.value - _targetHP) > 0.01f)
        {
            slider.value = Mathf.Lerp(slider.value, _targetHP, Time.deltaTime * smoothSpeed);
        }
        else
        {
            slider.value = _targetHP;
        }
    }

    private void OnDestroy()
    {
        if (_stat != null)
        {
            _stat.OnBigIntStatChanged -= OnStatChanged;
            _stat.OnHpChanged -= OnHpChanged;
        }
    }

    private void OnStatChanged(Define.StatType type, BigInteger value)
    {
        if (type == Define.StatType.MaxHp)
        {
            _maxHp = value;
            CalculateTargetValue();
            // 최대 체력 변화 시 현재 체력보다 작아질 수 있음 → _targetHP도 갱신
            //_targetHP = Mathf.Min(_targetHP, (float)value);
        }
    }

    private void OnHpChanged(BigInteger value)
    {
        _currentHp = value;
        CalculateTargetValue();
    }

    private void CalculateTargetValue()
    {
        double currentHpDouble = (double)_currentHp;
        double maxHpDouble = (double)_maxHp;
        
        _targetHP = (float)(currentHpDouble / maxHpDouble);
    }
}
