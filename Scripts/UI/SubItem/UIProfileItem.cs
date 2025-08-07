using System;
using UnityEngine;

public class UIProfileItem : UIBase
{
    #region UI 기능 리스트
    //ProfileImage: 프로필 이미지
    //ProfileEquipObject: 장착 오브젝트
    //ProfileLockObject: 잠금 오브젝트
    #endregion

    #region Enum
    enum GameObjects
    {
        ProfileEquipObject,
        ProfileLockObject,
        
        ProfileRedDotObject,
    }

    enum Images
    {
        ProfileImage,
    }
    #endregion

    private ProfileState data;

    private void OnEnable()
    {
        EventBus.Subscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<ProfileRedDotChangedEvent>(ProfileRedDotChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        
        gameObject.BindEvent(OnClickProfileItem);

        return true;
    }

    public void SetInfo(ProfileState data)
    {
        this.data = data;

        RefreshUI();
    }

    private void RefreshUI()
    {
        Sprite spr = Managers.Resource.Load<Sprite>(data.data.name + ".sprite");
        GetImage((int)Images.ProfileImage).sprite = spr;
        GetObject((int)GameObjects.ProfileLockObject).gameObject.SetActive(!data.IsUnlocked);
        GetObject((int)GameObjects.ProfileEquipObject).gameObject.SetActive(data.IsEquipped);
        
        GetObject((int)GameObjects.ProfileRedDotObject).SetActive(data.IsNewlyUnlocked);
    }

    private void OnClickProfileItem()
    {
        if (data.IsNewlyUnlocked)
        {
            data.IsNewlyUnlocked = false;
            Managers.Profile.Save();
            EventBus.Raise(new ProfileRedDotChangedEvent());
        }
        
        //이벤트 호출
        EventBus.Raise(new ProfileSelectedEvent(data));
    }
    
    private void ProfileRedDotChangedEventHandler(ProfileRedDotChangedEvent evnt)
    {
        RefreshUI();
    }
}
