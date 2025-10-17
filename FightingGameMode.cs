using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;


public class FightingGameMode : MonoBehaviour
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
    private int p1Wins = 0;
    private int p2Wins = 0;
    public ValBar P1HPBar;
    public ValBar P2HPBar;
    public ValBar P1GuardBar;

    public ValBar P2GuardBar;

    public ValBar P1SpeBar;

    public ValBar P2SpeBar;
    public GameObject P1 = null;
    public GameObject P2 = null;
    public PhotonView photonView; //owner is host
    public Player player1Ref;
    public Player player2Ref;




    void Start()
    {
        photonView = GetComponent<PhotonView>();
        player1Ref = PhotonNetwork.MasterClient;
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

        player1.SetBars(P1HPBar, P1GuardBar, P1SpeBar); 
        player2.SetBars(P2HPBar, P2GuardBar, P2SpeBar);

        if (PhotonNetwork.IsMasterClient)
        {
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
            roundText.text = "Match Over!";
            yield break; // stops loop - PLACEHOLDER for returning to main menu or restarting match
        }
        else
        {
            StartCoroutine(GameLoop());
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
}