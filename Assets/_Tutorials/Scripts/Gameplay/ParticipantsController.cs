using System;
using System.Collections.Generic;
using ConjureKitShooter.Models;
using UnityEngine;

namespace ConjureKitShooter.Gameplay
{
    public class ParticipantsController : MonoBehaviour
    {
        private Dictionary<uint, (string, int)> _scoreCache = new();
        private Main _main;
    
        public void SetListener(Main main)
        {
            main.OnParticipantScore += OnParticipantScores;
        }
    
        /// <summary>
        /// Invoked when a participant joins
        /// </summary>
        /// <param name="id">participant device entity Id</param>
        /// <param name="participantName">Participant name</param>
        private void OnParticipantJoins(uint id, string participantName)
        {
            _scoreCache.TryAdd(id, (participantName, 0));
        }

        /// <summary>
        /// Invoked when a participant scores
        /// </summary>
        /// <param name="id">participant device entity Id</param>
        /// <param name="data">ScoreData payload</param>
        private void OnParticipantScores(uint id, ScoreData data)
        {
            if (!_scoreCache.ContainsKey(id))
            {
                OnParticipantJoins(id, data.name);
                return;
            }
        
            _scoreCache[id] = (data.name, data.score);
        }

        public void GetScoreEntries(Action<Dictionary<uint, (string, int)>> valueCallback)
        {
            Dictionary<uint, (string, int)> result = new();

            foreach (var entry in _scoreCache)
            {
                result.Add(entry.Key, entry.Value);
            }

            valueCallback?.Invoke(result);
            _scoreCache.Clear();
        }
    }
}