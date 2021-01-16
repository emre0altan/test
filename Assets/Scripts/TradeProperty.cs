using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeProperty : MonoBehaviour
{
    public GameObject glow;

    public void ChangeVisib()
    {
        glow.SetActive(!glow.activeSelf);
    }
}
