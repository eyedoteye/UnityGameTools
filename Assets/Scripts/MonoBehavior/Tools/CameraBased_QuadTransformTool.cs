using UnityEngine;

public class CameraBased_QuadTransformTool : MonoBehaviour {

  private class MeshVertex_State
  {
    public int index;

    // 3D Format
    public Vector3 vertex;

    // RelativeToCamera Format
    public Vector2 screenPosition;
    public float relativeDistance;
    public Camera targetCamera;

    public bool Apply_RelativeToCamera_Format()
    {
      if(targetCamera == null)
        return false;

      vertex = targetCamera.ScreenToWorldPoint(new Vector3(
        screenPosition.x,
        screenPosition.y,
        relativeDistance));

      return true;
    }
  }

  public Camera targetCamera;
  public GameObject targetObject;

  public Vector2 screenPosition;
  public float relativeDistance = 10f;

  public Vector2 pixelGridSize = new Vector2(16f, 16f);
  public bool gridEnabled = true;

  private Vector2 viewportPosition = new Vector2(0.5f, 0.5f);
  private MeshVertex_State meshVertex_States;

  private Mesh cached_QuadMesh;
  private Vector2 cached_ScreenDimensions;
  private Vector3 cachedGizmo_QuadBotLeft;
  private Vector3 cachedGizmo_QuadBotRight;
  private Vector3 cachedGizmo_QuadTopLeft;
  private Vector3 cachedGizmo_QuadTopRight;

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

  public bool GetMesh()
  {
    if(targetObject == null)
      return false;

    cached_QuadMesh = targetObject.GetComponent<MeshFilter>().mesh;

    if(cached_QuadMesh == null)
      return false;

    return true;
  }

  public void UpdateScreenDimensions()
  {
    if(targetCamera == null)
      return;

    cached_ScreenDimensions = targetCamera.ViewportToScreenPoint(
      new Vector3(1f, 1f));
  }

  public void UpdateGizmoCache()
  {
    if(targetCamera == null)
      return;

    cachedGizmo_QuadBotLeft = targetCamera.ViewportToWorldPoint(
      new Vector3(0, 0, relativeDistance));
    cachedGizmo_QuadBotRight = targetCamera.ViewportToWorldPoint(
      new Vector3(1, 0, relativeDistance));
    cachedGizmo_QuadTopLeft = targetCamera.ViewportToWorldPoint(
      new Vector3(0, 1, relativeDistance));
    cachedGizmo_QuadTopRight = targetCamera.ViewportToWorldPoint(
      new Vector3(1, 1, relativeDistance));
  }

  public void OnDrawGizmosSelected()
  {
    if(targetCamera != null)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(cachedGizmo_QuadTopLeft, cachedGizmo_QuadTopRight);
      Gizmos.DrawLine(cachedGizmo_QuadTopRight, cachedGizmo_QuadBotRight);
      Gizmos.DrawLine(cachedGizmo_QuadBotRight, cachedGizmo_QuadBotLeft);
      Gizmos.DrawLine(cachedGizmo_QuadBotLeft, cachedGizmo_QuadTopLeft);

      Gizmos.color = Color.yellow;
      if(gridEnabled)
        DrawGrid();

      Gizmos.color = Color.black;
      DrawCross();

      Vector3 crossMid_WorldCoords = targetCamera.ViewportToWorldPoint(new Vector3(
        viewportPosition.x,
        viewportPosition.y));
      Gizmos.DrawLine(targetCamera.transform.position, crossMid_WorldCoords);
    }
  }

  private void DrawCross()
  {
    Vector2 screenMid = targetCamera.ViewportToScreenPoint(new Vector2(
      viewportPosition.x, viewportPosition.y));
    Vector2 crossOffset = pixelGridSize / 2;
    Vector3 crossTopLeft = targetCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x - crossOffset.x,
      screenMid.y + crossOffset.y,
      relativeDistance));
    Vector3 crossBotRight = targetCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x + crossOffset.x,
      screenMid.y - crossOffset.y,
      relativeDistance));
    Vector3 crossBotLeft = targetCamera.ScreenToWorldPoint(new Vector3(
      screenMid.x - crossOffset.x,
      screenMid.y - crossOffset.y,
      relativeDistance));
    Vector3 crossTopRight = targetCamera.ScreenToWorldPoint(new Vector3(
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

      while(currentScreenXPosition < cached_ScreenDimensions.x)
      {
        Vector3 botPosition = targetCamera.ScreenToWorldPoint(new Vector3(
          currentScreenXPosition, 0f, relativeDistance));
        Vector3 topPosition = targetCamera.ScreenToWorldPoint(new Vector3(
          currentScreenXPosition,
          cached_ScreenDimensions.y,
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

      while(currentScreenYPosition < cached_ScreenDimensions.y)
      {
        Vector3 leftPosition = targetCamera.ScreenToWorldPoint(new Vector3(
          0f, currentScreenYPosition, relativeDistance));
        Vector3 rightPosition = targetCamera.ScreenToWorldPoint(new Vector3(
          cached_ScreenDimensions.x,
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
