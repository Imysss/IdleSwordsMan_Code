using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;

    private Transform _target;
    private Vector3 _offset;
    private StatManager _stat;
    private Canvas _parentCanvas;

    private float _smoothSpeed = 10f;

    private void OnEnable()
    {
        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(Transform target, StatManager stat, Vector3 offset)
    {
        _target = target;
        _stat = stat;
        _offset = offset;

        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>();

        _stat.OnBigIntStatChanged += OnStatChanged;

        hpSlider.maxValue = (float)_stat.GetBigIntValue(StatType.MaxHp);
        hpSlider.value = (float)_stat.CurrentHp;
    }

    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy || _target == null || _stat == null || _parentCanvas == null)
        {
            HPBarManager.Instance?.Detach(this); // 자가 반환
            return;
        }

        Vector3 worldPos = _target.position + _offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                screenPos,
                _parentCanvas.worldCamera,
                out Vector2 localPoint))
        {
            (transform as RectTransform).localPosition = localPoint;
        }

        BigInteger currentHp = _stat.CurrentHp;
        hpSlider.value = Mathf.Lerp(hpSlider.value, (float)currentHp, Time.deltaTime * _smoothSpeed);
    }

    private void OnStatChanged(StatType type, BigInteger value)
    {
        if (type == StatType.MaxHp)
        {
            hpSlider.maxValue = (float)value;
        }
    }

    public void ResetHPBar()
    {
        if (_stat != null)
            _stat.OnBigIntStatChanged -= OnStatChanged;

        _target = null;
        _stat = null;
        _offset = Vector3.zero;

        if (hpSlider != null)
        {
            hpSlider.value = 0f;
            hpSlider.maxValue = 1f;
        }
    }
}