using System;
using System.Collections;
using System.Collections.Generic;
using Auki.ConjureKit;
using ConjureKitShooter.UI;
using ConjureKitShooter.Models;
using UnityEngine;

namespace ConjureKitShooter.Gameplay
{
    public class ParticipantsController : MonoBehaviour
    {
        [SerializeField] private ParticipantNameUi participantNameUiPrefab;
        [SerializeField] private LineRenderer shootFxPrefab;

        private IConjureKit _conjureKit;

        private readonly Dictionary<uint, ParticipantComponent> _participantComponents = new();
        private Dictionary<uint, (string, int)> _scoreCache = new();
        private Main _main;
        private WaitForSeconds _delay;
        private Session _session;
        private Transform _camera;
    
        public void Initialize(IConjureKit conjureKit, Transform camera)
        {
            _conjureKit = conjureKit;
            _delay = new WaitForSeconds(0.2f);
            _camera = camera;

            _conjureKit.OnJoined += session => _session = session;
            _conjureKit.OnLeft += session =>
            {
                _scoreCache.Clear();
                foreach (var c in _participantComponents)
                {
                    if (c.Value == null) continue;
                    Destroy(c.Value.NameUi.gameObject);
                    Destroy(c.Value.LineRenderer.gameObject);
                }
                _participantComponents.Clear();
            };
        }
        
        public void SetListener(ParticipantsSystem participantsSystem)
        {
            participantsSystem.OnParticipantScores += OnParticipantScores;
            participantsSystem.InvokeShootFx += ShowParticipantShootLine;
        }

        public void RemoveListener(ParticipantsSystem participantsSystem)
        {
            participantsSystem.OnParticipantScores -= OnParticipantScores;
            participantsSystem.InvokeShootFx -= ShowParticipantShootLine;
        }
    
        public void GetAllPreviousComponents(ParticipantsSystem participantsSystem)
        {
            participantsSystem.GetAllScoresComponent(result =>
            {
                foreach (var ec in result)
                {
                    var data = ec.Data.FromJsonByteArray<ScoreData>();
                    OnParticipantScores(ec.EntityId, data);
                }
            });
        }

        public void Restart()
        {
            foreach (var c in _participantComponents)
            {
                c.Value.NameUi.SetScore(0.ToString("0000000"));
                _scoreCache[c.Key] = (c.Value.NameUi.GetName(), 0);
            }
        }
        
        /// <summary>
        /// Invoked when a participant joins
        /// </summary>
        /// <param name="id">participant device entity Id</param>
        /// <param name="participantName">Participant name</param>
        private void OnParticipantJoins(uint id, string participantName)
        {
            if (_participantComponents.TryGetValue(id, out var c))
            {
                c.NameUi.SetName(participantName);
                return;
            }
        
            var lRenderer = Instantiate(shootFxPrefab, transform);
            var nameSign = Instantiate(participantNameUiPrefab, transform);

            nameSign.SetName(participantName);
            
            lRenderer.enabled = false;
            nameSign.gameObject.SetActive(false);
        
            _participantComponents.Add(id, new ParticipantComponent(lRenderer, nameSign));
            _scoreCache.TryAdd(id, (participantName, 0));
        }

        /// <summary>
        /// Invoked when a participant scores
        /// </summary>
        /// <param name="id">participant device entity Id</param>
        /// <param name="data">ScoreData payload</param>
        private void OnParticipantScores(uint id, ScoreData data)
        {
            if (!_participantComponents.ContainsKey(id))
            {
                OnParticipantJoins(id, data.name);
                return;
            }

            _participantComponents[id].Score = data.score;
            _participantComponents[id].NameUi.SetName(data.name);
            _participantComponents[id].NameUi.SetScore(data.score.ToString("0000000"));
            _scoreCache[id] = (data.name, data.score);
        }
        
        public void OnParticipantLeft(uint id)
        {
            if (!_participantComponents.ContainsKey(id))
                return;

            Destroy(_participantComponents[id].LineRenderer.gameObject);
            Destroy(_participantComponents[id].NameUi.gameObject);

            _participantComponents.Remove(id);
        }
        
        private void UpdateScoreBoardPosition(uint id, Vector3 pos, Vector3 cameraPos)
        {
            if (!_participantComponents.ContainsKey(id))
                return;

            var nameSign = _participantComponents[id].NameUi.transform;
        
            nameSign.gameObject.SetActive(true);

            var offsetPos = pos + (0.6f * Vector3.up);
            var direction = -(cameraPos - offsetPos);

            var distance = direction.magnitude;

            nameSign.position = offsetPos;
            nameSign.rotation = Quaternion.LookRotation(direction);

            nameSign.transform.localScale = Mathf.Clamp(distance, 0.2f, 1.2f) * Vector3.one;
        }
        
        private void UpdateParticipantsScoreBoard()
        {
            if (_participantComponents.Count <= 0) return;
        
            foreach (var c in _participantComponents)
            {
                if (c.Value == null) continue;

                var entity = _session.GetEntity(c.Key);

                if (entity == null || entity.ParticipantId == _session.ParticipantId)
                    continue;

                var pos = _session.GetEntityPose(c.Key).position;
                UpdateScoreBoardPosition(c.Key, pos, _camera.position);
            }
        }

        private void Update()
        {
            UpdateParticipantsScoreBoard();
        }

        public void GetScoreEntries(Action<Dictionary<uint, (string, int)>> valueCallback)
        {
            Dictionary<uint, (string, int)> result = new();

            foreach (var entry in _scoreCache)
            {
                result.Add(entry.Key, entry.Value);
            }

            valueCallback?.Invoke(result);
        }
        
        private void ShowParticipantShootLine(uint id, ShootData data)
        {
            if (!_participantComponents.ContainsKey(id))
                return;

            StartCoroutine(ShowShootLine(id, data.StartPos.ToVector3(), data.EndPos.ToVector3()));
        }
    
        IEnumerator ShowShootLine(uint id, Vector3 pos, Vector3 hit)
        {
            var lineRenderer = _participantComponents[id].LineRenderer;
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[]{pos, hit});

            yield return _delay;

            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
}