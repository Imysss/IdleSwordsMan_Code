using System.Collections;
using UnityEngine;

public class PoolableProjectile : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private float _maxRange = 20f;      // 투사체의 최대 사정거리
    private Coroutine _returnCoroutine;
    private TrailRenderer _trailRenderer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }
    
    private void OnDisable()
    {
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
            _returnCoroutine = null;
        }
        
        // 물리적 상태 초기화
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        
        // 트레일 렌더러 초기화
        if (_trailRenderer != null)
        {
            _trailRenderer.Clear();
        }
    }

    /// <summary>
    /// 지정된 타겟을 향해 주어진 속도로 투사체 발사
    /// </summary>
    public void Fire(Vector3 targetPos, float speed)
    {
        // 1. 방향 계산
        Vector2 direction = (targetPos - transform.position).normalized;
        
        // 2. 투사체 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    
        // 3. 속력을 기반으로 lifeTime 결정, 코루틴 시작
        float lifeTime = (speed > 0) ? _maxRange / speed : float.MaxValue;
        if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
        _returnCoroutine = StartCoroutine(ReturnToPoolAfterTime(lifeTime));
        
        // 4. 힘 적용
        _rigidbody.AddForce(direction * speed, ForceMode2D.Impulse);
    }
    
    // 시간이 지나면 알아서 pool로 반환
    private IEnumerator ReturnToPoolAfterTime(float lifeTIme)
    {
        // lifeTime 동안 대기
        yield return new WaitForSeconds(lifeTIme);

        Managers.Pool.Push(this.gameObject);
    }
}
