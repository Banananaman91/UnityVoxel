using UnityEngine;

namespace VoxelTerrain.Editor
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private int _rotateSpeed;
        [SerializeField] private float _movementSpeed;
        private Vector3 _moveDirection = Vector3.zero;
        // Update is called once per frame
        void Update()
        {
            _moveDirection.x = Input.GetAxis("Horizontal");
            _moveDirection.z = Input.GetAxis("Vertical");
            if (Input.GetKey(KeyCode.Space)) _moveDirection.y = 1;
            else if (Input.GetKey((KeyCode.LeftShift))) _moveDirection.y = -1;
            else _moveDirection.y = 0;
            float moveRotation = Input.GetAxis("Mouse X") * _rotateSpeed;
            transform.Rotate(0, moveRotation, 0);
            transform.position += _moveDirection * (_movementSpeed * Time.deltaTime);
        }
    }
}
