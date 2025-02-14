
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
using com.baisound.tiktokintegration;

namespace com.baisound.tiktokintegration
{
    private ApiClient apiClient;

    public class TikTokIntegration : MonoBehaviour
    {
        // 設定情報保持用クラス（JSONをマッピング）
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
        public string configFilePath = "Assets/configs/config.json"; // 例として固定パス

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
            // TikTokリスニング開始
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

        //private void HandleGift(object sender, GiftMessage gift)
        private void HandleGift(object sender, TikTokLiveSharp.Events.Objects.GiftMessage gift)
        {
            Debug.Log($"[TikTokIntegration] Gift Received: ID={gift.GiftId}, Name={gift.GiftName}, Amount={gift.Amount}");

            // GiftAction 取得
            GiftAction action = null;
            if (config.GiftActions != null && config.GiftActions.ContainsKey(gift.GiftId))
            {
                action = config.GiftActions[gift.GiftId];
            }

            // API送信
            StartCoroutine(apiClient.PostGiftInfo(
                config.RestApiEndpoint,
                config.BearerToken,
                gift,
                config.LicenseKey,
                config.ContentsId
            ));

            // イベント発火
            OnGiftReceived?.Invoke(gift, action);
        }
    }
}