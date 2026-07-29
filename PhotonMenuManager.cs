using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PhotonMenuManager : MonoBehaviourPunCallbacks
{
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup tutorialSecondaryCanvasGroup; // this will be used to switch canvas 
    public TMP_InputField RoomNameInput;
    public TMP_InputField PlayerName;
    private const string PlayerPrefsNameKey = "PlayerNickname";
    public TextMeshProUGUI nameDisplay;
    public TextMeshProUGUI currentRoomDisplay;
    public Toggle PrivateRoomToggle;

    void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            string savedName = PlayerPrefs.GetString(PlayerPrefsNameKey);
            nameDisplay.text = savedName;
            PhotonNetwork.NickName = savedName;
            Debug.Log("Loaded nickname: " + savedName);
            currentRoomDisplay.text = "Not in a Room";
        }
        else
        {
            // Give a temporary name
            PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);
        }
        // Connect to Photon cloud
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");

        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !mainMenuCanvasGroup.interactable)
        {
            // Switch back to main menu
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;

            tutorialSecondaryCanvasGroup.alpha = 0f;
            tutorialSecondaryCanvasGroup.interactable = false;
            tutorialSecondaryCanvasGroup.blocksRaycasts = false;
        }
    }        

    public override void OnConnectedToMaster() //creates room
    {
        Debug.Log("Connected to Photon Master Server!");
        PhotonNetwork.JoinLobby(); //default lobby
    }

    public void CreateRoom()
    {
        string roomName = RoomNameInput.text;
         RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = !PrivateRoomToggle.isOn,  //stops quickjoin if private
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(roomName,options, TypedLobby.Default);
    }

    public void JoinRoom()
    {
        string roomName = RoomNameInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public void QuickJoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");
        currentRoomDisplay.text = "Currently in Room: " + PhotonNetwork.CurrentRoom.Name;
        
        //

    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        currentRoomDisplay.text = "Not in a Room";
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {


        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Both players joined. Loading CharacterSelect..");
            SceneManager.LoadScene("CharacterSelect");
            //here goes character select...
        }
    }

    public void NameChange()
    {
        PhotonNetwork.NickName = PlayerName.text;
        nameDisplay.text = PlayerName.text;
        PlayerPrefs.SetString(PlayerPrefsNameKey, PlayerName.text);
        PlayerPrefs.Save();
        
    }

    public void SwitchCanvas()//switch between main menu and tutorial.
    {
        if (tutorialSecondaryCanvasGroup.alpha == 1f)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
            tutorialSecondaryCanvasGroup.alpha = 0f;
            tutorialSecondaryCanvasGroup.interactable = false;
            tutorialSecondaryCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
            tutorialSecondaryCanvasGroup.alpha = 1f;
            tutorialSecondaryCanvasGroup.interactable = true;
            tutorialSecondaryCanvasGroup.blocksRaycasts = true;
        }
    }
}
