using System.Numerics;
using UnityEngine;
using TMPro;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private GameObject criticalIcon; // 치명타 표시 이미지

    private float _duration = 1f;
    private float _timer;
    private bool _isReturned = false;

    private Canvas _parentCanvas;

    private Vector2 _startPos;
    private Vector2 _randomOffsetDir;
    private float _maxHeight = 80f;
    private float _fallDistance = 120f;

    private void OnEnable()
    {
        _isReturned = false;
        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Show(Transform target, BigInteger damage, Vector3 offset, bool isCritical = false)
    {
        _timer = _duration;
        _isReturned = false;

        damageText.richText = true;
        damageText.text = "";
        transform.localScale = Vector3.one;

        string formatted = NumberFormatter.FormatNumber((BigInteger)damage);
        damageText.text = isCritical
            ? $"<b><color=#FF3A3A>{formatted}</color></b>"
            : formatted;

        if (criticalIcon != null)
        {
            criticalIcon.SetActive(isCritical);
            criticalIcon.transform.localScale = Vector3.one; // 초기화
        }

        if (_parentCanvas == null)
            _parentCanvas = GetComponentInParent<Canvas>();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        if (screenPos.z > 0f &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                screenPos,
                _parentCanvas.worldCamera,
                out _startPos))
        {
            (transform as RectTransform).localPosition = _startPos;
        }

        float xOffset = Random.Range(-40f, 40f);
        _randomOffsetDir = new Vector2(xOffset, 0f);

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_isReturned || _parentCanvas == null) return;

        float progress = 1f - (_timer / _duration);

        float height = Mathf.Sin(progress * Mathf.PI) * _maxHeight;
        float fall = progress * _fallDistance;
        Vector2 pos = _startPos + _randomOffsetDir * progress + new Vector2(0f, height - fall);
        (transform as RectTransform).localPosition = pos;

        float startScale = 2f;
        float endScale = 0.6f;
        float currentScale = Mathf.Lerp(startScale, endScale, progress);
        Vector3 scaleVec = Vector3.one * currentScale;

        transform.localScale = scaleVec;

        if (criticalIcon != null && criticalIcon.activeSelf)
            criticalIcon.transform.localScale = scaleVec;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _isReturned = true;
            Managers.Pool.Push(gameObject);
        }
    }

    public void ResetText()
    {
        damageText.text = "";
        damageText.richText = true;

        if (criticalIcon != null)
            criticalIcon.SetActive(false);

        _timer = 0f;
        _isReturned = true;
    }

    private void OnDisable()
    {
        ResetText();
    }
}
