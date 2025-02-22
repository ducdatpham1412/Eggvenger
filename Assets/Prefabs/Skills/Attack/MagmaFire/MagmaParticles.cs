using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MagmaParticles : MonoBehaviour {
    [Header("VFXs")]
    [SerializeField] Sprite BurnEffect;
    [SerializeField] MagmaFire Fire;

    List<PlayerManager> burnedPlayers = new List<PlayerManager>();
    float effectDuration;
    PlayerManager Creator;

    void Awake() {
        effectDuration = Fire.GetEffectDuration();
        Creator = Fire.GetCreator();
    }

    void OnParticleCollision(GameObject other) {
        PlayerManager player = other.gameObject.GetComponent<PlayerManager>();
        if (player != null && player.team != Creator.team && !burnedPlayers.Contains(player)) {
            BurnPlayer(player);
        }
    }

    public async void BurnPlayer(PlayerManager player) {
        if (!burnedPlayers.Contains(player)) {
            burnedPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            playerVFX.SetSkin(BurnEffect, 0.2f);
            await Task.Delay((int)effectDuration * 1000);
            playerVFX.ResetSkin(BurnEffect, 0.1f);
            burnedPlayers.Remove(player);
        }
    }
}
