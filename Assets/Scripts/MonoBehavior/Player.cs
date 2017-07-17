using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

  public GameObject orientation;

  public float rotationSpeed;
  public float jumpVelocity;
  public float gravity;

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
  }

  public void FixedUpdate()
  {
    rigidbody.AddForce(
      Vector3.down * gravity * Time.deltaTime,
      ForceMode.VelocityChange);

    rigidbody.angularVelocity =
      transform.forward *
      (-leftJoystickInput.x * rotationSpeed);

    float angle;
    Vector3 axis;
    rigidbody.rotation.ToAngleAxis(out angle, out axis);

    if(leftJoystickInput.x == 0 && angle % 90 < 2)
    {
      int mod = (int)(angle % 90);
      int intangle = (int)(angle / 90);
      angle = mod * intangle;
      rigidbody.MoveRotation(Quaternion.AngleAxis(angle, axis));
      rigidbody.angularVelocity = Vector3.zero;
    }

    float hypot = Mathf.Sqrt(
      Mathf.Pow(transform.localScale.y / 2f, 2) +
      Mathf.Pow(transform.localScale.x / 2f, 2));

    RaycastHit hitInfo;
    bool groundHit = Physics.Raycast(
      transform.position, Vector3.down,
      out hitInfo,
      hypot * 2);

    if(groundHit)
    {
      float nextDeltaTimeGuess = (Time.fixedDeltaTime + Time.deltaTime) / 2;
      float yVelocityGuess = rigidbody.velocity.y * nextDeltaTimeGuess;
      if(rigidbody.velocity.y < 0 &&
        -hitInfo.distance > yVelocityGuess)
      {
        rigidbody.velocity = new Vector3(
          rigidbody.velocity.x,
          hitInfo.distance * nextDeltaTimeGuess,
          rigidbody.velocity.z);

      }
      onGround = Physics.Raycast(
        transform.position, Vector3.down,
        hypot);
    }

    Debug.DrawRay(
      transform.position,
      Vector3.down * (hypot + 1f),
      onGround ? Color.green : Color.white, 0, false);

    //float yVelocity = rigidbody.velocity.y; 
    //rigidbody.velocity = Quaternion.Inverse(transform.rotation) * transform.right *
    //                     (leftJoystickInput.x * moveSpeed);
    //rigidbody.velocity += Vector3.up * yVelocity;
    //if(leftJoystickInput.x == 0)
    //  rigidbody.angularVelocity = new Vector3(0f,0f,0f);
    //rigidbody.AddTorque(transform.forward * moveSpeed * -leftJoystickInput.x);

    //float rightEdge = 1;
    //if(leftJoystickInput.x > 0)
    //  rightEdge = -1; 

    //Vector3 edge = transform.position +
    //  (Vector3.down + Vector3.right * rightEdge) * (transform.localScale.y / 2);

    //edge = transform.rotation * edge; 

    //bool rightEdgeOnGround = Physics.Raycast(edge, Vector3.down,
    //                                         1f);
    //Debug.DrawRay(edge, Vector3.down,
    //              onGround ? Color.green : Color.white,
    //              0, false);

    //if(leftJoystickInput.x != 0)
    //  rigidbody.MoveRotation(rigidbody.rotation *
    //    Quaternion.Euler(transform.forward *
    //    Mathf.Deg2Rad * rotationSpeed * rightEdge));

    if(jumpInput && onGround)
    {
      rigidbody.velocity += Vector3.up * jumpVelocity;
      jumpInput = false;
    }
  }
}
