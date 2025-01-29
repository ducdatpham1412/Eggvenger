using Unity.Netcode;
using UnityEngine;

public class ConnectManager : NetworkBehaviour {
    [ServerRpc(RequireOwnership = false)]
    void DisconnectedServerRpc(ServerRpcParams rpcParams = default) {
        bool allDisconnected = true;
        string DisconnectedStatus = MatchState.Player.Status.disconnected.ToString();
        foreach (var player in GameManager.Instance.gameState.matchState.players) {
            if (player.clientID == (int)rpcParams.Receive.SenderClientId) {
                player.status = DisconnectedStatus;
            }
            else if (player.status != DisconnectedStatus) {
                allDisconnected = false;
            }
        }

        if (allDisconnected) {
            // When calling this, server will auto send event socket "Disconnected", and UDP will auto terminate this server process
            Application.Quit();
        }
    }

    void OnApplicationQuit() {
        if (IsClient) {
            Debug.Log("Client send disconnected");
            DisconnectedServerRpc();
        }
    }

    // TODO: Handle ReConnected
}
