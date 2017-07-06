using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerspectiveViewTransformTools))]
[CanEditMultipleObjects]
public class PerspectiveViewTransformToolsEditor : Editor
{
  private PerspectiveViewTransformTools perspectiveViewTransformTools;

  //private Camera perspectiveCameraCached;

  // Using Serialized Properties allow you take advantage of Unity's undo capabilities.
  private SerializedProperty targetQuadProperty;
  private SerializedProperty perspectiveCameraProperty;
  private SerializedProperty relativePositionProperty;

  private const string targetQuadPropertyName = "targetQuad";
  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativePositionPropertyName = "relativePosition";

  private const float applyButtonWidth = 125f;

  private void OnEnable()
  {
    perspectiveViewTransformTools = (PerspectiveViewTransformTools)target; // Target is the object this script is attached to.

    // Note: look into where serializedObject is being instantiated
    targetQuadProperty = serializedObject.FindProperty(targetQuadPropertyName);
    perspectiveCameraProperty = serializedObject.FindProperty(perspectiveCameraPropertyName);
    relativePositionProperty = serializedObject.FindProperty(relativePositionPropertyName);
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    float minRelativePosition = 0.001f;
    float maxRelativePosition = 10.0f;

    if(perspectiveViewTransformTools.perspectiveCamera != null)
    {
      minRelativePosition = perspectiveViewTransformTools.perspectiveCamera.nearClipPlane;
      maxRelativePosition = perspectiveViewTransformTools.perspectiveCamera.farClipPlane;
    }

    EditorGUILayout.PropertyField(targetQuadProperty);
    EditorGUILayout.PropertyField(perspectiveCameraProperty);
    EditorGUILayout.Slider(
      relativePositionProperty,
      minRelativePosition, maxRelativePosition);

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Apply Transform", GUILayout.Width(applyButtonWidth)))
    {
      ApplyTransforms();
    }
    EditorGUILayout.EndHorizontal();

    serializedObject.ApplyModifiedProperties();
  }

  private void ApplyTransforms()
  {
    Vector3 newQuadPosition, newQuadScale;
    perspectiveViewTransformTools.ComputeTransforms(out newQuadPosition, out newQuadScale);

    Undo.RecordObject(perspectiveViewTransformTools.gameObject.transform, "Changed Position Transform");
    perspectiveViewTransformTools.gameObject.transform.position = newQuadPosition; 

    Undo.RecordObject(perspectiveViewTransformTools.gameObject.transform, "Changed LocalScale Transform");
    perspectiveViewTransformTools.gameObject.transform.localScale = newQuadScale; 
  }
}
