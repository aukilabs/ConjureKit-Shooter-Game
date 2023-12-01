using System;
using System.Collections.Generic;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }

        public void Initialize(Main main)
        {
            _main = main;

            main.OnGameStart += StartSpawning;
            main.OnGameEnd += StopSpawning;
        
            _minInterval = easySetting.minInterval;
            _maxInterval = easySetting.maxInterval;
            _hostileSpeed = easySetting.hostileSpeed;

            _player = Camera.main.transform;
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
            var targetPos = _player.position;
            _spawnTime = Time.time + Random.Range(_minInterval, _maxInterval);
            var types = Enum.GetValues(typeof(HostileType));
            var spawnData = new SpawnData()
            {
                startPos = pos,
                speed =  _hostileSpeed,
                targetPos = targetPos,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                type = (HostileType)types.GetValue(Random.Range(0, types.Length))
            };
        
            SpawnHostileInstance(spawnData);

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
                (uint)_totalSpawnCount,
                data.targetPos, 
                data.speed, 
                _main.OnHit,
                InvokeRemoveHostileInstance, 
                timeCompensation);
            hostile.transform.SetParent(hostileGroup);
            _spawnedHostiles.Add((uint)_totalSpawnCount, hostile);
            _totalSpawnCount++;
        }

        private void InvokeRemoveHostileInstance(uint entityId)
        {
            _spawnedHostiles.Remove(entityId);
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
