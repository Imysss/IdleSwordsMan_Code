using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    // 평타용
    public void ApplyDamage()
    {
        _player?.ApplyDamage();
    }

    // 스킬 애니메이션의 특정 타이밍에서 호출 (이펙트 및 데미지 발동)
    public void SkillFire()
    {
        _player?.SendMessage("OnAnimationEvent", "SkillFire");
    }

    // 스킬 애니메이션 종료 시 호출 (평타 차단 해제)
    public void SkillEnd()
    {
        _player?.SendMessage("OnAnimationEvent", "SkillEnd");
    }
}