using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

  public float moveSpeed;
  public float jumpVelocity;

  private Vector2 leftJoystickInput;
  private bool jumpInput;

  private Rigidbody rigidbody;
  private BoxCollider boxCollider;

  private bool onGround;

  public void Start()
  {
    boxCollider = GetComponent<BoxCollider>();
    rigidbody = GetComponent<Rigidbody>();
  }

  public void Update()
  {
    leftJoystickInput.x = Input.GetAxis("Horizontal");
    leftJoystickInput.y = Input.GetAxis("Vertical");

    jumpInput = Input.GetButtonDown("Jump");

    Debug.DrawRay(boxCollider.transform.position,
                  Vector3.down * 16f,
                  onGround ? Color.green : Color.white,
                  0, false);
  }

  public void FixedUpdate()
  {
    float yVelocity = rigidbody.velocity.y; 
    rigidbody.velocity = boxCollider.transform.right * leftJoystickInput.x * moveSpeed;
    rigidbody.velocity += Vector3.up * yVelocity;

    onGround = Physics.Raycast(transform.position, Vector3.down, 16f);
    if(jumpInput && onGround)
    {
      rigidbody.velocity += Vector3.up * jumpVelocity;
      jumpInput = false;
    }
  }
}
