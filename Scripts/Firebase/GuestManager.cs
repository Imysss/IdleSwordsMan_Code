using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json;

//public class GuestManager : MonoBehaviour
public class GuestManager : MonoBehaviour
{
    public static GuestManager Instance { get; private set; }

    private const string PREF_USER_ID = "user_id";
    private const string PREF_GUEST_ID = "guest_id";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        InitGuestIdIfNeeded();

        //Managers.SaveLoad.Save();
    }

    //  게스트 ID 없으면 생성
    private void InitGuestIdIfNeeded()
    {
        if (!PlayerPrefs.HasKey(PREF_GUEST_ID))
        {
            string uuid = "guest-" + System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(PREF_GUEST_ID, uuid);
            PlayerPrefs.Save();
            Debug.Log($"[Guest] UUID 생성됨: {uuid}");
        }
        else
        {
            Debug.Log($"[Guest] 기존 guest_id: {PlayerPrefs.GetString(PREF_GUEST_ID)}");
        }

        if (!PlayerPrefs.HasKey(PREF_USER_ID))
        {
            PlayerPrefs.SetString(PREF_USER_ID, PlayerPrefs.GetString(PREF_GUEST_ID));
        }
        PlayerPrefs.Save();  // 꼭 저장
    }

    //  현재 유저 ID 반환 (UID 또는 게스트)
    public string GetCurrentUserId()
    {
        return PlayerPrefs.GetString(PREF_USER_ID);
    }

    //  로그인 성공 시 호출 (Firebase UID를 유저 ID로 등록)
    public void OnLoginSuccess(string firebaseUid)
    {
        string guestId = PlayerPrefs.GetString(PREF_GUEST_ID);
        CoroutineManager.StartCoroutine(MigrateGuestData(guestId, firebaseUid));
        PlayerPrefs.SetString(PREF_USER_ID, firebaseUid);
        PlayerPrefs.Save();
    }

    //  서버에 게스트 데이터 → UID로 마이그레이션
    private IEnumerator MigrateGuestData(string guestId, string firebaseUid)
    {
        var body = new { guestId, uid = firebaseUid };
        string json = JsonConvert.SerializeObject(body);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("https://us-central1-team-55ee2.cloudfunctions.net/api/save/migrate", "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            Debug.Log("[Guest] 마이그레이션 성공");
        else
            Debug.LogWarning("[Guest] 마이그레이션 실패: " + request.error);
    }
    public void SetCurrentUserId(string uid)
    {
        PlayerPrefs.SetString(PREF_USER_ID, uid);
        PlayerPrefs.Save();
    }
}