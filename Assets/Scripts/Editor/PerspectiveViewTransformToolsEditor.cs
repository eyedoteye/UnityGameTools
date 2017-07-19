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
  private SerializedProperty pixelGridSizeProperty;
  private SerializedProperty gridEnabledProperty;

  private const string targetQuadPropertyName = "targetQuad";
  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativeDistancePropertyName = "relativeDistance";
  private const string viewportXPropertyName = "viewportX";
  private const string viewportYPropertyName = "viewportY";
  private const string relativeRotationXPropertyName = "relativeRotationX";
  private const string relativeRotationYPropertyName = "relativeRotationY";
  private const string relativeRotationZPropertyName = "relativeRotationZ";
  private const string pixelGridSizePropertyName = "pixelGridSize";
  private const string gridEnabledPropertyName = "gridEnabled";

  private const float applyButtonWidth = 125f;
  private const float resetButtonWidth = 50f;
  private const float minMaxTextFieldWidth = 50f;

  private float minRelativePosition = -1;
  private float maxRelativePosition = -1;

  // Non Undo-able (Keeping these in the undo is just a waste of undo)
  bool shouldApplyScale = true;
  bool shouldApplyRotation = true;
  bool shouldApplyPosition = true;

  // Cache
  private bool isCached = false;
  private bool shouldUpdateGizmoPositions = false;
  Quaternion cachedNewQuadRotation;
  Vector3 cachedNewQuadPosition;
  Vector3 cachedNewQuadScale;

  // Screen Info
  Vector2 screenDimensions;

  // Alternative Input
  Vector2 screenPosition;

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
    pixelGridSizeProperty = serializedObject.FindProperty(pixelGridSizePropertyName);
    gridEnabledProperty = serializedObject.FindProperty(gridEnabledPropertyName);

    UpdateScreenDimensions();
  }

  private void UpdateScreenDimensions()
  {
    if(perspectiveViewTransformTools.perspectiveCamera != null)
      screenDimensions = perspectiveViewTransformTools.perspectiveCamera.
        ViewportToScreenPoint(new Vector3(1f, 1f, 0f));

    perspectiveViewTransformTools.UpdateScreenDimensions();
  }

  private void UpdateScreenPositions()
  {
    if(perspectiveViewTransformTools.perspectiveCamera != null)
      screenPosition = perspectiveViewTransformTools.perspectiveCamera.
        ViewportToScreenPoint(new Vector3(viewportXProperty.floatValue,
                                           viewportYProperty.floatValue));
  }

  private bool EndChangeCheck()
  {
    bool propertyHasChanged = EditorGUI.EndChangeCheck();

    if(propertyHasChanged)
      isCached = false;

    return propertyHasChanged;
  }

  public override void OnInspectorGUI()
  {
    shouldUpdateGizmoPositions = false;
    serializedObject.Update();

    EditorGUI.BeginChangeCheck();

    EditorGUILayout.PropertyField(targetQuadProperty);

    EditorGUILayout.PropertyField(perspectiveCameraProperty);
    if(perspectiveCameraProperty.objectReferenceValue
      != perspectiveViewTransformTools.perspectiveCamera)
    {
      serializedObject.ApplyModifiedProperties();
      UpdateScreenDimensions();
      UpdateScreenPositions();
    }

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
    if(relativeDistanceProperty.floatValue != perspectiveViewTransformTools.relativeDistance)
      shouldUpdateGizmoPositions = true;

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel("↑ Limits");
    minRelativePosition = EditorGUILayout.FloatField(
      minRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    minRelativePosition = minRelativePosition < minRelativePositionLimit ?
      minRelativePositionLimit : minRelativePosition;
    EditorGUILayout.MinMaxSlider(
      ref minRelativePosition, ref maxRelativePosition,
      minRelativePositionLimit, maxRelativePositionLimit);
    maxRelativePosition = EditorGUILayout.FloatField(
      maxRelativePosition, GUILayout.Width(minMaxTextFieldWidth));
    maxRelativePosition = maxRelativePosition > maxRelativePositionLimit ?
      maxRelativePositionLimit : maxRelativePosition;
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.LabelField("Viewport Position");

    EndChangeCheck();
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.Slider(viewportXProperty, 0, 1, "↑ X");
    EditorGUILayout.EndHorizontal();
    if(EndChangeCheck())
    {
      shouldUpdateGizmoPositions = true;
      UpdateScreenPositions();
      UpdateScreenDimensions();
    }
    EditorGUI.BeginChangeCheck();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel(" ↑ [ 0 , " + screenDimensions.x + " ]");
    float oldScreenXPositionValue = screenPosition.x;
    screenPosition.x = EditorGUILayout.FloatField(screenPosition.x);
    if(screenPosition.x < 0)
      screenPosition.x = 0;
    if(screenPosition.x > screenDimensions.x)
      screenPosition.x = screenDimensions.x;
    if(screenPosition.x != oldScreenXPositionValue)
      viewportXProperty.floatValue =
        perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).x;
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
    {
      viewportXProperty.floatValue = 0.5f;
      shouldUpdateGizmoPositions = true;
      UpdateScreenPositions();
    }
    EditorGUILayout.EndHorizontal();

    EndChangeCheck();
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.Slider(viewportYProperty, 0, 1, "↑ Y");
    EditorGUILayout.EndHorizontal();
    if(EndChangeCheck())
    {
      shouldUpdateGizmoPositions = true;
      UpdateScreenPositions();
      UpdateScreenDimensions();
    }
    EditorGUI.BeginChangeCheck();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PrefixLabel(" ↑ [ 0 , " + screenDimensions.y + " ]");
    float oldScreenYPositionValue = screenPosition.y;
    screenPosition.y = EditorGUILayout.FloatField(screenPosition.y);
    if(screenPosition.y < 0)
      screenPosition.y = 0;
    if(screenPosition.y > screenDimensions.y)
      screenPosition.y = screenDimensions.y;
    if(screenPosition.y != oldScreenYPositionValue)
      viewportYProperty.floatValue = perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).y;
    GUILayout.FlexibleSpace();
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
    {
      viewportYProperty.floatValue = 0.5f;
      shouldUpdateGizmoPositions = true;
      UpdateScreenPositions();
    }
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(gridEnabledProperty);
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.PropertyField(pixelGridSizeProperty);
    if(pixelGridSizeProperty.vector2Value.x < 0)
      pixelGridSizeProperty.vector2Value.Set(
        0f,
        pixelGridSizeProperty.vector2Value.y);
    if(pixelGridSizeProperty.vector2Value.y < 0)
      pixelGridSizeProperty.vector2Value.Set(
        pixelGridSizeProperty.vector2Value.x,
        0f);
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
    
    if(shouldUpdateGizmoPositions)
      perspectiveViewTransformTools.UpdateGizmoCache();
  }

  private void ApplyScale()
  {
    if(!isCached)
    {
      perspectiveViewTransformTools.ComputeTransforms(out cachedNewQuadPosition, out cachedNewQuadScale, out cachedNewQuadRotation);
      isCached = true;
      perspectiveViewTransformTools.transform.hasChanged = false;
    }

    Undo.RecordObject(perspectiveViewTransformTools.targetQuad.transform,
      "Changed Transform LocalScale");
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
