using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public bool isPlayerActive = false;

    [SerializeField] private float _moveSpeed = 2.0f;
    [SerializeField] private float _horizontalLookSpeed = 2.0f;
    [SerializeField] private float _verticalLookSpeed = 1.6f;

    [SerializeField] private float _maxLookXAxis = 89;
    [SerializeField] private float _minLookXAxis = -89;

    [SerializeField] private PlayerInput _playerInput;
    public PlayerInput PlayerInput { get => _playerInput; }

    [SerializeField] private Vector2 _lastMove;
    public Vector2 LastMove { get => _lastMove; }

    [SerializeField] private Vector2 _lastLook;
    public Vector2 LastLook { get => _lastLook; }

    [SerializeField] private int _playerIndex;
    public int PlayerIndex { get => _playerIndex; }

    [SerializeField] private GameObject _monsterFinderObject;
    [SerializeField] private GameObject _playerModelObject;
    [SerializeField] private GameObject _swordObject;

    [SerializeField] private MonsterCuller _monsterCuller;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private CollectableController _collectableController;
    [SerializeField] private MonsterPointer _monsterPointer;

    [SerializeField] private Animator _swordAnimator;
    [SerializeField] private Animator _playerAnimator;

    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _cameraTransform;

    [SerializeField] private LevelController _levelController;
    [SerializeField] private AudioController _audioController;

    [SerializeField] private float _lastPitchRot = 0f;
    [SerializeField] private bool _hasMonsterFinderOpen = false;
    public bool HasMonsterFinderOpen { get => _hasMonsterFinderOpen; }

    [SerializeField] private float _maxMonsterDistance = 24f;
    [SerializeField] private bool _canSeeMonster = false;
    public bool CanSeeMonster { get => _canSeeMonster; }

    [SerializeField] private float _lastDistance = 0f;
    [SerializeField] private Vector3 _lastViewportPoint = Vector3.zero;

    [SerializeField] private float _monsterFinderCooldown = 1.2f;
    [SerializeField] private float _monsterAttackCooldown = 1.2f;

    [SerializeField] private float _monsterFinderTick = 0f;
    [SerializeField] private float _monsterAttackTick = 0f;

    [SerializeField] private int _requiredRunes = 3;
    [SerializeField] private int _runesCollected = 0;

    [SerializeField] private bool _hasSword = false;
    [SerializeField] private bool _canCollect = true;

    [SerializeField] private bool _isInitialized = false;

    private void Awake() {
        if (!_playerTransform) _playerTransform = GetComponent<Transform>();
        if (!_levelController) _levelController = FindObjectOfType<LevelController>();
        if (!_audioController) _audioController = FindObjectOfType<AudioController>();
        if (!_collectableController) _collectableController = FindObjectOfType<CollectableController>();
        if (!_playerInput) _playerInput = GetComponent<PlayerInput>();
        if (!_controller) _controller = GetComponent<CharacterController>();
        if (!_cameraTransform) _cameraTransform = _playerInput.camera.transform;
        _playerIndex = _playerInput.playerIndex;
        
        if (_levelController.AllPlayersJoined) Initialize();
    }

    public void Initialize() {

        if (_hasSword) {
            _swordObject.SetActive(true);
        } else {
            _swordObject.SetActive(false);
        }
        _monsterFinderObject.SetActive(false);

        int modelLayer = _levelController.PlayerModelLayer(_playerIndex);
        SetLayerRecursively(_playerModelObject, modelLayer);

        int finderLayer = _levelController.PlayerItemLayer(_playerIndex);
        SetLayerRecursively(_monsterFinderObject, finderLayer);
        SetLayerRecursively(_swordObject, finderLayer);

        int colliderLayer = _levelController.PlayerColliderLayer(_playerIndex);
        gameObject.layer = colliderLayer;

        _monsterCuller.InitializeForPlayer(_playerIndex);

        _monsterFinderTick = _monsterFinderCooldown;
        _monsterAttackTick = _monsterAttackCooldown;

        Transform otherPlayerTransform = _levelController.GetOtherPlayer(_playerIndex).gameObject.transform;
        _monsterPointer.target = otherPlayerTransform;
        
        _isInitialized = true;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if (!isPlayerActive) return;
        if (_isInitialized) {
            if (hit.gameObject.layer == _levelController.OtherPlayerColliderLayer(_playerIndex)) {
                // Collided with visible monster
                // Trigger game win
                bool otherPlayerSees = _levelController.GetOtherPlayer(_playerIndex).HasMonsterFinderOpen || _hasSword;
                if (otherPlayerSees) {
                    _levelController.OnGameWin(_playerIndex);
                }
            }
        }
    }

    public void SetLayerRecursively(GameObject target, int layer) {
        if (target == null) return;
        target.layer = layer;

        foreach (Transform child in target.transform) {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public void SnapToGround() {
        RaycastHit[] hits = Physics.RaycastAll(_playerTransform.position, Vector3.down, 50f);
        foreach (RaycastHit hit in hits) {
            if (hit.collider.gameObject.tag != "Player") {
                Vector3 hitPoint = hit.point;
                _playerTransform.position = hitPoint + (Vector3.up * 0.1f);
            }
        }
    }

    private void Start() {
        _levelController.RegisterPlayer(this);
    }

    private void Update() {
        _controller.Move(Physics.gravity * Time.deltaTime);
        if (!_isInitialized) return;

        // Move
        float moveMag = _lastMove.magnitude;
        if (moveMag > float.Epsilon && !_hasMonsterFinderOpen) {
            float moveMultiplier = 1f;
            if (_hasMonsterFinderOpen) moveMultiplier = 0f;

            Vector3 movement = new Vector3(_lastMove.x, 0, _lastMove.y).normalized * _moveSpeed * Time.deltaTime * moveMultiplier;
            _controller.Move(_playerTransform.forward * movement.z + _playerTransform.right * movement.x);
        }
        _playerAnimator.SetFloat("Move", _lastMove.normalized.magnitude);


        // Vertical Look
        if (Mathf.Abs(LastLook.y) > float.Epsilon) {
            float newPitchRot = Mathf.Clamp(_lastPitchRot - (_verticalLookSpeed * _lastLook.y * Time.deltaTime), _minLookXAxis, _maxLookXAxis);
            _lastPitchRot = newPitchRot;
            _cameraTransform.localEulerAngles = new Vector3(_lastPitchRot, 0, 0);
        }

        // Horizontal Look
        if (Mathf.Abs(_lastLook.x) > float.Epsilon) {
            _playerTransform.Rotate(Vector3.up, _lastLook.x * _horizontalLookSpeed);
        }

        bool monsterInView = false;
        Transform otherPlayerTransform = _levelController.GetOtherPlayer(_playerIndex).transform;
        _lastDistance = Vector3.Distance(_playerTransform.position, otherPlayerTransform.position);
        _lastViewportPoint = _playerInput.camera.WorldToViewportPoint(otherPlayerTransform.position);
        if (_lastDistance < _maxMonsterDistance && _lastViewportPoint.z > 0 && _lastViewportPoint.x > 0 && _lastViewportPoint.x < 1 && _lastViewportPoint.y > -1 && _lastViewportPoint.y < 1) {
            monsterInView = true;
        }

        if (_canSeeMonster) {
            if (!monsterInView || !_hasMonsterFinderOpen) {
                _canSeeMonster = false;
                _audioController.UseNormalWind(_playerIndex);
            }
        } else {
            if (monsterInView && _hasMonsterFinderOpen) {
                _canSeeMonster = true;
                _audioController.UseDistortedWind(_playerIndex);
            }
        }

        if (_monsterAttackTick < _monsterAttackCooldown) {
            _monsterAttackTick += Time.deltaTime;
        }

        if (_monsterFinderTick < _monsterFinderCooldown) {
            _monsterFinderTick += Time.deltaTime;
        }
    }

    public void OnPickupSword() {
        if (!_hasSword) {
            _hasSword = true;
            if (_hasMonsterFinderOpen) {
                OnDeactivateMonsterFinder();
            }
            _swordObject.SetActive(true);
            _levelController.OnUnlockSword(_playerIndex);
            _monsterCuller.ShowOtherPlayer(_playerIndex);
            _monsterPointer.gameObject.SetActive(true);
        }
    }



    public void OnMove(InputAction.CallbackContext context) {
        _lastMove = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context) {
        _lastLook = context.ReadValue<Vector2>();
    }

    public void OnTrigger(InputAction.CallbackContext context) {
        if (!isPlayerActive) return;

        bool active = context.ReadValue<float>() > 0.98f;

        if (_hasSword) {
            if (active && _monsterAttackTick >= _monsterAttackCooldown) {
                OnAttack();
            }
        } else {
            if (active) {
                if (!_hasMonsterFinderOpen && _monsterFinderTick >= _monsterFinderCooldown) {
                    OnActivateMonsterFinder();
                }
            } else if (_hasMonsterFinderOpen) {
                OnDeactivateMonsterFinder();
            }
        }
    }

    public void OnMenu(InputAction.CallbackContext context) {
        _levelController.OnMenu(_playerIndex);
    }

    public void OnBack(InputAction.CallbackContext context) {
        _levelController.OnBack(_playerIndex);
    }

    public void OnTriggerEnter(Collider other) {
        if (!isPlayerActive) return;
        if (other.tag == "Rune" && _canCollect) {
            other.gameObject.SetActive(false);
            _runesCollected++;
            _levelController.OnCollectRune(_playerIndex, _runesCollected, _runesCollected >= _requiredRunes);
            if (_runesCollected >= _requiredRunes) {
                _canCollect = false;
                OnPickupSword();
            }
        }
    }

    

    public void OnActivateMonsterFinder() {
        _hasMonsterFinderOpen = true;
        _monsterFinderObject.SetActive(true);
        _monsterCuller.ShowOtherPlayer(_playerIndex);
        _levelController.OnUseMonsterFinder(_playerIndex);
    }

    public void OnDeactivateMonsterFinder() {
        _monsterFinderTick = 0f;
        _hasMonsterFinderOpen = false;
        _monsterFinderObject.SetActive(false);
        _monsterCuller.HideOtherPlayer(_playerIndex);
    }

    public void OnAttack() {
        _monsterAttackTick = 0f;
        _swordAnimator.SetTrigger("Attack");
        _levelController.OnUseSword(_playerIndex);
        Collider[] collisions = Physics.OverlapBox(_playerTransform.position + _playerTransform.forward * 2f, Vector3.one * 0.5f, Quaternion.identity);
        foreach (Collider collider in collisions) {
            Debug.Log("Hit Collider", collider.gameObject);
            if (collider.gameObject.layer == _levelController.OtherPlayerColliderLayer(_playerIndex) && collider.gameObject != gameObject) {
                // Attacked visible monster
                // Game Over
                _levelController.OnGameLose(_playerIndex);
                return;
            }
        }
    }
}
