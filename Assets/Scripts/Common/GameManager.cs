using System;
using System.Collections;
using UnityEngine;



public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    // Background
    public Sprite[] listBackgrounds;
    private SpriteRenderer spriteRenderer;

    // State profile
    public Profile profileState = new Profile {
        name = "Duc Dat",
        avatar = "https://cdn.dribbble.com/users/17793/screenshots/16101765/media/beca221aaebf1d3ea7684ce067bc16e5.png",
        eggs = 99
    };
    private event Action<Profile> OnProfileChanged;


    // GameState
    public GameState gameState;
    private event Action<GameState> OnGameStateChanged;


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
        profileState = newState;
        OnProfileChanged?.Invoke(profileState);
    }
    public void ListenProfileChanged(Action<Profile> listener) {
        OnProfileChanged += listener;
    }
    public void RemoveListenProfileChanged(Action<Profile> listener) {
        OnProfileChanged -= listener;
    }


    private IEnumerator CountUpCoroutine() {
        UpdateGameState(new GameState {
            status = GameState.Status.findingMatch,
            data = new FindingMatchState {
                secondsElapsed = 0,
            }
        });

        FindingMatchState data = (FindingMatchState)gameState.data;

        while (gameState.status == GameState.Status.findingMatch) {
            yield return new WaitForSeconds(1f);
            data.secondsElapsed++;
            UpdateGameState(gameState);
        }
    }
    public void UpdateGameState(GameState state) {
        gameState = state;
        OnGameStateChanged?.Invoke(gameState);
    }
    public void ListenGameStateChanged(Action<GameState> listener) {
        OnGameStateChanged += listener;
    }
    public void RemoveListenGameStateChanged(Action<GameState> listener) {
        OnGameStateChanged -= listener;
    }
    public void StartCountUp() {
        StartCoroutine(CountUpCoroutine());
    }
    public void StopCountUp() {
        gameState = new GameState {
            status = GameState.Status.none,
            data = new object(),
        };
    }
}

