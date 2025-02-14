using UnityEngine;
using TikTokLiveSharp;
using TikTokLiveSharp.Client;
using TikTokLiveSharp.Events;
using TikTokLiveSharp.Events.Objects;
using TikTokLiveUnity.Utils;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;

public class TikTokIntegration : MonoBehaviour
{
    // �ݒ���ێ��p�N���X�iJSON���}�b�s���O�j
    [System.Serializable]
    public class TikTokIntegrationConfig
    {
        public string TikTokUser;
        public string BearerToken;
        public string RestApiEndpoint;
        public string LicenseKey;
        public string ContentsId;
        public Dictionary<string, GiftAction> GiftActions;
    }

    [System.Serializable]
    public class GiftAction
    {
        public string command;
        public Dictionary<string, object> parameters;
    }

    public delegate void GiftReceivedDelegate(GiftMessage gift, GiftAction action);
    public event GiftReceivedDelegate OnGiftReceived;

    private TikTokLiveClient client;
    private ApiClient apiClient;
    private TikTokIntegrationConfig config;

    [Header("Config File Path")]
    public string configFilePath = "Assets/configs/config.json"; // ��Ƃ��ČŒ�p�X

    void Awake()
    {
        LoadConfig();
        apiClient = this.GetComponent<ApiClient>();
        if (apiClient == null)
        {
            apiClient = this.gameObject.AddComponent<ApiClient>();
        }
    }

    void Start()
    {
        // TikTok���X�j���O�J�n
        if (!string.IsNullOrEmpty(config.TikTokUser))
        {
            client = new TikTokLiveClient(config.TikTokUser);
            client.OnGiftReceived += HandleGift;
            client.Start();
            Debug.Log("TikTokIntegration: Started listening to " + config.TikTokUser);
        }
        else
        {
            Debug.LogError("TikTokIntegration: TikTokUser not configured!");
        }
    }

    private void LoadConfig()
    {
        if (File.Exists(configFilePath))
        {
            string json = File.ReadAllText(configFilePath);
            config = JsonConvert.DeserializeObject<TikTokIntegrationConfig>(json);
        }
        else
        {
            Debug.LogError("Config file not found at: " + configFilePath);
            config = new TikTokIntegrationConfig();
        }
    }

    private void HandleGift(object sender, GiftMessage gift)
    {
        Debug.Log($"[TikTokIntegration] Gift Received: ID={gift.GiftId}, Name={gift.GiftName}, Amount={gift.Amount}");

        // GiftAction �擾
        GiftAction action = null;
        if (config.GiftActions != null && config.GiftActions.ContainsKey(gift.GiftId))
        {
            action = config.GiftActions[gift.GiftId];
        }

        // API���M
        StartCoroutine(apiClient.PostGiftInfo(
            config.RestApiEndpoint,
            config.BearerToken,
            gift,
            config.LicenseKey,
            config.ContentsId
        ));

        // �C�x���g����
        OnGiftReceived?.Invoke(gift, action);
    }
}
