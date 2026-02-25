using System;
using System.Collections;
using UnityEngine;

namespace CodeBase.Logic
{
    public class LookAtTarget : MonoBehaviour
    {
        private Transform _target;
        public float speed = 0.1f;
        private Coroutine _lookCoroutine;

        public void StartRotation(Transform target)
        {
            StopThisCoroutine();
            if (target != null)
            {
                _target = target;
                _lookCoroutine = StartCoroutine(LookAt());
            }
        }

        private void StopThisCoroutine()
        {
            if (_lookCoroutine != null)
            {
                StopCoroutine(_lookCoroutine);
            }
        }

        private void OnDisable()
        {
            StopThisCoroutine();
        }

        private void OnDestroy()
        {
            StopThisCoroutine();
        }

        private IEnumerator LookAt()
        {
            if (_target)
            {
                Quaternion lookRotation = Quaternion.LookRotation(_target.position - transform.position);
                float time = 0f;
                while (time<1)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
                    time += Time.deltaTime * speed;
                }
                yield return null;
            }
        }
    }
}