using System;
using System.Collections.Generic;
using ConjureKitShooter.Models;
using ConjureKitShooter.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConjureKitShooter.UI
{
    public class UiManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text tutorialText;
        [SerializeField] private TMP_Text versionLabel;

        [Header("Splash Screen")] 
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button playButton;
        [SerializeField] private Button instructionButton;
        [SerializeField] private Button instructionCloseButton;
        [SerializeField] private BaseTween instructionPanel;
        [SerializeField] private BaseTween splashScreenPanel;
    
        [Header("Game Screen")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button placeButton;
        [SerializeField] private Button qrButton;
        [SerializeField] private ButtonSwitch soundButton;
        [SerializeField] private BaseTween scorePanel;
        [SerializeField] private Transform scoreEntryParent;
        [SerializeField] private ScoreEntry scoreEntryPrefab;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text sessionText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private CanvasGroup bloodVignette;

        private GameState _currentGameState;

        private bool _soundOff;
        private bool _scoreBoardDisplayed;
        private bool _nameFilled;
        private bool _instructionOut;
        private bool _bloodShown;
        private Action _startGame;
        private Action _repositionBeam;

        private const string NameKey = "Name";
        private const string PlayText = "Play";
        private const string PlaceText = "Place";
        private const string TutWaitToStart = "Press Play to start.";
        private const string TutPlaceBeam = "Place Beam on the indicator.";

        public event Action<GameState> OnChangeState;

        public void Initialize(Action startGame, Action repositionBeam, Action toggleQr, Action<string> setName, Action<bool> toggleAudio)
        {
            instructionPanel.Initialize();
            splashScreenPanel.Initialize();
            scorePanel.Initialize();
            _startGame = startGame;
            _repositionBeam = repositionBeam;
        
            nameInput.text  = PlayerPrefs.GetString(NameKey, string.Empty);
            setName?.Invoke(nameInput.text);

            _nameFilled = !string.IsNullOrEmpty(nameInput.text);

            playButton.interactable = _nameFilled;
            playButton.onClick.AddListener(() =>
            {
                ChangeUiState(GameState.WaitToStart);
                PlayerPrefs.SetString(NameKey, nameInput.text);
                PlayerPrefs.Save();
            });

            placeButton.onClick.AddListener(() =>
            {
                ChangeUiState(GameState.PlaceSpawner);
            });
        
            startButton.onClick.AddListener(() =>
            {
                switch (_currentGameState)
                {
                    case GameState.WaitToStart:
                        _startGame?.Invoke();
                        break;
                    case GameState.PlaceSpawner:
                        _repositionBeam?.Invoke();
                        ChangeUiState(GameState.WaitToStart);
                        break;
                }
            });

            soundButton.AddListener(()=>
            {
                _soundOff = !_soundOff;
                toggleAudio?.Invoke(!_soundOff);
            });
        
            qrButton.onClick.AddListener(() =>
            {
                toggleQr?.Invoke();
            });

            nameInput.onValueChanged.AddListener(str =>
            {
                setName?.Invoke(str);
                _nameFilled = !string.IsNullOrEmpty(str);
                playButton.interactable = _nameFilled;
            });

            instructionButton.onClick.AddListener(() =>
            {
                instructionPanel.PlaySequence();
            });
            instructionCloseButton.onClick.AddListener(() =>
            {
                instructionPanel.PlaySequence(true);
            });

            versionLabel.text = Application.version;
            qrButton.interactable = false;
            bloodVignette.alpha = 0f;
        }

        public void ChangeUiState(GameState state)
        {
            if (_currentGameState == state) return;
            _currentGameState = state;
            OnChangeState?.Invoke(_currentGameState);
            switch (state)
            {
                case GameState.WaitToStart:
                    startButton.gameObject.SetActive(true);
                    placeButton.gameObject.SetActive(true);
                    tutorialText.SetText(TutWaitToStart);
                    if (!splashScreenPanel.IsSequenceComplete(true))
                        splashScreenPanel.PlaySequence(true);
                    startButton.GetComponentInChildren<TMP_Text>().SetText(PlayText);
                    startButton.interactable = true;
                    qrButton.interactable = true;
                    placeButton.interactable = true;
                    qrButton.GetComponent<LayoutElement>().ignoreLayout = false;
                    break;
                case GameState.PlaceSpawner:
                    startButton.gameObject.SetActive(true);
                    placeButton.gameObject.SetActive(true);
                    tutorialText.SetText(TutPlaceBeam);
                    startButton.GetComponentInChildren<TMP_Text>().SetText(PlaceText);
                    startButton.interactable = true;
                    qrButton.interactable = false;
                    placeButton.interactable = false;
                    qrButton.GetComponent<LayoutElement>().ignoreLayout = false;
                    HideScoreBoard();
                    break;
                case GameState.GameOn:
                    tutorialText.SetText(string.Empty);
                    UpdateScore(0);
                    startButton.gameObject.SetActive(false);
                    placeButton.gameObject.SetActive(false);
                    qrButton.GetComponent<LayoutElement>().ignoreLayout = true;
                    HideScoreBoard();
                    break;
            }
        }
    
        public void ShowHitVisualFeedback()
        {
            if (_bloodShown)
                return;

            _bloodShown = true;
            this.LerpFloat(0f, 1f, 0.25f, f =>
            {
                bloodVignette.alpha = f;
            }, () =>
            {
                this.LerpFloat(1f, 0f, 0.25f, f =>
                {
                    bloodVignette.alpha = f;
                }, () =>
                {
                    _bloodShown = false;
                });
            });
        }

        public void UpdateScore(int value)
        {
            scoreText.SetText(value.ToString("0000000"));
        }

        public void ShowScoreBoard(Dictionary<uint, (string, int)> scores, string mainPlayerName)
        {
            foreach (Transform c in scoreEntryParent)
            {
                c.gameObject.SetActive(false);
            }

            var i = 0;
            foreach (var entry in scores)
            {
                ScoreEntry currentEntry;
                if (i >= scoreEntryParent.childCount)
                {
                    currentEntry = Instantiate(scoreEntryPrefab, scoreEntryParent);
                }
                else
                {
                    currentEntry = scoreEntryParent.GetChild(i).GetComponent<ScoreEntry>();
                }
                currentEntry.gameObject.SetActive(true);
                currentEntry.UpdateContent(entry.Value.Item1, entry.Value.Item2);
                i++;
            }

            scorePanel.PlaySequence();
            _scoreBoardDisplayed = true;
        }

        private void HideScoreBoard()
        {
            if (!_scoreBoardDisplayed)
                return;
        
            scorePanel.PlaySequence(true);

            _scoreBoardDisplayed = false;
        }
    }
}