using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace GameNet.Client
{
    /// <summary>
    /// Simple debug UI for GameNet connection controls.
    /// Provides Connect/Disconnect buttons and connection status.
    /// </summary>
    public class GameNetDebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ClientBootstrap clientBootstrap;

        [Header("UI Elements")]
        [SerializeField]
        private Button connectButton;

        [SerializeField]
        private Button disconnectButton;

        [SerializeField]
        private Text statusText;

        [Header("Settings")]
        [SerializeField]
        private bool hideOnConnect = false;

        private void Start()
        {
            if (clientBootstrap == null)
            {
                clientBootstrap = FindFirstObjectByType<ClientBootstrap>();
                if (clientBootstrap == null)
                {
                    Debug.LogError("[GameNetDebugUI] ClientBootstrap not found. Assign it in the Inspector.", this);
                    return;
                }
            }

            SetupButtons();
            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void SetupButtons()
        {
            if (connectButton != null)
            {
                connectButton.onClick.AddListener(OnConnectClicked);
            }

            if (disconnectButton != null)
            {
                disconnectButton.onClick.AddListener(OnDisconnectClicked);
            }
        }

        private void OnConnectClicked()
        {
            if (clientBootstrap != null)
            {
                clientBootstrap.Connect();
            }
        }

        private void OnDisconnectClicked()
        {
            if (clientBootstrap != null)
            {
                clientBootstrap.Disconnect();
            }
        }

        private void UpdateUI()
        {
            bool isConnected = NetworkClient.isConnected || NetworkClient.active;

            if (connectButton != null)
            {
                connectButton.interactable = !isConnected;
            }

            if (disconnectButton != null)
            {
                disconnectButton.interactable = isConnected;
            }

            if (statusText != null)
            {
                if (isConnected)
                {
                    statusText.text = "Status: Connected";
                    statusText.color = Color.green;
                }
                else
                {
                    statusText.text = "Status: Offline";
                    statusText.color = Color.yellow;
                }
            }

            // Hide panel when connected if enabled
            if (hideOnConnect && isConnected)
            {
                gameObject.SetActive(false);
            }
        }

        [ContextMenu("Toggle Visibility")]
        public void ToggleVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}
