// 모든 아이템의 기본이 될 추상 클래스
// 아이템의 고정된 정보만 담음
using System;

[Serializable]
public abstract class ItemData
{
    public int dataId;           // 아이디
    public string name;         // 이름
    public string description;      // 설명
    public RarityType rarity;    //등급
}