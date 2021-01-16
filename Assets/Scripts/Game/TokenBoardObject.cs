using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using Monopoly.Game;

namespace Monopoly.GameBoard
{
    public class TokenBoardObject : MonoBehaviour
    {
        public PhotonView photonView;
        public bool isInJail;
        public int remainJailTurns;

        private void Start()
        {
            transform.SetParent(GameManager.Instance.transform);
        }

        public void MoveToken(Vector3 dest, bool inJail, int remained)
        {
            photonView.RPC("MoveTokenRPCAll", RpcTarget.All, dest, inJail, remained);
        }


        [PunRPC]
        public void MoveTokenRPCAll(Vector3 dest, bool inJail, int remained)
        {
            transform.DOMove(dest, 2f);
            isInJail = inJail;
            remainJailTurns = remained;
        }

    }
}