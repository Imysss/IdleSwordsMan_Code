using UnityEngine;

public class SkillEffectController : MonoBehaviour
{
    private float _duration;
    private const float DefaultDuration = 1f;

    public void Setup(float duration)
    {
        _duration = duration > 0 ? duration : DefaultDuration;
        Invoke(nameof(Release), _duration);
    }

    private void Release()
    {
        Managers.Pool.Push(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}