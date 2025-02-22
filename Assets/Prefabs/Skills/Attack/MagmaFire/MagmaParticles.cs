using System.Threading.Tasks;
using UnityEngine;

public class MagmaParticles : MonoBehaviour {
    [Header("VFXs")]
    [SerializeField] Sprite BurnEffect;
    [SerializeField] MagmaFire Fire;

    void OnParticleCollision(GameObject other) {
        PlayerManager player = other.gameObject.GetComponent<PlayerManager>();
        if (player != null && player.team != Fire.Creator.team && !Fire.effectedPlayers.Contains(player)) {
            BurnPlayer(player);
        }
    }

    public async void BurnPlayer(PlayerManager player) {
        if (!Fire.effectedPlayers.Contains(player)) {
            Fire.effectedPlayers.Add(player);
            PlayerVFX playerVFX = player.GetComponent<PlayerVFX>();
            playerVFX.SetSkin(BurnEffect, 0.2f);
            await Task.Delay((int)Fire.effectDuration * 1000);
            playerVFX.ResetSkin(BurnEffect, 0.1f);
            Fire.effectedPlayers.Remove(player);
        }
    }
}
