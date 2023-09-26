using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using FreeLives;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    RoundFinished,
    JoinScreen
}


public class GameController : MonoBehaviour
{
    public static bool charactersBounceEachOther = false;
    public static bool weirdBounceTrajectories = false;
    public static bool onlyBounceBeforeRecover = true;
    public static bool allowTeamMode = true;
    public static bool randomizeMaps = false;
    public static bool winnerTakesAll = true;
    public static bool punishSelfDeath = false;
    public static bool addPodium3 = false;
    public static bool allowCustomScoreToWin = false;
    public static float customScoreToWin = 10f;
    public static bool enableFly = true;
    public static float minTimeForFlySpawn = 15f;
    public static float maxTimeForFlySpawn = 45f;

    public static List<Player> activePlayers = new List<Player>();

    static List<Player> inactivePlayers;

    GameState state;

    public static Color overallWinnerColor = Color.red;

    public static GameState State { get { return instance.state; } }

    public Character characterPrefab;

    public Fly flyPrefab, activeFly;

    float flySpawnDelay;

    public SpriteRenderer offscreenDotPrefab;

    public Canvas scoreCanvas;

    public List<PlayerScoreDisplay> playerScoreDisplays;

    public PlayerScoreDisplay scoreDisplayPrefab;

    static GameController instance;

    public Color[] playerColors;

    List<Color> availableColors;

    public bool isJoinScreen;

    private static List<string> originalLevelNames = new List<string> { "1BusStop", "2DownSmash", "3Moon", "4FinalFrogstination", "5Skyline", "6Finale" };
    public static string[] levelNames = new string[] { "1BusStop", "2DownSmash", "3Moon", "4FinalFrogstination", "5Skyline", "6Finale" };
    public JoinCanvas[] joinCanvas;

    float finishDelay = 7.5f;

    public Text joinCountdownText, joinGameModeText;

    public Image joinGameModeToggle;

    public static bool isTeamMode;

    public static bool playersCanDropIn;

    public static bool isShowDown;

    public Dictionary<Team, PlayerScoreDisplay> teamScoreDisplays;
    public Dictionary<Team, int> teamScores;
    public static Dictionary<Team, Color> colorPerTeam = new Dictionary<Team, Color>{ { Team.Red, Color.red }, { Team.Green, Color.green }, { Team.Blue, Color.blue }, { Team.Yellow, Color.yellow } };

    void Awake()
    {
        //Camera.main.aspect = 16f / 9f;
        if (isJoinScreen)
        {
            isShowDown = false;
            state = GameState.JoinScreen;
            finishDelay = 5f;
            SetupForJoinScreen();

            inactivePlayers = null;
            activePlayers.Clear();
            levelNo = 0;
            playersCanDropIn = true;

        }
        playerScoreDisplays = new List<PlayerScoreDisplay>();
        teamScoreDisplays = new Dictionary<Team, PlayerScoreDisplay>();
        teamScores = new Dictionary<Team, int>();
        instance = this;
        if (inactivePlayers == null)
        {
            inactivePlayers = new List<Player>();
            Player p;
            p = new Player(FreeLives.InputReader.Device.Gamepad1, playerColors[0], 0);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad2, playerColors[1], 1);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad3, playerColors[2], 2);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad4, playerColors[3], 3);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Keyboard1, playerColors[4], 4);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Keyboard2, playerColors[5], 5);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad5, playerColors[6], 6);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad6, playerColors[7], 7);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad7, playerColors[8], 8);
            inactivePlayers.Add(p);
            p = new Player(FreeLives.InputReader.Device.Gamepad8, playerColors[9], 9);
            inactivePlayers.Add(p);
        }
        else
        {
            int i = 0;
            if (isTeamMode)
            {
                foreach (var player in activePlayers)
                {
                    player.score = 0;
                    player.spawnDelay = 0.5f + 0.2f * i;
                    i++;
                }
                var psd = Instantiate(scoreDisplayPrefab, scoreCanvas.transform) as PlayerScoreDisplay;
                foreach (var p in activePlayers)
                {
                    if (!teamScoreDisplays.ContainsKey(p.team)) 
                    {
                        psd = Instantiate(scoreDisplayPrefab, scoreCanvas.transform) as PlayerScoreDisplay;
                        var colorForPsd = colorPerTeam[p.team];
                        psd.color = colorForPsd;
                        psd.text.color = colorForPsd;
                        teamScoreDisplays[p.team] = psd;
                        teamScores[p.team] = 0;
                        psd.player = p;
                        playerScoreDisplays.Add(psd);
                    }
                }
            }
            else
            {
                foreach (var player in activePlayers)
                {
                    player.score = 0;
                    player.spawnDelay = 0.5f + 0.2f * i;
                    i++;
                    var psd = Instantiate(scoreDisplayPrefab, scoreCanvas.transform) as PlayerScoreDisplay;
                    psd.player = player;
                    psd.color = player.color;
                    psd.text.color = player.color;
                    playerScoreDisplays.Add(psd);

                }
            }
        }

        if (isShowDown)
            foreach (var psd in playerScoreDisplays)
            {
                psd.gameObject.SetActive(false);
            }

        InputReader.GetInput(combinedInput);
    }

    internal static void SetupPlayersForShowdown()
    {
        List<Player> winningPlayers = GetLeadingPlayers();
        activePlayers.Clear();
        foreach (var p in winningPlayers)
        {
            activePlayers.Add(p);
        }
        playersCanDropIn = false;
       
    }

    void Start()
    {
        float aspect = ((float)Screen.width / Screen.height);
        float screenWidth = 18f * aspect;
        float adust = 32f / screenWidth;
        Camera.main.orthographicSize = adust * 18f;
    }

    FreeLives.InputState input = new InputState();
    FreeLives.InputState combinedInput = new InputState();
    void Update()
    {
        if (state == GameState.JoinScreen)
        {
            for (int i = inactivePlayers.Count - 1; i >= 0; i--)
            {
                InputReader.GetInput(inactivePlayers[i].inputDevice, input);

                if (input.xButton)
                {
                    for (int j = 0; j < joinCanvas.Length; j++)
                    {
                        if (!joinCanvas[j].HasAssignedPlayer())
                        {
                            joinCanvas[j].AssignPlayer(inactivePlayers[i]);
                            inactivePlayers.RemoveAt(i);
                            j = joinCanvas.Length;
                        }
                    }
                }
            }


            int assignedPlayers = 0;

            for (int i = 0; i < joinCanvas.Length; i++)
            {
                if (joinCanvas[i].HasAssignedPlayer())
                {
                    assignedPlayers++;
                }
            }

            if (allowTeamMode)
            {
                joinGameModeText.enabled = true;
                joinGameModeText.text = isTeamMode ? "TEAMS" : "FREE  FOR  ALL";
                if (assignedPlayers == 0)
                {
                    joinGameModeText.color = Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * 1.5f, 1f));
                    joinGameModeToggle.enabled = true;
                    if (Input.GetKeyDown(KeyCode.F5) || Input.GetKeyDown(KeyCode.JoystickButton6)) // Controller back button
                    {
                        isTeamMode = !isTeamMode;       
                    }
                }
                else
                {
                    joinGameModeText.color = Color.white;
                    joinGameModeToggle.enabled = false;
                }
            }
            else
            {
                // Only do this when allowTeamMode is flipped while isTeamMode was true
                if (isTeamMode && assignedPlayers > 0)
                {
                    foreach (var j in instance.joinCanvas)
                    {
                        if (j.HasAssignedPlayer()) 
                        {
                            j.UnAssignPlayer();
                        }
                        
                    }
                }
                joinGameModeText.enabled = false;
                isTeamMode = false;
                joinGameModeToggle.enabled = false;
            }
            
            bool playersAreReady = CheckReadyPlayers();
            if (playersAreReady)
            {
                finishDelay -= Time.deltaTime;
                joinCountdownText.text = ((int)(finishDelay) + 1).ToString();
                if (finishDelay <= 0f)
                {
                    FinishRound();
                }
            }
            else
            {
                joinCountdownText.text = "";
                finishDelay = 5f;
            }


        }
        else if (state == GameState.Playing)
        {
            if (enableFly && activeFly == null && !isShowDown)
            {
                if (flySpawnDelay > 0f)
                {
                    flySpawnDelay -= Time.deltaTime;
                    if (flySpawnDelay <= 0f)
                    {
                        activeFly = Instantiate(flyPrefab, Terrain.GetFlySpawnPoint(), Quaternion.identity);
                    }
                }
                else
                {
                    flySpawnDelay = UnityEngine.Random.Range(minTimeForFlySpawn, maxTimeForFlySpawn);
                }
            }


            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].character == null)
                {
                    activePlayers[i].spawnDelay -= Time.deltaTime;
                    if (activePlayers[i].spawnDelay < 0f)
                    {
                        SpawnCharacter(activePlayers[i]);
                    }
                }

                if (activePlayers[i].character != null && activePlayers[i].character.transform.position.y > Terrain.ScreenTop)
                {
                    var spr = activePlayers[i].offscreenDot;
                    if (spr == null)
                    {
                        activePlayers[i].offscreenDot = spr = Instantiate(offscreenDotPrefab);
                        spr.color = activePlayers[i].color;
                    }
                    spr.enabled = true;
                    spr.transform.position = new Vector3(activePlayers[i].character.transform.position.x, Terrain.ScreenTop, -6f);
                }
                else
                {
                    var spr = activePlayers[i].offscreenDot;
                    if (spr != null)
                    {
                        spr.enabled = false;
                    }
                }
            }

            ArrangeScoreboards();

            if (!isShowDown)
            {
                for (int i = inactivePlayers.Count - 1; i >= 0; i--)
                {
                    InputReader.GetInput(inactivePlayers[i].inputDevice, input);

                    if (input.xButton)
                    {
                        print(inactivePlayers[i].color + ", " + inactivePlayers[i].inputDevice);
                        inactivePlayers[i].color = playerColors[UnityEngine.Random.Range(0, playerColors.Length)];
                        AddPlayer(inactivePlayers[i]);


                        inactivePlayers.RemoveAt(i);
                    }
                }

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    SpawnCharacter(null);
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
            }
        }
        else if (state == GameState.RoundFinished)
        {
            ArrangeScoreboards();

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].character == null && winningPlayer == activePlayers[i])
                {
                    activePlayers[i].spawnDelay -= Time.deltaTime;
                    if (activePlayers[i].spawnDelay < 0f)
                    {
                        SpawnCharacter(activePlayers[i]);
                    }
                }

                if (activePlayers[i].character != null && activePlayers[i].character.transform.position.y > Terrain.ScreenTop)
                {
                    var spr = activePlayers[i].offscreenDot;
                    if (spr == null)
                    {
                        activePlayers[i].offscreenDot = spr = Instantiate(offscreenDotPrefab);
                        spr.color = activePlayers[i].color;
                    }
                    spr.enabled = true;
                    spr.transform.position = new Vector3(activePlayers[i].character.transform.position.x, Terrain.ScreenTop, -6f);
                }
                else
                {
                    var spr = activePlayers[i].offscreenDot;
                    if (spr != null)
                    {
                        spr.enabled = false;
                    }
                }
            }


            finishDelay -= Time.deltaTime;
            if (finishDelay < 0f)
            {
                FinishRound();
            }
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            showGui = !showGui;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void SetupForJoinScreen()
    {
        availableColors = new List<Color>();
        availableColors.AddRange(playerColors);

    }

    bool CheckReadyPlayers()
    {
        int readyPlayers = 0;
        if (GameController.isTeamMode)
        {
            Dictionary<Team, bool> teamsWithPlayers = new Dictionary<Team, bool>();
            for (int i = 0; i < joinCanvas.Length; i++)
            {
                if (joinCanvas[i].HasAssignedPlayer())
                {
                    if (joinCanvas[i].state == JoinCanvas.State.Ready)
                    {
                        Team assignedPlayerTeam = joinCanvas[i].assignedPlayer.team;
                        teamsWithPlayers[assignedPlayerTeam] = true;
                        readyPlayers++;
                    }
                    else
                    {
                        readyPlayers = -100;
                    }
                }
            }

            return teamsWithPlayers.Count >= 2 && readyPlayers >= 2;
        }
        else
        {
            for (int i = 0; i < joinCanvas.Length; i++)
            {
                if (joinCanvas[i].HasAssignedPlayer())
                {
                    if (joinCanvas[i].state == JoinCanvas.State.Ready)
                        readyPlayers++;
                    else
                    {
                        readyPlayers = -100;
                    }
                }
            }
            return readyPlayers >= 2;
        }
    }

    void ArrangeScoreboards()
    {
        for (int i = 0; i < playerScoreDisplays.Count; i++)
        {
            var p = playerScoreDisplays[i].transform.localPosition;
            //print(p);
            p.y = Mathf.Lerp(p.y, i * -2f, Time.deltaTime * 3f);
            p.z = 0f;
            //print(p);
            playerScoreDisplays[i].transform.localPosition = p;


        }
    }

    public static int levelNo;
    void FinishRound()
    {
        if (state == GameState.JoinScreen)
        {
            levelNo = 0;
            foreach (var jc in joinCanvas)
            {
                if (jc.HasAssignedPlayer())
                {
                    activePlayers.Add(jc.assignedPlayer);
                }
            }
            var mapsToUse = originalLevelNames.ToList();
            if (addPodium3)
            {
                mapsToUse.Add("3Podium");
            }
            if (randomizeMaps)
            {
                System.Random random = new System.Random();
                levelNames = mapsToUse.OrderBy(x => random.Next()).ToArray();
            }
            else 
            {
                levelNames = mapsToUse.ToArray();
            }
            
            SceneManager.LoadScene(levelNames[0]);
        }
        else
        {
            levelNo++;
            //if (levelNo >= 5)
            {
                SceneManager.LoadScene("ScoreScreen");
            }
        }
    }

    void AddPlayer(Player player)
    {
        activePlayers.Add(player);

        var psd = Instantiate(scoreDisplayPrefab, scoreCanvas.transform) as PlayerScoreDisplay;
        psd.player = player;
        psd.color = player.color;
        psd.text.color = player.color;
        playerScoreDisplays.Add(psd);

    }


    void SpawnCharacter(Player player)
    {
        var point = Terrain.GetSpawnPoint();
        var ch = Instantiate(characterPrefab, point, Quaternion.identity) as Character;
        if (player != null)
        {
            ch.player = player;
            player.character = ch;
            player.spawnDelay = 1f;

            EffectsController.CreateSpawnEffects(point + Vector3.up, player.color);
            SoundController.PlaySoundEffect("CharacterSpawn", 0.4f, point);
        }
    }

    public static void SpawnCharacterJoinScreen(Player player)
    {
        for (int i = 0; i < instance.joinCanvas.Length; i++)
        {
            if (instance.joinCanvas[i].assignedPlayer == player)
            {
                var ch = Instantiate(instance.characterPrefab, Terrain.GetSpawnPoint(i), Quaternion.identity) as Character;
                ch.player = player;
                player.character = ch;
            }
        }
    }

    Player winningPlayer;

    public static Player lastWinningPlayer { get; private set; }
    public static bool HasInstance { get { return instance != null; } }

    public static Player GetWinningPlayer()
    {
        if (State == GameState.RoundFinished)
        {
            return instance.winningPlayer;
        }
        return null;
    }

    void SortScoreboard()
    {
        if (GameController.isTeamMode)
        {
            foreach (var teamAndScore in teamScoreDisplays)
            {
                var teamScoreDisplay = teamAndScore.Value;
                Team team = teamAndScore.Key;
                teamScoreDisplay.player.score = teamScores[team];
            }
        }
        instance.playerScoreDisplays.Sort((x, y) => (y.player.score * 100 + y.player.sortPriority) - (x.player.score * 100 + x.player.sortPriority));
    }


    internal static void RegisterKill(Player gotPoint, Player gotKilled, int hits)
    {
        if (State == GameState.RoundFinished)
        {
            return;
        }

        if (isShowDown)
        {
            bool wonRound = false;
            if (activePlayers.Contains(gotKilled))
            {
                activePlayers.Remove(gotKilled);
                if (gotKilled.offscreenDot != null)
                {
                    GameObject.Destroy(gotKilled.offscreenDot);
                }
            }
            if (activePlayers.Count == 1)
            {
                wonRound = true;
                var winner = activePlayers[0];
                if (winner.character != null)
                {
                    winner.character.GetComponent<ScorePlum>().ShowText("WIN!", 5f);
                }
                instance.winningPlayer = winner;
                lastWinningPlayer = winner;
            }
            else if (isTeamMode)
            {
                Dictionary<Team, bool> teamsWithLivingPlayers = new Dictionary<Team, bool>();
                foreach (var player in activePlayers)
                {
                    teamsWithLivingPlayers[player.team] = true;
                }

                // Just in case everyone is dead, not sure if that is even possible
                if (teamsWithLivingPlayers.Count < 2)
                {
                    wonRound = true;
                    var winner = activePlayers[0];
                    if (winner.character != null)
                    {
                        winner.character.GetComponent<ScorePlum>().ShowText("WIN!", 5f);
                    }
                    instance.winningPlayer = winner;
                    lastWinningPlayer = winner;
                }

            }

            if (wonRound)
            {
                SoundController.PlaySoundEffect("VictorySting", 0.5f);
                SoundController.StopMusic();
                instance.state = GameState.RoundFinished;
            }
            return;
        }

        if (gotPoint != null)
        {
            if (hits <= 0)
            {
                hits = 1;
            }

            if (GameController.isTeamMode)
            {
                instance.teamScores[gotPoint.team] += hits;
            }
            else
            {
                gotPoint.score += hits;
            }

            bool wonRound = false;
            if (GameController.isTeamMode)
            {
                int requiredScoreToWin = allowCustomScoreToWin ? (int)customScoreToWin : 10;
                foreach (var teamAndScore in instance.teamScores)
                {
                    int scoreOfTeam = teamAndScore.Value;
                    Team currentTeam = teamAndScore.Key;
                    if (gotPoint.team == currentTeam && scoreOfTeam >= requiredScoreToWin)
                    {
                        wonRound = true;
                        break;
                    }
                }
            }
            else
            { 
                if (allowCustomScoreToWin)
                {
                    wonRound = gotPoint.score >= (int)customScoreToWin;
                }
                else if (activePlayers.Count == 2)
                {
                    wonRound = gotPoint.score >= 5;
                }
                else
                {
                    wonRound = gotPoint.score >= 10;
                }
            }
            if (wonRound)
            {
                SoundController.PlaySoundEffect("VictorySting", 0.5f);
                SoundController.StopMusic();
                instance.state = GameState.RoundFinished;
                instance.GetPlayerScoreDisplay(gotPoint).TemorarilyDisplay("WINNER ! ! !", 5f);
                if (gotPoint.character != null)
                {
                    gotPoint.character.GetComponent<ScorePlum>().ShowText("WIN!", 5f);
                }
                instance.winningPlayer = gotPoint;
                lastWinningPlayer = gotPoint;
            }
            else
            {
                instance.GetPlayerScoreDisplay(gotPoint).TemorarilyDisplay("+" + hits.ToString());
                if (gotPoint.character != null)
                {
                    gotPoint.character.GetComponent<ScorePlum>().ShowText("+" + hits.ToString());
                }
            }




        }
        else if (gotKilled != null && punishSelfDeath)
        {
            if (isTeamMode)
            {
                instance.teamScores[gotKilled.team]--;
            }
            else
            {
               gotKilled.score--;
            }
            instance.GetPlayerScoreDisplay(gotKilled).TemorarilyDisplay("-" + 1);
        }

        instance.SortScoreboard();


    }

    PlayerScoreDisplay GetPlayerScoreDisplay(Player player)
    {
        if (isTeamMode)
        {
            return teamScoreDisplays[player.team];
        }
        else
        {
            for (int i = 0; i < playerScoreDisplays.Count; i++)
            {
                if (playerScoreDisplays[i].player == player)
                {
                    return playerScoreDisplays[i];
                }
            }
        }
        return null;
    }

    bool showGui;

    public void OnGUI()
    {
        if (showGui)
        {
            GUILayout.BeginArea(new Rect(0, 0, 800, 800));
            charactersBounceEachOther = GUILayout.Toggle(charactersBounceEachOther, "Characters Bounce Each Other");
            weirdBounceTrajectories = GUILayout.Toggle(weirdBounceTrajectories, "Weird Bounce Trajectories");
            onlyBounceBeforeRecover = GUILayout.Toggle(onlyBounceBeforeRecover, "Only Bounce Before Recover");
            allowTeamMode = GUILayout.Toggle(allowTeamMode, "Allow Team Deathmatch (F5/back to toggle mode)");
            randomizeMaps = GUILayout.Toggle(randomizeMaps, "Randomize Map Order");
            winnerTakesAll = GUILayout.Toggle(winnerTakesAll, "Only Winners Get Points");
            punishSelfDeath = GUILayout.Toggle(punishSelfDeath, "Self-Kills Lose Points");
            addPodium3 = GUILayout.Toggle(addPodium3, "Add Podium3 To Maps");
            allowCustomScoreToWin = GUILayout.Toggle(allowCustomScoreToWin, "Use Custom Score To Win");
            customScoreToWin = GUILayout.HorizontalScrollbar(customScoreToWin, 1.0f, 1.0f, 100.0f);
            GUILayout.Label($"Custom score to win is {(int)customScoreToWin}");
            enableFly = GUILayout.Toggle(enableFly, "Enable Fly");
            minTimeForFlySpawn = GUILayout.HorizontalScrollbar(minTimeForFlySpawn, 1f, 1f, 100f);
            GUILayout.Label($"Minimum Seconds For Fly Spawn is {(int)minTimeForFlySpawn}");
            maxTimeForFlySpawn = GUILayout.HorizontalScrollbar(maxTimeForFlySpawn, 1f, 1f, 100f);
            GUILayout.Label($"Maximum Seconds For Fly Spawn is {(int)maxTimeForFlySpawn}");
            GUILayout.EndArea();
        }
    }

    public static Color GetAvailableColor()
    {
        int i = UnityEngine.Random.Range(0, instance.availableColors.Count);
        var col = instance.availableColors[i];
        instance.availableColors.RemoveAt(i);
        return col;
    }

    public static void ReturnColor(Color color)
    {
        instance.availableColors.Add(color);
    }

    public static void ReturnPlayer(Player player)
    {
        activePlayers.Remove(player);
        inactivePlayers.Add(player);
    }


    public static List<Player> GetLeadingPlayers()
    {
        int topScore = -1;
        List<Player> tiedPlayers = new List<Player>();
        foreach (var player in activePlayers)
        {
            if (player.roundWins == topScore)
            {
                tiedPlayers.Add(player);
            }
            else if (player.roundWins > topScore)
            {
                topScore = player.roundWins;
                tiedPlayers.Clear();
                tiedPlayers.Add(player);
            }
        }

        return tiedPlayers;
    }

    public static bool AreAnyPlayersTiedForVictory()
    {
        int topScore = -1;
        List<Player> tiedPlayers = new List<Player>();
        foreach (var player in activePlayers)
        {
            if (player.roundWins == topScore)
            {
                tiedPlayers.Add(player);
            }
            else if (player.roundWins > topScore)
            {
                topScore = player.roundWins;
                tiedPlayers.Clear();
                tiedPlayers.Add(player);
            }
        }
        if (GameController.isTeamMode)
        {
            Dictionary<Team, bool> teamsWithTiedPlayers = new Dictionary<Team, bool>();
            foreach (var player in tiedPlayers)
            {
                teamsWithTiedPlayers[player.team] = true;
            }
            // At least 2 teams have the same score
            return teamsWithTiedPlayers.Count > 1;
        }
        else
        {
            return tiedPlayers.Count > 1;
        }
    }

}
