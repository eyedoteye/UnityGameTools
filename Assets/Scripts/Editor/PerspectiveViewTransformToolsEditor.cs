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
  private const float minMaxTextFieldWidth = 70f;

  private float minRelativePosition = -1;
  private float maxRelativePosition = -1;

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

    EditorGUILayout.PropertyField(targetQuadProperty);
    EditorGUILayout.PropertyField(perspectiveCameraProperty);

    float minRelativePositionLimit = 0.001f;
    float maxRelativePositionLimit = 10.0f;

    if(perspectiveViewTransformTools.perspectiveCamera != null)
    {
      minRelativePositionLimit = perspectiveViewTransformTools.perspectiveCamera.nearClipPlane;
      maxRelativePositionLimit = perspectiveViewTransformTools.perspectiveCamera.farClipPlane;
      if(minRelativePosition == -1)
      {
        minRelativePosition = minRelativePositionLimit;
        maxRelativePosition = maxRelativePositionLimit;
      }
    }

    EditorGUILayout.Slider(
      relativePositionProperty,
      minRelativePosition, maxRelativePosition);

    EditorGUILayout.BeginHorizontal();

    EditorGUILayout.PrefixLabel("↑ Limits");

    minRelativePosition = EditorGUILayout.FloatField(minRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    minRelativePosition = minRelativePosition < minRelativePositionLimit ? minRelativePositionLimit : minRelativePosition;
    maxRelativePosition = EditorGUILayout.FloatField(maxRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    maxRelativePosition = maxRelativePosition > maxRelativePositionLimit ? maxRelativePositionLimit : maxRelativePosition;

    EditorGUILayout.MinMaxSlider(
      ref minRelativePosition, ref maxRelativePosition,
      minRelativePositionLimit, maxRelativePositionLimit);

    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
  
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Apply Transform", GUILayout.Width(applyButtonWidth)))
      ApplyTransforms();
  
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
