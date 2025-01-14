using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCardManager : MonoBehaviour {
    public Text Title;
    public GameObject PlayerLeft;
    public GameObject PlayerRight;
    public Text TextScore;
    public Text TextStatus;
    public string roomID;


    private void SetPlayerUI(GameObject Player, MatchState.Player data) {
        ImageLoader loader = Player.transform.Find("Avatar").transform.Find("Avatar").GetComponent<ImageLoader>();
        loader.SetImageUrl(data.avatar);
        Text Name = Player.transform.Find("Name").GetComponent<Text>();
        Name.text = data.name;
        if (data.id == GameManager.Instance.appState.profile.id) {
            Name.color = Helper.ColorFromHex(Configs.Color.green01);
        }
    }

    private string GetText(string key) {
        return Helper.GetLocalizedValue(LocalizationManager.Table.Game, key);
    }

    private string GetGameByType(string roomType) {
        if (roomType == "throw_egg") {
            return GetText("throwEgg");
        }
        if (roomType == "solve_math") {
            return GetText("solveMath");
        }
        if (roomType == "flappy_egg") {
            return GetText("flappyEgg");
        }
        return "";
    }

    public void Initialize(string _roomID) {
        roomID = _roomID;
        MatchState matchState = GameManager.Instance.gameState.matchState;
        List<JObject> rooms = matchState.rooms;
        int index = rooms.FindIndex(r => r["id"].ToString() == roomID);
        JObject room = rooms[index];
        RoomThrowEgg roomThrowEgg = room.ToObject<RoomThrowEgg>();

        Title.text = $"{GetText("round")} {index + 1}: {GetGameByType(roomThrowEgg.type)}";

        MatchState.Player Player01 = matchState.players.Find(p => p.id == roomThrowEgg.players[0].id);
        MatchState.Player Player02 = matchState.players.Find(p => p.id == roomThrowEgg.players[1].id);

        int sum01 = (Player01.id + Player01.name).ToIntArray().Sum();
        int sum02 = (Player02.id + Player02.name).ToIntArray().Sum();
        bool isLess = sum01 < sum02;

        MatchState.Player left = isLess ? Player01 : Player02;
        MatchState.Player right = isLess ? Player02 : Player01;
        int leftPoint = isLess ? roomThrowEgg.players[0].point : roomThrowEgg.players[1].point;
        int rightPoint = isLess ? roomThrowEgg.players[1].point : roomThrowEgg.players[0].point;

        SetPlayerUI(PlayerLeft, left);
        SetPlayerUI(PlayerRight, right);

        TextScore.text = $"{leftPoint}  -  {rightPoint}";
        if (roomThrowEgg.status == BaseRoom.Status.ended.ToString()) {
            TextStatus.text = GetText("ended");
        }
    }

    private IEnumerator _CountDownToBegin(int seconds) {
        GetComponent<Outline>().enabled = true;
        string BeginText = GetText("beginIn");
        for (int i = seconds; i >= 1; i--) {
            string count = i > 10 ? $"{i}" : $"0{i}";
            TextStatus.text = $"{BeginText} {count}";
            yield return new WaitForSeconds(1f);
        }
        TextStatus.text = GetText("start");
        yield return new WaitForSeconds(1f);
        Navigator.Instance.NetworkLoad(Navigator.Scene.GameThrowEgg);
    }

    public void CountDownToBegin(int seconds = 3) {
        StartCoroutine(_CountDownToBegin(seconds));
    }
}
