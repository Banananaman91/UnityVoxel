using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        float moveRotation = Input.GetAxis("Mouse X") * _rotateSpeed;
        transform.Rotate(0, moveRotation, 0);
        var directionVector = (transform.right * _moveDirection.x) + (transform.forward * _moveDirection.z);
        transform.position += directionVector * (_movementSpeed * Time.deltaTime);
    }
}
