using System.Collections;
using UnityEngine;
using PlayerType = ThrowEggState.Player;

public class PlayerManager : EntityManager {
    private PlayerType player;
    private SpriteRenderer Renderer;
    public string ID;


    void Awake() {
        Renderer = GetComponent<SpriteRenderer>();
        Renderer.enabled = false;
    }


    void Update() {
        HandlePanning();
    }

    private void UpdateCG(ThrowEggState state) {
        if (Renderer == null) {
            Renderer = GetComponent<SpriteRenderer>();
            Renderer.enabled = false;
        }

        if (player.id == state.data.turn && Renderer.enabled) {
            Renderer.enabled = false;
        }
        else if (player.id != state.data.turn && !Renderer.enabled) {
            Renderer.enabled = true; // TODO: Set interactable if appState.myId = player_id
        }
    }


    public void Initialize(PlayerType _player, ThrowEggState state) {
        player = _player;
        ID = player.id;
        UpdateCG(state);
        panable = true; // TODO: Update this with condition
    }

    protected override void OnUpdateState(ThrowEggState state) {
        player = (PlayerType)Helper.Get(state.data.players, player.id);
        ID = player.id;
        UpdateCG(state);
    }


    private void HandlePanning() {
        if (!panable) {
            return;
        }
    }
}
