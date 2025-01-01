using System;
using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    // Background
    public Sprite[] listBackgrounds;
    private SpriteRenderer spriteRenderer;

    // AppState
    public AppState appState = new AppState {
        profile = new Profile {
            name = "Duc Dat",
            avatar = "https://cdn.dribbble.com/users/17793/screenshots/16101765/media/beca221aaebf1d3ea7684ce067bc16e5.png",
            eggs = 99,
            ranking = 9,
            localeID = 0,
        },
        resource = new Resource {
            backgrounds = new string[] {
                "https://www.seekpng.com/png/small/115-1150622_avatar-demo2x-man-avatar-icon-png.png",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTG7ZHHUdN_3p6I5EAb0khNR1ESNmRw_z-vLgs-qma5nH4xSxAGC38uSZ9rldLMUTmGkfw&usqp=CAU",
                "https://user-images.githubusercontent.com/5709133/50445980-88299a80-0912-11e9-962a-6fd92fd18027.png",
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT5aZnsestsA7FsrvvOF-dFwvfNJx1VphgRRISfSQDYV1lzclKTTCu5wnFuUKXDpLq6FUM&usqp=CAU"
            },
        },
        account = new Account {
            username = "Test username",
            password = "Test password",
            token = "",
            refresh_token = ""
        }
    };
    private event Action<AppState> OnAppStateChanged;


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


    public void UpdateAppState(AppState newState) {
        appState = newState;
        OnAppStateChanged?.Invoke(appState);
    }
    public AppState UpdateAppState(Func<AppState, AppState> action) {
        appState = action(appState);
        return appState;
    }
    public void ListenAppStateChanged(Action<AppState> listener) {
        OnAppStateChanged += listener;
    }
    public void RemoveListenAppStateChanged(Action<AppState> listener) {
        OnAppStateChanged -= listener;
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

