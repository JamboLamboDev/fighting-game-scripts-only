using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class PreGameMenuController : MonoBehaviourPunCallbacks//menu that deals with character select and stage select
{

    public string[] stages;
    public PreviewChar[] previewChars; // prefabs with descriptions and the character previewed
    public TextMeshProUGUI desc;
    public TextMeshProUGUI stats;
    public TextMeshProUGUI specialStats;
    public TextMeshProUGUI chosenCharacterP1;
    public TextMeshProUGUI chosenCharacterP2;
    public TextMeshProUGUI readyStatusP1;
    public TextMeshProUGUI readyStatusP2;
    public TextMeshProUGUI playerNick1;
    public TextMeshProUGUI playerNick2;
    public PreviewChar currentChar;
    private bool player1Ready;
    private bool player2Ready;
    public TextMeshProUGUI readyButtonLabel;
    private bool readied = false;
    private int characterID;
    public GameObject charSelectUI;
    public GameObject levelSelectUI;
    public bool isMaster;

    // Start is called before the first frame update
    void Start()
    {

        if (PhotonNetwork.IsMasterClient){
            isMaster = true;
        }
        else
        {
            
        }
        readyStatusP1.text = "Not Ready";
        readyStatusP2.text = "Not Ready";
        readyButtonLabel.text = "Ready";
        StartCoroutine(GetNicknames());

    }

    IEnumerator GetNicknames() {
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == 2);
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.MasterClient)
            {
                playerNick1.text = player.NickName;
            }
            else
            {
                playerNick2.text = player.NickName;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {//setnicks
        
   
    }
    public void selectCharacter(int character)
    {
        //spawn character
        if (!readied)
        {
            currentChar = previewChars[character];
            desc.text = previewChars[character].characterDescription;
            stats.text = previewChars[character].characterStats;
            specialStats.text = previewChars[character].specialMoveDescription;
            characterID = character;
        }
        //
    }
    
    public void readyUp()
    {
        if (readied)
        {
            readied = false;
            photonView.RPC("RPC_resetReady", RpcTarget.All,isMaster);
        }
        else
        {
            readied = true;
            readyButtonLabel.text = "Unready";
            photonView.RPC("RPC_UpdateSelectedCharacter", RpcTarget.All,characterID,isMaster);          
        }
        
    }
    [PunRPC]
    void RPC_UpdateSelectedCharacter(int character, bool isMasterC)
    {
        if (isMasterC)
        {
            chosenCharacterP1.text = previewChars[character].characterName;
            readyStatusP1.text = "Ready";
            player1Ready = true;
        }
        else
        {
            chosenCharacterP2.text = previewChars[character].characterName;
            readyStatusP2.text = "Ready";
            player2Ready = true;
        }

        if (player1Ready && player2Ready)
        {
            readyStatusP1.text = "Choosing Map";
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("RPC_SaveCharSelect", RpcTarget.All);
                charSelectUI.SetActive(false);
                levelSelectUI.SetActive(true);
            }
        }


    }


        
       

    [PunRPC]
    void RPC_resetReady(bool isMasterC)
    {

        if (isMasterC)
        {
            chosenCharacterP1.text = "???";
            readyStatusP1.text = "Not Ready";
            readyButtonLabel.text = "Ready";
            player1Ready = false;
        }
        else
        {
            chosenCharacterP2.text = "???";
            readyStatusP2.text = "Not Ready";
            player2Ready = false;
            readyButtonLabel.text = "Ready";
        }

    }

    public void MapSelected(int mapid)
    {
        string stage = stages[mapid];
        PhotonNetwork.LoadLevel(stage);
        return;
        //load map
    }

    [PunRPC]
    void RPC_SaveCharSelect()
    {
        ExitGames.Client.Photon.Hashtable selectedChar = new ExitGames.Client.Photon.Hashtable
    {
        { "Character", currentChar.characterPrefabLocation }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(selectedChar); //
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room. Returning to main menu...");

        // Option 1 — load a local scene (not synced)
        SceneManager.LoadScene("MainMenu");
    }
}

