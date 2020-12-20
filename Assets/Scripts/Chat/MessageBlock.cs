using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MessageBlock : MonoBehaviour
{
    public Text message;
    public Text playerName;
    public RectTransform rectTransform;

    private void Start()
    {
        transform.SetParent(GameObject.Find("MessageGrid").transform);
        playerName.text = GetComponent<PhotonView>().Owner.NickName;
        rectTransform = GetComponent<RectTransform>();
    }

    [PunRPC]
    public void SendMyMessage(string x)
    {
        message.text = x;
    }

    private void Update()
    {
        if(rectTransform.anchoredPosition.y > -280) Destroy(gameObject);
    }
}
