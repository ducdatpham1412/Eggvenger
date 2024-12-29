using UnityEngine;


public class GameManager : Singleton<GameManager> {
    protected GameManager() { }

    public Sprite[] listBackgrounds;
    private SpriteRenderer spriteRenderer;


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
        if (listBackgrounds.Length > 0) {
            int indexBg = Random.Range(0, listBackgrounds.Length);
            spriteRenderer.sprite = listBackgrounds[indexBg];
            FitTheScreen();
        }
    }
}

