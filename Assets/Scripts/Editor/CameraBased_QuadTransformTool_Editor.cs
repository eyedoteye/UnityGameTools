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


  private void OnEnable()
  {
    cameraBased_QuadTransformTool = (CameraBased_QuadTransformTool)target;

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

  public override void OnInspectorGUI()
  {
    serializedObject.Update();
    UpdateVectorCache();

    EditorGUILayout.PropertyField(targetObjectProperty);
    EditorGUILayout.PropertyField(targetCameraProperty);

    //float minRelativePositionLimit = 0.001f;
    //float maxRelativePositionLimit = 10.0f;

    //if(cameraBased_QuadTransformTool.targetCamera != null)
    //{
    //  minRelativePositionLimit = cameraBased_QuadTransformTool.targetCamera.nearClipPlane;
    //  maxRelativePositionLimit = cameraBased_QuadTransformTool.targetCamera.farClipPlane;
    //  if(minRelativePosition == -1)
    //  {
    //    minRelativePosition = minRelativePositionLimit;
    //    maxRelativePosition = maxRelativePositionLimit;
    //  }
    //}

    //EditorGUILayout.Slider(
    //  relativeDistanceProperty,
    //  minRelativePosition, maxRelativePosition);
    //if(relativeDistanceProperty.floatValue != cameraBased_QuadTransformTool.relativeDistance)
    //  shouldUpdateGizmoPositions = true;

    //EditorGUILayout.BeginHorizontal();
    //EditorGUILayout.PrefixLabel("↑ Limits");
    //minRelativePosition = EditorGUILayout.FloatField(
    //  minRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    //minRelativePosition = minRelativePosition < minRelativePositionLimit ?
    //  minRelativePositionLimit : minRelativePosition;
    //EditorGUILayout.MinMaxSlider(
    //  ref minRelativePosition, ref maxRelativePosition,
    //  minRelativePositionLimit, maxRelativePositionLimit);
    //maxRelativePosition = EditorGUILayout.FloatField(
    //  maxRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    //maxRelativePosition = maxRelativePosition > maxRelativePositionLimit ?
    //  maxRelativePositionLimit : maxRelativePosition;
    //EditorGUILayout.EndHorizontal();

    ApplyVectorCache();
    serializedObject.ApplyModifiedProperties();
  }
}
