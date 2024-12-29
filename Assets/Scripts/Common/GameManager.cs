using System;
using UnityEngine;


[Serializable]
public class Profile {
    public string name;
    public int eggs;
}


public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public Sprite[] listBackgrounds;
    private SpriteRenderer spriteRenderer;
    public Profile ProfileState = new Profile { name = "Duc Dat", eggs = 99 };

    private event Action<Profile> OnProfileChanged;


    private void FitTheScreen() {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;

        float ratioWidth = screenWidth / spriteWidth;
        float ratioHeight = screenHeight / spriteHeight;

        float ratio = ratioWidth > ratioHeight ? ratioWidth : ratioHeight;

        Vector3 scale = transform.localScale;
        scale.x = ratio;
        scale.y = ratio;

        transform.localScale = scale;
    }


    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (listBackgrounds != null && listBackgrounds.Length > 0) {
            int indexBg = UnityEngine.Random.Range(0, listBackgrounds.Length);
            spriteRenderer.sprite = listBackgrounds[indexBg];
            FitTheScreen();
        }
    }

    public void UpdateProfile(Profile newState) {
        ProfileState = newState;
        OnProfileChanged?.Invoke(ProfileState);
    }
    public void ListenProfileChanged(Action<Profile> listener) {
        OnProfileChanged += listener;
    }
    public void RemoveListenProfileChanged(Action<Profile> listener) {
        OnProfileChanged -= listener;
    }
}

