using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Game Menu logic for buttons, toggles etc.
/// Also hosts the logic for moving between the Main Menu and Game scenes.
/// Contains code to open the secondary window (teacher input).
/// </summary>
public class PlayMenu : MonoBehaviour
{
    
    [SerializeField] ToggleSpriteSwap vowelsToggle;
    [SerializeField] ToggleSpriteSwap consonantsToggle;
    [SerializeField] ToggleSpriteSwap coloringToggle;
    [SerializeField] ToggleSpriteSwap gameModeToggle;
    [SerializeField] ToggleSpriteSwap characterCountsToggle;
    [SerializeField] Button launchGame;
    [SerializeField] Button launchTest;
    [SerializeField] Button secondaryWindowButton;

    [Space(5)]
    [SerializeField] UnityEvent onActiveChange;

    [DllImport("__Internal")]
    private static extern bool Open();

    void OnEnable()
    {
        UpdateUI();
    }

    private void Start()
    {
        // Game Mode Toggle
        if (gameModeToggle != null)
        { gameModeToggle.onValueChanged.AddListener(ToggleGameMode); gameModeToggle.Initialize(Config.gameType == Config.GameType.WordToShape); }

        // Consonants & Vowel Toggles
        if (vowelsToggle != null)
        { vowelsToggle.onValueChanged.AddListener(ToggleVowels); vowelsToggle.Initialize(Config.vowelsActive); }
        if (consonantsToggle != null)
        { consonantsToggle.onValueChanged.AddListener(ToggleConsonnants); consonantsToggle.Initialize(Config.consonantsActive); }

        // Character Counts Toggle
        if (characterCountsToggle != null)
        { characterCountsToggle.onValueChanged.AddListener(ToggleCharacterCounts); characterCountsToggle.Initialize(Config.characterCounts); }

        // Coloring Toggle
        if (coloringToggle != null)
        { coloringToggle.onValueChanged.AddListener(ToggleColoring); coloringToggle.Initialize(Config.coloring); }

        // Launch Game Button
        if (launchGame != null)
        { launchGame.onClick.AddListener(LaunchGame); }

        // Launch Test Button
        if (launchTest != null)
        { launchTest.onClick.AddListener(LaunchTest); }

        // Secondary Window Button
        if (secondaryWindowButton != null)
        { secondaryWindowButton.onClick.AddListener(OpenSecondaryWindow); secondaryWindowButton.gameObject.SetActive(!Config.secondaryWindowOpen); }

        // Initial setup
        ToggleGameMode(Config.gameType == Config.GameType.WordToShape);
    }

    public void UpdateUI()
    {
        if (vowelsToggle != null) vowelsToggle.Initialize(Config.vowelsActive);
        if (consonantsToggle != null) consonantsToggle.Initialize(Config.consonantsActive);
    }

    void ToggleGameMode(bool b)
    {
        // true => word2shape | false => shape2word
        Config.gameType = b ? Config.GameType.WordToShape : Config.GameType.ShapeToWord;
        
        characterCountsToggle?.gameObject.SetActive(!b);
        
        coloringToggle?.gameObject.SetActive(b);
        vowelsToggle?.gameObject.SetActive(b && coloringToggle.IsOn);
        consonantsToggle?.gameObject.SetActive(b && coloringToggle.IsOn);
    }

    void ToggleConsonnants(bool b)
    {
        var change = Config.SetConsonantsActive(b);
        if (change)
        {
            UpdateUI();
        }
        onActiveChange?.Invoke();
    }

    void ToggleVowels(bool b)
    {
        var change = Config.SetVowelsActive(b);
        if (change)
        {
            UpdateUI();
        }
        onActiveChange?.Invoke();
    }

    void ToggleCharacterCounts(bool b)
    {
        Config.characterCounts = b;
    }

    void ToggleColoring(bool b)
    {
        Config.coloring = b;
        vowelsToggle.gameObject.SetActive(b);
        consonantsToggle.gameObject.SetActive(b);
    }

    void LaunchGame()
    {
        Config.testMode = false;
        Config.freeWriting = false;
        Config.progressiveCorrection = true;
        OpenSecondaryWindow();
        StartCoroutine(Launch("Game"));
    }

    void LaunchTest()
    {
        Config.testMode = true;
        Config.freeWriting = false;
        Config.progressiveCorrection = true;
        OpenSecondaryWindow();
        StartCoroutine(Launch("Test1"));
    }

    private IEnumerator Launch(string sceneName)
    {
        var circle = GameObject.Find("Circle");
        circle.GetComponent<Animator>().SetTrigger("Start");
        yield return new WaitForSeconds(1);
        iTween.Stop();
        SceneManager.LoadScene(sceneName);
    }

    public void BackToMainMenu()
    {
        StartCoroutine(Launch("Main Menu")); //SceneManager.LoadScene("Main Menu");
    }

    // called from javascript
    public void SecondaryWindowClosed()
    {
        if (secondaryWindowButton != null)
            secondaryWindowButton.gameObject.SetActive(true);
        Config.secondaryWindowOpen = false;
    }

    // calls javascript
    private void OpenSecondaryWindow()
    {
#if UNITY_EDITOR
        Debug.Log("Opening secondary window...");
        return;
#endif
        var openedNew = Open();
        if (secondaryWindowButton != null)
            secondaryWindowButton.gameObject.SetActive(!openedNew);
        Config.secondaryWindowOpen = true;
    }
}
