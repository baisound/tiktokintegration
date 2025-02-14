using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using TikTokLiveSharp;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;

public IEnumerator PostGiftInfo(
    string endpoint,
    string bearerToken,
    GiftMessage gift,
    string licenseKey,
    string contentsId
)
{
    Debug.Log($"[ApiClient] Sending gift info: ID={gift.GiftId}, Name={gift.GiftName}, Amount={gift.Amount}");

    string jsonPayload = $@"{{
        ""license_key"": ""{licenseKey}"",
        ""contents_id"": ""{contentsId}"",
        ""gift_time"": ""{DateTime.Now:yyyy-MM-dd HH:mm:ss}"",
        ""gift_id"": ""{gift.GiftId}"",
        ""gift_name"": ""{gift.GiftName}"",
        ""gift_coin"": ""{gift.Cost}"",
        ""gift_num"": ""{gift.Amount}"",
        ""from_unique_id"": ""{gift.UserId}"",
        ""from_name"": ""{gift.UserName}"",
        ""from_id"": ""{gift.UserId}"",
        ""to_unique_id"": ""{gift.RoomId}"",
        ""to_name"": ""{gift.AnchorName}"",
        ""to_id"": ""{gift.AnchorId}""
    }}";

    Debug.Log($"[ApiClient] JSON Payload: {jsonPayload}");

    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
    UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + bearerToken);

    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log($"[ApiClient] ギフト情報送信成功: {request.downloadHandler.text}");
    }
    else
    {
        Debug.LogError($"[ApiClient] ギフト情報送信失敗: {request.error}");
    }
}
