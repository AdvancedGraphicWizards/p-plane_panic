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
            // Do nothing
        }

        if (IsOwner)
        {

            // Enable the gyroscope if it is supported, otherwise use the accelerometer
            if (SystemInfo.supportsGyroscope)
                Input.gyro.enabled = m_gyroEnabled = true;
            else
                accelerometerInput = Input.acceleration;

            SetName(ClientUI.Instance.playerName);
            playerColor.OnValueChanged += (prevValue, newValue) =>
            {
                ClientUI.Instance.playerColor = newValue;
            };
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
    }

    // Overloaded method, connecting with a code defined in the prefab
    public void Connect()
    {
        Connect(m_joinCode);
    }

    public void Connect(string joinCode)
    {
        Debug.Log("Joining relay server with join code: " + joinCode);
        RelayManager.JoinRelay(joinCode);
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

    void OnDestroy()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
