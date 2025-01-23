using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager netManager;

    void Start()
    {
        netManager = GetComponentInParent<NetworkManager>();

        // If running in Unity Editor, skip
        if (Application.isEditor) return;

        // 1) Check environment variable first (Docker/Coolify can set this)
        string envMode = System.Environment.GetEnvironmentVariable("APP_MODE");
        if (!string.IsNullOrEmpty(envMode))
        {
            StartBasedOnMode(envMode);
            return;
        }

        // 2) Otherwise, fallback to original command-line argument parsing
        var args = GetCommandLineArgs();
        if (args.TryGetValue("-mode", out string mode))
        {
            StartBasedOnMode(mode);
        }
    }

    private void StartBasedOnMode(string mode)
    {
        switch (mode.ToLower())
        {
            case "server":
                netManager.StartServer();
                Debug.Log("Server started");
                break;
            case "host":
                netManager.StartHost();
                Debug.Log("Host started");
                break;
            case "client":
                netManager.StartClient();
                Debug.Log("Client started");
                break;
            default:
                Debug.LogWarning($"Unknown mode: {mode}");
                break;
        }
    }

    private Dictionary<string, string> GetCommandLineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = (i < args.Length - 1) ? args[i + 1] : null;
                if (value != null && value.StartsWith("-"))
                {
                    value = null;
                }

                argDictionary.Add(arg, value?.ToLower());
            }
        }
        return argDictionary;
    }
}