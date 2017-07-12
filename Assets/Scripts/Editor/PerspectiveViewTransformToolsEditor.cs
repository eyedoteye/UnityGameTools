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
  private SerializedProperty relativeDistanceProperty;
  private SerializedProperty viewportXProperty;
  private SerializedProperty viewportYProperty;
  private SerializedProperty relativeRotationXProperty;
  private SerializedProperty relativeRotationYProperty;
  private SerializedProperty relativeRotationZProperty;

  private const string targetQuadPropertyName = "targetQuad";
  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativeDistancePropertyName = "relativeDistance";
  private const string viewportXPropertyName = "viewportX";
  private const string viewportYPropertyName = "viewportY";
  private const string relativeRotationXPropertyName = "relativeRotationX";
  private const string relativeRotationYPropertyName = "relativeRotationY";
  private const string relativeRotationZPropertyName = "relativeRotationZ";

  private const float applyButtonWidth = 125f;
  private const float resetButtonWidth = 50f;
  private const float minMaxTextFieldWidth = 70f;

  private float minRelativePosition = -1;
  private float maxRelativePosition = -1;

  // Non Undo-able (Keeping these in the undo is just a waste of undo)
  bool shouldApplyScale = true;
  bool shouldApplyRotation = true;
  bool shouldApplyPosition = true;

  // Cache
  private bool isCached = false;
  Quaternion cachedNewQuadRotation;
  Vector3 cachedNewQuadPosition;
  Vector3 cachedNewQuadScale;

  private void OnEnable()
  {
    perspectiveViewTransformTools = (PerspectiveViewTransformTools)target; // Target is the object this script is attached to.

    // Note: look into where serializedObject is being instantiated
    targetQuadProperty = serializedObject.FindProperty(targetQuadPropertyName);
    perspectiveCameraProperty = serializedObject.FindProperty(perspectiveCameraPropertyName);
    relativeDistanceProperty = serializedObject.FindProperty(relativeDistancePropertyName);
    viewportXProperty = serializedObject.FindProperty(viewportXPropertyName);
    viewportYProperty = serializedObject.FindProperty(viewportYPropertyName);
    relativeRotationXProperty = serializedObject.FindProperty(relativeRotationXPropertyName);
    relativeRotationYProperty = serializedObject.FindProperty(relativeRotationYPropertyName);
    relativeRotationZProperty = serializedObject.FindProperty(relativeRotationZPropertyName);
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUI.BeginChangeCheck();

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
      relativeDistanceProperty,
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

    EditorGUILayout.LabelField("Viewport Position");

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.Slider(viewportXProperty, 0, 1, "↑ X");
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
      viewportXProperty.floatValue = 0.5f;
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.Slider(viewportYProperty, 0, 1, "↑ Y");
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
      viewportYProperty.floatValue = 0.5f;
    EditorGUILayout.EndHorizontal();

    Vector3 localRotation;
    localRotation.x = relativeRotationXProperty.floatValue;
    localRotation.y = relativeRotationYProperty.floatValue;
    localRotation.z = relativeRotationZProperty.floatValue;

    localRotation = EditorGUILayout.Vector3Field("Relative Rotation", localRotation);
    if(localRotation.x != relativeRotationXProperty.floatValue ||
       localRotation.y != relativeRotationYProperty.floatValue ||
       localRotation.z != relativeRotationZProperty.floatValue ||
       perspectiveViewTransformTools.transform.hasChanged)
    {
      relativeRotationXProperty.floatValue = localRotation.x;
      relativeRotationYProperty.floatValue = localRotation.y;
      relativeRotationZProperty.floatValue = localRotation.z;
      perspectiveViewTransformTools.UpdateQuaternionWithEuler(
        localRotation.x,
        localRotation.y,
        localRotation.z);
    }
    
    if(EditorGUI.EndChangeCheck() || perspectiveViewTransformTools.transform.hasChanged)
      isCached = false;

    shouldApplyPosition = EditorGUILayout.ToggleLeft("Position", shouldApplyPosition);
    shouldApplyRotation = EditorGUILayout.ToggleLeft("Rotation", shouldApplyRotation);
    shouldApplyScale = EditorGUILayout.ToggleLeft("Scale", shouldApplyScale);

    EditorGUILayout.BeginHorizontal();
  
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Apply Transform", GUILayout.Width(applyButtonWidth)))
    {
      if(shouldApplyPosition)
        ApplyPosition();

      if(shouldApplyScale)
        ApplyScale();

      if(shouldApplyRotation)
        ApplyRotation();
    }
  
    EditorGUILayout.EndHorizontal();

    serializedObject.ApplyModifiedProperties();
  }

  private void ApplyScale()
  {
    if(!isCached)
    {
      perspectiveViewTransformTools.ComputeTransforms(out cachedNewQuadPosition, out cachedNewQuadScale, out cachedNewQuadRotation);
      isCached = true;
      perspectiveViewTransformTools.transform.hasChanged = false;
    }

    Undo.RecordObject(perspectiveViewTransformTools.targetQuad.transform, "Changed Transform LocalScale");
    perspectiveViewTransformTools.targetQuad.transform.localScale = cachedNewQuadScale; 
  }

  private void ApplyPosition()
  {
    if(!isCached)
    {
      perspectiveViewTransformTools.ComputeTransforms(out cachedNewQuadPosition, out cachedNewQuadScale, out cachedNewQuadRotation);
      isCached = true;
      perspectiveViewTransformTools.transform.hasChanged = false;
    }

    Undo.RecordObject(perspectiveViewTransformTools.targetQuad.transform, "Changed Transform Position");
    perspectiveViewTransformTools.targetQuad.transform.position = cachedNewQuadPosition; 
  }

  private void ApplyRotation()
  {
    if(!isCached)
    {
      perspectiveViewTransformTools.ComputeTransforms(out cachedNewQuadPosition, out cachedNewQuadScale, out cachedNewQuadRotation);
      isCached = true;
      perspectiveViewTransformTools.transform.hasChanged = false;
    }

    Undo.RecordObject(perspectiveViewTransformTools.targetQuad.transform, "Changed Transform Rotation");
    perspectiveViewTransformTools.targetQuad.transform.rotation = cachedNewQuadRotation; 
  }
}
