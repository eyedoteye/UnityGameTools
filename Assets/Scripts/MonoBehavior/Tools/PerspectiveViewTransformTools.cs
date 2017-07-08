using UnityEngine;

public class PerspectiveViewTransformTools : MonoBehaviour {

  public Camera perspectiveCamera;
  public GameObject targetQuad;
  public float relativeDistance = 10;

  public void ComputeTransforms(out Vector3 newQuadPosition, out Vector3 newQuadScale, out Quaternion newQuadRotation)
  {
    newQuadPosition = perspectiveCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, relativeDistance));

    Vector3 botLeft = perspectiveCamera.ViewportToWorldPoint(new Vector3(0, 0, relativeDistance));
    Vector3 topRight = perspectiveCamera.ViewportToWorldPoint(new Vector3(1, 1, relativeDistance));

    Quaternion inverseRotation = Quaternion.Inverse(perspectiveCamera.transform.rotation); 
    botLeft = inverseRotation * botLeft;
    topRight = inverseRotation * topRight; 

    newQuadScale = (topRight - botLeft);
    if(newQuadScale.x == 0)
      newQuadScale.x = 1;
    else if(newQuadScale.y == 0)
      newQuadScale.y = 1;
    else if(newQuadScale.z == 0)
      newQuadScale.z = 1;

    Vector3 vectorPointingToCameraFromQuad = newQuadPosition - perspectiveCamera.transform.position;
    Vector3 newQuadRotationEuler = Quaternion.LookRotation(vectorPointingToCameraFromQuad).eulerAngles;
    Vector3 perspectiveCameraRotationEuler = perspectiveCamera.transform.rotation.eulerAngles;
    newQuadRotation = Quaternion.Euler(newQuadRotationEuler.x, newQuadRotationEuler.y, perspectiveCameraRotationEuler.z);
  }

  public void OnDrawGizmosSelected()
  {
    if(perspectiveCamera != null)
    {
      Vector3 botLeft = perspectiveCamera.ViewportToWorldPoint(new Vector3(0, 0, relativeDistance));
      Vector3 botRight = perspectiveCamera.ViewportToWorldPoint(new Vector3(1, 0, relativeDistance));
      Vector3 topLeft = perspectiveCamera.ViewportToWorldPoint(new Vector3(0, 1, relativeDistance));
      Vector3 topRight = perspectiveCamera.ViewportToWorldPoint(new Vector3(1, 1, relativeDistance));

      //Vector3 screenTopLeft = perspectiveCamera.ViewportToScreenPoint(new Vector2(0, 0));
      //Vector3 screenTopRight = perspectiveCamera.ViewportToScreenPoint(new Vector2(1, 0));
      //Vector3 screenBotLeft = perspectiveCamera.ViewportToScreenPoint(new Vector2(0, 1));
      //Vector3 screenBotRight = perspectiveCamera.ViewportToScreenPoint(new Vector2(0, 1));
      //Debug.Log(screenTopLeft);
      //Debug.Log(screenTopRight);
      //Debug.Log(screenBotLeft);
      //Debug.Log(screenBotRight);

      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(topLeft, topRight);
      Gizmos.DrawLine(topRight, botRight);
      Gizmos.DrawLine(botRight, botLeft);
      Gizmos.DrawLine(botLeft, topLeft);
    }
  }
}
