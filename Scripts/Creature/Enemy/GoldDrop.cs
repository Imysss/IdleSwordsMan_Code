using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GoldDrop : MonoBehaviour
{
    private Transform _player;
    private BigInteger _goldAmount;
    private bool _isFollowing = false;

    private Vector3 _velocity;
    private float _gravity = -20f; // 중력 값 좀 세게 설정
    private float _airTime = 0f;

    private Vector3 _playerOffset = new Vector3(0f, 0.8f, 0f);
    // 스테이지마다 골드량이 증가하는 배율
    private float randomRange = 0.2f;       // 드랍량의 랜덤 범위
    
    public void Init()
    {
        _isFollowing = false;
        _airTime = 0f;

        _player = GameObject.FindWithTag("Player")?.transform;

        // 무작위 방향으로 위로 튀는 초기 속도
        _velocity = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(5f, 7f),
            0f
        );
        _goldAmount = Managers.Level.GetCurrentWaveEnemyStat().gold;
        float rand = 1 + Random.Range(-randomRange, randomRange);
        _goldAmount = (BigInteger)((double)_goldAmount * rand);
        
        Invoke(nameof(StartFollowing), 0.5f); // 튄 후 일정 시간 지나고 플레이어 추적 시작
    }

    //현재 스테이지에 맞는 몬스터의 골드 드랍량 계산
    // private BigInteger CaculateGoldAmount()
    // {
    //     // // 핵심 공식 : < 드랍 골드 = 기본 드랍량 * (성장 계수 ^ S ) >
    //     // double averageGold = _baseGoldDrop * Mathf.Pow(growthFactor, Managers.Level.GetCurrentLevel());
    //     // double minGold = averageGold * (1 - randomRange);
    //     // double maxGold = averageGold * (1 + randomRange);
    //     //
    //     // BigInteger finalGold = (BigInteger)Mathf.Round(Random.Range((float)minGold, (float)maxGold));
    //
    //     return finalGold;
    // }

    private void StartFollowing()
    {
        _isFollowing = true;
    }

    private void Update()
    {
        if (!_isFollowing)
        {
            _velocity.y += _gravity * Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;
            return;
        }

        if (_player == null) return;

        // 플레이어 위치 + 오프셋
        Vector3 targetPos = _player.position + _playerOffset;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Managers.Game.AddGold(_goldAmount);
            PlayRandomSFX();
            Managers.Pool.Push(gameObject);
        }
    }

    private void PlayRandomSFX()
    {
        int randNum = Random.Range(1, 7);
        
        string sfxName = "GoldDrop_" + randNum.ToString();
        
        Managers.Sound.Play(Define.Sound.Sfx, sfxName);
    }

    public void ResetGold()
    {
        CancelInvoke();
        _isFollowing = false;
        _airTime = 0f;
        _velocity = Vector3.zero;
    }
}