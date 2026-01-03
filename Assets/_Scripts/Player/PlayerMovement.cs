using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private InputActionReference _moveAction;
	[SerializeField] private CharacterController _controller;
	[SerializeField] private float _moveSpeed;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private float _jumpForce;

	private float _velocityY;

    private void Update()
    {
        var input = _moveAction.action.ReadValue<Vector2>();
        var direction = (transform.forward * input.y + transform.right * input.x).normalized;
        var newVelocity = direction * _moveSpeed;

        if (_jumpAction.action.triggered && _controller.isGrounded)
        {
            _velocityY = _jumpForce;
        }
        else
        {
            UpdateFalling();
        }

        newVelocity.y = _velocityY;
        _controller.Move(newVelocity * Time.deltaTime);
    }

    private void UpdateFalling()
    {
        if (_controller.isGrounded)
        {
            _velocityY = -1;
        }
        else
        {
            _velocityY += Physics.gravity.y * Time.deltaTime;
        }
    }
}