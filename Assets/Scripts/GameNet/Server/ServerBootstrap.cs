using UnityEngine;
using Mirror;

namespace GameNet.Server
{
    /// <summary>
    /// Server bootstrap helper for starting a dedicated server.
    /// Automatically starts the server when running in headless/batch mode.
    /// Can also be triggered manually for testing.
    /// </summary>
    /// 
    public class ServerBootstrap : MonoBehaviour
    {
        [SerializeField]
        private RelayServerNetworkManager networkManager;

        [SerializeField]
        private bool autoStartInBatchMode = true;

        [SerializeField]
        private bool autoStartInEditor = false;

        private void Start()
        {
            if (networkManager == null)
            {
                Debug.LogError("[ServerBootstrap] RelayServerNetworkManager not assigned.", this);
                return;
            }

            bool shouldAutoStart = false;

            // Check if running in batch/headless mode (dedicated server)
            if (Application.isBatchMode && autoStartInBatchMode)
            {
                shouldAutoStart = true;
                Debug.Log("[ServerBootstrap] Running in batch mode. Auto-starting server...");
            }
            else if (Application.isEditor && autoStartInEditor)
            {
                shouldAutoStart = true;
                Debug.Log("[ServerBootstrap] Running in editor with auto-start enabled. Starting server...");
            }

            if (shouldAutoStart)
            {
                StartServer();
            }
        }

        public void StartServer()
        {
            if (NetworkServer.active)
            {
                Debug.LogWarning("[ServerBootstrap] Server already running.", this);
                return;
            }

            networkManager.StartServer();
            Debug.Log("[ServerBootstrap] Server started successfully.");
        }

        public void StopServer()
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[ServerBootstrap] Server not running.", this);
                return;
            }

            networkManager.StopServer();
            Debug.Log("[ServerBootstrap] Server stopped.");
        }

        private void OnApplicationQuit()
        {
            if (NetworkServer.active)
            {
                StopServer();
            }
        }
    }
}
