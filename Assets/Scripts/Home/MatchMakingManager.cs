using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class MatchmakingManager : NetworkBehaviour {
    [Serializable]
    private class NetworkClient {
        public ulong ID;
    }

    private List<NetworkClient> connectedClients = new List<NetworkClient>();
    private List<NetworkClient> waitingClients = new List<NetworkClient>();

    public override void OnNetworkSpawn() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }


    private void OnClientDisconnected(ulong clientId) {
        int index = connectedClients.FindIndex(client => client.ID == clientId);
        connectedClients.RemoveAt(index);
    }

    private void OnClientConnected(ulong clientId) {
        connectedClients.Add(new NetworkClient {
            ID = clientId,
        });
    }

    private void StartMatch() {
        var client1 = waitingClients[0];
        var client2 = waitingClients[1];

        waitingClients.RemoveRange(0, 2);
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        StartMatchClientRpc(client1.ID, client2.ID);
    }

    [ClientRpc]
    private void StartMatchClientRpc(ulong client1Id, ulong client2Id) {
        if (NetworkManager.Singleton.IsClient) {
            SceneManager.LoadScene("GameScene");
            SpawnPlayerForClient(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId) {
        GameObject playerPrefab = Resources.Load<GameObject>("PlayerPrefab");
        var playerInstance = Instantiate(playerPrefab);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }


    [ServerRpc(RequireOwnership = false)]
    private void FindMatchServerRpc(ServerRpcParams rpcParams = default) {
        ulong clientId = rpcParams.Receive.SenderClientId;
        NetworkClient Client = connectedClients.Find(client => client.ID == clientId);
        if (Client != null) {
            waitingClients.Add(new NetworkClient {
                ID = clientId,
            });
        }
    }

    public void FindMatch() {
        Debug.Log("Find match");
        FindMatchServerRpc();
    }
}
