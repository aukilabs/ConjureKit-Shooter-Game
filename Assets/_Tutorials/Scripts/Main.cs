using System;
using System.Collections.Generic;
using ConjureKitShooter.Gameplay;
using ConjureKitShooter.Models;
using ConjureKitShooter.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class Main : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private ParticipantsController participantsController;
    [SerializeField] private HostileController hostileController;
    [SerializeField] private UiManager uiManager;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private GunScript gunPrefab;
    [SerializeField] private Transform placementIndicator;

    [Header("AR Rig")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private AROcclusionManager arOcclusionManager;
    [SerializeField] private ARPlaneManager arPlaneManager;

    private GameState _currentGameState;

    private bool _isSharing;

    private GunScript _spawnedGun;

    private readonly Vector3 _screenMiddle = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
    private uint _myId;
    private string _myName;
    private int _score;
    private int _health;
    private bool _bloodShown;
    private bool _planeOcc;
    private bool _planeHit;
    private GameObject _planePrefab;
    
    private List<ARRaycastHit> _arRaycastHits = new();
    
    private float _nextCalibrationFx;
    private bool _onDestroy;

    public event Action OnGameStart;
    public event Action OnGameEnd;
    public event Action<uint, ScoreData> OnParticipantScore;

    // Start is called before the first frame update
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (Application.isEditor)
            arCamera.gameObject.AddComponent<EditorCamera>();

        _spawnedGun = Instantiate(gunPrefab, arCamera.transform);
        
        _planePrefab = arPlaneManager.planePrefab;
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        ToggleOcclusion(false);
        
        hostileController.Initialize(this);
        uiManager.Initialize(GameStart, PlaceSpawner, null, OnNameSet, ToggleAudio);
        uiManager.UpdateScore(0);
        healthBar.SetHealthBarAlpha(0f);

        uiManager.OnChangeState += OnChangeGameState;
        participantsController.SetListener(this);
    }

    private void OnChangeGameState(GameState gameState)
    {
        _currentGameState = gameState;
    }

    private void ToggleOcclusion(bool state)
    {
        arOcclusionManager.requestedHumanDepthMode = state ? HumanSegmentationDepthMode.Fastest : HumanSegmentationDepthMode.Disabled;
        arOcclusionManager.requestedHumanStencilMode = state ? HumanSegmentationStencilMode.Fastest : HumanSegmentationStencilMode.Disabled;
        arOcclusionManager.requestedEnvironmentDepthMode = state ? EnvironmentDepthMode.Fastest : EnvironmentDepthMode.Disabled;
    }
    
    private void ToggleAudio(bool on)
    {
        AudioListener.volume = on ? 1f : 0f;
    }

    private void TogglePlaneOcclusion()
    {
        _planeOcc = !_planeOcc;

        arPlaneManager.planePrefab = _planeOcc ? _planePrefab : null;
    }

    private void OnNameSet(string name)
    {
        _myName = name;
    }

    private void GameStart()
    {
        _score = 0;
        _health = maxHealth;

        healthBar.UpdateHealth(_health/(float)maxHealth);
        healthBar.ShowHealthBar(true);
        uiManager.ChangeUiState(GameState.GameOn);
        OnGameStart?.Invoke();
    }

    private void GameOver()
    {
        OnGameEnd?.Invoke();
        if (_currentGameState == GameState.WaitToStart) return;
        
        uiManager.ChangeUiState(GameState.WaitToStart);
        healthBar.ShowHealthBar(false);

        participantsController.GetScoreEntries(result =>
        {
            uiManager.ShowScoreBoard(result, _myName);
        });
    }

    private void Update()
    {
        UpdateRaycastIndicator();
        ShootLogic();
    }

    private void UpdateRaycastIndicator()
    {
        placementIndicator.gameObject.SetActive(_currentGameState == GameState.PlaceSpawner);
        if (_currentGameState != GameState.PlaceSpawner) return;

        var ray = arCamera.ScreenPointToRay(_screenMiddle);

        _planeHit = arRaycastManager.Raycast(ray, _arRaycastHits, TrackableType.PlaneWithinPolygon);

        if (arRaycastManager.Raycast(ray, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            var pose = _arRaycastHits[0].pose;
            
            placementIndicator.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }

    private void PlaceSpawner()
    {
        if (!_planeHit) return;
        
        var pose = new Pose(placementIndicator.position, quaternion.identity);
        hostileController.transform.position = pose.position;
    }

    private void ShootLogic()
    {
        if (_currentGameState != GameState.GameOn) return;
        if (Input.GetMouseButtonDown(0))
        {
            var hit = arCamera.transform.position + 10f * arCamera.transform.forward;
            var ray = arCamera.ViewportPointToRay(0.5f * Vector3.one);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f))
            {
                if (hitInfo.collider == null)
                    return;

                if (hitInfo.collider.TryGetComponent(out HostileScript hostile))
                {
                    hit = hitInfo.point;

                    if (hostile.Hit(hitInfo.point))
                    {
                        _score += 10;
                        OnParticipantScore?.Invoke(0, new ScoreData(){name = _myName, score = _score});
                        uiManager.UpdateScore(_score);
                    }
                }
            }

            _spawnedGun.ShootFx(hit);
        }
    }

    public void OnHit()
    {
        if (_health <= 0) return;
        
        uiManager.ShowHitVisualFeedback();

        _health--;
        
        healthBar.UpdateHealth(_health/(float)maxHealth);

        if (_health <= 0)
        {
            GameOver();
        }
    }
}