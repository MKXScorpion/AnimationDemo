using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Grid")]
    public GameCard cardPrefab;
    public Transform gridTransform;

    [Header("Symbols")]
    public Sprite[] sprites;

    [Header("UI")]
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI timerText;
    public GameObject winPopup;
    public GameObject losePopup;

    [Header("Stars")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Sound")]
    public AudioSource fxaudio;
    public AudioClip popsound;

    // runtime
    List<Sprite> spritePairs;
    List<GameCard> spawnedCards = new List<GameCard>();

    GameCard firstSelected;
    GameCard secondSelected;

    bool canSelect = true;
    bool gameEnded = false;

    public int totalCards;
    public int matchedPairs;
    public int movesLeft;
    int initialMoves;
    public float timer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetupFromGameSettings();
        ReadyCards();
        CreateCards();
        UpdateMovesUI();
        StartCoroutine(TimerRoutine());
        ResetStars();
        Debug.Log("Total Cards: " + totalCards);
        Debug.Log("SpritePairs Count: " + spritePairs.Count);
    }

    // ---------------- SETTINGS ----------------
    void SetupFromGameSettings()
    {
        int level = GameSettings.level;

        if (level >= 1 && level <= 3) // EASY
        {
            SetGrid(3, 3);
            totalCards = 9;
            movesLeft = 12 - (level - 1) * 2;
            timer = 120f;
        }
        else if (level >= 4 && level <= 6) // MEDIUM
        {
            SetGrid(5, 4);
            totalCards = 20;
            movesLeft = 16 - (level - 4) * 2;
            timer = 100f;
        }
        else // HARD
        {
            SetGrid(5, 5);
            totalCards = 25;
            movesLeft = 14 - (level - 7) * 2;
            timer = 80f;
        }

        initialMoves = movesLeft;
    }

    void SetGrid(int cols, int rows)
    {
        GridLayoutGroup grid = gridTransform.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
    }

    // ---------------- CARD SPAWN ----------------
    void ReadyCards()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("Sprites array is empty! Assign sprites in the Inspector.");
            return;
        }

        spritePairs = new List<Sprite>();

        int pairCount = totalCards / 2;

        for (int i = 0; i < pairCount; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }

        // Handle odd card 
        if (totalCards % 2 != 0)
        {
            spritePairs.Add(sprites[0]); // dummy duplicate
        }

        ShuffleCards(spritePairs);
    }

    void CreateCards()
    {
        foreach (Transform t in gridTransform)
            Destroy(t.gameObject);

        spawnedCards.Clear();

        for (int i = 0; i < spritePairs.Count; i++)
        {
            GameCard card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePairs[i]);
            card.gameController = this;
            spawnedCards.Add(card);
        }
    }

    void ShuffleCards(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            Sprite temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // ---------------- CARD CLICK ----------------
    public void SelectedCard(GameCard card)
    {
        if (!canSelect || gameEnded || card.isSelected) return;

        card.ShowCard();
        fxaudio.PlayOneShot(popsound);

        if (firstSelected == null)
        {
            firstSelected = card;
            return;
        }

        secondSelected = card;
        canSelect = false; //  LOCK
        StartCoroutine(CheckMatching());
    }

    IEnumerator CheckMatching()
    {
        yield return new WaitForSeconds(0.5f);

        movesLeft--;
        UpdateMovesUI();

        if (firstSelected.iconSprite == secondSelected.iconSprite)
        {
            // Matched
            firstSelected.isMatched = true;
            secondSelected.isMatched = true;
            matchedPairs++;

            if (matchedPairs >= spritePairs.Count / 2)
                WinGame();
        }
        else
        {
            firstSelected.HideCard();
            secondSelected.HideCard();
        }

        if (movesLeft <= 0)
        {
            LoseGame();
            yield break;
        }

        firstSelected = null;
        secondSelected = null;
        canSelect = true; //  UNLOCK
    }



    // ---------------- TIMER ----------------
    IEnumerator TimerRoutine()
    {
        while (timer > 0 && !gameEnded)
        {
            timer -= Time.deltaTime;
            timerText.text = Mathf.Ceil(timer).ToString();
            yield return null;
        }

        if (!gameEnded)
            LoseGame();
    }

    // ---------------- WIN / LOSE ----------------
    void WinGame()
    {
        gameEnded = true;
        winPopup.SetActive(true);

        int stars = CalculateStars();
        ShowStars(stars);
        SaveStars(stars);

        StartCoroutine(LoadNextLevel());
    }

    void LoseGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        losePopup.SetActive(true);
        canSelect = false;
        StartCoroutine(RestartLevel());
}
    
    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(3f);
        GameSettings.level++;

        if (GameSettings.level > GameSettings.MAX_LEVEL)
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------------- STARS ----------------
    int CalculateStars()
    {
        float ratio = (float)movesLeft / initialMoves;
        if (ratio >= 0.6f) return 3;
        if (ratio >= 0.3f) return 2;
        return 1;
    }

    void ShowStars(int stars)
    {
        star1.SetActive(stars >= 1);
        star2.SetActive(stars >= 2);
        star3.SetActive(stars >= 3);
    }

    void SaveStars(int stars)
    {
        string key = "LEVEL_" + GameSettings.level + "_STARS";
        int old = PlayerPrefs.GetInt(key, 0);
        if (stars > old)
            PlayerPrefs.SetInt(key, stars);
    }

    void ResetStars()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Time.timeScale = 0f;
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
    void UpdateMovesUI()
    {
        movesText.text = "Moves: " + movesLeft;
    }
}