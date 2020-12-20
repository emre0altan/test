using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Username : MonoBehaviour
{
    public Text nameOfUser;

    private void Start()
    {
        transform.SetParent(GameObject.Find("InfoArea").transform);
        nameOfUser.text = GetComponent<PhotonView>().Owner.NickName;
    }
}
