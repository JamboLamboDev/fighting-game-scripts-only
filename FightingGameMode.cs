using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.ComponentModel;


public class FightingGameMode : MonoBehaviourPunCallbacks
{

    public Transform player1Spawn;
    public Transform player2Spawn;
    private FightingPlayerController player1;
    private FightingPlayerController player2;
    public float roundTime = 99f; //stc
    private float currentTime;
    private bool roundActive = false;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI matchWonText;
    public TextMeshProUGUI player1NameText;
    public TextMeshProUGUI player2NameText;
    public TextMeshProUGUI p1comboCount;
    public TextMeshProUGUI p2comboCount;
    private int p1Wins = 0;
    private int p2Wins = 0;
    private int p1matchWins = 0;
    private int p2matchWins = 0;
    public ValBar P1HPBar;
    public ValBar P2HPBar;
    public ValBar P1GuardBar;
    public ValBar P2GuardBar;
    public ValBar P1SpeBar;
    public ValBar P2SpeBar;
    public ValBar P1FirstWin; //value from 0-1, fills up when win, using Valbar script since it already exists.
    public ValBar P2FirstWin;
    public ValBar P1SecondWin;
    public ValBar P2SecondWin;
    public TextMeshProUGUI p1SpecialMeterText;
    public TextMeshProUGUI p2SpecialMeterText;
    public TextMeshProUGUI winCountText;

    public GameObject P1 = null;
    public GameObject P2 = null;
    public Player player1Ref;
    private string player1Nick;
    public Player player2Ref;
    private string player2Nick;
    public GameObject hud;
    public GameObject EndGameScreen;
    private bool player1Ready = false;
    private bool player2Ready = false;




    void Start()
    {
        player1Ref = PhotonNetwork.MasterClient;
        StartCoroutine(GetNicknames());
        StartCoroutine(GameLoop());
        
    }

    [PunRPC]
    void RPC_UpdateRoundText(string text)
    {
        roundText.text = text;
    }

    

    [PunRPC]
    void RPC_UpdateTimerText(string text)
    {
        timerText.text = text;
    }

    IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStart());
        yield return StartCoroutine(RoundActive());
        yield return StartCoroutine(RoundEnd());
    }
    
    IEnumerator RoundStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (player1 != null)
            {
                Debug.Log("Tried to destroy");
                player1.photonView.RPC("RPC_Destroy", RpcTarget.All);
            }

            if (player2 != null)
            {
                player2.photonView.RPC("RPC_Destroy", RpcTarget.All);
            }

            yield return new WaitForSeconds(0.5f); //WAIT FOR PLAYERS TO ALL JOIN
            player2Ref = PhotonNetwork.PlayerListOthers[0];



            if (PhotonNetwork.IsMasterClient)
            {
                string P1Char = GetCharacterChosen(player1Ref);
                string P2Char = GetCharacterChosen(player2Ref);
                P1 = PhotonNetwork.Instantiate(P1Char, player1Spawn.position, player1Spawn.rotation);
                P2 = PhotonNetwork.Instantiate(P2Char, player2Spawn.position, player2Spawn.rotation);
            }


            // Wait for both players to be instantiated
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length == 2);
            player1 = P1.GetComponent<FightingPlayerController>();
            player2 = P2.GetComponent<FightingPlayerController>();
            player1.photonView.TransferOwnership(player1Ref);
            player2.photonView.TransferOwnership(player2Ref);


            if (P1 != null)
            {
                int player1mask = LayerMask.NameToLayer("Player1");
                player1 = P1.GetComponent<FightingPlayerController>();
                player1.photonView.RPC("RPC_Spawn", RpcTarget.All, player1mask);
            }

            if (P2 != null)
            {
                player2 = P2.GetComponent<FightingPlayerController>();
                int player2mask = LayerMask.NameToLayer("Player2");
                player2.photonView.RPC("RPC_Spawn", RpcTarget.All, player2mask);

            }
        }

        if (!PhotonNetwork.IsMasterClient) //player 2 needs to figure out which player is whch
        {
            var players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in players)
            {
                var pv = p.GetComponent<PhotonView>();
                if (pv.Owner.IsLocal)
                    player2 = p.GetComponent<FightingPlayerController>();
                else
                    player1 = p.GetComponent<FightingPlayerController>();
            }
        }

        player1.SetBars(P1HPBar, P1GuardBar, P1SpeBar,p1SpecialMeterText,p1comboCount); 
        player2.SetBars(P2HPBar, P2GuardBar, P2SpeBar,p2SpecialMeterText,p2comboCount);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_FillWinBars", RpcTarget.All, p1Wins, p2Wins); //UPDATE WIN BARS BETWEEN ROU
            photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "3");
            yield return new WaitForSeconds(1f);
            photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "2");
            yield return new WaitForSeconds(1f);
            photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "1");
            yield return new WaitForSeconds(1f);
            player1.photonView.RPC("RPC_SetStun", player1.photonView.Owner, 0f);
            player2.photonView.RPC("RPC_SetStun", player2.photonView.Owner, 0f);

            photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Match Start!");
            currentTime = roundTime;
            yield return new WaitForSeconds(2f);

            photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "");
            roundActive = true;
        }
    }

    IEnumerator RoundActive() //only host gets access
    {
        float secondPassed = 0f; //otherwise rpc timer text updated too often.
        while (roundActive)
        {
            currentTime -= Time.deltaTime;
            secondPassed += Time.deltaTime;
            if (secondPassed >= 1f)//time the timer.
            {
                string StringCurrentTime = Mathf.Ceil(currentTime).ToString();
                photonView.RPC("RPC_UpdateTimerText", RpcTarget.All, StringCurrentTime);
                secondPassed = 0f;
            }

 

            if (player1.health <= 0 || player2.health <= 0 || currentTime <= 0)
            {
                roundActive = false;
            }

            yield return null;
        }
    }

    IEnumerator RoundEnd()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (player1.health <= 0 && player2.health <= 0)
            {

                player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "KO");
                player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "KO");
                photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Double KO!");
            }
            else if (player1.health <= 0)
            {
                player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "KO");
                player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Victory");
                photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Player 2 Wins!");
                p2Wins++;
            }
            else if (player2.health <= 0)
            {
                player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "KO");
                player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Victory");
                photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Player 1 Wins!");
                p1Wins++;
            }
            else
            {

                if (player1.health > player2.health)
                {
                    player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Defeated");
                    player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Victory");
                    photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Player 1 Wins!");
                    p1Wins++;


                }
                else
                {
                    player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Defeated");
                    player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All, "Victory");
                    photonView.RPC("RPC_UpdateRoundText", RpcTarget.All, "Player 2 Wins!"); ;
                    p2Wins++;
                }
            }
        }

        yield return new WaitForSeconds(5f);

        if (p1Wins >= 2 || p2Wins >= 2)
        {

            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_EndGame", RpcTarget.All, p1Wins, p1matchWins, p2matchWins);
            }
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    IEnumerator GetNicknames() {
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == 2);
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.MasterClient)
            {
                player1Nick = player.NickName;
                player1NameText.text = player1Nick;
            }
            else
            {
                player2Nick = player.NickName;
                player2NameText.text = player2Nick;
            }
        }
    }


    private string GetCharacterChosen(Player player)
    {
        if (player.CustomProperties.TryGetValue("Character", out object charPath))
        {
            string characterPath = charPath as string;
            Debug.Log($"{player.NickName} selected character: {characterPath}");
            return characterPath;
        }
        else
        {
            Debug.Log($"{player.NickName} has not selected a character yet.");
            return "Characters/GregPlayable"; //default character if not chosen
        }
    }
    [PunRPC]
    public void RPC_SetPlayerReady(bool isMaster)
    {
        if (isMaster) //p1 is always host.
        {
            if (!player1Ready)
            {
                player1Ready = true;
            }
            else
            {
                player1Ready = false;

            }
        }
        else
        {
            if (!player2Ready)
            {
                player2Ready = true;
            }
            else
            {
                player2Ready = false;
            }
        }

        if (player1Ready && player2Ready)
        {
            photonView.RPC("RPC_RestartGame", RpcTarget.All);
            StartCoroutine(GameLoop());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room. Returning to main menu...");
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitToMainMenu()
    {
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitToDesktop()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }

    public void ReadyButtonPressed()
    {
        bool isMaster = PhotonNetwork.IsMasterClient;
        photonView.RPC("RPC_SetPlayerReady", RpcTarget.MasterClient, isMaster);
    }

    [PunRPC]
    void RPC_FillWinBars(int player1Wins, int player2Wins)
    {
        //RESET ALL BARS
        P1FirstWin.SetVal(0f);
        P1SecondWin.SetVal(0f);
        P2FirstWin.SetVal(0f);
        P2SecondWin.SetVal(0f);
        if (player1Wins == 1)
        {
            P1FirstWin.SetVal(1f);
            ;
        }
        else if (player1Wins == 2)
        {

            P1SecondWin.SetVal(1f);
        }

        if (player2Wins == 1)
        {
            P2FirstWin.SetVal(1f);
        }
        else if (player2Wins == 2)
        {
            P2SecondWin.SetVal(1f);
        }
    }


    [PunRPC]
    public void RPC_EndGame(int player1Wins, int player1matchWins, int player2matchWins)
    {
        StartCoroutine(RPC_EndGameCoroutine(player1Wins, player1matchWins, player2matchWins));
    }

    [PunRPC]
    public void RPC_RestartGame()
    {
        p1Wins = 0;
        p2Wins = 0;
        EndGameScreen.SetActive(false);
        hud.SetActive(true);
    }
    IEnumerator RPC_EndGameCoroutine(int player1Wins, int player1matchWins, int player2matchWins)
    {
        roundText.text = "Match Over!";
        yield return new WaitForSeconds(3f);
        hud.SetActive(false);
        EndGameScreen.SetActive(true);
        if (player1Wins >= 2)
        {
            player1matchWins++;
            matchWonText.text = player1Nick + "Survived the Match!";
            if (PhotonNetwork.IsMasterClient)
            {
                p1matchWins++;
            }
        }
        else
        {
            player2matchWins++;
            matchWonText.text = player2Nick + "Survived the Match!";
            if (!PhotonNetwork.IsMasterClient)
            {
                p2matchWins++;
            }
        }
        winCountText.text = player1matchWins.ToString() + " | " + player2matchWins.ToString();
    }
    
}