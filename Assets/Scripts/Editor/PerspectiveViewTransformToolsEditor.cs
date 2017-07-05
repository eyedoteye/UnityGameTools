using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerspectiveViewTransformTools))]
[CanEditMultipleObjects]
public class PerspectiveViewTransformToolsEditor : Editor
{
  //private GameObject quad;

  //private Camera perspectiveCameraCached;

  // Using Serialized Properties allow you take advantage of Unity's undo capabilities.
  private SerializedProperty perspectiveCameraProperty;
  private SerializedProperty relativePositionProperty;

  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativePositionPropertyName = "relativePosition";

  private const float applyButtonWidth = 125f;

  private void OnEnable()
  {
    //quad = (GameObject)target; // Target is the object this script is attached to.

    // Note: look into where serializedObject is being instantiated
    perspectiveCameraProperty = serializedObject.FindProperty(perspectiveCameraPropertyName);
    relativePositionProperty = serializedObject.FindProperty(relativePositionPropertyName);

    Debug.Log(perspectiveCameraProperty);
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUILayout.PropertyField(perspectiveCameraProperty);
    EditorGUILayout.Slider(relativePositionProperty, 0, 10);

    EditorGUILayout.BeginHorizontal();
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Apply Transform", GUILayout.Width(applyButtonWidth)))
    {
      Debug.Log("Bakakaboo");
    }
    EditorGUILayout.EndHorizontal();

    serializedObject.ApplyModifiedProperties();
  }
}
