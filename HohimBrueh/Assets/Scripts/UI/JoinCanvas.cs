using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FreeLives;
public class JoinCanvas : MonoBehaviour
{
    public enum State
    {
        Join,
        ChooseColor,
        Ready
    }

    public Transform effectPos;
    public Canvas joinPromptCanvas;
    public Canvas chooseColorCanvas;
    public Canvas backPromptCanvas;
    public GameObject teamChangeColorObject;
    public Text changeColorText;

    public Text[] texts;

    public Player assignedPlayer;

    public State state;

    Color color;

    public Image frogImage;

    // Use this for initialization
    void Start()
    {

    }
    InputState input = new InputState();
    float colorLerpAmount;
    // Update is called once per frame
    void Update()
    {
        foreach (var text in texts)
        {
            text.color = Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * 3f, 1f));
        }

        if (state == State.ChooseColor)
        {

            FreeLives.InputReader.GetInput(assignedPlayer.inputDevice, input);


            if (GameController.isTeamMode)
            {
                if (input.bButton && !input.wasBButton)
                {
                    List<Team> possibleTeams = GameController.colorPerTeam.Keys.ToList();
                    int indexOfCurrentTeam = possibleTeams.IndexOf(assignedPlayer.team);
                    if (indexOfCurrentTeam == -1)
                    {
                        indexOfCurrentTeam = 0;
                    }
                    assignedPlayer.team = possibleTeams[(indexOfCurrentTeam + 1) % GameController.colorPerTeam.Count];

                    colorLerpAmount = Random.Range(0f, 0.7f);
                    color = assignedPlayer.color = Color.Lerp(GameController.colorPerTeam[assignedPlayer.team], Color.white, colorLerpAmount);
                    frogImage.color = color;
                    EffectsController.CreateSpawnEffects(effectPos.position, color);
                    SoundController.PlaySoundEffect("CharacterSpawn", 0.3f, effectPos.position);
                }
                if (input.left)
                {
                    colorLerpAmount = Mathf.MoveTowards(colorLerpAmount, 0f, Time.deltaTime * 0.5f);
                }
                else if (input.right)
                {
                    colorLerpAmount = Mathf.MoveTowards(colorLerpAmount, 0.7f, Time.deltaTime * 0.5f);
                }
                color = assignedPlayer.color = frogImage.color = Color.Lerp(GameController.colorPerTeam[assignedPlayer.team], Color.white, colorLerpAmount);


            }
            else
            {

                if (input.bButton && !input.wasBButton)
                {
                    var newColor = GameController.GetAvailableColor();
                    GameController.ReturnColor(color);
                    color = assignedPlayer.color = newColor;
                    frogImage.color = color;
                    EffectsController.CreateSpawnEffects(effectPos.position, color);
                    SoundController.PlaySoundEffect("CharacterSpawn", 0.3f, effectPos.position);
                }
            }
            if (input.yButton && !input.wasYButton)
            {
                UnAssignPlayer();
            }
            else if (input.xButton && !input.wasXButton)
            {
                assignedPlayer.color = color;
                GameController.SpawnCharacterJoinScreen(assignedPlayer);
                state = State.Ready;
                chooseColorCanvas.enabled = false;
                backPromptCanvas.enabled = true;

            }
        }
        else if (state == State.Ready)
        {
            FreeLives.InputReader.GetInput(assignedPlayer.inputDevice, input);
            if (input.yButton && !input.wasYButton)
            {
                UnAssignPlayer();
            }
        }
    }

    public void AssignPlayer(Player player)
    {
        assignedPlayer = player;
        state = State.ChooseColor;
        joinPromptCanvas.enabled = false;
        chooseColorCanvas.enabled = true;

        if (GameController.isTeamMode)
        {
            teamChangeColorObject.SetActive(true);
            changeColorText.text = "CHANGE TEAM";


            List<Team> possibleTeams = GameController.colorPerTeam.Keys.ToList();
            float amountOfTeamsAsFloat = (float)GameController.colorPerTeam.Count;
            int randomTeamIndex = (int)Random.Range(0f, amountOfTeamsAsFloat);
            Team randomTeamResult = possibleTeams[randomTeamIndex];
            player.team = randomTeamResult;
            color = assignedPlayer.color = Color.Lerp(GameController.colorPerTeam[assignedPlayer.team], Color.white, Random.Range(0f, 0.7f));
            frogImage.color = color;
        }
        else
        {
            teamChangeColorObject.SetActive(false);
            changeColorText.text = "CHANGE COLOR";

            color = GameController.GetAvailableColor();
            player.color = color;
            frogImage.color = color;
        }
        FreeLives.InputReader.GetInput(assignedPlayer.inputDevice, input);
        EffectsController.CreateSpawnEffects(effectPos.position, color);
        SoundController.PlaySoundEffect("CharacterSpawn", 0.3f, effectPos.position);
    }

    public bool HasAssignedPlayer()
    {
        return assignedPlayer != null;
    }

    public void UnAssignPlayer()
    {
        GameController.ReturnPlayer(assignedPlayer);
        GameController.ReturnColor(color);
        if (assignedPlayer.character != null)
        {
            Destroy(assignedPlayer.character.gameObject);
        }
        state = State.Join;
        joinPromptCanvas.enabled = true;
        chooseColorCanvas.enabled = false;
        backPromptCanvas.enabled = false;
        assignedPlayer = null;
    }
}
