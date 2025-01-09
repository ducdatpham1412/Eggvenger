using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class SocketManager {
    private static UdpClient udpClient;
    private static object lastData;
    public static event Action<JObject> OnHandleData;

    static SocketManager() {
        udpClient = new UdpClient();
        udpClient.Connect("127.0.0.1", 5000);
        Task.Run(HandleState);
        Connect();
    }

    public static void Connect() {
        Send(new Event.Send.Connect {
            player_id = GameManager.Instance.appState.profile.id
        });
    }

    private static async void HandleState() {
        while (true) {
            try {
                var res = await udpClient.ReceiveAsync();
                string json = Encoding.UTF8.GetString(res.Buffer);
                // Deserialize and invoke event
                JObject state = JsonConvert.DeserializeObject<JObject>(json);
                string type = state["type"].ToString();
                if (type == "unauthorized") {
                    Connect();
                }
                else if (type == "connect") {
                    ClientValue client = JsonConvert.DeserializeObject<ClientValue>(state["data"].ToString());
                    GameManager.Instance.UpdateAppState(state => {
                        state.client = client;
                        return state;
                    });
                    if (lastData != null) {
                        Send(lastData);
                        lastData = null;
                    }
                }
                else {
                    lastData = null;
                    OnHandleData?.Invoke(state);
                }
            }
            catch (Exception ex) {
                Debug.LogError($"Error in HandleState: {ex.Message}");
            }
        }
    }

    public static void Send(object data) {
        lastData = data;
        string jsonData = JsonConvert.SerializeObject(data);
        byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
        udpClient.Send(dataBytes, dataBytes.Length);
    }
}
