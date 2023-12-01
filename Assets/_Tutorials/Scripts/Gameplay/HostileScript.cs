using System;
using UnityEngine;

namespace ConjureKitShooter.Gameplay
{
    public class HostileScript : MonoBehaviour
    {
        [SerializeField] private GameObject visual;
        [SerializeField] private ParticleSystem explosion, hitParticles, spawnParticle;
        [SerializeField] private AudioClip hitSfx, explodeSfx, spawnSfx;
        private uint _entityId;

        private float _speed = 3f, nearPlayerDistance = 0.75f;
        private int _health = 1;
        private Rigidbody _rb;

        private Vector3 _targetPos;
        private AudioSource _audio;
    
        private Action _onPlayerHit;
        private Action<uint, Vector3> _onHit;
        private Action<uint> _onDestroy;

        public void Initialize(
            uint id,
            Vector3 target, 
            float speed,
            Action onPlayerHit, 
            Action<uint> onDestroy,
            float timeCompensation = 0f)
        {
            _rb = GetComponent<Rigidbody>();
            _audio = GetComponent<AudioSource>();
        
            _entityId = id;
            _targetPos = target;
            _speed = speed;

            _onPlayerHit = onPlayerHit;
            _onDestroy = onDestroy;

            var velocity = _speed * (_targetPos - _rb.position).normalized;
            _rb.position += timeCompensation * velocity;
        
            spawnParticle.Play();
            PlaySound(spawnSfx);
        }

        /// <summary>
        /// Hostile hit method
        /// </summary>
        /// <param name="hitPos">hit position from raycast</param>
        /// <param name="onHit"></param>
        /// <param name="onDestroy"></param>
        /// <returns>true if the drone is destroyed</returns>
        public bool Hit(Vector3 hitPos)
        {
            _health--;

            var hitDirection = (hitPos - transform.position).normalized;

            _onHit?.Invoke(_entityId, hitPos);
        
            var particleT = hitParticles.transform;
            particleT.rotation = Quaternion.LookRotation(hitDirection);
            particleT.position = hitPos;
            hitParticles.Play();
        
            PlaySound(hitSfx);
        
            if (_health > 0) return false;
        
            _onDestroy?.Invoke(_entityId);
            DestroyInstance();
            return true;
        }

        public void DestroyInstance()
        {
            _onPlayerHit = null;
            PlaySound(explodeSfx);
            explosion.Play();
            var destroyDuration = explosion.main.duration;
            GetComponent<Collider>().enabled = false;
            Destroy(visual);
            Destroy(gameObject, destroyDuration);
        }

        private void FixedUpdate()
        {
            if (_onPlayerHit == null)
                return;

            var move = Vector3.MoveTowards(_rb.position, _targetPos, _speed * Time.fixedDeltaTime);
            _rb.MovePosition(move);

            var direction = (_targetPos - _rb.position).normalized;
            _rb.rotation = Quaternion.LookRotation(direction);

            //if game object is too close to the target (player)
            if (!((_targetPos - _rb.position).magnitude < nearPlayerDistance)) return;
            _onDestroy?.Invoke(_entityId);
            _onPlayerHit?.Invoke();
            DestroyInstance();
        }

        private void PlaySound(AudioClip clip)
        {
            _audio.PlayRandomPitch(clip, 0.12f);
        }

        private void OnDestroy()
        {
            _audio.Stop();
        }
    }
}