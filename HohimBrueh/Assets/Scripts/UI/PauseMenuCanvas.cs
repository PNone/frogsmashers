using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuCanvas : MonoBehaviour
{
    public Toggle charactersBounceEachOther;
    public Toggle weirdBounceTrajectories;
    public Toggle onlyBounceBeforeRecover;
    public Toggle allowTeamMode;
    public Toggle randomizeMaps;
    public Toggle winnerTakesAll;
    public Toggle punishSelfDeath;
    public Toggle addPodium3;
    public Toggle allowCustomScoreToWin;
    public Slider customScoreToWinSlider;
    public Text customScoreToWinText;
    public Toggle enableFly;
    public Slider minTimeForFlySpawnSlider;
    public Text minTimeForFlySpawnText;
    public Slider maxTimeForFlySpawnSlider;
    public Text maxTimeForFlySpawnText;
    public Button backButton;

    public Button resumeButton;
    public Button settingsButton;
    public Button restartButton;
    public Button goMainMenuButton;
    public Button quitButton;

    public Canvas mainCanvas;
    public Canvas settingsCanvas;
    // Start is called before the first frame update
    void Start()
    {
        Button backBtn = backButton.GetComponent<Button>();
		backBtn.onClick.AddListener(CloseSettings);
        Button resumeBtn = resumeButton.GetComponent<Button>();
		resumeBtn.onClick.AddListener(Resume);
        Button settingsBtn = settingsButton.GetComponent<Button>();
		settingsBtn.onClick.AddListener(OpenSettings);
        Button restartBtn = restartButton.GetComponent<Button>();
		restartBtn.onClick.AddListener(RestartScene);
        Button goMainMenuBtn = goMainMenuButton.GetComponent<Button>();
		goMainMenuBtn.onClick.AddListener(GoToMainMenu);
        Button quitBtn = quitButton.GetComponent<Button>();
		quitBtn.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        GameController.charactersBounceEachOther = charactersBounceEachOther.isOn;
        GameController.weirdBounceTrajectories = weirdBounceTrajectories.isOn;
        GameController.onlyBounceBeforeRecover = onlyBounceBeforeRecover.isOn;
        GameController.allowTeamMode = allowTeamMode.isOn;
        GameController.randomizeMaps = randomizeMaps.isOn;
        GameController.winnerTakesAll = winnerTakesAll.isOn;
        GameController.punishSelfDeath = punishSelfDeath.isOn;
        GameController.addPodium3 = addPodium3.isOn;
        GameController.allowCustomScoreToWin = allowCustomScoreToWin.isOn;
        GameController.enableFly = enableFly.isOn;
        customScoreToWinSlider.interactable = allowCustomScoreToWin.isOn;
        customScoreToWinText.enabled = allowCustomScoreToWin.isOn;
        minTimeForFlySpawnSlider.interactable = enableFly.isOn;
        minTimeForFlySpawnText.enabled = enableFly.isOn;
        maxTimeForFlySpawnSlider.interactable = enableFly.isOn;
        maxTimeForFlySpawnText.enabled = enableFly.isOn;

        if (customScoreToWinSlider.interactable)
        {
            GameController.customScoreToWin = customScoreToWinSlider.value;
            customScoreToWinText.text = customScoreToWinSlider.value.ToString();
        }
        if (enableFly.isOn)
        {
            GameController.minTimeForFlySpawn = minTimeForFlySpawnSlider.value;
            GameController.maxTimeForFlySpawn = maxTimeForFlySpawnSlider.value;
            minTimeForFlySpawnText.text = minTimeForFlySpawnSlider.value.ToString();
            maxTimeForFlySpawnText.text = maxTimeForFlySpawnSlider.value.ToString();
        }
    }

    void Resume()
    {
        GameController.isPaused = false;
    }

    void OpenSettings()
    {
        settingsCanvas.enabled = true;
        mainCanvas.enabled = false;
    }

    void CloseSettings()
    {
        mainCanvas.enabled = true;
        settingsCanvas.enabled = false;
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMainMenu()
    {
        GameController.isPaused = false;
        SceneManager.LoadScene("TitleScreen");
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
