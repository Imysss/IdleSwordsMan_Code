//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using Data;

//public class TestApi : MonoBehaviour
//{
//    // Firebase Functions 배포된 URL
//    private string apiUrl = "https://us-central1-team-55ee2.cloudfunctions.net/api/gear-data/list";

//    void Start()
//    {
//        // 게임 시작 시 API 호출
//        StartCoroutine(GetGearDataFromServer());
//    }

//    IEnumerator GetGearDataFromServer()
//    {
//        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
//        yield return request.SendWebRequest();

//        if (request.result == UnityWebRequest.Result.Success)
//        {
//            string json = request.downloadHandler.text;
//            Debug.Log("API 응답 성공: \n" + json);

//            // JSON 파싱
//            GearDataList gearList = JsonUtility.FromJson<GearDataList>(json);

//            if (gearList != null && gearList.Items != null)
//            {
//                foreach (GearData gear in gearList.Items)
//                {
//                    Debug.Log($"장비 이름: {gear.name}, ID: {gear.dataId}, 타입: {gear.type}, 희귀도: {gear.rarity}");
//                }
//            }
//            else
//            {
//                Debug.LogWarning("JSON 파싱은 되었지만 Items가 비었거나 null입니다.");
//            }
//        }
//        else
//        {
//            Debug.LogError("API 요청 실패: " + request.error);
//        }
//    }

//    [System.Serializable]
//    public class GearDataList
//    {
//        public List<GearData> Items;
//    }
//}