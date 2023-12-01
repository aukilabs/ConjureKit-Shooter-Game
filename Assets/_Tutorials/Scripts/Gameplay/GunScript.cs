using System.Collections;
using UnityEngine;

namespace ConjureKitShooter.Gameplay
{
    public class GunScript : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private ParticleSystem muzzle;
        [SerializeField] private AudioClip shootSfx;
        [SerializeField] private Animator gunAnimation;

        private WaitForSeconds _delay;
        private AudioSource _audio;
        private uint _myEntityId;
        private readonly int _shootTrigger = Animator.StringToHash("Shoot");

        private void Start()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;

            _audio = GetComponent<AudioSource>();
        
            Initialize();
        }

        private void Initialize()
        {
            _delay = new WaitForSeconds(0.2f);
        }

        public void ShootFx(Vector3 hit)
        {
            gunAnimation.SetTrigger(_shootTrigger);
            StartCoroutine(ShowShootLine(_myEntityId,muzzle.transform.position, hit));
        }

        IEnumerator ShowShootLine(uint entityId, Vector3 pos, Vector3 hit)
        {
            _audio.PlayRandomPitch(shootSfx, 0.18f);
            muzzle.Play();
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[]{pos, hit});

            yield return _delay;

            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }
}