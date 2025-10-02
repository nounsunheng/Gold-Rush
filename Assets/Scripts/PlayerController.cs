using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float speed = 5;
    [SerializeField] private float rotateSpeed = 500;

    private Quaternion targetRotation;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if(cameraController == null)
            cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp(Mathf.Abs(h)+ Mathf.Abs(v), 0, 1);

        var moveInput = (new Vector3(h, 0, v)).normalized;
        var moveDirection = cameraController.GetDirection * moveInput;

        if(moveAmount > 0)
        {
            transform.position += moveDirection * Time.deltaTime * speed;
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        animator.SetFloat("Blend", moveAmount);
    }
}
