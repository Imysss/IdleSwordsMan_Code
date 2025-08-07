using UnityEngine;

public class SelectAttack : StateMachineBehaviour
{
    private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");
    public int numberOfAttacks = 2;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 0부터 (공격 개수 - 1)까지의 정수 중 하나를 랜덤으로 선택
        int attackIndex = Random.Range(0, numberOfAttacks);

        // 애니메이터의 "AttackIndex" 파라미터에 랜덤으로 생성된 값을 설정
        animator.SetInteger(AttackIndex, attackIndex);
    }
}
