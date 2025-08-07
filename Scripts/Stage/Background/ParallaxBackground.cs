using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    // isMoving이 true일 때만 움직임
    private bool isMoving = false;
    
    // 이 레이어가 얼마나 빠르게 움직일지에 대한 배율 (원경은 낮게, 근경은 높게)
    // 0.1: 아주 느리게 (먼 배경), 1.0: 기준 속도 (가까운 배경)
    [SerializeField]
    private float parallaxEffectMultiplier;

    private float masterSpeed; // Controller로부터 전달받는 기본 속도
    private Transform _transform;
    private float _width;
    private Vector3 _startPosition;

    private void Start()
    {
        _transform = transform;
        _startPosition = _transform.position;
        // SpriteRenderer의 실제 월드 경계(bounds)를 기준으로 너비를 계산
        _width = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        if (!isMoving) return;

        // 실제 이동 속도 = 마스터 속도 * 레이어별 배율
        float currentSpeed = masterSpeed * parallaxEffectMultiplier;
        
        // 왼쪽으로 이동
        _transform.Translate(Vector3.left * (currentSpeed * Time.deltaTime));

        // 무한 스크롤을 위한 반복 로직
        if (_transform.position.x < -_width)
        {
            _transform.position += new Vector3(_width * 2f, 0, 0);
        }
    }

    // Controller가 이 배경의 움직임 상태를 제어하기 위한 함수
    public void SetMovingState(bool state)
    {
        isMoving = state;
    }

    // Controller가 마스터 속도를 전달하기 위한 함수
    public void SetMasterSpeed(float speed)
    {
        masterSpeed = speed;
    }
}