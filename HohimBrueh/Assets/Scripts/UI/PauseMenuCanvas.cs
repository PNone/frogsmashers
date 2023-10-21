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
        Toggle cBounce = charactersBounceEachOther.GetComponent<Toggle>();
        cBounce.isOn = GameController.charactersBounceEachOther;
        cBounce.onValueChanged.AddListener(delegate {
            CharBounceChanged(cBounce);
        });
        Toggle weirdBounce = weirdBounceTrajectories.GetComponent<Toggle>();
        weirdBounce.isOn = GameController.weirdBounceTrajectories;
        weirdBounce.onValueChanged.AddListener(delegate {
            WeirdBounceChanged(weirdBounce);
        });
        Toggle bounceBefRecover = onlyBounceBeforeRecover.GetComponent<Toggle>();
        bounceBefRecover.isOn = GameController.onlyBounceBeforeRecover;
        bounceBefRecover.onValueChanged.AddListener(delegate {
            BounceBeforeRecoverChanged(bounceBefRecover);
        });
        Toggle teams = allowTeamMode.GetComponent<Toggle>();
        teams.isOn = GameController.allowTeamMode;
        teams.onValueChanged.AddListener(delegate {
            AllowTeamsChanged(teams);
        });
        Toggle randMaps = randomizeMaps.GetComponent<Toggle>();
        randMaps.isOn = GameController.randomizeMaps;
        randMaps.onValueChanged.AddListener(delegate {
            RandomMapsChanged(randMaps);
        });
        Toggle winerAll = winnerTakesAll.GetComponent<Toggle>();
        winerAll.isOn = GameController.winnerTakesAll;
        winerAll.onValueChanged.AddListener(delegate {
            WinnerTakesAllChanged(winerAll);
        });
        Toggle selfDeath = punishSelfDeath.GetComponent<Toggle>();
        selfDeath.isOn = GameController.punishSelfDeath;
        selfDeath.onValueChanged.AddListener(delegate {
           PunishSelfDeathChanged(selfDeath);
        });
        Toggle podium = addPodium3.GetComponent<Toggle>();
        podium.isOn = GameController.addPodium3;
        podium.onValueChanged.AddListener(delegate {
           PodiumChanged(podium);
        });

        Toggle allowCustomScore = allowCustomScoreToWin.GetComponent<Toggle>();
        allowCustomScore.isOn = GameController.allowCustomScoreToWin;
        allowCustomScore.onValueChanged.AddListener(delegate {
           CustomScoreChanged(allowCustomScore);
        });

        Slider customScoreSlider = customScoreToWinSlider.GetComponent<Slider>();
        customScoreSlider.value = GameController.customScoreToWin;
        customScoreSlider.interactable = GameController.allowCustomScoreToWin;
        customScoreSlider.onValueChanged.AddListener(delegate {
           CustomScoreValueChanged(customScoreSlider);
        });
        if (GameController.allowCustomScoreToWin)
        {
            customScoreToWinText.enabled = true;
            customScoreToWinText.text = GameController.customScoreToWin.ToString();
        }

        Toggle flyToggle = enableFly.GetComponent<Toggle>();
        flyToggle.isOn = GameController.enableFly;
        flyToggle.onValueChanged.AddListener(delegate {
           FlyChanged(flyToggle);
        });
        Slider minFlySlider = minTimeForFlySpawnSlider.GetComponent<Slider>();
        minFlySlider.value = GameController.minTimeForFlySpawn;
        minFlySlider.interactable = GameController.enableFly;
        minFlySlider.onValueChanged.AddListener(delegate {
           MinFlySpawnValueChanged(minFlySlider);
        });
        if (GameController.enableFly)
        {
            minTimeForFlySpawnText.enabled = true;
            minTimeForFlySpawnText.text = GameController.minTimeForFlySpawn.ToString();
        }
        Slider maxFlySlider = maxTimeForFlySpawnSlider.GetComponent<Slider>();
        maxFlySlider.value = GameController.maxTimeForFlySpawn;
        maxFlySlider.interactable = GameController.enableFly;
        maxFlySlider.onValueChanged.AddListener(delegate {
           MaxFlySpawnValueChanged(maxFlySlider);
        });
        if (GameController.enableFly)
        {
            maxTimeForFlySpawnText.enabled = true;
            maxTimeForFlySpawnText.text = GameController.maxTimeForFlySpawn.ToString();
        }

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
    }

    bool ShouldAllowEditSettings()
    {
        return GameController.isPaused && settingsCanvas.enabled;
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
        GameController.isPaused = false;
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

    void CharBounceChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.charactersBounceEachOther = t.isOn;
    }

    void WeirdBounceChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.weirdBounceTrajectories = t.isOn;
    }

    void BounceBeforeRecoverChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.onlyBounceBeforeRecover = t.isOn;
    }

    void AllowTeamsChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.allowTeamMode = t.isOn;
    }

    void RandomMapsChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.randomizeMaps = t.isOn;
    }

    void WinnerTakesAllChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.winnerTakesAll = t.isOn;
    }

    void PunishSelfDeathChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.punishSelfDeath = t.isOn;
    }

    void PodiumChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.addPodium3 = t.isOn;
    }

    void CustomScoreChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.allowCustomScoreToWin = t.isOn;
        customScoreToWinSlider.interactable = t.isOn;
        customScoreToWinText.enabled = t.isOn;
        if (!t.isOn)
        {
            GameController.customScoreToWin = 10f;
        }
        customScoreToWinSlider.value = GameController.customScoreToWin;
        customScoreToWinText.text = GameController.customScoreToWin.ToString();
    }

    void CustomScoreValueChanged(Slider s)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        if (GameController.allowCustomScoreToWin)
        {
            GameController.customScoreToWin = s.value;
            customScoreToWinText.text = s.value.ToString();
        }
    }

    void FlyChanged(Toggle t)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        GameController.enableFly = t.isOn;
        minTimeForFlySpawnSlider.interactable = t.isOn;
        minTimeForFlySpawnText.enabled = t.isOn;
        maxTimeForFlySpawnSlider.interactable = t.isOn;
        maxTimeForFlySpawnText.enabled = t.isOn;
        if (!t.isOn)
        {
            GameController.minTimeForFlySpawn = 15f;
            GameController.maxTimeForFlySpawn = 45f;
        }
        minTimeForFlySpawnSlider.value = GameController.minTimeForFlySpawn;
        minTimeForFlySpawnText.text = GameController.minTimeForFlySpawn.ToString();
        maxTimeForFlySpawnSlider.value = GameController.maxTimeForFlySpawn;
        maxTimeForFlySpawnText.text = GameController.maxTimeForFlySpawn.ToString();
    }

    void MinFlySpawnValueChanged(Slider s)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        if (GameController.enableFly)
        {
            GameController.minTimeForFlySpawn = s.value;
            minTimeForFlySpawnText.text = s.value.ToString();
        }
    }

    void MaxFlySpawnValueChanged(Slider s)
    {
        if (!ShouldAllowEditSettings())
        {
            return;
        }
        if (GameController.enableFly)
        {
            GameController.maxTimeForFlySpawn = s.value;
            maxTimeForFlySpawnText.text = s.value.ToString();
        }
    }
}
