using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCuller : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private LayerMask _player1ModelMask;
    [SerializeField] private LayerMask _player2ModelMask;
    public LayerMask PlayerModelMask(int index) => index == 0 ? _player1ModelMask : _player1ModelMask;

    [SerializeField] private LayerMask _player1FinderMask;
    [SerializeField] private LayerMask _player2FinderMask;
    public LayerMask PlayerFinderMask(int index) => index == 0 ? _player1FinderMask : _player2FinderMask;

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    public void InitializeForPlayer(int playerIndex) {
        if (playerIndex == 0) {
            cam.cullingMask &= ~(_player2ModelMask | _player2FinderMask | _player1ModelMask);
        } else {
            cam.cullingMask &= ~(_player1ModelMask | _player1FinderMask | _player2ModelMask);
        }
    }

    public void ShowOtherPlayer(int playerIndex) {
        if (playerIndex == 0) {
            ShowPlayer2();
        } else {
            ShowPlayer1();
        }
    }

    public void HideOtherPlayer(int playerIndex) {
        if (playerIndex == 0) {
            HidePlayer2();
        } else {
            HidePlayer1();
        }
    }

    public void ShowPlayer1()
    {
        cam.cullingMask |= _player1ModelMask;
    }

    public void ShowPlayer2() {
        cam.cullingMask |= _player2ModelMask;
    }

    public void HidePlayer1()
    {
        cam.cullingMask &= ~_player1ModelMask;
    }

    public void HidePlayer2() {
        cam.cullingMask &= ~_player2ModelMask;
    }
}
