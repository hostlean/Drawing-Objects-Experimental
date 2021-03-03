
using UnityEngine;
using UnityEditor;

namespace ProceduralShapes.Editor {

   [CustomEditor(typeof(ProceduralCircle))]
   [CanEditMultipleObjects]

   public class ProcCircleEditor : UnityEditor.Editor {

      public static bool plotDiagonals = true;
      public static bool onePlotColor = true;
      public static Color plotColor = Color.white;
      public static Color[] plotColors = new Color[] { Color.red, Color.blue, Color.green, Color.green };
      public static Color plotBackColor = Color.gray;
      public static EncodeType plotEncode = EncodeType.PNG;
      public static PlotSize plotSize = PlotSize._1K;

      public static bool tools = false;
      public static bool debug = false;

      //public static bool enabledDebug = false;
      //public static Color debugColor = new Color(1f, 0.4f, 1f);

      public static bool showHandles = true;
      public static GUIContent disableContent;
      public static int activeHandle = 0;
      public static GUIStyle areaStyle = null;
      public static GUIStyle titleStyle = null;
      public static GUIStyle buttonStyle = null;
      public static GUIStyle labelStyle = null;
      public static GUIStyle disableStyle = null;

      /*private void OnEnable() {
         ProcCircle circle = (ProcCircle)target;
         MeshRenderer render = circle.GetComponent<MeshRenderer>();
         if (render) {
            Material[] materials = render.sharedMaterials;
            if (materials.Length > 2) {
               Color a = materials[0].color;
               Color b = materials[1].color;
               Color c = materials[2].color;
               a.r = 1f - a.r;
               a.b = 1f - a.b;
               a.g = 1f - a.g;
               b.r = 1f - b.r;
               b.b = 1f - b.b;
               b.g = 1f - b.g;
               c.r = 1f - c.r;
               c.b = 1f - c.b;
               c.g = 1f - c.g;
               float h, s, v;
               handleColor = Color.Lerp(a, b, 0.5f);
               Color.RGBToHSV(handleColor, out h, out s, out v);
               if (v < 0.6f)
                  v = 0.6f;
               handleColor = Color.HSVToRGB(h, s, v);
               handleColor2 = Color.Lerp(a, c, 0.5f);
               Color.RGBToHSV(handleColor2, out h, out s, out v);
               if (v < 0.6f)
                  v = 0.6f;
               handleColor2 = Color.HSVToRGB(h, s, v);
            }
         }
      }*/

      public override void OnInspectorGUI() {
         ProceduralCircle circle = (ProceduralCircle)target;

         DoRadiusField(circle);
         DoBorderField(circle);
         DoResolutionField(circle);
         DoCircleOffsetField(circle);         
         EditorGUILayout.Space();

         EditorGUI.BeginDisabledGroup(circle.pie > 0f);
         DoFillSlider(circle);
         DoFillOffsetField(circle);
         EditorGUI.EndDisabledGroup();
         EditorGUI.BeginDisabledGroup(circle.fill > 0f);
         DoPieSlider(circle);
         DoPieOffsetField(circle);
         EditorGUI.EndDisabledGroup();         
         EditorGUILayout.Space();

         DoSkipMainToggle(circle);
         DoSkipBorderToggle(circle);
         DoSkipFillToggle(circle);
         DoSkipPieToggle(circle);         
         EditorGUILayout.Space();

         //DoImmediateUpdateToggle(circle);
         DoSynchroniseField(circle);
         EditorGUILayout.Space();

         DoFillWarning(circle);         
         EditorGUILayout.Space();

         DoToolsFoldout(circle);         
         EditorGUILayout.Space();

         //DoDebugFoldout(circle);         
         //EditorGUILayout.Space();
      }

      void OnSceneGUI() {
         ProceduralCircle circle = (ProceduralCircle)target;

         /*if (enabledDebug)
            DoVerticesDebug(circle);*/

         if (showHandles) {
            if (activeHandle == 0)
               DoRadiusHandle(circle);
            else if (activeHandle == 1)
               DoBorderHandle(circle);
            else if (activeHandle == 2)
               DoCircleOffsetHandle(circle);

            else if (activeHandle == 3)
               DoFillHandle(circle);
            else if (activeHandle == 4)
               DoFillOffsetHandle(circle);

            else if (activeHandle == 5)
               DoPieHandle(circle);
            else if (activeHandle == 6)
               DoPieOffsetHandle(circle);
         }
         DoHandleButtons(circle);
      }

      #region # Inspector GUI Methods
      private void DoRadiusField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Radius", circle.radius);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Radius Change");
            circle.radius = value;
            circle.CreateMesh();
         }
      }
      private void DoBorderField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Border", circle.border);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Border Change");
            circle.border = value;
            circle.CreateMesh();
         }
      }
      private void DoResolutionField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         int value = EditorGUILayout.IntField("Resolution", circle.resolution);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Resolution Change");
            circle.resolution = value;
            circle.CreateMesh();
         }
      }
      private void DoCircleOffsetField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Circle Offset", circle.circleOffset);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Offset Change");
            circle.circleOffset = value;
            circle.CreateMesh();
         }
      }

      private void DoFillSlider(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.Slider("Fill", circle.fill, 0f, 1f);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Fill Change");
            circle.fill = value;
            circle.CreateMesh();
         }
      }
      private void DoFillOffsetField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Fill Offset", circle.fillOffset);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Fill Offset Change");
            circle.fillOffset = value;
            circle.CreateMesh();
         }
      }
      private void DoPieSlider(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.Slider("Pie", circle.pie, 0f, 360f);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Pie Change");
            circle.pie = value;
            circle.CreateMesh();
         }
      }
      private void DoPieOffsetField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Pie Offset", circle.pieOffset);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Pie Offset Change");
            circle.pieOffset = value;
            circle.CreateMesh();
         }
      }

      private void DoFillWarning(ProceduralCircle circle) {
         if (circle.fill > 0f && circle.resolution % 3 != 0) {
            GUIStyle style = GUI.skin.GetStyle("HelpBox");
            style.richText = true;
            EditorGUILayout.TextArea("<b><color=brown><size=16>\u25C8</size> <size=11>Resolution % 3 != 0</size></color></b>\n" +
                                     "<size=10> Fill percentage will not be accurate! The Resolution must be divisable by 3!</size>", style);
         }
      }

      /*private void DoImmediateUpdateToggle(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Immediate Update", circle.immediateUpdate);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Immediate Update Change");
            circle.immediateUpdate = value;
            circle.CreateMesh();
         }
      }*/
      private void DoSynchroniseField(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Synchonisation", circle.synchronise);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Synchronisation Change");
            circle.synchronise = value;
            circle.CreateMesh();
         }
      }

      private void DoToolsFoldout(ProceduralCircle circle) {
         EditorGUI.BeginDisabledGroup(!circle.Mesh);
         tools = EditorGUILayout.Foldout(tools, "Editor Tools:", true);
         EditorGUI.EndDisabledGroup();
         EditorGUILayout.Space();


         if (circle.Mesh && tools) {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("Copy Mesh into Asset File"))
               ProcEditor.CopyMeshToAsset(circle.Mesh);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("Copy GameObject into Static Prefab"))
               ProcEditor.CopyToStaticPrefab(circle.name, circle.Mesh, circle.GetComponent<MeshRenderer>().sharedMaterials);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("Plot current Texture Coordinates")) {
               if (onePlotColor)
                  ProcEditor.PlotTextureCoordinates(circle.Mesh, plotColor, plotBackColor, plotDiagonals, plotSize, Vector2.one, plotEncode);
               else
                  ProcEditor.PlotTextureCoordinates(circle.Mesh, new int[] { 1, 0, 2, 3 }, plotColors, plotBackColor, plotDiagonals, plotSize, Vector2.one, plotEncode);
            }
            EditorGUILayout.EndHorizontal();
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
               plotColors[2] = EditorGUILayout.ColorField("Fill / Pie Lines", plotColors[2]);
               plotColors[3] = plotColors[2];
            }
            plotBackColor = EditorGUILayout.ColorField("Back Color", plotBackColor);
            plotDiagonals = EditorGUILayout.Toggle("Plot Diagonals", plotDiagonals);
            plotSize = (PlotSize)EditorGUILayout.Popup("Plot Size", (int)plotSize, ProcEditor.plotSizeNames);
            plotEncode = (EncodeType)EditorGUILayout.Popup("File Format", (int)plotEncode, System.Enum.GetNames(typeof(EncodeType)));
            EditorGUI.indentLevel--;
         }
      }
      /*private void DoDebugFoldout(ProceduralCircle circle) {
         debug = EditorGUILayout.Foldout(debug, "Debug Mesh:", true);
         if (debug) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Vertices", circle.Vertices.Length);
            EditorGUILayout.IntField("MainTrIs", circle.MainTrIs.Length);
            EditorGUILayout.IntField("BorderTrIs", circle.BorderTrIs.Length);
            EditorGUILayout.IntField("FillTrIs", circle.FillTrIs.Length);
            EditorGUILayout.IntField("PieTrIs", circle.PieTrIs.Length);
            EditorGUILayout.Space();
            EditorGUILayout.IntField("Skipped Vertices", circle.SkippedVertices);
            EditorGUILayout.IntField("Skipped TrIs", circle.SkippedTrIs);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            enabledDebug = EditorGUILayout.Toggle("Show Vertices", enabledDebug);
            debugColor = EditorGUILayout.ColorField(debugColor);
            EditorGUILayout.EndHorizontal();
         }
      }*/

      private void DoSkipMainToggle(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Main Area", circle.skipMainArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Skip Main Change");
            circle.skipMainArea = value;
            circle.CreateMesh();
         }
      }
      private void DoSkipBorderToggle(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Border Area", circle.skipBorderArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Skip Border Change");
            circle.skipBorderArea = value;
            circle.CreateMesh();
         }
      }
      private void DoSkipFillToggle(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Fill Area", circle.skipFillArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Skip Fill Change");
            circle.skipFillArea = value;
            circle.CreateMesh();
         }
      }
      private void DoSkipPieToggle(ProceduralCircle circle) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Pie Area", circle.skipPieArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(circle, "Circle Skip Pie Change");
            circle.skipPieArea = value;
            circle.CreateMesh();
         }
      }
      #endregion

      #region # Scene GUI Methods
      /*private void DoVerticesDebug(ProceduralCircle circle) {
         Mesh mesh = circle.Mesh;
         if (mesh) {
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length > 0)
               ProcEditor.GUIDebugVertices(circle.transform.position, circle.transform.rotation, vertices, debugColor);
         }
      }*/

      private void DoHandleButtons(ProceduralCircle circle) {
         if (titleStyle == null) {
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fixedWidth = 95.5f;
            titleStyle.fixedHeight = 15.5f;
            titleStyle.fontSize = 10;
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
         }
         if (buttonStyle == null) {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fixedWidth = 63.5f;
         }
         if (labelStyle == null) {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fixedWidth = 63.5f;
            labelStyle.alignment = TextAnchor.UpperCenter;
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
         }
         if (areaStyle == null) {
            areaStyle = new GUIStyle(GUI.skin.box);
         }
         if (disableStyle == null) {
            disableStyle = new GUIStyle(GUI.skin.button);
            disableStyle.fixedWidth = 22f;
            disableStyle.fixedHeight = 13.5f;
            disableStyle.fontSize = 8;
            disableStyle.onActive = disableStyle.normal;
            disableStyle.onNormal.background = disableStyle.active.background;
            disableStyle.onNormal.textColor = new Color(0.2f, 0.2f, 0.2f);
         }
         
         Handles.BeginGUI();
         Color originalBack = GUI.backgroundColor;
         GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
         GUILayout.BeginArea(new Rect((SceneView.currentDrawingSceneView.camera.pixelWidth / 2) - 64f, 4f, 130f, 19.5f), areaStyle);
         GUI.backgroundColor = originalBack;
         EditorGUILayout.BeginHorizontal();
         GUILayout.Label("Procedural Circle", titleStyle);
         if (showHandles) {
            disableContent = new GUIContent("\u25BC", "Hide or show Handles");
         }
         else {
            disableContent = new GUIContent("\u25B2", "Hide or show Handles");
         }
         int originalFont = GUI.skin.button.fontSize;
         GUI.skin.button.fontSize = 8;
         showHandles = GUILayout.Toggle(showHandles, disableContent, disableStyle);
         GUI.skin.button.fontSize = originalFont;
         EditorGUILayout.EndHorizontal();
         GUILayout.EndArea();

         if (showHandles) {
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            GUILayout.BeginArea(new Rect((SceneView.currentDrawingSceneView.camera.pixelWidth / 2) - 237f, 26f, 476f, 41f), areaStyle);
            GUI.backgroundColor = originalBack;
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle((activeHandle == 0), "Radius", buttonStyle))
               activeHandle = 0;
            if (GUILayout.Toggle((activeHandle == 1), "Border", buttonStyle))
               activeHandle = 1;
            if (GUILayout.Toggle((activeHandle == 2), "C. Offset", buttonStyle))
               activeHandle = 2;
            EditorGUI.BeginDisabledGroup(circle.pie > 0f);
            if (GUILayout.Toggle((activeHandle == 3), "Fill", buttonStyle))
               activeHandle = 3;
            if (GUILayout.Toggle((activeHandle == 4), "F. Offset", buttonStyle))
               activeHandle = 4;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(circle.fill > 0f);
            if (GUILayout.Toggle((activeHandle == 5), "Pie", buttonStyle))
               activeHandle = 5;
            if (GUILayout.Toggle((activeHandle == 6), "P. Offset", buttonStyle))
               activeHandle = 6;
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(circle.radius.ToString(), labelStyle);
            GUILayout.Label(circle.border.ToString(), labelStyle);
            GUILayout.Label(circle.circleOffset.ToString(), labelStyle);
            GUILayout.Label(circle.fill.ToString(), labelStyle);
            GUILayout.Label(circle.fillOffset.ToString(), labelStyle);
            GUILayout.Label(circle.pie.ToString(), labelStyle);
            GUILayout.Label(circle.pieOffset.ToString(), labelStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
         }
         Handles.EndGUI();
      }

      private void DoRadiusHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         Vector3 handlePosition = circle.transform.position;
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.right);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.right * radius) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(radius, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            Undo.RecordObject(circle, "Circle Radius Change");
            circle.radius = handleValue;
            circle.CreateMesh();
         }
         if (radius > 0f) {
            Vector3[] arcPoints = ProcUtility.CornyArc(0f, 360f, circleOffset, radius, resolution, circle.transform.position, circle.transform.rotation);
            if (arcPoints.Length > 0)
               Handles.DrawPolyLine(arcPoints);
         }
      }
      private void DoBorderHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         float border = circle.border;
         if (border < 0f)
            border = 0f;
         if (border > radius)
            border = radius;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float handleValue = border;
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.left);
         Vector3 handlePosition = circle.transform.position + handleRotation * (Vector3.back * radius);
         float handleSize = HandleUtility.GetHandleSize(handlePosition) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > radius)
               handleValue = radius;
            Undo.RecordObject(circle, "Circle Border Change");
            circle.border = handleValue;
            circle.CreateMesh();
         }
         if (radius > 0f) {
            Vector3[] radiusArc = ProcUtility.CornyArc(0f, 360f, circleOffset, radius, resolution, circle.transform.position, circle.transform.rotation);
            if (radiusArc.Length > 0)
               Handles.DrawPolyLine(radiusArc);
            if (border > 0f && border < radius) {
               Vector3[] borderArc = ProcUtility.CornyArc(0f, 360f, circleOffset, radius - border, resolution, circle.transform.position, circle.transform.rotation);
               if (borderArc.Length > 0)
                  Handles.DrawPolyLine(borderArc);
            }
         }
      }
      private void DoCircleOffsetHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float handleValue = circleOffset;
         float handleStart = -circleOffset;
         float handleOffset = circleOffset;
         float handleRadius = radius;
         Vector3 handlePosition = circle.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up, circleOffset);
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.up * radius) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.ArcHandle(handleValue, handleStart, handleOffset, handleRadius, 0f, resolution,
                                                 handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(circle, "Circle Offset Change");
            circle.circleOffset = handleValue;
            circle.CreateMesh();
         }
      }

      private void DoFillHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         float border = circle.border;
         if (border < 0f)
            border = 0f;
         if (border > radius)
            border = radius;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float fill = circle.fill;
         if (fill < 0f)
            fill = 0f;
         if (fill > 1f)
            fill = 1f;
         float fillOffset = circle.fillOffset;
         while (fillOffset < 0f)
            fillOffset += 360f;
         while (fillOffset > 360f)
            fillOffset -= 360f;
         float handleValue = (radius - border) * 2 * fill;
         Vector3 handleForward = ProcUtility.RotateAxis(Vector3.up, circleOffset + fillOffset);
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(handleForward, Vector3.back);
         Vector3 handlePosition = circle.transform.position + handleRotation * (Vector3.back * (radius - border));
         float handleSize = HandleUtility.GetHandleSize(handlePosition) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.LineHandle(handleValue, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue /= ((radius - border) * 2);
            handleValue = Handles.SnapValue(handleValue, 0.01f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > 1f)
               handleValue = 1f;
            Undo.RecordObject(circle, "Circle Fill Change");
            circle.fill = handleValue;
            circle.CreateMesh();
         }
         if (border < radius && fill > 0f) {
            Vector3 arcUp = ProcUtility.RotateAxis(Vector3.up, circleOffset);
            Quaternion arcRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.forward, arcUp);
            float start = fillOffset + (Mathf.PI - Mathf.Acos(1 - 2 * fill)) * 180f / Mathf.PI;
            float size = (fillOffset + 360f - (Mathf.PI - Mathf.Acos(1 - 2 * fill)) * 180f / Mathf.PI) - start;
            Vector3[] arcPoints = ProcUtility.CornyArc(start, size, 0f, radius - border, resolution, circle.transform.position, arcRotation);
            if (arcPoints.Length > 0) {
               if (fill < 1f) Handles.DrawLine(arcPoints[0], arcPoints[arcPoints.Length - 1]);
               Handles.DrawPolyLine(arcPoints);
            }
         }
      }
      private void DoFillOffsetHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         float border = circle.border;
         if (border < 0f)
            border = 0f;
         if (border > radius)
            border = radius;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float fillOffset = circle.fillOffset;
         while (fillOffset < 0f)
            fillOffset += 360f;
         while (fillOffset > 360f)
            fillOffset -= 360f;
         float handleValue = fillOffset;
         float handleStart = 0f;
         float handleOffset = 0f;
         float handleRadius = radius - border;

         Vector3 handlePosition = circle.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.down, circleOffset);
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.down * (radius - border)) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.ArcHandle(handleValue, handleStart, handleOffset, handleRadius, 0f, resolution,
                                                 handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(circle, "Circle Fill Offset Change");
            circle.fillOffset = handleValue;
            circle.CreateMesh();
         }
      }

      private void DoPieHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         float border = circle.border;
         if (border < 0f)
            border = 0f;
         if (border > radius)
            border = radius;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float pie = circle.pie;
         if (pie < 0f)
            pie = 0f;
         if (pie > 360f)
            pie = 360f;
         float pieOffset = circle.pieOffset;
         while (pieOffset < 0f)
            pieOffset += 360f;
         while (pieOffset > 360f)
            pieOffset -= 360f;
         float handleValue = pie;
         float handleStart = pieOffset;
         float handleOffset = 0f;
         float handleRadius = radius - border;

         Vector3 handlePosition = circle.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up * (radius - border), circleOffset);
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + handleUp) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.ArcHandle(handleValue, handleStart, handleOffset, handleRadius, 0f, resolution,
                                                 handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(circle, "Circle Pie Change");
            circle.pie = handleValue;
            circle.CreateMesh();
         }
      }
      private void DoPieOffsetHandle(ProceduralCircle circle) {
         float radius = circle.radius;
         if (radius < 0f)
            radius = 0f;
         float border = circle.border;
         if (border < 0f)
            border = 0f;
         if (border > radius)
            border = radius;
         int resolution = circle.resolution;
         if (resolution < 3)
            resolution = 3;
         float circleOffset = circle.circleOffset;
         while (circleOffset < 0f)
            circleOffset += 360f;
         while (circleOffset > 360f)
            circleOffset -= 360f;
         float pieOffset = circle.pieOffset;
         while (pieOffset < 0f)
            pieOffset += 360f;
         while (pieOffset > 360f)
            pieOffset -= 360f;
         float handleValue = pieOffset;
         float handleStart = 0f;
         float handleOffset = 0f;
         float handleRadius = radius - border;

         Vector3 handlePosition = circle.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up, circleOffset);
         Quaternion handleRotation = circle.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.up * radius) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);
         
         EditorGUI.BeginChangeCheck();
         handleValue = ProcHandles.ArcHandle(handleValue, handleStart, handleOffset, handleRadius, 0f, resolution,
                                            handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(circle, "Circle Pie Offset Change");
            circle.pieOffset = handleValue;
            circle.CreateMesh();
         }
      }
      #endregion
   }

}
