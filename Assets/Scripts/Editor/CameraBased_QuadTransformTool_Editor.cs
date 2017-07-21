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


  private const string targetObjectPropertyName = "targetQuad";
  private const string targetCameraPropertyName = "targetCamera";

  private const string screenPositionPropertyName = "screenPosition"; 
  private const string relativeDistancePropertyName = "relativeDistance";

  private const string pixelGridSizePropertyName = "pixelGridSize";
  private const string gridEnabledPropertyName = "gridEnabled";


  private const float applyButtonWidth = 125f;
  private const float resetButtonWidth = 50f;
  private const float minMaxTextFieldWidth = 50f;


  private float minRelativePosition = -1;
  private float maxRelativePosition = -1;


  private bool shouldUpdateGizmoPositions = false;

  private void UpdateVectorCache()
  {
  }

  private void ApplyVectorCache()
  {
  }

  private void OnEnable()
  {
    cameraBased_QuadTransformTool = (CameraBased_QuadTransformTool)target;

    targetObjectProperty = serializedObject.FindProperty(targetObjectPropertyName);
    targetCameraProperty = serializedObject.FindProperty(targetCameraPropertyName);

    screenPositionProperty = serializedObject.FindProperty(screenPositionPropertyName);
    relativeDistanceProperty = serializedObject.FindProperty(relativeDistancePropertyName);

    pixelGridSizeProperty = serializedObject.FindProperty(pixelGridSizePropertyName);
    gridEnabledProperty = serializedObject.FindProperty(gridEnabledPropertyName);

    UpdateScreenDimensions();
  }

  private void UpdateScreenDimensions()
  {
    cameraBased_QuadTransformTool.UpdateScreenDimensions();
  }

  private void UpdateScreenPositions()
  {
  }

  private bool EndChangeCheck()
  {
    bool propertyHasChanged = EditorGUI.EndChangeCheck();

    return propertyHasChanged;
  }

  public override void OnInspectorGUI()
  {
    shouldUpdateGizmoPositions = false;

    serializedObject.Update();
    UpdateVectorCache();

    EditorGUILayout.PropertyField(targetObjectProperty);
    EditorGUILayout.PropertyField(targetCameraProperty);

    float minRelativePositionLimit = 0.001f;
    float maxRelativePositionLimit = 10.0f;

    if(cameraBased_QuadTransformTool.targetCamera != null)
    {
      minRelativePositionLimit = cameraBased_QuadTransformTool.targetCamera.nearClipPlane;
      maxRelativePositionLimit = cameraBased_QuadTransformTool.targetCamera.farClipPlane;
      if(minRelativePosition == -1)
      {
        minRelativePosition = minRelativePositionLimit;
        maxRelativePosition = maxRelativePositionLimit;
      }
    }

    EditorGUILayout.Slider(
      relativeDistanceProperty,
      minRelativePosition, maxRelativePosition);
    if(relativeDistanceProperty.floatValue != cameraBased_QuadTransformTool.relativeDistance)
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
        cameraBased_QuadTransformTool.targetCamera.ScreenToViewportPoint(screenPosition).x;
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
        cameraBased_QuadTransformTool.targetCamera.ScreenToViewportPoint(screenPosition).x;
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
        cameraBased_QuadTransformTool.targetCamera.ScreenToViewportPoint(screenPosition).y;
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
        cameraBased_QuadTransformTool.targetCamera.ScreenToViewportPoint(screenPosition).y;
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

    ApplyVectorCache();
    serializedObject.ApplyModifiedProperties();
    
    if(shouldUpdateGizmoPositions)
      cameraBased_QuadTransformTool.UpdateGizmoCache();
  }
}
