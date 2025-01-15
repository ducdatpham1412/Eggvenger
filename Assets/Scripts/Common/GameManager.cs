using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public Sprite background;
    private SpriteRenderer spriteRenderer;


    // AppState
    public AppState appState = new AppState {
        profile = new Profile {
            // id = $"player_1",
            // name = "Duc Dat",
            // avatar = "https://cdn.dribbble.com/users/17793/screenshots/16101765/media/beca221aaebf1d3ea7684ce067bc16e5.png",
            // eggs = 99,
            // ranking = 9,
            // localeID = 0,
        },
        resource = new Resource(),
        account = new Account {
            // username = "Test username",
            // password = "Test password",
            // token = "",
            // refresh_token = ""
        },
        client = new ClientValue(),
    };
    public event Action<AppState> OnAppStateChanged;


    // GameState
    public GameState gameState = new GameState();
    public event Action<GameState> OnGameStateChanged;

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

    void Awake() {
        // TODO: Remove this, call api here
        appState.profile.id = $"player_{UnityEngine.Random.Range(0, 20)}";
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite[] bgSprites = Resources.LoadAll<Sprite>("Images");
        if (bgSprites != null && bgSprites.Length > 0) {
            int indexBg = UnityEngine.Random.Range(0, bgSprites.Length);
            background = bgSprites[indexBg];
            spriteRenderer.sprite = background;
        }
    }

    void Start() {
        FitTheScreen();
    }

    public void Initialize() { }

    public void UpdateAppState(AppState newState) {
        appState = newState;
        OnAppStateChanged?.Invoke(appState);
    }
    public AppState UpdateAppState(Func<AppState, AppState> action) {
        appState = action(appState);
        return appState;
    }

    IEnumerator CountUpCoroutine() {
        UpdateGameState(new GameState {
            status = GameState.Status.findingMatch,
            data = new FindingMatchState {
                secondsElapsed = 0,
            }
        });

        while (gameState.status == GameState.Status.findingMatch) {
            yield return new WaitForSeconds(1f);
            gameState.data.secondsElapsed++;
            UpdateGameState(gameState);
        }
    }
    public void UpdateGameState(GameState state) {
        gameState = state;
        OnGameStateChanged?.Invoke(gameState);
    }

    public void StartCountUp() {
        StartCoroutine(CountUpCoroutine());
    }
    public void StopCountUp() {
        gameState = new GameState {
            status = GameState.Status.none,
            data = new FindingMatchState(),
        };
    }
}

