using UnityEngine;

public class ExitButtonScript : MonoBehaviour
{
    public void DisconnectAllPlayers() {
        if (ServerManager.Instance) ServerManager.Instance.DisconnectAllPlayers();
    }
}
