using UnityEngine;

namespace VoxelTerrain
{
    public class Controller : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _movementSpeed;
#pragma warning restore 0649
        private Vector3 _moveDirection = Vector3.zero;
        // Update is called once per frame
        void Update()
        {
            _moveDirection.x = Input.GetAxis("Horizontal");
            _moveDirection.z = Input.GetAxis("Vertical");
            if (Input.GetKey(KeyCode.Space)) _moveDirection.y = 1;
            else if (Input.GetKey((KeyCode.LeftShift))) _moveDirection.y = -1;
            else _moveDirection.y = 0;
            transform.position += _moveDirection * (_movementSpeed * Time.deltaTime);
        }
    }
}
