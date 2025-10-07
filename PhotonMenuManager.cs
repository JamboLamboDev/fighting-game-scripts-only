using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonMenuManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        // Connect to Photon cloud
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Connecting to Photon...");
            
        }
    }

    public override void OnConnectedToMaster() //creates room
    {
        Debug.Log("Connected to Photon Master Server!");
        PhotonNetwork.JoinOrCreateRoom("TestRoom", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room!");

    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player joined: {newPlayer.NickName} ({PhotonNetwork.CurrentRoom.PlayerCount}/2");

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("Both players joined. Loading fight scene...");
            PhotonNetwork.LoadLevel("TestScene"); //normally goes to char select, but for testing purpose
        }
    }
}
