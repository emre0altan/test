using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MessageManager : MonoBehaviourPunCallbacks
{
    public MessageBlock messageBlockPrefab;
    public Username usernamePrefab;
    public GameObject loginCanvas, messageCanvas, loadingPanel;
    public Transform usernameGrid, messageGrid;
    public InputField nameInput, messageInput;

    private PhotonView thisUsername;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();       
    }

    public void Login()
    {
        if (PhotonNetwork.CountOfRooms == 0) PhotonNetwork.CreateRoom("1");
        else PhotonNetwork.JoinRoom("1");

        PlayerPrefs.SetString("PlayerName", nameInput.text);
        loginCanvas.SetActive(false);
        messageCanvas.SetActive(true);
        PhotonNetwork.LocalPlayer.NickName = nameInput.text;
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        loadingPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        UpdateUserNames();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (thisUsername != null) PhotonNetwork.Destroy(thisUsername.gameObject);
    }

    public void UpdateUserNames()
    {
        if (thisUsername != null) PhotonNetwork.Destroy(thisUsername.gameObject);

        Username tmpUsername = PhotonNetwork.Instantiate("UserName", Vector3.zero, Quaternion.identity).GetComponent<Username>();
        tmpUsername.transform.SetParent(usernameGrid);
        thisUsername = tmpUsername.GetComponent<PhotonView>();
    }

    public void BroadcastMyMessage()
    {
        MessageBlock messageBlock = PhotonNetwork.Instantiate("MessageBlock",Vector3.zero, Quaternion.identity).GetComponent<MessageBlock>();
        messageBlock.transform.SetParent(messageGrid);
        messageBlock.GetComponent<PhotonView>().RPC("SendMyMessage", RpcTarget.All, messageInput.text);
        messageInput.text = "";
    }

    private void OnApplicationQuit()
    {
        PhotonNetwork.LeaveRoom();
    }
}
