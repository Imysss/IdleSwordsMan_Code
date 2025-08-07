using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    public float m_MovingDistance;
    public float m_Duration;
    public bool autoStart = false;
    private RectTransform m_RectTransform;
    public RectTransform targetAuto;
    private bool m_IsRunning;
    private Vector3 _targetPosition;
    private void Awake() 
    {
        m_RectTransform = GetComponent<RectTransform>();
        if (autoStart)
        {
            _targetPosition = targetAuto.transform.position;
            StartCoroutine(MoveInternal(targetAuto));
        }
    }
    private void OnEnable() 
    {
        m_IsRunning = true;
    }
    private void OnDisable() 
    {
        m_IsRunning = false;
    }
    // 튜토리얼 화살표를 배치하는 메서드
    public void Place(RectTransform target, bool placeOnTop)
    {
        //  타겟의 실제 높이 계산
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);
        float actualTargetHeight = targetCorners[1].y - targetCorners[0].y;

        // 화살표의 실제 높이 계산 
        Vector3[] indicatorCorners = new Vector3[4];
        m_RectTransform.GetWorldCorners(indicatorCorners);
        float actualIndicatorHeight = Mathf.Abs(indicatorCorners[1].y - indicatorCorners[0].y); 
    
        // 간격 계산 
        float margin = 0f;
        float indicatordist = (actualTargetHeight / 2f) + (actualIndicatorHeight / 2f) + margin;
    
        // 타겟의 정중앙 위치 계산
        _targetPosition = (targetCorners[0] + targetCorners[2]) / 2f;
        Vector3 position = _targetPosition;
        Quaternion rotation;
    
        if(placeOnTop)
        {
            position.y += indicatordist;
            rotation = Quaternion.Euler(0, 0, 180.0f);
        }
        else
        {
            position.y -= indicatordist;
            rotation = Quaternion.identity;
        }

        m_RectTransform.gameObject.SetActive(true);
        m_RectTransform.position = position;
        m_RectTransform.localRotation = rotation;
    }
    public void Show(RectTransform target)
    {
        StartCoroutine( MoveInternal(target) );
    }
    private IEnumerator MoveInternal(RectTransform target)
    {
        Vector3 currPosition = m_RectTransform.position;
        Vector3 direction = (_targetPosition - currPosition).normalized;
        Vector3 diffPosition = direction * m_MovingDistance;
        Vector3 destPosition = currPosition + diffPosition;
        Vector3 delta = diffPosition / m_Duration;
        bool toDest = true;
        float accumTime = 0;
        while(m_IsRunning)
        {
            yield return null;
            float dt = Time.deltaTime;
            Vector3 dist = delta * dt;
            if(toDest)
            {
                m_RectTransform.position += dist;
                accumTime += dt;
            }
            else
            {
                m_RectTransform.position -= dist;
                accumTime -= dt;
            }
            if(accumTime >= m_Duration)
                toDest = false;
            else if(accumTime <= 0)
                toDest = true;
        }
    }
}
