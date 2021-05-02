using UnityEngine;

namespace Controller
{
    public class CameraOrbit : MonoBehaviour
    { 
        [SerializeField] private GameObject _target;
        [SerializeField] private int _heightAbovePlayer;
        [SerializeField] private float _upDistance;
        [SerializeField] private float _backDistance;
        [SerializeField] private float _trackingSpeed;
        private Vector3 _targetVector3;
        private Quaternion _targetQuaternion;
        private Transform TargetTransform => _target.transform;
        private Vector3 _aboveVector3;
        private void FixedUpdate()
        {
            _targetVector3 = TargetTransform.position - TargetTransform.forward * _backDistance + TargetTransform.up * _upDistance;
            _aboveVector3 = TargetTransform.position + TargetTransform.up * _heightAbovePlayer;
            transform.position = Vector3.Lerp (transform.position, _targetVector3, _trackingSpeed * Time.deltaTime);
            transform.LookAt(_aboveVector3, TargetTransform.up);
        }
    }
}