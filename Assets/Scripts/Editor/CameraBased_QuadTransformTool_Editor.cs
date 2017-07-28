using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraBased_QuadTransformTool))]
public class CameraBased_QuadTransformTool_Editor : Editor
{
  private CameraBased_QuadTransformTool cameraBased_QuadTransformTool;


  private SerializedProperty targetObjectProperty;
  private SerializedProperty targetCameraProperty;

  private SerializedProperty screenPositionProperty;
  private SerializedProperty relativeDistanceProperty;

  private SerializedProperty pixelGridSizeProperty;
  private SerializedProperty gridEnabledProperty;


  private const string targetObjectPropertyName = "targetObject";
  private const string targetCameraPropertyName = "targetCamera";

  private const string screenPositionPropertyName = "screenPosition"; 
  private const string relativeDistancePropertyName = "relativeDistance";

  private const string pixelGridSizePropertyName = "pixelGridSize";
  private const string gridEnabledPropertyName = "gridEnabled";

  private const float resetButtonWidth = 50f;
  private const float recenterButtonWidth = 90f;

  private bool isFirstAttach = true;
  private void OnFirstAttach()
  {
    if(isFirstAttach)
    {
      if(cameraBased_QuadTransformTool.targetCamera == null)
      {
        Camera targetCamera = cameraBased_QuadTransformTool.GetComponentInParent<Camera>();
        if(targetCamera != null)
          cameraBased_QuadTransformTool.targetCamera = targetCamera;
      }
    }
    isFirstAttach = false;
  }

  public void UndoMorph()
  {
    if(Event.current.commandName.Equals("UndoRedoPerformed"))
    {
      cameraBased_QuadTransformTool.Cache_Mesh_Into_MeshVertices();
      cameraBased_QuadTransformTool.Apply_MeshVertices_To_TargetMesh();
    }
  }

  private void OnEnable()
  {
    cameraBased_QuadTransformTool = (CameraBased_QuadTransformTool)target;
    OnFirstAttach();

    targetObjectProperty = serializedObject.FindProperty(targetObjectPropertyName);
    targetCameraProperty = serializedObject.FindProperty(targetCameraPropertyName);

    screenPositionProperty = serializedObject.FindProperty(screenPositionPropertyName);
    relativeDistanceProperty = serializedObject.FindProperty(relativeDistancePropertyName);

    pixelGridSizeProperty = serializedObject.FindProperty(pixelGridSizePropertyName);
    gridEnabledProperty = serializedObject.FindProperty(gridEnabledPropertyName);
  }

  private void UpdateVectorCache()
  {
  }

  private void ApplyVectorCache()
  {
  }

  bool meshMorphed = false;
  public void Build_MeshVertices_Editors()
  {
    if(cameraBased_QuadTransformTool.targetMesh == null)
      return;
    if(cameraBased_QuadTransformTool.meshVertices == null)
      return;
    
    for(
      int vertexIndex = 0;
      vertexIndex < cameraBased_QuadTransformTool.meshVertices.Length;
      ++vertexIndex)
    {
      CameraBased_QuadTransformTool.MeshVertex meshVertex =
        cameraBased_QuadTransformTool.meshVertices[vertexIndex];
      CameraBased_QuadTransformTool.MeshVertex.RelativeToCamera relativeToCamera =
        meshVertex.relativeToCamera;

      EditorGUILayout.LabelField("Vertex " + vertexIndex);

      Vector2 pixelGridSize = pixelGridSizeProperty.vector2Value;
      Vector2 gridPosition = relativeToCamera.screenPosition;
      gridPosition.x /= pixelGridSize.x;
      gridPosition.y /= pixelGridSize.y;

      gridPosition = EditorGUILayout.Vector2Field(
        " ↑ Grid Position",
        gridPosition);

      relativeToCamera.distance = EditorGUILayout.FloatField(
        " ↑ Distance From Camera",
        relativeToCamera.distance);

      relativeToCamera.screenPosition.x = gridPosition.x * pixelGridSize.x;
      relativeToCamera.screenPosition.y = gridPosition.y * pixelGridSize.y;

      CameraBased_QuadTransformTool.MeshVertex.RelativeToCamera oldRelativeToCamera =
        meshVertex.relativeToCamera;

      if(
        oldRelativeToCamera.screenPosition != relativeToCamera.screenPosition ||
        oldRelativeToCamera.distance != relativeToCamera.distance)
      {
        meshMorphed = true;
        cameraBased_QuadTransformTool.meshVertices[vertexIndex].relativeToCamera =
          relativeToCamera;

        meshVertex.Apply_RelativeToCamera();
      }
    }
  }

  public override void OnInspectorGUI()
  {
    UndoMorph();

    serializedObject.Update();
    UpdateVectorCache();

    if(
      cameraBased_QuadTransformTool.targetMesh != null
      && !cameraBased_QuadTransformTool.IsMeshInstantiated())
    {
      cameraBased_QuadTransformTool.GetMesh();
      cameraBased_QuadTransformTool.Cache_Mesh_Into_MeshVertices();
    }

    EditorGUILayout.PropertyField(targetObjectProperty);
    if(targetObjectProperty.objectReferenceValue != cameraBased_QuadTransformTool.targetObject)
    {
      serializedObject.ApplyModifiedProperties();

      if(cameraBased_QuadTransformTool.targetObject == null)
        cameraBased_QuadTransformTool.ClearMesh();
      else
      {
        cameraBased_QuadTransformTool.GetMesh();
        cameraBased_QuadTransformTool.Cache_Mesh_Into_MeshVertices();
      }
    }
    EditorGUILayout.PropertyField(targetCameraProperty);
    EditorGUILayout.PropertyField(pixelGridSizeProperty);

    if(!(targetObjectProperty.objectReferenceValue == null))
    {
      GameObject targetObject = cameraBased_QuadTransformTool.targetObject;

      Vector3 targetObjectPosition = 
        cameraBased_QuadTransformTool.targetCamera.WorldToScreenPoint(
        cameraBased_QuadTransformTool.targetObject.transform.position);
      Vector2 pixelGridSize = pixelGridSizeProperty.vector2Value;

      Vector2 targetObjectGridPosition = targetObjectPosition;
      targetObjectGridPosition.x /= pixelGridSize.x;
      targetObjectGridPosition.y /= pixelGridSize.y;

      Vector2 newTargetObjectGridPosition = EditorGUILayout.Vector2Field(
        "Object Grid Position",
        targetObjectGridPosition);
      float newTargetRelativeDistance = EditorGUILayout.FloatField(
        "Object Relative Distance",
        targetObjectPosition.z);

      Vector3 newTargetObjectPosition;

      newTargetObjectPosition.x = newTargetObjectGridPosition.x * pixelGridSize.x;
      newTargetObjectPosition.y = newTargetObjectGridPosition.y * pixelGridSize.y;
      newTargetObjectPosition.z = newTargetRelativeDistance;
      newTargetObjectPosition =
        cameraBased_QuadTransformTool.targetCamera.ScreenToWorldPoint(
        newTargetObjectPosition);

      if(Vector3.Distance(newTargetObjectPosition,
        cameraBased_QuadTransformTool.targetObject.transform.position)
        > 0.001f)
      {
        Undo.RecordObject(
          cameraBased_QuadTransformTool.targetObject.transform,
          "Transform Object Position");
        cameraBased_QuadTransformTool.targetObject.transform.position
          = newTargetObjectPosition;
        cameraBased_QuadTransformTool.GetMesh();
        cameraBased_QuadTransformTool.Cache_Mesh_Into_MeshVertices();
      }
    }
    Build_MeshVertices_Editors();

    ApplyVectorCache();
    serializedObject.ApplyModifiedProperties();
    
    if(meshMorphed)
    {
      Undo.RecordObject(
        cameraBased_QuadTransformTool.targetMesh,
        "Transform Mesh Vertex");
      cameraBased_QuadTransformTool.Apply_MeshVertices_To_TargetMesh();
      meshMorphed = false;
    }
  }
}
