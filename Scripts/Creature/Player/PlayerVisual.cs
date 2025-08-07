using UnityEngine;

// 플레이어 외형 제어
//각 장비 부위에 해당하는 SpriteRenderer 를 통해 장비 외형을 반영
// GearEquipment 등에서 호출 , 어드레서블 스프라이트를 로드하여 외형을 적용
public class PlayerVisual : MonoBehaviour
{
    [Header("외형 슬롯 (SpriteRenderer)")]
    [SerializeField] SpriteRenderer hatRenderer;
    [SerializeField] SpriteRenderer weaponRenderer;


    //좌우 어깨 갑옷
    [SerializeField] SpriteRenderer armorRenderer;
    [SerializeField] SpriteRenderer armRRenderer;
    [SerializeField] SpriteRenderer armLRenderer;
    [SerializeField] SpriteRenderer pelvisRenderer;
    
    // 좌우 장갑
    [SerializeField] SpriteRenderer handRendererL;
    [SerializeField] SpriteRenderer handRendererR;
    [SerializeField] SpriteRenderer forearmRendererL;
    [SerializeField] SpriteRenderer forearmRendererR;
    [SerializeField] SpriteRenderer sleeveRendererR;

    // 좌우 신발
    [SerializeField] SpriteRenderer shoesRendererL;
    [SerializeField] SpriteRenderer shoesRendererR;

    // 장비 장착 시 외형 반영 (Addressables에서 Sprite 이름으로 불러오기)
    public void SetGearVisual(Define.GearType type, string partName, string spriteName)
    {
        // 어드레서블 스프라이트 로드해서 완료 시 해당 부위에 적용
        Managers.Resource.LoadSprite(spriteName, (sprite) =>
        {
            if (sprite == null)
            {
                //Debug.LogWarning($"[PlayerVisual] 스프라이트 {spriteName} 불러오기 실패");
                return;
            }

            //Debug.Log($"SetGearVisual: {sprite.name}");
            ApplySprite(type, partName, sprite); //해당 부위에 스프라이트 적용
        });
    }

    // // 직접 Sprite를 넘기는 오버로드 버전
    // public void SetGearVisual(Define.GearType type, Sprite sprite)
    // {
    //     if (sprite == null)
    //     {
    //         Debug.LogWarning($"[PlayerVisual] null 스프라이트가 전달됨 (타입: {type})");
    //         return;
    //     }
    //
    //     ApplySprite(type, sprite);
    // }

    // 부위명 기준으로 SpriteRenderer에 직접 적용 (ArmL, ArmR 등)
    // type 은 장비 타입, partName은 적용할 부위 이름, sprite는 적용할 스프라이트
    public void ApplySprite(Define.GearType type, string partName, Sprite sprite)
    {
        switch (partName)
        {
            case "Hat":
                hatRenderer.sprite = sprite;
                hatRenderer.enabled = sprite != null;
                break;
            case "Weapon":
                weaponRenderer.sprite = sprite;
                weaponRenderer.enabled = sprite != null;
                break;
            case "Torso":
                armorRenderer.sprite = sprite;
                armorRenderer.enabled = sprite != null;
                break;
            case "ArmL":
                armLRenderer.sprite = sprite;
                armLRenderer.enabled = sprite != null;
                break;
            case "ArmR":
                armRRenderer.sprite = sprite;
                armRRenderer.enabled = sprite != null;
                break;
            case "Pelvis":
                pelvisRenderer.sprite = sprite;
                pelvisRenderer.enabled = sprite != null;
                break;
            case "HandL":
                handRendererL.sprite = sprite;
                handRendererL.enabled = sprite != null;
                break;
            case "HandR":
                handRendererR.sprite = sprite;
                handRendererR.enabled = sprite != null;
                break;
            case "ForearmL":
                forearmRendererL.sprite = sprite;
                forearmRendererL.enabled = sprite != null;
                break;
            case "ForearmR":
                forearmRendererR.sprite = sprite;
                forearmRendererR.enabled = sprite != null;
                break;
            case "SleeveR":
                sleeveRendererR.sprite = sprite;
                sleeveRendererR.enabled = sprite != null;
                break;
            case "Shin":
                shoesRendererL.sprite = sprite;
                shoesRendererL.enabled = sprite != null;
                shoesRendererR.sprite = sprite;
                shoesRendererR.enabled = sprite != null;
                break;
            default:
                Debug.LogWarning($"[PlayerVisual] 알 수 없는 partName: {partName}");
                break;
        }
    }
}