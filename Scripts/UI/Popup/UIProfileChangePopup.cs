using System;
using System.Linq;
using Assets.HeroEditor.Common.Scripts.Common;
using UnityEngine;
using UnityEngine.UI;

public class UIProfileChangePopup : UIPopup
{
    #region UI 기능 리스트
    //ProfileImage: 프로필 이미지
    //ProfileFrameImage: 프레임 이미지
    //ProfileScrollContentObject: 프로필/프레임 인벤토리 나열
    //ConditionText: 획득 조건 텍스트
    
    //ProfileToggle: 프로필 토글 
    //FrameToggle: 프레임 토글
    
    //ClaimButton: 선택 버튼
    //ExitButton: 나가기 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        ProfileScrollContentObject,
        ProfileToggleRedDotObject,
        FrameToggleRedDotObject
    }
    
    enum Buttons
    {
        ClaimButton,
        ExitButton,
    }

    enum Texts
    {
        ConditionText,
        EquipText,
    }

    enum Images
    {
        ProfileImage,
        ProfileFrameImage,
    }

    enum Toggles
    {
        ProfileToggle,
        FrameToggle,
    }
    #endregion

    private Define.ProfileType type;
    private ProfileState selectedState;

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        
        EventBus.Subscribe<ProfileSelectedEvent>(ProfileSelectedEventHandler);
        EventBus.Subscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<ProfileSelectedEvent>(ProfileSelectedEventHandler);
        EventBus.UnSubscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        BindToggle(typeof(Toggles));

        GetToggle((int)Toggles.ProfileToggle).gameObject.BindEvent(OnClickProfileToggle);
        GetToggle((int)Toggles.FrameToggle).gameObject.BindEvent(OnClickFrameToggle);
        
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        GetObject((int)GameObjects.ProfileToggleRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Profile));
        GetObject((int)GameObjects.FrameToggleRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Frame));
        
        return true;
    }

    public void SetInfo()
    {
        selectedState = null;
        GetToggle((int)Toggles.ProfileToggle).isOn = true;
        OnClickProfileToggle();
    }

    private void RefreshUI()
    {
        //0. 기본값
        string defaultProfileSpriteName = "emptyIcon";
        string defaultFrameSpriteName = "emptyIcon";
        string profileSpriteName = "";
        string frameSpriteName = "";

        //1. 선택된 상태가 유효하지 않으면 타입별로 장착된 상태 또는 첫 번재 해금 상태를 선택
        if (selectedState == null || selectedState.data.type != type)
        {
            selectedState = Managers.Profile.GetEquippedState(type) ??
                                 Managers.Profile.GetProfileList(type).FirstOrDefault(x => x.IsUnlocked);
        }

        //2. 이미지 이름 결정
        if (type == Define.ProfileType.Profile)
        {
            profileSpriteName = selectedState?.data.name ?? defaultProfileSpriteName;
            frameSpriteName = Managers.Profile.GetEquippedState(Define.ProfileType.Frame)?.data.name ??
                              defaultFrameSpriteName;
        }
        else if (type == Define.ProfileType.Frame)
        {
            profileSpriteName = Managers.Profile.GetEquippedState(Define.ProfileType.Profile)?.data.name ??
                                defaultProfileSpriteName;
            frameSpriteName = selectedState?.data.name ?? defaultFrameSpriteName;
        }
        
        //3. 실제 리소스 로드 및 반영
        Sprite image = Managers.Resource.Load<Sprite>(profileSpriteName + ".sprite");
        Sprite frame = Managers.Resource.Load<Sprite>(frameSpriteName + ".sprite");
        GetImage((int)Images.ProfileImage).sprite = image;
        GetImage((int)Images.ProfileFrameImage).sprite = frame;
        
        //4. 리스트 갱신
        GameObject container = GetObject((int)GameObjects.ProfileScrollContentObject);
        container.DestroyChilds();
        foreach (var item in Managers.Profile.GetProfileList(type))
        {
            UIProfileItem profileItem = Managers.UI.MakeSubItem<UIProfileItem>(container.transform);
            profileItem.SetInfo(item);
            profileItem.GetComponent<Outline>().enabled = (selectedState != null && item.data.key == selectedState.data.key);
        }
        
        //조건 텍스트
        GetText((int)Texts.ConditionText).text = selectedState?.data.condition ?? "";
        GetButton((int)Buttons.ClaimButton).interactable = (selectedState != null && selectedState.IsUnlocked);
        GetButton((int)Buttons.ClaimButton).gameObject.SetActive(selectedState != null && !selectedState.IsEquipped);
        GetText((int)Texts.EquipText).gameObject.SetActive(selectedState != null && selectedState.IsEquipped);
    }

    private void OnClickProfileToggle()
    {
        type = Define.ProfileType.Profile;
        selectedState = null;
        RefreshUI();
    }

    private void OnClickFrameToggle()
    {
        type = Define.ProfileType.Frame;
        selectedState = null;
        RefreshUI();
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Profile.Equip(selectedState.data.key);
        RefreshUI();
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void ProfileSelectedEventHandler(ProfileSelectedEvent evnt)
    {
        selectedState = evnt.data;
        
        RefreshUI();
    }

    private void ProfileRedDotChangedEventHandler(ProfileRedDotChangedEvent evnt)
    {
        GetObject((int)GameObjects.ProfileToggleRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Profile));
        GetObject((int)GameObjects.FrameToggleRedDotObject).SetActive(Managers.Profile.HasNewlyUnlocked(Define.ProfileType.Frame));
    }
}
