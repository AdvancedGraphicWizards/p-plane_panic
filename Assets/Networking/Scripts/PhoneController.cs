using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

// Phone controller class to send input from a phone controller to a server
public class PhoneController : NetworkBehaviour
{
    // Input data from the phone
    bool m_gyroEnabled = false;
    Vector3 accelerometerInput;

    // Input data from the WebGL UI
    [SerializeField] private string m_joinCode;
    [SerializeField] private string m_playerName = "Player";

    // RTT intervals
    private float rttUpdateInterval = 1.0f;
    private float lastRttUpdateTime = 0.0f;

    // Network phone controller variables
    public NetworkVariable<Quaternion> m_rotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // Network player name variable
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        string.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // Network player colour variable
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.black,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    // Called when the object is spawned on the network
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("is owner: " + IsOwner + " is spawned: " + IsSpawned + " is client: " + IsClient);

        if (!IsOwner)
        {
            playerName.OnValueChanged += (prevValue, newValue) =>
            {
                ServerManager.Instance.UpdateNames();
            };
        }

        if (IsOwner)
        {
            // Enable the gyroscope if it is supported, otherwise use the accelerometer
            if (SystemInfo.supportsGyroscope)
                Input.gyro.enabled = m_gyroEnabled = true;
            else
                accelerometerInput = Input.acceleration;

            SetName(ClientUI.Instance.playerName);
            ClientUI.Instance.IsConnected(true);

            // Change the colour of the player represented on the phone UI
            playerColor.OnValueChanged += (prevValue, newValue) =>
            {
                ClientUI.Instance.ChangeColor(newValue);
            };

            // Register for disconnection events
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void Start()
    {
        // Nothing for now
    }

    // Update is called once per frame
    public void Update()
    {
        // Check if the NetworkObject is spawned and if the player is the owner
        if (!IsOwner || !IsSpawned) return;

        // Uses the gyroscope if it is enabled, otherwise uses the accelerometer
        if (m_gyroEnabled)
        {
            Gyroscope m_gyro = Input.gyro;
            m_rotation.Value = new Quaternion(m_gyro.attitude.x, -m_gyro.attitude.z, m_gyro.attitude.y, m_gyro.attitude.w);
        }
        else
        {
            accelerometerInput = Vector3.Lerp(accelerometerInput, Input.acceleration, 0.1f);
            m_rotation.Value = Quaternion.Euler(new Vector3(accelerometerInput.y, 0, -accelerometerInput.x) * 90);
        }

        if (Time.time - lastRttUpdateTime >= rttUpdateInterval)
        {
            UpdatePing();
            lastRttUpdateTime = Time.time;
        }
    }

    public Quaternion GetRotation()
    {
        return m_rotation.Value;
    }

    public string GetName()
    {
        return playerName.Value.ToString();
    }

    public void SetName(string name)
    {
        playerName.Value = m_playerName = name;
    }

    public void SetColor(Color color)
    {
        playerColor.Value = color;
    }

    // Ping is actually super complicated to get properly, we can get it once
    // but client-side without any libraries this is as good as it gets.
    // TODO: Take a look at adding PingTool from Unity.
    private void UpdatePing()
    {
        ClientUI.Instance.UpdatePing(NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId));
        Debug.Log($"Ping: {NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)} ms");
    }

    // Callback for handling unexpected disconnections
    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Disconnected from server.");
            ClientUI.Instance.IsConnected(false);
        }
    }

    new private void OnDestroy()
    {
        // Unregister the disconnection callback
        if (NetworkManager.Singleton != null)
        {
            Debug.Log("Unregistering network disconnection callback.");
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
