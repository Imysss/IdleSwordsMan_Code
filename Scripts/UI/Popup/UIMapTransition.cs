using System.Collections.Generic;
using Data;
using TMPro;
using UnityEngine;
using static Define;
using UnityEngine.UI;

public class UIMapTransition : UIPopup
{
    #region  enums

    enum Texts
    {
        MapNameText,
    }
    enum Images
    {
        MapImage,
    }

    #endregion

    // 1. 맵 이름과 색상을 매핑하는 딕셔너리를 선언하고 초기화합니다.
    private readonly Dictionary<string, Color> _mapNameColors = new Dictionary<string, Color>
    {
        // 신선하고 활기찬 느낌의 밝은 연두색
        { "시작의 숲", new Color(0.45f, 0.82f, 0.45f) }, 
        // 따뜻한 노을빛을 반영한 금빛 주황색
        { "황혼의 숲", new Color(1.0f, 0.65f, 0.2f) }, 
        // 건조한 모래와 볕을 표현하는 부드러운 황토색
        { "메마른 사막", new Color(0.95f, 0.85f, 0.55f) }, 
        // 탁하고 음산한 분위기의 어두운 녹색 또는 보라색
        { "타락한 습지", new Color(0.4f, 0.5f, 0.3f) }, 
        // 차갑고 서리가 낀 느낌의 옅은 하늘색
        { "얼어붙은 숲", new Color(0.7f, 0.85f, 0.95f) }, 
        // 용암과 불꽃을 연상시키는 강렬한 붉은색
        { "분노의 화산", new Color(0.9f, 0.2f, 0.1f) }, 
    };
    public override bool Init()
    {
        if(base.Init() == false)
            return false;
        
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        return true;
    }

    public void SetInfo(StageData stageData)
    {
        string mapName = stageData.mapName;
        TextMeshProUGUI mapNameText = GetText((int)Texts.MapNameText);
        string mapKey = MapNameToAddressableKey[mapName];
        mapNameText.SetText(mapName);
        GetImage((int)Images.MapImage).sprite = Managers.Resource.Load<Sprite>(mapKey + ".sprite");
        
        if (_mapNameColors.TryGetValue(mapName, out Color textColor))
        {
            // 딕셔너리에 해당 맵 이름이 있으면
            mapNameText.color = textColor;
        }
        else
        {
            // 없으면 기본색(흰색)으로 설정
            mapNameText.color = Color.white;
        }
    }
    
    
}
