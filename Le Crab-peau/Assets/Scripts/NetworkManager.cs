using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // instance
    public static NetworkManager instance;

    private RoomOptions roomOptions;

    void Awake()
    {
        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            //set the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start ()
    {
        PhotonNetwork.ConnectUsingSettings();
        roomOptions.MaxPlayers = 4;
    }

    public void CreateRoom(string roomName)
    {
        //PhotonNetwork.CreateRoom(roomName);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }
}
