using UnityEngine;

public class PerspectiveViewTransformTools : MonoBehaviour {

  public Camera perspectiveCamera;
  public GameObject targetQuad;
  public float relativeDistance = 10;
  public float viewportX = 0.5f;
  public float viewportY = 0.5f;
  public float relativeRotationX = 0f;
  public float relativeRotationY = 0f;
  public float relativeRotationZ = 0f;
  public Vector2 pixelGridSize = new Vector2(16f, 16f);
  public bool gridEnabled = false;

  private Vector3 cachedBotLeftBase;
  private Vector3 cachedBotRightBase;
  private Vector3 cachedTopLeftBase;
  private Vector3 cachedTopRightBase;
  private Vector3 cachedBotLeft;
  private Vector3 cachedBotRight;
  private Vector3 cachedTopLeft;
  private Vector3 cachedTopRight;
  private Quaternion cachedQuaternion;
  private bool cachedQuaternionIsNonZero = false;

  private Vector2 cachedScreenDimensions;

  private Vector3 rotatePositionOverPoint(
    Vector3 position,
    Vector3 point,
    Quaternion rotation,
    Vector3 forward)
  {
    position = position - point;
    position = position - forward;
    position = rotation * position;
    position = position + forward;
    position = position + point;

    return position;
  }

  public void UpdateScreenDimensions()
  {
    if(perspectiveCamera == null)
      return;

    cachedScreenDimensions = perspectiveCamera.ViewportToScreenPoint(
      new Vector3(1f, 1f));
  }

  public void UpdateGizmoCache()
  {
    if(perspectiveCamera == null)
      return;

    if(relativeRotationX != 0 || relativeRotationY != 0 || relativeRotationZ != 0)
      cachedQuaternionIsNonZero = true;
    else
      cachedQuaternionIsNonZero = false;

    Quaternion relativeRotation = Quaternion.Euler(
      relativeRotationX,
      relativeRotationY,
      relativeRotationZ);
    cachedQuaternion = relativeRotation;

    cachedBotLeftBase = perspectiveCamera.ViewportToWorldPoint(
      new Vector3(0, 0, relativeDistance));
    cachedBotRightBase = perspectiveCamera.ViewportToWorldPoint(
      new Vector3(1, 0, relativeDistance));
    cachedTopLeftBase = perspectiveCamera.ViewportToWorldPoint(
      new Vector3(0, 1, relativeDistance));
    cachedTopRightBase = perspectiveCamera.ViewportToWorldPoint(
      new Vector3(1, 1, relativeDistance));

    Vector3 relativeDistanceVector = perspectiveCamera.transform.forward *
      relativeDistance;
    Vector3 offsetVector = perspectiveCamera.ViewportToWorldPoint(
      new Vector3(viewportX, viewportY, relativeDistance));

    cachedBotLeft = cachedBotLeftBase - relativeDistanceVector;
    cachedBotRight = cachedBotRightBase - relativeDistanceVector;
    cachedTopLeft = cachedTopLeftBase - relativeDistanceVector;
    cachedTopRight = cachedTopRightBase - relativeDistanceVector;

    Quaternion inverseCameraRotation = Quaternion.Inverse(
      perspectiveCamera.transform.rotation);

    cachedBotLeft = inverseCameraRotation * cachedBotLeft;
    cachedBotRight = inverseCameraRotation * cachedBotRight;
    cachedTopLeft = inverseCameraRotation * cachedTopLeft;
    cachedTopRight = inverseCameraRotation * cachedTopRight;

    cachedBotLeft = relativeRotation * cachedBotLeft;
    cachedBotRight = relativeRotation * cachedBotRight;
    cachedTopLeft = relativeRotation * cachedTopLeft;
    cachedTopRight = relativeRotation * cachedTopRight;

    cachedBotLeft = perspectiveCamera.transform.rotation * cachedBotLeft;
    cachedBotRight = perspectiveCamera.transform.rotation * cachedBotRight;
    cachedTopLeft = perspectiveCamera.transform.rotation * cachedTopLeft;
    cachedTopRight = perspectiveCamera.transform.rotation * cachedTopRight;

    cachedBotLeft = cachedBotLeft + offsetVector;
    cachedBotRight = cachedBotRight + offsetVector;
    cachedTopLeft = cachedTopLeft + offsetVector;
    cachedTopRight = cachedTopRight + offsetVector;
  }

  public void ComputeTransforms(
    out Vector3 newQuadPosition,
    out Vector3 newQuadScale,
    out Quaternion newQuadRotation)
  {
    newQuadPosition = perspectiveCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, relativeDistance));

    Vector3 botLeft = perspectiveCamera.ViewportToWorldPoint(new Vector3(0, 0, relativeDistance));
    Vector3 topRight = perspectiveCamera.ViewportToWorldPoint(new Vector3(1, 1, relativeDistance));

    Quaternion inverseRotation = Quaternion.Inverse(perspectiveCamera.transform.rotation); 
    botLeft = inverseRotation * botLeft;
    topRight = inverseRotation * topRight; 

    newQuadScale = (topRight - botLeft);
    if(newQuadScale.x <= 0)
      newQuadScale.x = 1;
    else if(newQuadScale.y <= 0)
      newQuadScale.y = 1;
    else if(newQuadScale.z <= 0)
      newQuadScale.z = 1;

    Vector3 newQuadRotationEuler = Quaternion.LookRotation(perspectiveCamera.transform.forward).eulerAngles;
    Vector3 perspectiveCameraRotationEuler = perspectiveCamera.transform.rotation.eulerAngles;
    newQuadRotation = Quaternion.Euler(newQuadRotationEuler.x, newQuadRotationEuler.y, perspectiveCameraRotationEuler.z);
    newQuadRotation = newQuadRotation * cachedQuaternion;
  }

  public void OnDrawGizmosSelected()
  {
    if(perspectiveCamera != null)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(cachedTopLeftBase, cachedTopRightBase);
      Gizmos.DrawLine(cachedTopRightBase, cachedBotRightBase);
      Gizmos.DrawLine(cachedBotRightBase, cachedBotLeftBase);
      Gizmos.DrawLine(cachedBotLeftBase, cachedTopLeftBase);

      Vector3 viewportPosition = perspectiveCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, relativeDistance));

      Gizmos.DrawLine(perspectiveCamera.transform.position, viewportPosition);

      Gizmos.color = Color.yellow;
      if(gridEnabled)
        DrawGrid();

      if(cachedQuaternionIsNonZero
        || viewportX != 0.5f
        || viewportY != 0.5f)
      {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(cachedTopLeft, cachedTopRight);
        Gizmos.DrawLine(cachedTopRight, cachedBotRight);
        Gizmos.DrawLine(cachedBotRight, cachedBotLeft);
        Gizmos.DrawLine(cachedBotLeft, cachedTopLeft);
      }

      Gizmos.color = Color.black;
      DrawCross();
    }
  }

  private void DrawCross()
  {
    Vector2 screenMid = perspectiveCamera.ViewportToScreenPoint(new Vector2(
      viewportX, viewportY));
    Vector2 crossOffset = pixelGridSize / 2;
    Vector3 crossTopLeft = perspectiveCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x - crossOffset.x,
      screenMid.y + crossOffset.y,
      relativeDistance));
    Vector3 crossBotRight = perspectiveCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x + crossOffset.x,
      screenMid.y - crossOffset.y,
      relativeDistance));
    Vector3 crossBotLeft = perspectiveCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x - crossOffset.x,
      screenMid.y - crossOffset.y,
      relativeDistance));
    Vector3 crossTopRight = perspectiveCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x + crossOffset.x,
      screenMid.y + crossOffset.y,
      relativeDistance));
    
    Gizmos.DrawLine(crossTopLeft, crossBotRight);
    Gizmos.DrawLine(crossBotLeft, crossTopRight);
  }

  private void DrawGrid()
  {
    UpdateScreenDimensions();

    if((int)pixelGridSize.x > 0)
    {
      float currentScreenXPosition = pixelGridSize.x;

      while(currentScreenXPosition < cachedScreenDimensions.x)
      {
        Vector3 botPosition = perspectiveCamera.ScreenToWorldPoint(new Vector3(
          currentScreenXPosition, 0f, relativeDistance));
        Vector3 topPosition = perspectiveCamera.ScreenToWorldPoint(new Vector3(
          currentScreenXPosition,
          cachedScreenDimensions.y,
          relativeDistance));
        Gizmos.DrawLine(
          botPosition,
          topPosition);

        currentScreenXPosition += pixelGridSize.x;
      }
    }

    if((int)pixelGridSize.y > 0)
    {
      float currentScreenYPosition = pixelGridSize.y;

      while(currentScreenYPosition < cachedScreenDimensions.y)
      {
        Vector3 leftPosition = perspectiveCamera.ScreenToWorldPoint(new Vector3(
          0f, currentScreenYPosition, relativeDistance));
        Vector3 rightPosition = perspectiveCamera.ScreenToWorldPoint(new Vector3(
          cachedScreenDimensions.x,
          currentScreenYPosition,
          relativeDistance));
        Gizmos.DrawLine(
          leftPosition,
          rightPosition);

        currentScreenYPosition += pixelGridSize.y;
      }
    }
  }
}
