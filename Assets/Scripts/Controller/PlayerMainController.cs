using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Controller
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMainController : MainController
    {
#pragma warning disable 0649
        [Header("Player")]
        [SerializeField] private float _walk;
        [SerializeField, Range(1,5)] private int _run;
        [SerializeField] private int _rotateSpeed;
        [SerializeField] private int _jump;
        [SerializeField] private int _dash;
        [SerializeField] private float _maxSpeed;
#pragma warning restore 0649
        private Vector3 _moveDirection = Vector3.zero;
        private Vector3 _directionVector = Vector3.zero;

        private bool _jumped;
        private bool _dashed;

        private void Update()
        {
            _moveDirection.x = Input.GetAxis("Horizontal");
            _moveDirection.z = Input.GetAxis("Vertical");
            float moveRotation = Input.GetAxis("Mouse X") * _rotateSpeed;
            transform.Rotate(0, moveRotation, 0);

            //Run
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _walk *= _run;
            }

            //Stop running
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                _walk /= _run;
            }

            //Dash
            if (Input.GetKeyDown(KeyCode.Space) && _jumped && !_dashed)
            {
                _rb.velocity = transform.forward * _dash;
                if (!_dashed) _dashed = true;
            }

            //Jump
            if (Input.GetKeyDown(KeyCode.Space) && !_jumped)
            {
                _rb.velocity = transform.up * _jump;
                if (!_jumped) _jumped = true;
            }
        }

        private void FixedUpdate()
        {
            _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, _maxSpeed);
            _directionVector = (RbTransform.right * _moveDirection.x) + (RbTransform.forward * _moveDirection.z);
            _rb.MovePosition(RbTransform.position + _directionVector * _walk * Time.deltaTime);
        }
        
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == 9)
            {
                if (_jumped) _jumped = false;
                if (_dashed) _dashed = false;
            }
        }
    }
}