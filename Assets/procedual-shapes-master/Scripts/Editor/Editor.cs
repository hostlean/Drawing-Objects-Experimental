
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace ProceduralShapes.Editor {

   [Serializable] public enum EncodeType {
      PNG = 0, JPG = 1
   }
   [Serializable] public enum PlotSize {
      _256 = 0,
      _512 = 1,
      _1K = 2,
      _2K = 3,
      _4K = 4,
      _8K = 5
   }

   public class ProcEditor {

      private static GUIStyle debugStyle = null;

      public static string[] plotSizeNames = new string[] {
         "256 x 256", "512 x 512", "1024 x 1024", "2048 x 2048", "4096 x 4096", "8192 x 8192"
      };

      public static Texture2D PlotTexture(PlotSize plotSize, Color color, Vector2 proportions) {
         int longSide;
         switch (plotSize) {
            default:
            case PlotSize._256:
               longSide = 256;
               break;
            case PlotSize._512:
               longSide = 512;
               break;
            case PlotSize._1K:
               longSide = 1024;
               break;
            case PlotSize._2K:
               longSide = 2048;
               break;
            case PlotSize._4K:
               longSide = 4096;
               break;
            case PlotSize._8K:
               longSide = 8192;
               break;
         }
         int h = longSide;
         int w = longSide;
         if (proportions.y < proportions.x)
            h = Mathf.RoundToInt(longSide * (proportions.y / proportions.x));
         else if (proportions.y > proportions.x)
            w = Mathf.RoundToInt(longSide * (proportions.x / proportions.y));

         Texture2D result = new Texture2D(w, h);
         Color[] colors = new Color[w * h];
         for (int i = 0; i < colors.Length; i++) {
            colors[i] = color;
         }
         result.SetPixels(colors);
         result.Apply();

         return result;
      }
      public static Texture2D PlotTexture(PlotSize plotSize, Color color) {
         return PlotTexture(plotSize, color, Vector2.one);
      }

      public static Vector2Int[] TriangleLines(Mesh mesh, bool diagonals) {
         List<Vector2Int> lines = new List<Vector2Int>();
         Vector2Int l;
         for (int i = 0; i < mesh.subMeshCount; i++) {
            int[] st = mesh.GetTriangles(i);
            for (int n = 0; n < st.Length; n += 3) {
               l = new Vector2Int(st[n], st[n + 1]);
               if (!lines.Contains(l))
                  lines.Add(l);
               l = new Vector2Int(st[n + 1], st[n + 2]);
               if (!lines.Contains(l))
                  lines.Add(l);
               l = new Vector2Int(st[n + 2], st[n]);
               if (diagonals && !lines.Contains(l))
                  lines.Add(l);
            }
         }
         Vector2Int[] result = new Vector2Int[lines.Count];
         for (int i = 0; i < lines.Count; i++) {
            result[i] = lines[i];
         }
         return result;
      }
      public static Vector2Int[] TriangleLines(Mesh mesh, int submesh, bool diagonals) {
         List<Vector2Int> lines = new List<Vector2Int>();
         Vector2Int l;
         int[] st = mesh.GetTriangles(submesh);
         for (int i = 0; i < st.Length; i += 3) {
            l = new Vector2Int(st[i], st[i + 1]);
            if (!lines.Contains(l))
               lines.Add(l);
            l = new Vector2Int(st[i + 1], st[i + 2]);
            if (!lines.Contains(l))
               lines.Add(l);
            l = new Vector2Int(st[i + 2], st[i]);
            if (diagonals && !lines.Contains(l))
               lines.Add(l);
         }
         Vector2Int[] result = new Vector2Int[lines.Count];
         for (int i = 0; i < lines.Count; i++) {
            result[i] = lines[i];
         }
         return result;
      }
      public static Vector2Int[][] TextureLines(int width, int height, Vector2[] uv, Vector2Int[] trisLines) {
         Vector2Int[][] result = new Vector2Int[trisLines.Length][];
         for (int i = 0; i < trisLines.Length; i++) {
            result[i] = new Vector2Int[] {
               new Vector2Int(Mathf.RoundToInt(uv[trisLines[i].x].x * (width - 1)), 
                              Mathf.RoundToInt(uv[trisLines[i].x].y * (height - 1))),
               new Vector2Int(Mathf.RoundToInt(uv[trisLines[i].y].x * (width - 1)),
                              Mathf.RoundToInt(uv[trisLines[i].y].y * (height - 1)))
            };
         }
         return result;
      }

      public static void LineToTexture(ref Texture2D texture, Vector2Int a, Vector2Int b, Color color) {
         if (a == b)
            return;

         int dy = (b.y - a.y);
         int dx = (b.x - a.x);
         int stepx, stepy;

         if (dy < 0) {
            dy = -dy;
            stepy = -1;
         }
         else
            stepy = 1;

         if (dx < 0) {
            dx = -dx;
            stepx = -1;
         }
         else
            stepx = 1;

         dy <<= 1;
         dx <<= 1;

         float fraction = 0;

         texture.SetPixel(a.x, a.y, color);
         if (dx > dy) {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(a.x - b.x) > 1) {
               if (fraction >= 0) {
                  a.y += stepy;
                  fraction -= dx;
               }
               a.x += stepx;
               fraction += dy;
               texture.SetPixel(a.x, a.y, color);
            }
         }
         else {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(a.y - b.y) > 1) {
               if (fraction >= 0) {
                  a.x += stepx;
                  fraction -= dy;
               }
               a.y += stepy;
               fraction += dx;
               texture.SetPixel(a.x, a.y, color);
            }
         }
      }
      public static void LinesToTexture(ref Texture2D texture, Vector2Int[][] lines, Color color) {
         for (int i = 0; i < lines.Length; i++) {
            LineToTexture(ref texture, lines[i][0], lines[i][1], color);
         }
      }
      
      public static void TextureToAsset(Texture2D texture, string assetName, EncodeType encoding) {
         byte[] data;
         string assets = Application.dataPath;
         string project = Application.dataPath.Remove(assets.Length - 6);
         string relative;

         switch (encoding) {
            default:
            case EncodeType.PNG:
               data = texture.EncodeToPNG();
               relative = AssetDatabase.GenerateUniqueAssetPath("Assets/" + assetName + ".png");
               break;
            case EncodeType.JPG:
               data = texture.EncodeToJPG(100);
               relative = AssetDatabase.GenerateUniqueAssetPath("Assets/" + assetName + ".jpg");
               break;
         }
         Debug.Log("Texture written to \"" + relative + "\"");
         System.IO.File.WriteAllBytes(project + relative, data);
         AssetDatabase.Refresh();
      }

      public static void PlotTextureCoordinates(
         Mesh mesh, Color line, Color back, bool diagonals,
         PlotSize size, Vector2 proportions, EncodeType encode)
      {
         Vector2Int[] trisLines = TriangleLines(mesh, diagonals);
         Texture2D texture = PlotTexture(size, back, proportions);
         Vector2Int[][] textureLines = TextureLines(texture.width, texture.height, mesh.uv, trisLines);
         LinesToTexture(ref texture, textureLines, line);
         texture.Apply();
         TextureToAsset(texture, mesh.name + "_UV" + size.ToString(), encode);
      }
      public static void PlotTextureCoordinates(
         Mesh mesh, Color[] colors, Color back, bool diagonals, 
         PlotSize size, Vector2 proportions, EncodeType encode)
      {
         Texture2D texture = PlotTexture(size, back, proportions);
         for (int i = 0; i < mesh.subMeshCount; i++) {
            Vector2Int[] trisLines = TriangleLines(mesh, i, diagonals);
            Vector2Int[][] textureLines = TextureLines(texture.width, texture.height, mesh.uv, trisLines);
            LinesToTexture(ref texture, textureLines, colors[i]);
         }
         texture.Apply();
         TextureToAsset(texture, mesh.name + "_UV" + size.ToString(), encode);
      }
      public static void PlotTextureCoordinates(
         Mesh mesh, int[] order, Color[] colors, Color back, bool diagonals,
         PlotSize size, Vector2 proportions, EncodeType encode)
      {
         Texture2D texture = PlotTexture(size, back, proportions);
         for (int i = 0; i < mesh.subMeshCount; i++) {
            Vector2Int[] trisLines = TriangleLines(mesh, order[i], diagonals);
            Vector2Int[][] textureLines = TextureLines(texture.width, texture.height, mesh.uv, trisLines);
            LinesToTexture(ref texture, textureLines, colors[order[i]]);
         }
         texture.Apply();
         TextureToAsset(texture, mesh.name + "_UV" + size.ToString(), encode);
      }
      
      public static Mesh CopyMesh(Mesh mesh) {
         Mesh copy = new Mesh();
         copy.name = mesh.name + " Copy";
         copy.vertices = mesh.vertices;
         copy.subMeshCount = mesh.subMeshCount;
         for (int i = 0; i < copy.subMeshCount; i++) {
            copy.SetTriangles(mesh.GetTriangles(i), i);
         }
         copy.normals = mesh.normals;
         copy.uv = mesh.uv;
         copy.bounds = mesh.bounds;
         copy.tangents = mesh.tangents;

         return copy;
      }
      public static Mesh CopyMeshToAsset(Mesh mesh) {
         Mesh meshAsset = CopyMesh(mesh);
         string path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + meshAsset.name + ".mesh");
         AssetDatabase.CreateAsset(meshAsset, path);
         AssetDatabase.SaveAssets();
         Debug.Log("Mesh copy written to \"" + path + "\"");

         return meshAsset;
      }
      public static GameObject CopyToStaticPrefab(string name, Mesh mesh, Material[] materials) {
         Mesh meshAsset = CopyMeshToAsset(mesh);
         GameObject prefab = new GameObject(name + " Static Copy", typeof(MeshFilter), typeof(MeshRenderer));
         prefab.hideFlags = HideFlags.HideInHierarchy;
         prefab.GetComponent<MeshFilter>().sharedMesh = meshAsset;
         prefab.GetComponent<MeshRenderer>().sharedMaterials = materials;
         string path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + prefab.name + ".prefab");
         GameObject result = PrefabUtility.SaveAsPrefabAsset(prefab, path);
         GameObject.DestroyImmediate(prefab);
         Debug.Log("Static Prefab written to \"" + path + "\"");

         return result;
      }

      public static void GUIDebugVertices(Vector3 position, Quaternion rotation, Vector3[] vertices, Color color) {
         float size = HandleUtility.GetHandleSize(position);
         float verticesSize = size * 0.05f;
         float labelOffset = size * 0.1f;
         if (debugStyle == null) {
            debugStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            debugStyle.fontSize = 10;
            debugStyle.fontStyle = FontStyle.Bold;
         }
         debugStyle.normal.textColor = color;
         Color originalColor = Handles.color;
         Handles.color = color;
         for (int i = 0; i < vertices.Length; i++) {
            Vector3 pos = position + rotation * vertices[i];
            Handles.DrawSolidDisc(pos, -Camera.main.transform.forward, verticesSize);
            Handles.Label(pos + Vector3.right * labelOffset, "[" + i.ToString() + "]", debugStyle);
         }
         Handles.color = originalColor;
      }
   }

   public class ProcHandles {

      private static Vector2 s_StartMousePosition, s_CurrentMousePosition;
      private static Vector3 s_StartPosition;
      private static bool s_MouseDown;
      private static int s_MousePassID;
      private static List<int> s_PassedIDs;

      public static Vector3 FreeMove(int id, Vector3 position, Quaternion rotation, float size, Vector3 snap, Handles.CapFunction handleFunction) {
         //Vector3 worldPosition = Handles.matrix.MultiplyPoint(position);
         Vector3 forward = rotation * (Vector3.forward * 1.8f);
         Vector3 worldPosition = Handles.matrix.MultiplyPoint(position + forward * (size / 2));
         Matrix4x4 origMatrix = Handles.matrix;

         //VertexSnapping.HandleMouseMove(id);

         Event evt = Event.current;
         switch (evt.GetTypeForControl(id)) {
            case EventType.Layout:
               Handles.matrix = Matrix4x4.identity;
               handleFunction(id, worldPosition, rotation, size, EventType.Layout);
               Handles.DrawLine(position, worldPosition);
               Handles.matrix = origMatrix;
               break;
            case EventType.MouseDown:
               if (HandleUtility.nearestControl == id && evt.button == 0) {
                  GUIUtility.hotControl = id;
                  s_CurrentMousePosition = s_StartMousePosition = evt.mousePosition;
                  s_StartPosition = position;
                  //HandleUtility.ignoreRaySnapObjects = null;
                  evt.Use();
                  EditorGUIUtility.SetWantsMouseJumping(1);
               }
               break;
            case EventType.MouseDrag:
               if (GUIUtility.hotControl == id) {
                  bool rayDrag = EditorGUI.actionKey && evt.shift;

                  if (rayDrag) {
                     //if (HandleUtility.ignoreRaySnapObjects == null)
                     //   Handles.SetupIgnoreRaySnapObjects();

                     object hit = HandleUtility.RaySnap(HandleUtility.GUIPointToWorldRay(evt.mousePosition));
                     if (hit != null) {
                        RaycastHit rh = (RaycastHit)hit;
                        float offset = 0;
                        /*if (Tools.pivotMode == PivotMode.Center) {
                           float geomOffset = HandleUtility.CalcRayPlaceOffset(HandleUtility.ignoreRaySnapObjects, rh.normal);
                           if (geomOffset != Mathf.Infinity)
                           {
                              offset = Vector3.Dot(position, rh.normal) - geomOffset;
                           }
                        }*/
                        position = Handles.inverseMatrix.MultiplyPoint(rh.point + (rh.normal * offset));
                     }
                     else {
                        rayDrag = false;
                     }
                  }

                  if (!rayDrag) {
                     s_CurrentMousePosition += new Vector2(evt.delta.x, -evt.delta.y) * EditorGUIUtility.pixelsPerPoint;
                     Vector3 screenPos = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(s_StartPosition));
                     screenPos += (Vector3)(s_CurrentMousePosition - s_StartMousePosition);
                     position = Handles.inverseMatrix.MultiplyPoint(Camera.current.ScreenToWorldPoint(screenPos));

                     if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
                        position.z = s_StartPosition.z;
                     if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
                        position.y = s_StartPosition.y;
                     if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
                        position.x = s_StartPosition.x;

                     /*if (Tools.vertexDragging) {
                        if (HandleUtility.ignoreRaySnapObjects == null)
                           Handles.SetupIgnoreRaySnapObjects();
                        Vector3 near;
                        if (HandleUtility.FindNearestVertex(evt.mousePosition, null, out near))
                        {
                           position = Handles.inverseMatrix.MultiplyPoint(near);
                        }
                     }*/

                     if (EditorGUI.actionKey && !evt.shift) {
                        Vector3 delta = position - s_StartPosition;
                        delta.x = Handles.SnapValue(delta.x, snap.x);
                        delta.y = Handles.SnapValue(delta.y, snap.y);
                        delta.z = Handles.SnapValue(delta.z, snap.z);
                        position = s_StartPosition + delta;
                     }
                  }
                  GUI.changed = true;
                  evt.Use();
               }
               break;
            case EventType.MouseUp:
               if (GUIUtility.hotControl == id && (evt.button == 0 || evt.button == 2)) {
                  GUIUtility.hotControl = 0;
                  //HandleUtility.ignoreRaySnapObjects = null;
                  evt.Use();
                  EditorGUIUtility.SetWantsMouseJumping(0);
               }
               break;
            case EventType.MouseMove:
               if (id == HandleUtility.nearestControl)
                  HandleUtility.Repaint();
               break;
            case EventType.Repaint:
               Color temp = Color.white;

               if (id == GUIUtility.hotControl) {
                  temp = Handles.color;
                  Handles.color = Handles.selectedColor;
               }
               else if (id == HandleUtility.nearestControl && GUIUtility.hotControl == 0) {
                  temp = Handles.color;
                  Handles.color = Handles.preselectionColor;
               }

               Handles.matrix = Matrix4x4.identity;
               handleFunction(id, worldPosition, rotation, size, EventType.Repaint);
               Handles.DrawLine(position, worldPosition);
               Handles.matrix = origMatrix;

               if (id == GUIUtility.hotControl || id == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
                  Handles.color = temp;
               break;
         }
         return position;
      }

      public static float ArcHandle(int id, float size, float start, float offset, float outerRadius, float innerRadius, int resolution,  Vector3 position, Quaternion rotation, float handleSize, Vector3 handleSnap, Handles.CapFunction cap) {
         if (size < 0f)
            size = 0f;
         else if (size > 360f)
            size = 360f;

         Vector3 sizePos = ProcUtility.RotateAxis(Vector3.up * outerRadius, start + size);
         sizePos = rotation * sizePos;
         sizePos += position;

         sizePos = Handles.FreeMoveHandle(id, sizePos, Quaternion.identity, handleSize, handleSnap, cap);
         if (innerRadius == 0f) {
            Vector3[] arc = ProcUtility.CornyArc(start, size, 0f, outerRadius, resolution, position, rotation);
            if (arc.Length > 0) {
               Handles.DrawLine(position, arc[0]);
               Handles.DrawPolyLine(arc);
            }
            Handles.DrawLine(position, sizePos);
         }
         else {
            Vector3[] outerArc = ProcUtility.CornyArc(start, size, 0f, outerRadius, resolution, position, rotation);
            Vector3[] innerArc = ProcUtility.CornyArc(start, size, 0f, innerRadius, resolution, position, rotation);
            if (innerArc.Length > 0) {
               Handles.DrawLine(innerArc[0], outerArc[0]);
               Handles.DrawPolyLine(outerArc);
               Handles.DrawPolyLine(innerArc);
            }
            Handles.DrawLine(position + rotation * ProcUtility.RotateAxis(Vector3.up * innerRadius, start + size), sizePos);
         }

         sizePos -= position;
         sizePos = Quaternion.Inverse(rotation) * sizePos;
         sizePos = ProcUtility.RotateVector(sizePos, -start);
         Vector2 zeroPos = Vector2.up * outerRadius;
         size = -Vector2.SignedAngle(zeroPos, sizePos);
         
         while (size < 0f)
            size += 360f;

         return size;
      }
      public static float ArcHandle(float size, float start, float offset, float outerRadius, float innerRadius, int resolution, Vector3 position, Quaternion rotation, float handleSize, Vector3 handleSnap, Handles.CapFunction cap) {
         int id = GUIUtility.GetControlID(FocusType.Passive);
         return ArcHandle(id, size, start, offset, outerRadius, innerRadius, resolution, position, rotation, handleSize, handleSnap, cap);
      }

      public static float LineHandle(int id, float length, Vector3 position, Quaternion rotation, float handleSize, Vector3 handleSnap, Handles.CapFunction cap) {
         Vector3 lengthPos = Vector3.forward * length;
         lengthPos = rotation * lengthPos;
         lengthPos += position;

         lengthPos = FreeMove(id, lengthPos, rotation, handleSize, handleSnap, cap);
         //Handles.DrawLine(position, lengthPos);

         lengthPos -= position;
         lengthPos = Quaternion.Inverse(rotation) * lengthPos;

         return lengthPos.z;
      }
      public static float LineHandle(float length, Vector3 position, Quaternion rotation, float handleSize, Vector3 handleSnap, Handles.CapFunction cap)
      {
         int id = GUIUtility.GetControlID(FocusType.Passive);
         return LineHandle(id, length, position, rotation, handleSize, handleSnap, cap);
      }
   }

}
