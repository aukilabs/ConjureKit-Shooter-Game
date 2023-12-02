using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using ConjureKitShooter.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ConjureKitShooter.Gameplay
{
    public class HostileController : MonoBehaviour
    {
        [SerializeField] private Transform hostileGroup;
        [SerializeField] private HostilePrefab[] hostilePrefabs;
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private Vector3 spawnOffset;

        [Header("Difficulty Settings")] 
        [SerializeField] private float minDecreaseRate;
        [SerializeField] private float maxDecreaseRate;
        [SerializeField] private float speedIncreaseRate;
        [SerializeField] private DifficultySetting easySetting;
        [SerializeField] private DifficultySetting hardSetting;

        private float _spawnTime;
        private Main _main;
    
        private Transform _player;
        private bool _isSpawning;
        private float _minInterval, _maxInterval, _hostileSpeed;
        private int _spawnCount, _totalSpawnCount;
        private Dictionary<uint, HostileScript> _spawnedHostiles = new();

        private Session _session;
        private HostilesSystem _hostilesSystem;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

        public void Initialize(IConjureKit conjureKit, Main main)
        {
            _main = main;

            conjureKit.OnJoined += session => _session = session;
            conjureKit.OnLeft += session => _session = null; 

            main.OnGameStart += StartSpawning;
            main.OnGameEnd += StopSpawning;
        
            _minInterval = easySetting.minInterval;
            _maxInterval = easySetting.maxInterval;
            _hostileSpeed = easySetting.hostileSpeed;

            _player = Camera.main.transform;
        }
        
        public void SetListener(HostilesSystem hostilesSystem)
        {
            _hostilesSystem = hostilesSystem;
            _hostilesSystem.InvokeSpawnHostile += SpawnHostileInstance;
            _hostilesSystem.InvokeDestroyHostile += DestroyHostileListener;
            _hostilesSystem.InvokeHitFx += SyncHitFx;
        }

        public void RemoveListener()
        {
            _hostilesSystem.InvokeSpawnHostile -= SpawnHostileInstance;
            _hostilesSystem.InvokeDestroyHostile -= DestroyHostileListener;
            _hostilesSystem.InvokeHitFx -= SyncHitFx;
        }

        private void StartSpawning()
        {
            _totalSpawnCount = 0;
            _isSpawning = true;
        }

        private void StopSpawning()
        {
            if (!_isSpawning) return;
        
            _isSpawning = false;

            foreach (var hostile in _spawnedHostiles)
            {
                if (hostile.Value == null) continue;
                hostile.Value.DestroyInstance();
            }
        
            _spawnedHostiles.Clear();
        }

        private void Update()
        {
            if (!_isSpawning) return;
            if (!(Time.time > _spawnTime)) return;
        
            var pos = transform.position + spawnOffset + (spawnRadius * Random.insideUnitSphere);

            _spawnTime = Time.time + Random.Range(_minInterval, _maxInterval);

            var targetEntityId = _main.GetRandomParticipantEntityId();

            var pose = new Pose(pos, Quaternion.identity);
            _session.AddEntity(pose, entity =>
            {
                _hostilesSystem.AddHostile(entity, _hostileSpeed, targetEntityId);
            }, Debug.LogError);

            _spawnCount++;

            if (_spawnCount > 5)
            {
                IncreaseDifficulty();
                _spawnCount = 0;
            }

            _spawnTime = Time.time + Random.Range(_minInterval, _maxInterval);
        }
    
        private void SpawnHostileInstance(SpawnData data)
        {
            var timeCompensation = ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - data.timestamp) / 1000f);
        
            var hostile = Instantiate(GetHostile(data.type), data.startPos, Quaternion.identity);
            hostile.Initialize(
                data.linkedEntity.Id,
                data.targetPos, 
                data.speed, 
                _hostilesSystem.SyncHitFx,
                _main.OnHit,
                InvokeRemoveHostileInstance, 
                timeCompensation);
            hostile.transform.SetParent(hostileGroup);
            _spawnedHostiles.Add(data.linkedEntity.Id, hostile);
        }
        
        private void DestroyHostileListener(uint entityId)
        {
            if (!_spawnedHostiles.ContainsKey(entityId))
            {
                return;
            }

            //Destroy the enemy instance
            _spawnedHostiles[entityId].DestroyInstance();
            _spawnedHostiles.Remove(entityId);

            //Check if the entity belongs to this local participant
            var hostileEntity = _session.GetEntity(entityId);
            if (hostileEntity == null || hostileEntity.ParticipantId != _session.ParticipantId)
                return;

            //If it is, then delete the Entity
            _session.DeleteEntity(entityId, null);
        }

        private void InvokeRemoveHostileInstance(uint entityId)
        {
            _hostilesSystem.DeleteHostile(entityId);
            _spawnedHostiles.Remove(entityId);
        }
        
        private void SyncHitFx(HitData data)
        {
            if (!_spawnedHostiles.ContainsKey(data.EntityId))
            {
                //skip if no hostile exist with the above entity Id
                return;
            }

            //skip if the hit position is zero (default)
            if (data.Pos.ToVector3() == Vector3.zero) return;

            //trigger the spawn hit fx, to the related ghost
            _spawnedHostiles[data.EntityId].SpawnHitFx(data);
        }

        private void IncreaseDifficulty()
        {
            _minInterval -= minDecreaseRate;
            _minInterval = Mathf.Clamp(_minInterval, hardSetting.minInterval, Single.MaxValue);
        
            _maxInterval -= maxDecreaseRate;
            _maxInterval = Mathf.Clamp(_maxInterval, hardSetting.maxInterval, Single.MaxValue);

            _hostileSpeed += speedIncreaseRate;
            _hostileSpeed = Mathf.Clamp(_hostileSpeed, 0, hardSetting.hostileSpeed);
        }

        private HostileScript GetHostile(HostileType type)
        {
            return Array.Find(hostilePrefabs, x => x.type == type)?.hostilePrefab;
        }
    }
    
    [Serializable]
    public struct DifficultySetting
    {
        public float minInterval;
        public float maxInterval;
        public float hostileSpeed;
    }

    [Serializable]
    public class HostilePrefab
    {
        public HostileType type;
        public HostileScript hostilePrefab;
    }
}
