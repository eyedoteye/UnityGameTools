using UnityEngine;

public class CameraBased_QuadTransformTool : MonoBehaviour {

  public class MeshVertex
  {
    public GameObject targetObject;
    public int vertexIndex;

    // Note: Stored In Local Coordinates
    public Vector3 vertex;

    // Note: Strored In Screen Coordinates
    public struct RelativeToCamera
    {
      public Vector2 screenPosition;
      public float distance;
    } public RelativeToCamera relativeToCamera;

    public Camera targetCamera;

    public bool Compute_RelativeToCamera()
    {
      if(targetCamera == null)
        return false;

      Vector3 worldSpace = targetObject.transform.localToWorldMatrix.MultiplyPoint3x4(vertex);
      Vector3 screenSpace = targetCamera.WorldToScreenPoint(worldSpace);
      relativeToCamera.screenPosition = new Vector2(screenSpace.x, screenSpace.y);
      relativeToCamera.distance = screenSpace.z;

      return true;
    }

    public bool Apply_RelativeToCamera()
    {
      if(targetCamera == null)
        return false;

      Vector3 worldSpace = targetCamera.ScreenToWorldPoint(new Vector3(
        relativeToCamera.screenPosition.x,
        relativeToCamera.screenPosition.y,
        relativeToCamera.distance));

      vertex =
        targetObject.transform.worldToLocalMatrix.MultiplyPoint3x4(worldSpace);

      return true;
    }
  }

  public GameObject targetObject;
  public Camera targetCamera;

  public Vector2 screenPosition;
  public float relativeDistance = 10f;

  public Vector2 pixelGridSize = new Vector2(16f, 16f);
  public bool gridEnabled = true;

  public MeshVertex[] meshVertices;
  public Mesh targetMesh;
  private Vector2 viewportPosition = new Vector2(0.5f, 0.5f);

  private Vector2 cached_ScreenDimensions;
  private Vector3 cached_GizmoQuad_BotLeft;
  private Vector3 cached_GizmoQuad_BotRight;
  private Vector3 cached_GizmoQuad_TopLeft;
  private Vector3 cached_GizmoQuad_TopRight;

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

  public void ClearMesh()
  {
    targetMesh = null;
  }

  public bool GetMesh()
  {
    if(targetObject == null)
      return false;

    targetMesh = targetObject.GetComponent<MeshFilter>().mesh;

    if(targetMesh == null)
      return false;

    return true;
  }

  public void Apply_MeshVertices_To_TargetMesh()
  {
    Vector3[] vertices = new Vector3[meshVertices.Length];
    for(int vertexIndex = 0; vertexIndex < meshVertices.Length; ++vertexIndex)
    {
      vertices[vertexIndex] = meshVertices[vertexIndex].vertex;
    }
    targetMesh.vertices = vertices;
  }

  public bool Cache_Mesh_Into_MeshVertices()
  {
    if(targetMesh == null)
      return false;

    Vector3[] vertices = targetMesh.vertices;
    meshVertices = new MeshVertex[vertices.Length];
    for(int vertexIndex = 0; vertexIndex < targetMesh.vertices.Length; ++vertexIndex)
    {
      MeshVertex meshVertex = new MeshVertex();

      meshVertex.targetObject = targetObject;
      meshVertex.vertexIndex = vertexIndex;
      meshVertex.vertex = vertices[vertexIndex];
      meshVertex.targetCamera = targetCamera;
      meshVertex.Compute_RelativeToCamera();

      meshVertices[vertexIndex] = meshVertex;
    }

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

    cached_GizmoQuad_BotLeft = targetCamera.ViewportToWorldPoint(
      new Vector3(0, 0, relativeDistance));
    cached_GizmoQuad_BotRight = targetCamera.ViewportToWorldPoint(
      new Vector3(1, 0, relativeDistance));
    cached_GizmoQuad_TopLeft = targetCamera.ViewportToWorldPoint(
      new Vector3(0, 1, relativeDistance));
    cached_GizmoQuad_TopRight = targetCamera.ViewportToWorldPoint(
      new Vector3(1, 1, relativeDistance));
  }

  public void OnDrawGizmosSelected()
  {
    if(targetCamera != null)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawLine(cached_GizmoQuad_TopLeft, cached_GizmoQuad_TopRight);
      Gizmos.DrawLine(cached_GizmoQuad_TopRight, cached_GizmoQuad_BotRight);
      Gizmos.DrawLine(cached_GizmoQuad_BotRight, cached_GizmoQuad_BotLeft);
      Gizmos.DrawLine(cached_GizmoQuad_BotLeft, cached_GizmoQuad_TopLeft);

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
