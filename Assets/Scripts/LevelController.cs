using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [SerializeField] private List<PlayerController> _playerControllers = new List<PlayerController>();
    public List<PlayerController> PlayerControllers { get => _playerControllers; }
    public PlayerController GetOtherPlayer(int playerIndex) => playerIndex == 0 ? _playerControllers[1] : _playerControllers[0];

    [SerializeField] private List<bool> _playerIsLooking = new List<bool>();
    public List<bool> PlayerIsLooking { get => _playerIsLooking; }

    [SerializeField] private List<Transform> _playerSpawnPoints = new List<Transform>();
    [SerializeField] private int _expectedPlayerCount = 2;

    [SerializeField] private Camera _overlayCamera;
    [SerializeField] private GameObject _overlayCanvas;
    [SerializeField] private Image _overlayImage;

    [SerializeField] private GameObject _preGameText;
    [SerializeField] private GameObject _winGameText;
    [SerializeField] private GameObject _loseGameText;

    [SerializeField] private GameObject _loseGameScene;
    [SerializeField] private Transform _loseGameCameraPos;

    [SerializeField] private GameObject _winGameScene;
    [SerializeField] private Transform _winGameCameraPos;

    [SerializeField] private GameObject _swordTutorialText;
    [SerializeField] private GameObject _monsterFinderTutorialText;

    [SerializeField] private TMPro.TextMeshProUGUI _player1RuneText;
    [SerializeField] private TMPro.TextMeshProUGUI _player2RuneText;

    [SerializeField] private string _menuSceneName = "MainMenu";

    [SerializeField] private int _player1ModelLayer;
    [SerializeField] private int _player2ModelLayer;
    public int PlayerModelLayer(int index) => index == 0 ? _player1ModelLayer : _player2ModelLayer;

    [SerializeField] private int _player1ItemLayer;
    [SerializeField] private int _player2ItemLayer;
    public int PlayerItemLayer(int index) => index == 0 ? _player1ItemLayer : _player2ItemLayer;


    [SerializeField] private int _player1ColliderLayer;
    [SerializeField] private int _player2ColliderLayer;
    public int PlayerColliderLayer(int index) => index == 0 ? _player1ColliderLayer : _player2ColliderLayer;
    public int OtherPlayerColliderLayer(int index) => index == 1 ? _player1ColliderLayer : _player2ColliderLayer;

    public bool AllPlayersJoined { get => _playerControllers.Count >= _expectedPlayerCount; }

    public void RegisterPlayer(PlayerController player) {
        if (_playerControllers.Contains(player) == false) {
            _playerControllers.Add(player);
        }

        if (_playerControllers.Count >= _expectedPlayerCount) StartCoroutine(DelayedPostPlayersRegistered());
    }

    public IEnumerator DelayedPostPlayersRegistered() {
        yield return new WaitForSeconds(1f);

        OnAllPlayersRegistered();
    }

    public void OnAllPlayersRegistered() {
        _overlayCamera.enabled = false;
        _preGameText.SetActive(false);
        _overlayImage.gameObject.SetActive(false);

        _monsterFinderTutorialText.SetActive(true);
        _player1RuneText.gameObject.SetActive(true);
        _player2RuneText.gameObject.SetActive(true);

        foreach (PlayerController controller in _playerControllers) {
            _playerIsLooking.Add(false);
            Transform spawn = _playerSpawnPoints[controller.PlayerIndex];
            controller.Initialize();
            controller.gameObject.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            controller.SnapToGround();
            controller.isPlayerActive = true;
        }
    }

    public void OnMenu(int playerIndex) {
        Debug.Log($"Player {playerIndex} hit Pause");
    }

    public void OnBack(int playerIndex) {
        Debug.Log($"Player {playerIndex} hit Back");
        SceneManager.LoadScene(_menuSceneName);
    }

    public void OnUseMonsterFinder(int playerIndex) {
        _monsterFinderTutorialText.SetActive(false);
    }

    public void OnUnlockSword(int playerIndex) {
        _swordTutorialText.SetActive(true);
    }

    public void OnUseSword(int playerIndex) {
        _swordTutorialText.SetActive(false);
    }

    public void OnCollectRune(int playerIndex, int newRuneCount, bool hideRuneDisplay) {
        if (hideRuneDisplay) {
            _player1RuneText.gameObject.SetActive(false);
            _player2RuneText.gameObject.SetActive(false);
        }

        if (playerIndex == 0) {
            _player1RuneText.text = $"{newRuneCount} Runes";
        } else {
            _player2RuneText.text = $"{newRuneCount} Runes";
        }
    }

    public void OnGameWin(int playerIndex) {
        foreach (PlayerController controller in _playerControllers) {
            controller.isPlayerActive = false;
        }
        StartCoroutine(WinRoutine());
    }

    public void OnGameLose(int playerIndex) {
        foreach (PlayerController controller in _playerControllers) {
            controller.isPlayerActive = false;
        }
        StartCoroutine(LoseRoutine());
    }

    private IEnumerator WinRoutine() {
        _overlayCamera.enabled = true;

        foreach (PlayerController controller in _playerControllers) {
            controller.isPlayerActive = false;
            controller.PlayerInput.camera.enabled = false;
        }

        _preGameText.SetActive(false);
        _winGameText.SetActive(true);
        _winGameScene.SetActive(true);

        Color _overlayColor = _overlayImage.color;
        _overlayColor.a = 0f;

        _overlayImage.color = _overlayColor;
        _overlayImage.gameObject.SetActive(true);
        _overlayCamera.transform.SetPositionAndRotation(_winGameCameraPos.position, _winGameCameraPos.rotation);

        yield return new WaitForSeconds(6);

        while (_overlayColor.a < 1f) {
            float newA = _overlayColor.a + Time.deltaTime * 0.2f;
            if (newA > 1f) newA = 1f;
            _overlayColor.a = newA;
            _overlayImage.color = _overlayColor;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator LoseRoutine() {
        _overlayCamera.enabled = true;

        foreach (PlayerController controller in _playerControllers) {
            controller.isPlayerActive = false;
            controller.PlayerInput.camera.enabled = false;
        }

        _preGameText.SetActive(false);
        _loseGameText.SetActive(true);
        _loseGameScene.SetActive(true);

        Color _overlayColor = _overlayImage.color;
        _overlayColor.a = 0f;

        _overlayImage.color = _overlayColor;
        _overlayImage.gameObject.SetActive(true);
        _overlayCamera.transform.SetPositionAndRotation(_loseGameCameraPos.position, _loseGameCameraPos.rotation);

        yield return new WaitForSeconds(6);

        while (_overlayColor.a < 1f) {
            float newA = _overlayColor.a + Time.deltaTime * 0.2f;
            if (newA > 1f) newA = 1f;
            _overlayColor.a = newA;
            _overlayImage.color = _overlayColor;
            yield return new WaitForEndOfFrame();
        }
    }
}
