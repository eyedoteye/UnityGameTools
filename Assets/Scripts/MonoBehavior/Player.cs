using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

  public GameObject orientation;

  public float rotationSpeed;
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
  }

  public void FixedUpdate()
  {
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

    Vector3 bottommostPoint = transform.position +
      Vector3.down * transform.localScale.y / 2f;
 
    if(angle % 90 != 0)
    {
      int quadrant = (int)(angle / 90) % 4;

      int rightMod = 1;
      int upMod = 1;
      switch(quadrant)
      {
        case 0:
        {
          rightMod = -1;
          upMod = -1;
        } break;
        case 1:
        {
          rightMod = -1;
          upMod = 1;
        } break;
        case 2:
        {
          rightMod = 1;
          upMod = 1;
        } break;
        case 3:
        {
          rightMod = 1;
          upMod = -1;
        } break;
      } 

      bottommostPoint = transform.position + 
        Vector3.right * transform.localScale.x / 2f * rightMod +
        Vector3.up * transform.localScale.y / 2f * upMod;
      
      Debug.Log(quadrant + " " + angle + " " + bottommostPoint);
    }

    float hypot = Mathf.Sqrt(
      Mathf.Pow(transform.localScale.y / 2f, 2) +
      Mathf.Pow(transform.localScale.x / 2f, 2)); 

    onGround = Physics.Raycast(
      transform.position, Vector3.down,
      hypot + 1f);

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
