
using UnityEngine;
using UnityEditor;

namespace ProceduralShapes.Editor {

   [CustomEditor(typeof(ProceduralRectangle))]
   [CanEditMultipleObjects]

   public class ProcRectangleEditor : UnityEditor.Editor {

      public static bool plotDiagonals = true;
      public static bool onePlotColor = true;
      public static Color plotColor = Color.white;
      public static Color[] plotColors = new Color[] { Color.red, Color.blue };
      public static Color plotBackColor = Color.gray;
      public static EncodeType plotEncode = EncodeType.PNG;
      public static PlotSize plotSize = PlotSize._1K;

      public static bool tools = false;
      public static bool debug = false;

      public static bool enabledDebug = false;
      public static Color debugColor = new Color(1f, 0.4f, 1f);

      public static bool enabledHandles = true;
      public static int activeHandle = 0;
      public static GUIStyle areaStyle = null;
      public static GUIStyle buttonStyle = null;
      public static GUIStyle labelStyle = null;

      public override void OnInspectorGUI() {
         ProceduralRectangle rectangle = (ProceduralRectangle)target;

         DoRadiusField(rectangle);
         DoBorderField(rectangle);
         DoRoundnessField(rectangle);
         DoResolutionField(rectangle);
         EditorGUILayout.Space();

         DoSkipMainToggle(rectangle);
         DoSkipBorderToggle(rectangle);
         EditorGUILayout.Space();

         DoToolsFoldout(rectangle);
         EditorGUILayout.Space();

         DoDebugFoldout(rectangle);
         EditorGUILayout.Space();
      }

      public void OnSceneGUI() {
         ProceduralRectangle rectangle = (ProceduralRectangle)target;

         if (enabledDebug)
            DoVerticesDebug(rectangle);

         if (enabledHandles) {
            if (activeHandle == 0)
               DoSizeHandles(rectangle);
            else if (activeHandle == 1)
               DoBorderHandles(rectangle);
            else if (activeHandle == 2)
               DoRoundnessHandles(rectangle);

            DoHandleButtons(rectangle);
         }
      }

      #region # Inspector GUI Methods
      private void DoRadiusField(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         Vector2 value = EditorGUILayout.Vector2Field("Size", rectangle.size);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Size Change");
            rectangle.size = value;
            rectangle.CreateMesh();
         }
      }
      private void DoBorderField(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Border", rectangle.border);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Border Change");
            rectangle.border = value;
            rectangle.CreateMesh();
         }
      }
      private void DoRoundnessField(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Roundness", rectangle.roundness);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Roundness Change");
            rectangle.roundness = value;
            rectangle.CreateMesh();
         }
      }
      private void DoResolutionField(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         int value = EditorGUILayout.IntField("Resolution", rectangle.resolution);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Resolution Change");
            rectangle.resolution = value;
            rectangle.CreateMesh();
         }
      }

      private void DoToolsFoldout(ProceduralRectangle rectangle) {
         tools = EditorGUILayout.Foldout(tools, "Editor Tools:", true);
         if (tools) {
            EditorGUI.BeginDisabledGroup(!rectangle.Mesh);
            if (GUILayout.Button("Copy Mesh into Asset File"))
               ProcEditor.CopyMeshToAsset(rectangle.Mesh);
            if (GUILayout.Button("Copy GameObject into Static Prefab"))
               ProcEditor.CopyToStaticPrefab(rectangle.name, rectangle.Mesh, rectangle.GetComponent<MeshRenderer>().sharedMaterials);
            if (GUILayout.Button("Plot Current Texture Coordinates")) {
               if (onePlotColor)
                  ProcEditor.PlotTextureCoordinates(rectangle.Mesh, plotColor, plotBackColor, plotDiagonals, plotSize, rectangle.size, plotEncode);
               else
                  ProcEditor.PlotTextureCoordinates(rectangle.Mesh, new int[] { 1, 0 }, plotColors, plotBackColor, plotDiagonals, plotSize, rectangle.size, plotEncode);
            }
            if (onePlotColor)
               EditorGUILayout.BeginHorizontal();
            onePlotColor = EditorGUILayout.Toggle("Single Line Color", onePlotColor);
            if (onePlotColor) {
               plotColor = EditorGUILayout.ColorField(plotColor);
               EditorGUILayout.EndHorizontal();
            }
            else {
               plotColors[0] = EditorGUILayout.ColorField("Main Lines", plotColors[0]);
               plotColors[1] = EditorGUILayout.ColorField("Border Lines", plotColors[1]);
            }
            plotBackColor = EditorGUILayout.ColorField("Back Color", plotBackColor);
            plotDiagonals = EditorGUILayout.Toggle("Plot Diagonals", plotDiagonals);
            plotSize = (PlotSize)EditorGUILayout.Popup("Plot Size", (int)plotSize, ProcEditor.plotSizeNames);
            plotEncode = (EncodeType)EditorGUILayout.Popup("File Format", (int)plotEncode, System.Enum.GetNames(typeof(EncodeType)));
            EditorGUI.EndDisabledGroup();
         }
      }
      private void DoDebugFoldout(ProceduralRectangle rectangle) {
         debug = EditorGUILayout.Foldout(debug, "Debug Mesh:", true);
         if (debug) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Vertices", rectangle.Vertices.Length);
            EditorGUILayout.IntField("MainTrIs", rectangle.MainTrIs.Length);
            EditorGUILayout.IntField("BorderTrIs", rectangle.BorderTrIs.Length);
            EditorGUILayout.Space();
            EditorGUILayout.IntField("Skipped Vertices", rectangle.SkippedVertices);
            EditorGUILayout.IntField("Skipped TrIs", rectangle.SkippedTrIs);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            enabledDebug = EditorGUILayout.Toggle("Show Vertices", enabledDebug);
            debugColor = EditorGUILayout.ColorField(debugColor);
            EditorGUILayout.EndHorizontal();
         }
      }

      private void DoSkipMainToggle(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Main Area", rectangle.skipMainArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Skip Main Change");
            rectangle.skipMainArea = value;
            rectangle.CreateMesh();
         }
      }
      private void DoSkipBorderToggle(ProceduralRectangle rectangle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Border Area", rectangle.skipBorderArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(rectangle, "Rectangle Skip Border Change");
            rectangle.skipBorderArea = value;
            rectangle.CreateMesh();
         }
      }
      #endregion

      #region # Scene GUI Methods
      private void DoVerticesDebug(ProceduralRectangle rectangle) {
         Mesh mesh = rectangle.Mesh;
         if (mesh) {
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length > 0)
               ProcEditor.GUIDebugVertices(rectangle.transform.position, rectangle.transform.rotation, vertices, debugColor);
         }
      }

      private void DoHandleButtons(ProceduralRectangle rectangle) {
         if (buttonStyle == null) {
            buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonStyle.fixedWidth = 93.5f;
         }
         if (labelStyle == null) {
            labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fixedWidth = 93.5f;
            labelStyle.alignment = TextAnchor.UpperCenter;
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
         }
         if (areaStyle == null) {
            areaStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
         }

         Handles.BeginGUI();
         Color originalBack = GUI.backgroundColor;
         GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
         GUILayout.BeginArea(new Rect((SceneView.currentDrawingSceneView.camera.pixelWidth / 2) - 147f, 20f, 296f, 41f), areaStyle);
         GUI.backgroundColor = originalBack;
         GUILayout.BeginHorizontal();
         if (GUILayout.Toggle((activeHandle == 0), "Size", buttonStyle))
            activeHandle = 0;
         if (GUILayout.Toggle((activeHandle == 1), "Border", buttonStyle))
            activeHandle = 1;
         if (GUILayout.Toggle((activeHandle == 2), "Roundness", buttonStyle))
            activeHandle = 2;
         GUILayout.EndHorizontal();

         GUILayout.BeginHorizontal();
         GUILayout.Label(Vector2String(rectangle.size), labelStyle);
         GUILayout.Label(rectangle.border.ToString("G5"), labelStyle);
         GUILayout.Label(rectangle.roundness.ToString("G5"), labelStyle);
         GUILayout.EndHorizontal();

         GUILayout.EndArea();
         Handles.EndGUI();
      }

      private void DoSizeHandles(ProceduralRectangle rectangle) {
         Vector2 halfSize = rectangle.size / 2;
         if (halfSize.y < 0f)
            halfSize.y = 0f;
         if (halfSize.x < 0f)
            halfSize.x = 0f;
         float roundness = rectangle.roundness;
         if (roundness < 0f)
            roundness = 0f;
         if (roundness > halfSize.x)
            roundness = halfSize.x;
         if (roundness > halfSize.y)
            roundness = halfSize.y;
         Vector2 roundedHalfSize = new Vector2(halfSize.x - roundness, halfSize.y - roundness);

         DoLeftWidthHandle(rectangle, halfSize);
         DoRightWidthHandle(rectangle, halfSize);
         DoUpperHeightHandle(rectangle, halfSize);
         DoLowerHeightHandle(rectangle, halfSize);

         Vector3 lineA, lineB;
         if (roundedHalfSize.x > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, halfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, -halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, -halfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundedHalfSize.y > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundedHalfSize.x > 0f && roundedHalfSize.y > 0f && roundness > 0f) {
            Vector3 transform = rectangle.transform.position;
            Quaternion rotation = rectangle.transform.rotation;
            Vector3[] arc = ProcUtility.CornyArc(
               270f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               180f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               0f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               90f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
         }
      }
      private void DoLeftWidthHandle(ProceduralRectangle rectangle, Vector2 halfSize) {
         Vector3 handlePosition = rectangle.transform.position;
         Quaternion handleRotation = rectangle.transform.rotation * Quaternion.LookRotation(Vector3.left);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.left * halfSize.x) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(halfSize.x, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            Undo.RecordObject(rectangle, "Rectangle Width Change");
            rectangle.size.x = handleValue * 2;
            rectangle.CreateMesh();
         }
      }
      private void DoRightWidthHandle(ProceduralRectangle rectangle, Vector2 halfSize) {
         Vector3 handlePosition = rectangle.transform.position;
         Quaternion handleRotation = rectangle.transform.rotation * Quaternion.LookRotation(Vector3.right);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.right * halfSize.x) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(halfSize.x, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            Undo.RecordObject(rectangle, "Rectangle Width Change");
            rectangle.size.x = handleValue * 2;
            rectangle.CreateMesh();
         }
      }
      private void DoUpperHeightHandle(ProceduralRectangle rectangle, Vector2 halfSize) {
         Vector3 handlePosition = rectangle.transform.position;
         Quaternion handleRotation = rectangle.transform.rotation * Quaternion.LookRotation(Vector3.up);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.up * halfSize.y) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(halfSize.y, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            Undo.RecordObject(rectangle, "Rectangle Height Change");
            rectangle.size.y = handleValue * 2;
            rectangle.CreateMesh();
         }
      }
      private void DoLowerHeightHandle(ProceduralRectangle rectangle, Vector2 halfSize) {
         Vector3 handlePosition = rectangle.transform.position;
         Quaternion handleRotation = rectangle.transform.rotation * Quaternion.LookRotation(Vector3.down);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.down * halfSize.y) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(halfSize.y, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            Undo.RecordObject(rectangle, "Rectangle Height Change");
            rectangle.size.y = handleValue * 2;
            rectangle.CreateMesh();
         }
      }

      private void DoBorderHandles(ProceduralRectangle rectangle) {
         Vector2 halfSize = rectangle.size / 2;
         if (halfSize.y < 0f)
            halfSize.y = 0f;
         if (halfSize.x < 0f)
            halfSize.x = 0f;
         float border = rectangle.border;
         if (border < 0f)
            border = 0f;
         if (border > halfSize.x)
            border = halfSize.x;
         if (border > halfSize.y)
            border = halfSize.y;
         Vector2 borderedHalfSize = new Vector2(halfSize.x - border, halfSize.y - border);
         float roundness = rectangle.roundness;
         if (roundness < 0f)
            roundness = 0f;
         if (roundness > halfSize.x)
            roundness = halfSize.x;
         if (roundness > halfSize.y)
            roundness = halfSize.y;
         Vector2 roundedHalfSize = new Vector2(halfSize.x - roundness, halfSize.y - roundness);
         float borderedRoundness = roundness - border;

         DoLeftBorderHandle(rectangle, halfSize, border);
         DoRightBorderHandle(rectangle, halfSize, border);
         DoUpperBorderHandle(rectangle, halfSize, border);
         DoLowerBorderHandle(rectangle, halfSize, border);

         Vector3 lineA, lineB;
         if (roundedHalfSize.x > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, halfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, -halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, -halfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundedHalfSize.y > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundedHalfSize.x > 0f && roundedHalfSize.y > 0f && roundness > 0f) {
            Vector3 transform = rectangle.transform.position;
            Quaternion rotation = rectangle.transform.rotation;
            Vector3[] arc = ProcUtility.CornyArc(
               270f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               180f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               0f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               90f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
         }
         if (border > 0f) {
            if (borderedRoundness <= 0f) {
               if (borderedHalfSize.x > 0f) {
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, -borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, -borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
               }
               if (borderedHalfSize.y > 0f) {
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, -borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, -borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
               }
            }
            else {
               if (borderedHalfSize.x > 0f && roundedHalfSize.x > 0f) {
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, -borderedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, -borderedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
               }
               if (borderedHalfSize.y > 0f && roundedHalfSize.y > 0f) {
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, roundedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-borderedHalfSize.x, -roundedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
                  lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, roundedHalfSize.y);
                  lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(borderedHalfSize.x, -roundedHalfSize.y);
                  Handles.DrawLine(lineA, lineB);
               }
               if (roundness > 0f) {
                  Vector3 transform = rectangle.transform.position;
                  Quaternion rotation = rectangle.transform.rotation;
                  Vector3[] arc = ProcUtility.CornyArc(
                     270f, 90f, 0f, borderedRoundness, rectangle.resolution * 4,
                     transform + rotation * new Vector3(-roundedHalfSize.x, roundedHalfSize.y), rotation
                  );
                  if (arc.Length > 0)
                     Handles.DrawPolyLine(arc);
                  arc = ProcUtility.CornyArc(
                     180f, 90f, 0f, borderedRoundness, rectangle.resolution * 4,
                     transform + rotation * new Vector3(-roundedHalfSize.x, -roundedHalfSize.y), rotation
                  );
                  if (arc.Length > 0)
                     Handles.DrawPolyLine(arc);
                  arc = ProcUtility.CornyArc(
                     0f, 90f, 0f, borderedRoundness, rectangle.resolution * 4,
                     transform + rotation * new Vector3(roundedHalfSize.x, roundedHalfSize.y), rotation
                  );
                  if (arc.Length > 0)
                     Handles.DrawPolyLine(arc);
                  arc = ProcUtility.CornyArc(
                     90f, 90f, 0f, borderedRoundness, rectangle.resolution * 4,
                     transform + rotation * new Vector3(roundedHalfSize.x, -roundedHalfSize.y), rotation
                  );
                  if (arc.Length > 0)
                     Handles.DrawPolyLine(arc);
               }
            }
         }
      }
      private void DoLeftBorderHandle(ProceduralRectangle rectangle, Vector2 halfSize, float border) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 handlePosition = transform + rotation * (Vector3.left * halfSize.x);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(Vector3.right);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (Vector3.right * border)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(border, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            Undo.RecordObject(rectangle, "Rectangle Border Change");
            rectangle.border = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoRightBorderHandle(ProceduralRectangle rectangle, Vector2 halfSize, float border) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 handlePosition = transform + rotation * (Vector3.right * halfSize.x);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(Vector3.left);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (Vector3.left * border)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(border, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            Undo.RecordObject(rectangle, "Rectangle Border Change");
            rectangle.border = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoUpperBorderHandle(ProceduralRectangle rectangle, Vector2 halfSize, float border) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 handlePosition = transform + rotation * (Vector3.up * halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(Vector3.down);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (Vector3.down * border)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(border, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Border Change");
            rectangle.border = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoLowerBorderHandle(ProceduralRectangle rectangle, Vector2 halfSize, float border) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 handlePosition = transform + rotation * (Vector3.down * halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(Vector3.up);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (Vector3.up * border)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(border, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Border Change");
            rectangle.border = handleValue;
            rectangle.CreateMesh();
         }
      }

      private void DoRoundnessHandles(ProceduralRectangle rectangle) {
         Vector2 halfSize = rectangle.size / 2;
         if (halfSize.y < 0f)
            halfSize.y = 0f;
         if (halfSize.x < 0f)
            halfSize.x = 0f;
         float roundness = rectangle.roundness;
         if (roundness < 0f)
            roundness = 0f;
         if (roundness > halfSize.x)
            roundness = halfSize.x;
         if (roundness > halfSize.y)
            roundness = halfSize.y;
         Vector2 roundedHalfSize = new Vector2(halfSize.x - roundness, halfSize.y - roundness);

         DoUpperLeftRoundnessHandle(rectangle, halfSize, roundness);
         DoUpperRightRoundnessHandle(rectangle, halfSize, roundness);
         DoLowerLeftRoundnessHandle(rectangle, halfSize, roundness);
         DoLowerRightRoundnessHandle(rectangle, halfSize, roundness);

         Vector3 lineA, lineB;
         if (roundedHalfSize.x > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, halfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-roundedHalfSize.x, -halfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(roundedHalfSize.x, -halfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundedHalfSize.y > 0f) {
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(-halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
            lineA = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, roundedHalfSize.y);
            lineB = rectangle.transform.position + rectangle.transform.rotation * new Vector3(halfSize.x, -roundedHalfSize.y);
            Handles.DrawLine(lineA, lineB);
         }
         if (roundness > 0f) {
            Vector3 transform = rectangle.transform.position;
            Quaternion rotation = rectangle.transform.rotation;
            Vector3[] arc = ProcUtility.CornyArc(
               270f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               180f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(-roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               0f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
            arc = ProcUtility.CornyArc(
               90f, 90f, 0f, roundness, rectangle.resolution * 4,
               transform + rotation * new Vector3(roundedHalfSize.x, -roundedHalfSize.y), rotation
            );
            if (arc.Length > 0)
               Handles.DrawPolyLine(arc);
         }
      }
      private void DoUpperLeftRoundnessHandle(ProceduralRectangle rectangle, Vector2 halfSize, float roundness) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 direction = ProcUtility.RotateVector(Vector2.right, 45f);
         Vector3 handlePosition = transform + rotation * new Vector3(-halfSize.x, halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(direction);
         float handleValue = Mathf.Sqrt(2) * roundness - roundness;
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (direction * handleValue)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue /= Mathf.Sqrt(2) - 1;
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Roundness Change");
            rectangle.roundness = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoUpperRightRoundnessHandle(ProceduralRectangle rectangle, Vector2 halfSize, float roundness) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 direction = ProcUtility.RotateVector(Vector2.down, 45f);
         Vector3 handlePosition = transform + rotation * new Vector3(halfSize.x, halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(direction);
         float handleValue = Mathf.Sqrt(2) * roundness - roundness;
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (direction * handleValue)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck())
         {
            handleValue /= Mathf.Sqrt(2) - 1;
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Roundness Change");
            rectangle.roundness = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoLowerLeftRoundnessHandle(ProceduralRectangle rectangle, Vector2 halfSize, float roundness) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 direction = ProcUtility.RotateVector(Vector2.up, 45f);
         Vector3 handlePosition = transform + rotation * new Vector3(-halfSize.x, -halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(direction);
         float handleValue = Mathf.Sqrt(2) * roundness - roundness;
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (direction * handleValue)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue /= Mathf.Sqrt(2) - 1;
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Roundness Change");
            rectangle.roundness = handleValue;
            rectangle.CreateMesh();
         }
      }
      private void DoLowerRightRoundnessHandle(ProceduralRectangle rectangle, Vector2 halfSize, float roundness) {
         Vector3 transform = rectangle.transform.position;
         Quaternion rotation = rectangle.transform.rotation;
         Vector3 direction = ProcUtility.RotateVector(Vector2.left, 45f);
         Vector3 handlePosition = transform + rotation * new Vector3(halfSize.x, -halfSize.y);
         Quaternion handleRotation = rotation * Quaternion.LookRotation(direction);
         float handleValue = Mathf.Sqrt(2) * roundness - roundness;
         float handleSize = HandleUtility.GetHandleSize(handlePosition + rotation * (direction * handleValue)) / 4f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue /= Mathf.Sqrt(2) - 1;
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > halfSize.x)
               handleValue = halfSize.x;
            if (handleValue > halfSize.y)
               handleValue = halfSize.y;
            Undo.RecordObject(rectangle, "Rectangle Roundness Change");
            rectangle.roundness = handleValue;
            rectangle.CreateMesh();
         }
      }

      private string Vector2String(Vector2 v) {
         return v.x.ToString("G4") + ", " + v.y.ToString("G4");
      }
      #endregion
   }
}

