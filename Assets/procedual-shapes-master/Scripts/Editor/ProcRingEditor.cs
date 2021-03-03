
using UnityEngine;
using UnityEditor;

namespace ProceduralShapes.Editor {

   [CustomEditor(typeof(ProceduralRing))]
   [CanEditMultipleObjects]

   public class ProcRingEditor : UnityEditor.Editor {

      public static bool plotDiagonals = true;
      public static bool onePlotColor = true;
      public static Color plotColor = Color.white;
      public static Color[] plotColors = new Color[] { Color.red, Color.blue, Color.green };
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
         ProceduralRing ring = (ProceduralRing)target;

         DoOuterRadiusField(ring);
         DoInnerRadiusField(ring);
         DoOuterBorderField(ring);
         DoInnerBorderField(ring);
         DoResolutionField(ring);
         DoRingOffsetField(ring);
         EditorGUILayout.Space();

         DoPieSlider(ring);
         DoPieOffsetField(ring);
         EditorGUILayout.Space();

         DoSkipMainToggle(ring);
         DoSkipBorderToggle(ring);
         DoSkipPieToggle(ring);
         EditorGUILayout.Space();

         DoToolsFoldout(ring);
         EditorGUILayout.Space();

         DoDebugFoldout(ring);
         EditorGUILayout.Space();
      }

      public void OnSceneGUI() {
         ProceduralRing ring = (ProceduralRing)target;

         if (enabledDebug)
            DoVerticesDebug(ring);

         if (enabledHandles) {
            DoHandleButtons(ring);

            if (activeHandle == 0)
               DoOuterRadiusHandle(ring);
            if (activeHandle == 1)
               DoInnerRadiusHandle(ring);
            if (activeHandle == 2)
               DoOuterBorderHandle(ring);
            if (activeHandle == 3)
               DoInnerBorderHandle(ring);
            if (activeHandle == 4)
               DoRingOffsetHandle(ring);
            if (activeHandle == 5)
               DoPieHandle(ring);
            if (activeHandle == 6)
               DoPieOffsetHandle(ring);
         }
      }

      #region # Inspector GUI Methods
      private void DoOuterRadiusField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Outer Radius", ring.outerRadius);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Outer Radius Change");
            ring.outerRadius = value;
            ring.CreateMesh();
         }
      }
      private void DoInnerRadiusField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Inner Radius", ring.innerRadius);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Inner Radius Change");
            ring.innerRadius = value;
            ring.CreateMesh();
         }
      }

      private void DoOuterBorderField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Outer Border", ring.outerBorder);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Outer Border Change");
            ring.outerBorder = value;
            ring.CreateMesh();
         }
      }
      private void DoInnerBorderField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Inner Border", ring.innerBorder);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Inner Border Change");
            ring.innerBorder = value;
            ring.CreateMesh();
         }
      }

      private void DoResolutionField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         int value = EditorGUILayout.IntField("Resolution", ring.resolution);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Resolution Change");
            ring.resolution = value;
            ring.CreateMesh();
         }
      }
      private void DoRingOffsetField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Ring Offset", ring.ringOffset);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Offset Change");
            ring.ringOffset = value;
            ring.CreateMesh();
         }
      }

      private void DoPieSlider(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.Slider("Pie", ring.pie, 0f, 360f);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Pie Change");
            ring.pie = value;
            ring.CreateMesh();
         }
      }
      private void DoPieOffsetField(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         float value = EditorGUILayout.FloatField("Pie Offset", ring.pieOffset);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Pie Offset Change");
            ring.pieOffset = value;
            ring.CreateMesh();
         }
      }

      private void DoToolsFoldout(ProceduralRing ring) {
         tools = EditorGUILayout.Foldout(tools, "Editor Tools:", true);
         if (tools) {
            enabledHandles = EditorGUILayout.Toggle("Enable Scene Handles", enabledHandles);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!ring.Mesh);
            if (GUILayout.Button("Copy Mesh into Asset File"))
               ProcEditor.CopyMeshToAsset(ring.Mesh);
            if (GUILayout.Button("Copy GameObject into Static Prefab"))
               ProcEditor.CopyToStaticPrefab(ring.name, ring.Mesh, ring.GetComponent<MeshRenderer>().sharedMaterials);
            if (GUILayout.Button("Plot Current Texture Coordinates")) {
               if (onePlotColor)
                  ProcEditor.PlotTextureCoordinates(ring.Mesh, plotColor, plotBackColor, plotDiagonals, plotSize, Vector2.one, plotEncode);
               else
                  ProcEditor.PlotTextureCoordinates(ring.Mesh, new int[] { 1, 0, 2 }, plotColors, plotBackColor, plotDiagonals, plotSize, Vector2.one, plotEncode);
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
               plotColors[2] = EditorGUILayout.ColorField("Pie Lines", plotColors[2]);
            }
            plotBackColor = EditorGUILayout.ColorField("Back Color", plotBackColor);
            plotDiagonals = EditorGUILayout.Toggle("Plot Diagonals", plotDiagonals);
            plotSize = (PlotSize)EditorGUILayout.Popup("Plot Size", (int)plotSize, ProcEditor.plotSizeNames);
            plotEncode = (EncodeType)EditorGUILayout.Popup("File Format", (int)plotEncode, System.Enum.GetNames(typeof(EncodeType)));
            EditorGUI.EndDisabledGroup();
         }
      }
      private void DoDebugFoldout(ProceduralRing ring) {
         debug = EditorGUILayout.Foldout(debug, "Debug Mesh:", true);
         if (debug) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Vertices", ring.Vertices.Length);
            EditorGUILayout.IntField("MainTrIs", ring.MainTrIs.Length);
            EditorGUILayout.IntField("BorderTrIs", ring.BorderTrIs.Length);
            EditorGUILayout.IntField("PieTrIs", ring.PieTrIs.Length);
            EditorGUILayout.Space();
            EditorGUILayout.IntField("Skipped Vertices", ring.SkippedVertices);
            EditorGUILayout.IntField("Skipped TrIs", ring.SkippedTrIs);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            enabledDebug = EditorGUILayout.Toggle("Show Vertices", enabledDebug);
            debugColor = EditorGUILayout.ColorField(debugColor);
            EditorGUILayout.EndHorizontal();
         }
      }

      private void DoSkipMainToggle(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Main Area", ring.skipMainArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Skip Main Change");
            ring.skipMainArea = value;
            ring.CreateMesh();
         }
      }
      private void DoSkipBorderToggle(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Border Area", ring.skipBorderArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Skip Border Change");
            ring.skipBorderArea = value;
            ring.CreateMesh();
         }
      }
      private void DoSkipPieToggle(ProceduralRing ring) {
         EditorGUI.BeginChangeCheck();
         bool value = EditorGUILayout.Toggle("Skip Pie Area", ring.skipPieArea);
         if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(ring, "Ring Skip Pie Change");
            ring.skipPieArea = value;
            ring.CreateMesh();
         }
      }
      #endregion

      #region # Scene GUI Methods
      private void DoVerticesDebug(ProceduralRing ring) {
         Mesh mesh = ring.Mesh;
         if (mesh) {
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length > 0)
               ProcEditor.GUIDebugVertices(ring.transform.position, ring.transform.rotation, vertices, debugColor);
         }
      }

      private void DoHandleButtons(ProceduralRing ring) {
         if (buttonStyle == null) {
            buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonStyle.fixedWidth = 67f;
         }
         if (labelStyle == null) {
            labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fixedWidth = 67f;
            labelStyle.alignment = TextAnchor.UpperCenter;
            labelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
         }
         if (areaStyle == null) {
            areaStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
         }

         Handles.BeginGUI();
         Color originalBack = GUI.backgroundColor;
         GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
         GUILayout.BeginArea(new Rect((SceneView.currentDrawingSceneView.camera.pixelWidth / 2) - 250f, 20f, 501f, 41f), areaStyle);
         GUI.backgroundColor = originalBack;
         GUILayout.BeginHorizontal();
         if (GUILayout.Toggle((activeHandle == 0), "O. Radius", buttonStyle))
            activeHandle = 0;
         if (GUILayout.Toggle((activeHandle == 1), "I. Radius", buttonStyle))
            activeHandle = 1;
         if (GUILayout.Toggle((activeHandle == 2), "O. Border", buttonStyle))
            activeHandle = 2;
         if (GUILayout.Toggle((activeHandle == 3), "I. Border", buttonStyle))
            activeHandle = 3;
         if (GUILayout.Toggle((activeHandle == 4), "R. Offset", buttonStyle))
            activeHandle = 4;
         if (GUILayout.Toggle((activeHandle == 5), "Pie", buttonStyle))
            activeHandle = 5;
         if (GUILayout.Toggle((activeHandle == 6), "P. Offset", buttonStyle))
            activeHandle = 6;
         GUILayout.EndHorizontal();

         GUILayout.BeginHorizontal();
         GUILayout.Label(ring.outerRadius.ToString(), labelStyle);
         GUILayout.Label(ring.innerRadius.ToString(), labelStyle);
         GUILayout.Label(ring.outerBorder.ToString(), labelStyle);
         GUILayout.Label(ring.innerBorder.ToString(), labelStyle);
         GUILayout.Label(ring.ringOffset.ToString(), labelStyle);
         GUILayout.Label(ring.pie.ToString(), labelStyle);
         GUILayout.Label(ring.pieOffset.ToString(), labelStyle);
         GUILayout.EndHorizontal();

         GUILayout.EndArea();
         Handles.EndGUI();
      }

      private void DoOuterRadiusHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         Vector3 handlePosition = ring.transform.position + ring.transform.rotation * (Vector3.right * innerRadius);
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.right);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.right * outerRadius) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(outerRadius - innerRadius, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue + innerRadius, 0.1f);
            if (handleValue < innerRadius)
               handleValue = innerRadius;
            Undo.RecordObject(ring, "Ring Outer Radius Change");
            ring.outerRadius = handleValue;
            ring.CreateMesh();
         }
         if (outerRadius > 0f) {
            Vector3[] arcPoints = ProcUtility.CornyArc(0f, 360f, ringOffset, outerRadius, resolution, ring.transform.position, ring.transform.rotation);
            if (arcPoints.Length > 0)
               Handles.DrawPolyLine(arcPoints);
         }
      }
      private void DoInnerRadiusHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         Vector3 handlePosition = ring.transform.position;
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.right);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.right * innerRadius) / 3.5f;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(innerRadius, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > outerRadius)
               handleValue = outerRadius;
            Undo.RecordObject(ring, "Ring Inner Radius Change");
            ring.innerRadius = handleValue;
            ring.CreateMesh();
         }
         if (innerRadius > 0f) {
            Vector3[] arcPoints = ProcUtility.CornyArc(0f, 360f, ringOffset, innerRadius, resolution, ring.transform.position, ring.transform.rotation);
            if (arcPoints.Length > 0)
               Handles.DrawPolyLine(arcPoints);
         }
      }

      private void DoOuterBorderHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         float ringWidth = outerRadius - innerRadius;
         float innerBorder = ring.innerBorder;
         if (innerBorder < 0f)
            innerBorder = 0f;
         float outerBorder = ring.outerBorder;
         if (outerBorder < 0f)
            outerBorder = 0f;
         if (outerBorder > ringWidth)
            outerBorder = ringWidth;
         if (outerBorder > ringWidth - innerBorder)
            outerBorder = ringWidth - innerBorder;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.left);
         Vector3 handlePosition = ring.transform.position + handleRotation * (Vector3.back * outerRadius);
         float handleSize = HandleUtility.GetHandleSize(handlePosition) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(outerBorder, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > ringWidth - innerBorder)
               handleValue = ringWidth - innerBorder;
            Undo.RecordObject(ring, "Ring Outer Border Change");
            ring.outerBorder = handleValue;
            ring.CreateMesh();
         }
         if (outerRadius > 0f) {
            Vector3[] radiusArc = ProcUtility.CornyArc(0f, 360f, ringOffset, outerRadius, resolution, ring.transform.position, ring.transform.rotation);
            if (radiusArc.Length > 0)
               Handles.DrawPolyLine(radiusArc);
            if (outerBorder > 0f && outerBorder < outerRadius) {
               Vector3[] borderArc = ProcUtility.CornyArc(0f, 360f, ringOffset, outerRadius - outerBorder, resolution, ring.transform.position, ring.transform.rotation);
               if (borderArc.Length > 0)
                  Handles.DrawPolyLine(borderArc);
            }
         }
      }
      private void DoInnerBorderHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         float ringWidth = outerRadius - innerRadius;
         float outerBorder = ring.outerBorder;
         if (outerBorder < 0f)
            outerBorder = 0f;
         if (outerBorder > ringWidth)
            outerBorder = ringWidth;
         float innerBorder = ring.innerBorder;
         if (innerBorder < 0f)
            innerBorder = 0f;
         if (innerBorder > ringWidth - outerBorder)
            innerBorder = ringWidth - outerBorder;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.right);
         Vector3 handlePosition = ring.transform.position + handleRotation * (Vector3.forward * innerRadius);
         float handleSize = HandleUtility.GetHandleSize(handlePosition) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.LineHandle(innerBorder, handlePosition, handleRotation, handleSize, handleSnap, Handles.ConeHandleCap);
         if (EditorGUI.EndChangeCheck())
         {
            handleValue = Handles.SnapValue(handleValue, 0.1f);
            if (handleValue < 0f)
               handleValue = 0f;
            if (handleValue > ringWidth - outerBorder)
               handleValue = ringWidth - outerBorder;
            Undo.RecordObject(ring, "Ring Inner Border Change");
            ring.innerBorder = handleValue;
            ring.CreateMesh();
         }
         if (innerRadius > 0f) {
            Vector3[] radiusArc = ProcUtility.CornyArc(0f, 360f, ringOffset, innerRadius, resolution, ring.transform.position, ring.transform.rotation);
            if (radiusArc.Length > 0)
               Handles.DrawPolyLine(radiusArc);
            if (innerBorder > 0f) {
               Vector3[] borderArc = ProcUtility.CornyArc(0f, 360f, ringOffset, innerRadius + innerBorder, resolution, ring.transform.position, ring.transform.rotation);
               if (borderArc.Length > 0)
                  Handles.DrawPolyLine(borderArc);
            }
         }
      }

      private void DoRingOffsetHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         float handleStart = -ringOffset;
         float handleOffset = ringOffset;
         float handleRadius = outerRadius;
         float handleRadius2 = innerRadius;
         Vector3 handlePosition = ring.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up, ringOffset);
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.up * outerRadius) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.ArcHandle(ringOffset, handleStart, handleOffset, handleRadius, handleRadius2, resolution,
                                                 handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(ring, "Ring Offset Change");
            ring.ringOffset = handleValue;
            ring.CreateMesh();
         }
      }

      private void DoPieHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         float ringWidth = outerRadius - innerRadius;
         float innerBorder = ring.innerBorder;
         if (innerBorder < 0f)
            innerBorder = 0f;
         float outerBorder = ring.outerBorder;
         if (outerBorder < 0f)
            outerBorder = 0f;
         if (outerBorder > ringWidth)
            outerBorder = ringWidth;
         if (outerBorder > ringWidth - innerBorder)
            outerBorder = ringWidth - innerBorder;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         float pie = ring.pie;
         if (pie < 0f)
            pie = 0f;
         if (pie > 360f)
            pie = 360f;
         float pieOffset = ring.pieOffset;
         while (pieOffset < 0f)
            pieOffset += 360f;
         while (pieOffset > 360f)
            pieOffset -= 360f;
         float handleStart = pieOffset;
         float handleOffset = 0f;
         float handleRadius = outerRadius - outerBorder;
         float handleRadius2 = innerRadius + innerBorder;

         Vector3 handlePosition = ring.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up * (outerRadius - outerBorder), ringOffset);
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + handleUp) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.ArcHandle(pie, handleStart, handleOffset, handleRadius, handleRadius2, resolution,
                                                 handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(ring, "Ring Pie Change");
            ring.pie = handleValue;
            ring.CreateMesh();
         }
      }
      private void DoPieOffsetHandle(ProceduralRing ring) {
         float outerRadius = ring.outerRadius;
         if (outerRadius < 0f)
            outerRadius = 0f;
         float innerRadius = ring.innerRadius;
         if (innerRadius < 0f)
            innerRadius = 0f;
         if (innerRadius > outerRadius)
            innerRadius = outerRadius;
         float ringWidth = outerRadius - innerRadius;
         float innerBorder = ring.innerBorder;
         if (innerBorder < 0f)
            innerBorder = 0f;
         float outerBorder = ring.outerBorder;
         if (outerBorder < 0f)
            outerBorder = 0f;
         if (outerBorder > ringWidth)
            outerBorder = ringWidth;
         if (outerBorder > ringWidth - innerBorder)
            outerBorder = ringWidth - innerBorder;
         int resolution = ring.resolution;
         if (resolution < 3)
            resolution = 3;
         float ringOffset = ring.ringOffset;
         while (ringOffset < 0f)
            ringOffset += 360f;
         while (ringOffset > 360f)
            ringOffset -= 360f;
         float pieOffset = ring.pieOffset;
         while (pieOffset < 0f)
            pieOffset += 360f;
         while (pieOffset > 360f)
            pieOffset -= 360f;
         float handleRadius = outerRadius - outerBorder;
         float handleRadius2 = innerRadius + innerBorder;

         Vector3 handlePosition = ring.transform.position;
         Vector3 handleUp = ProcUtility.RotateAxis(Vector3.up, ringOffset);
         Quaternion handleRotation = ring.transform.rotation * Quaternion.LookRotation(Vector3.forward, handleUp);
         float handleSize = HandleUtility.GetHandleSize(handlePosition + Vector3.up * (outerRadius - outerBorder)) / 4;
         Vector3 handleSnap = new Vector3(0.01f, 0.01f, 0.01f);

         EditorGUI.BeginChangeCheck();
         float handleValue = ProcHandles.ArcHandle(pieOffset, 0f, 0f, handleRadius, handleRadius2, resolution,
                                            handlePosition, handleRotation, handleSize, handleSnap, Handles.SphereHandleCap);
         if (EditorGUI.EndChangeCheck()) {
            handleValue = Handles.SnapValue(handleValue, 5f);
            Undo.RecordObject(ring, "Ring Pie Offset Change");
            ring.pieOffset = handleValue;
            ring.CreateMesh();
         }
      }
      #endregion
   }

}
