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
      meshVertex.Compute_RelativeToCamera();
      CameraBased_QuadTransformTool.MeshVertex.RelativeToCamera relativeToCamera =
        meshVertex.relativeToCamera;

      EditorGUILayout.LabelField("Vertex " + vertexIndex);

      relativeToCamera.screenPosition = EditorGUILayout.Vector2Field(
        " ↑ Grid Position",
        relativeToCamera.screenPosition);

      relativeToCamera.distance = EditorGUILayout.FloatField(
        " ↑ Distance From Camera",
        relativeToCamera.distance);


      CameraBased_QuadTransformTool.MeshVertex.RelativeToCamera oldRelativeToCamera =
        meshVertex.relativeToCamera;

      if(
        oldRelativeToCamera.screenPosition != relativeToCamera.screenPosition ||
        oldRelativeToCamera.distance != relativeToCamera.distance)
      {
        cameraBased_QuadTransformTool.meshVertices[vertexIndex].relativeToCamera =
          relativeToCamera;

        meshVertex.Apply_RelativeToCamera();
      }
    }
  }

  public override void OnInspectorGUI()
  {
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

    Build_MeshVertices_Editors();

    ApplyVectorCache();
    serializedObject.ApplyModifiedProperties();
    
    if(cameraBased_QuadTransformTool.targetMesh != null)
    {
      Undo.RecordObject(
        cameraBased_QuadTransformTool.targetMesh,
        "Transform Mesh Vertex");
      cameraBased_QuadTransformTool.Apply_MeshVertices_To_TargetMesh();
    }
  }
}
