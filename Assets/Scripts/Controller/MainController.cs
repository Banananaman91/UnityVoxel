using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(Renderer))]
    public class MainController : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] protected Rigidbody _rb;
        // [SerializeField] protected float movementSpeed;
        // [SerializeField] protected float rotationSpeed;
        public Rigidbody Rb => _rb == null ? _rb : _rb = GetComponent<Rigidbody>();
        public Transform RbTransform => _rb.transform;
    }
}