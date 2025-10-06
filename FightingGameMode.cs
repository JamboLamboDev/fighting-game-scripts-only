using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


public class FightingGameMode : MonoBehaviour
{
    public FightingPlayerController player1Prefab;
    public FightingPlayerController player2Prefab;
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
    
    


    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStart());
        yield return StartCoroutine(RoundActive());
        yield return StartCoroutine(RoundEnd());
    }

    IEnumerator RoundStart()
    {
        // Spawn players fresh each round
        if (player1 != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(player1.gameObject);
        }

        if (player2 != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(player2.gameObject);
        }

        if (PhotonNetwork.IsMasterClient)
        {
             P1 = PhotonNetwork.Instantiate("Characters/GregPlayable", player1Spawn.position, player1Spawn.rotation);
        }
        else
        {
             P2 = PhotonNetwork.Instantiate("Characters/GregPlayable", player2Spawn.position, player2Spawn.rotation);
        }
        

        // Wait for both players to be instantiated
        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Player").Length == 2);

        // Find players by tag or other identifier
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var photonView = player.GetComponent<PhotonView>();
            if (photonView.Owner.IsMasterClient)
            {
                P1 = player;
                player1 = P1.GetComponent<FightingPlayerController>();
            }
            else
            {
                P2 = player;
                player2 = P2.GetComponent<FightingPlayerController>();
            }
        }
                
        
        if (P1 != null && P1.GetComponent<PhotonView>().IsMine)
        {
            int player1mask = LayerMask.NameToLayer("Player1");
            player1 = P1.GetComponent<FightingPlayerController>();
            player1.photonView.RPC("RPC_Spawn", RpcTarget.All, player1mask);
        }
        if (P2 != null && P2.GetComponent<PhotonView>().IsMine)
        {
            player2 = P2.GetComponent<FightingPlayerController>();
            int player2mask = LayerMask.NameToLayer("Player2");
            player2.photonView.RPC("RPC_Spawn", RpcTarget.All, player2mask);
            
        }

        player1.SetBars(P1HPBar, P1GuardBar, P1SpeBar);
        player2.SetBars(P2HPBar, P2GuardBar, P2SpeBar);
  
        roundText.text = "3";
        yield return new WaitForSeconds(1f);
        roundText.text = "2";
        yield return new WaitForSeconds(1f);
        roundText.text = "1";
        yield return new WaitForSeconds(1f);
        player1.stunTimer = 0f;
        player2.stunTimer = 0f;


        roundText.text = "Round Start!";
        currentTime = roundTime;
        yield return new WaitForSeconds(2f);

        roundText.text = "";
        roundActive = true;
    }

    IEnumerator RoundActive()
    {
        while (roundActive)
        {
            currentTime -= Time.deltaTime;
            timerText.text = Mathf.Ceil(currentTime).ToString();

            if (player1.health <= 0 || player2.health <= 0 || currentTime <= 0)
            {
                roundActive = false;
            }

            yield return null;
        }
    }

    IEnumerator RoundEnd()
    {
        if (player1.health <= 0 && player2.health <= 0)
        {
            
            player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"KO");
            player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"KO");
            roundText.text = "Double KO!";
        }
        else if (player1.health <= 0)
        {
            player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"KO");
            player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Victory");
            roundText.text = "Player 2 Wins!";
            p2Wins++;
        }
        else if (player2.health <= 0)
        {
            player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"KO");
            player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Victory");
            roundText.text = "Player 1 Wins!";
            p1Wins++;
        }
        else
        {

            if (player1.health > player2.health)
            {
                player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Defeated");
                player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Victory");
                roundText.text = "Player 1 Wins!";
                p1Wins++;
                
                
            }
            else
            {
                player1.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Defeated");
                player2.photonView.RPC("RPC_PlayAnimation", RpcTarget.All,"Victory");
                roundText.text = "Player 2 Wins!";
                p2Wins++;
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
            if (PhotonNetwork.IsMasterClient)
            {
                 StartCoroutine(GameLoop());
            }


        }
    }
}