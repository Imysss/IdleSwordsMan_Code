// 아이템의 가변성 데이터만 담음
using System;

[Serializable]
public class ItemState
{
    public int dataId;           // 아이디
    public int level = 1;               // 레벨
    public int experience = 0;          // 경험치
    public bool isUnlocked = false;         // 소유 여부
    public bool canUpgrade = false;         // 업그레이드 가능 여부
}

// ItemState 가 어떤 종류의 ItemData 를 참고하는지 알 수 있도록 제너릭으로 생성
[System.Serializable]
public class ItemState<TData> : ItemState where TData : ItemData
{
    public TData data;
}