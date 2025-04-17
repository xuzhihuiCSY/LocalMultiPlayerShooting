using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public TMP_InputField ipInputField;
    public TextMeshProUGUI hostIPText;

    private Canvas canvas;

    void Start()
    {
        canvas = GetComponent<Canvas>();

        // ✅ Show IP immediately
        if (hostIPText != null)
        {
            hostIPText.text = "Your IP: " + GetLocalIPAddress();
        }

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log($"Host is listening on IP: {GetLocalIPAddress()} and port: 7777");
            canvas.enabled = false;
        });

        clientButton.onClick.AddListener(() =>
        {
            string ip = (ipInputField != null && !string.IsNullOrWhiteSpace(ipInputField.text))
                ? ipInputField.text
                : "127.0.0.1";

            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Address = ip;

            Debug.Log("Attempting to join host at IP: " + ip);

            NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
            {
                Debug.Log($"✅ Client connected! Client ID: {clientId}");
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
            {
                Debug.Log($"❌ Client disconnected. Client ID: {clientId}");
            };

            NetworkManager.Singleton.StartClient();
            canvas.enabled = false;
        });
    }

    private string GetLocalIPAddress()
    {
        string localIP = "Unavailable";
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint?.Address.ToString();
            }
        }
        catch { }

        return localIP;
    }
}
