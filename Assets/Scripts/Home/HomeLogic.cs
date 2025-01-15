using UnityEngine;

public class HomeLogic : MonoBehaviour {
    public void PauseUnPauseMusicBackground() {
        SoundManager.Instance.PauseUnPauseMusicBackground();
    }


    public void GoToProfile() {
        Navigator.Instance.NavigateTo(Navigator.Scene.Profile);
    }


    public void GoToMatchMaking() {
        Navigator.Instance.NavigateTo(Navigator.Scene.MatchMaking);
    }
}
