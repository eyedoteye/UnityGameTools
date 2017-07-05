using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerspectiveViewTransformTools))]
[CanEditMultipleObjects]
public class PerspectiveViewTransformToolsEditor : Editor
{
  private PerspectiveViewTransformTools perspectiveViewTransformTools;

  //private Camera perspectiveCameraCached;

  // Using Serialized Properties allow you take advantage of Unity's undo capabilities.
  private SerializedProperty perspectiveCameraProperty;
  private SerializedProperty relativePositionProperty;

  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativePositionPropertyName = "relativePosition";

  private const float applyButtonWidth = 125f;

  private void OnEnable()
  {
    perspectiveViewTransformTools = (PerspectiveViewTransformTools)target; // Target is the object this script is attached to.

    // Note: look into where serializedObject is being instantiated
    perspectiveCameraProperty = serializedObject.FindProperty(perspectiveCameraPropertyName);
    relativePositionProperty = serializedObject.FindProperty(relativePositionPropertyName);

    Debug.Log(perspectiveCameraProperty);
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

    EditorGUILayout.PropertyField(perspectiveCameraProperty);
    EditorGUILayout.Slider(
      relativePositionProperty,
      minRelativePosition, maxRelativePosition);

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Apply Transform", GUILayout.Width(applyButtonWidth)))
    {
      
    }
    EditorGUILayout.EndHorizontal();

    serializedObject.ApplyModifiedProperties();
  }
}
