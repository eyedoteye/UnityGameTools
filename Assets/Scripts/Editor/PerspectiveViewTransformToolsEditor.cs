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
  private SerializedProperty viewportPositionProperty;
  private SerializedProperty relativeRotationXProperty;
  private SerializedProperty relativeRotationYProperty;
  private SerializedProperty relativeRotationZProperty;
  private SerializedProperty pixelGridSizeProperty;
  private SerializedProperty gridEnabledProperty;

  private const string targetQuadPropertyName = "targetQuad";
  private const string perspectiveCameraPropertyName = "perspectiveCamera";
  private const string relativeDistancePropertyName = "relativeDistance";
  private const string viewportPositionPropertyName = "viewportPosition";
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
  private Quaternion cachedNewQuadRotation;
  private Vector3 cachedNewQuadPosition;
  private Vector3 cachedNewQuadScale;
  private float cachedViewportXPosition;
  private float cachedViewportYPosition;

  // Screen Info
  Vector2 screenDimensions;

  // Alternative Input
  Vector2 screenPosition;
  Vector2 gridPosition;

  private void UpdateVectorCache()
  {
    cachedViewportXPosition = viewportPositionProperty.vector2Value.x;
    cachedViewportYPosition = viewportPositionProperty.vector2Value.y;
  }

  private void ApplyVectorCache()
  {
    viewportPositionProperty.vector2Value = new Vector2(
      cachedViewportXPosition,
      cachedViewportYPosition);
  }

  private void OnEnable()
  {
    perspectiveViewTransformTools = (PerspectiveViewTransformTools)target; // Target is the object this script is attached to.

    // Note: look into where serializedObject is being instantiated
    targetQuadProperty = serializedObject.FindProperty(targetQuadPropertyName);
    perspectiveCameraProperty = serializedObject.FindProperty(perspectiveCameraPropertyName);
    relativeDistanceProperty = serializedObject.FindProperty(relativeDistancePropertyName);
    viewportPositionProperty = serializedObject.FindProperty(viewportPositionPropertyName);
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
    {
      screenPosition = perspectiveViewTransformTools.perspectiveCamera.
        ViewportToScreenPoint(new Vector3(
          cachedViewportXPosition,
          cachedViewportYPosition));
      gridPosition.x = screenPosition.x / pixelGridSizeProperty.vector2Value.x;
      gridPosition.y = screenPosition.y / pixelGridSizeProperty.vector2Value.y;
    }
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
    UpdateVectorCache();

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
    cachedViewportXPosition = EditorGUILayout.Slider(
      "↑ X", cachedViewportXPosition, 0, 1);
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
      cachedViewportXPosition =
        perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).x;
    if(gridEnabledProperty.boolValue)
    {
      gridPosition.x = EditorGUILayout.FloatField(gridPosition.x);
      if(gridPosition.x < 0)
        gridPosition.x = 0;
      float maxGridPosition = screenDimensions.x / pixelGridSizeProperty.vector2Value.x;
      if(gridPosition.x > maxGridPosition)
        gridPosition.x = maxGridPosition;
      screenPosition.x = gridPosition.x * pixelGridSizeProperty.vector2Value.x;
      cachedViewportXPosition =
        perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).x;
    }
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
    {
      cachedViewportXPosition = 0.5f;
      shouldUpdateGizmoPositions = true;
      UpdateScreenPositions();
    }
    EditorGUILayout.EndHorizontal();

    EndChangeCheck();
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.BeginHorizontal();
    cachedViewportYPosition = EditorGUILayout.Slider(
      "↑ Y", cachedViewportYPosition, 0, 1);
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
      cachedViewportYPosition =
        perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).y;
    if(gridEnabledProperty.boolValue)
    {
      gridPosition.y = EditorGUILayout.FloatField(gridPosition.y);
      if(gridPosition.y < 0)
        gridPosition.y = 0;
      float maxGridPosition = screenDimensions.y / pixelGridSizeProperty.vector2Value.y;
      if(gridPosition.y > maxGridPosition)
        gridPosition.y = maxGridPosition;
      screenPosition.y = gridPosition.y * pixelGridSizeProperty.vector2Value.y;
      cachedViewportYPosition =
        perspectiveViewTransformTools.perspectiveCamera.ScreenToViewportPoint(screenPosition).y;
    }
    if(GUILayout.Button("Reset", GUILayout.Width(resetButtonWidth)))
    {
      cachedViewportYPosition = 0.5f;
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

    ApplyVectorCache();
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
