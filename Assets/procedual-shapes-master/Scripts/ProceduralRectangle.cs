
using UnityEngine;

namespace ProceduralShapes {

   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   [ExecuteInEditMode]

   public class ProceduralRectangle : MonoBehaviour {

      private MeshFilter filter;
      private bool needUpdate;
      private float roundnessStep;
      private Vector2 halfSize;
      private Vector2 borderedHalfSize;
      private Vector2 roundedHalfSize;
      private float borderedRoundness;
      
      [SerializeField] private Vector3[] vertices = new Vector3[0];
      [SerializeField] private Vector3[] normals = new Vector3[0];
      [SerializeField] private Vector2[] uv = new Vector2[0];
      [SerializeField] private int[] mainTris = new int[0];
      [SerializeField] private int[] borderTris = new int[0];
      [SerializeField] private int skippedVertices;
      [SerializeField] private int skippedTris;

      public bool immediateUpdate = false;
      public Vector2 size = new Vector2(1.2f, 0.8f);
      public float border = 0.1f;
      public float roundness = 0.25f;
      public int resolution = 6;
      public bool skipMainArea = false;
      public bool skipBorderArea = false;

      public Vector2 readSize { get; private set; }
      public float readBorder { get; private set; }
      public float readRoundness { get; private set; }
      public int readResolution { get; private set; }

      public Vector2 clampedSize { get; private set; }
      public float clampedBorder { get; private set; }
      public float clampedRoundness { get; private set; }
      public int clampedResolution { get; private set; }

      public bool NeedUpdate { get { return needUpdate; } }
      public Vector3[] Vertices { get { return vertices; } }
      public Vector3[] Normals { get { return normals; } }
      public Vector2[] UV { get { return uv; } }
      public int[] MainTrIs { get { return mainTris; } }
      public int[] BorderTrIs { get { return borderTris; } }
      public int SkippedVertices { get { return skippedVertices; } }
      public int SkippedTrIs { get { return skippedTris; } }
      public Mesh Mesh { get {
         if (filter)
            return filter.sharedMesh;
         else
            return null;
      } }
      
      void Start() {
         filter = GetComponent<MeshFilter>();
         if (filter) {
            if (vertices.Length > 0 && !filter.sharedMesh)
               CreateMesh(true);
         }
      }
      void LateUpdate() {
         if (filter) {
            if (immediateUpdate) {
               ReadUserValues(out needUpdate);
               if (needUpdate) {
                  ClampUserValues();
                  UpdateMesh();
               }
            }
         }
      }

      private void ReadUserValues(out bool valuesChanged) {
         valuesChanged = true;

         if (readSize != size)
            valuesChanged = true;
         else if (readBorder != border)
            valuesChanged = true;
         else if (readRoundness != roundness)
            valuesChanged = true;
         else if (readResolution != resolution)
            valuesChanged = true;

         if (!valuesChanged)
            return;

         readSize = size;
         readBorder = border;
         readRoundness = roundness;
         readResolution = resolution;
      }
      private void ClampUserValues() {
         clampedSize = readSize;
         clampedBorder = readBorder;
         clampedRoundness = readRoundness;
         clampedResolution = readResolution;

         float x = clampedSize.x;
         float y = clampedSize.y;
         if (x < 0f)
            x = 0f;
         if (y < 0f)
            y = 0f;
         clampedSize = new Vector2(x, y);

         halfSize = clampedSize / 2f;

         if (clampedBorder < 0f)
            clampedBorder = 0f;
         if (clampedBorder > halfSize.x)
            clampedBorder = halfSize.x;
         if (clampedBorder > halfSize.y)
            clampedBorder = halfSize.y;

         if (clampedRoundness < 0f)
            clampedRoundness = 0f;
         if (clampedRoundness > halfSize.x)
            clampedRoundness = halfSize.x;
         if (clampedRoundness > halfSize.y)
            clampedRoundness = halfSize.y;

         if (clampedResolution < 1)
            clampedResolution = 1;
      }

      private void GenericMath() {
         borderedHalfSize = new Vector2(halfSize.x - clampedBorder, halfSize.y - clampedBorder);
         roundedHalfSize = new Vector2(halfSize.x - clampedRoundness, halfSize.y - clampedRoundness);
         borderedRoundness = clampedRoundness - clampedBorder;
         roundnessStep = 90f / clampedResolution;
      }
      private void Indices() {
         if (clampedSize == Vector2.zero) {
            /// Null
            vertices = new Vector3[0];
            mainTris = new int[0];
            borderTris = new int[0];
         }
         else {
            if (clampedRoundness == 0f) {
               /// No Roundness
               if (clampedBorder == 0f) {
                  /// No Border
                  vertices = new Vector3[4];
                  mainTris = new int[2 * 3];
                  borderTris = new int[0];
               }
               else {
                  /// With Border
                  if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
                     vertices = new Vector3[4];
                     mainTris = new int[0];
                     borderTris = new int[2 * 3];
                  }
                  else {
                     vertices = new Vector3[8];
                     mainTris = new int[2 * 3];
                     borderTris = new int[8 * 3];
                  }
               }
            }
            else {
               /// With Roundness
               if (clampedBorder == 0f) {
                  /// No Border
                  if (roundedHalfSize.x == 0f && roundedHalfSize.y == 0f) {
                     vertices = new Vector3[5 + ((clampedResolution - 1) * 4)];
                     mainTris = new int[(clampedResolution * 4) * 3];
                     borderTris = new int[0];
                  }
                  else if (roundedHalfSize.x == 0f || roundedHalfSize.y == 0f) {
                     vertices = new Vector3[8 + (((clampedResolution - 1) * 2) * 4)];
                     mainTris = new int[(2 + (clampedResolution * 4)) * 3];
                     borderTris = new int[0];
                  }
                  else {
                     vertices = new Vector3[12 + ((clampedResolution - 1) * 4)];
                     mainTris = new int[(10 + (clampedResolution * 4)) * 3];
                     borderTris = new int[0];
                  }
               }
               else {
                  /// With Border
                  if (clampedRoundness > clampedBorder) {
                     /// Roundness > Border
                     if (roundedHalfSize.x == 0f && roundedHalfSize.y == 0f) {
                        vertices = new Vector3[9 + (((clampedResolution - 1) * 2) * 4)];
                        mainTris = new int[(clampedResolution * 4) * 3];
                        borderTris = new int[((clampedResolution * 2) * 4) * 3];
                     }
                     else if (roundedHalfSize.x == 0f || roundedHalfSize.y == 0f) {
                        vertices = new Vector3[14 + (((clampedResolution - 1) * 2) * 4)];
                        mainTris = new int[(2 + (clampedResolution * 4)) * 3];
                        borderTris = new int[(4 + ((clampedResolution * 2) * 4)) * 3];
                     }
                     else {
                        vertices = new Vector3[20 + (((clampedResolution - 1) * 2) * 4)];
                        mainTris = new int[(10 + (clampedResolution * 4)) * 3];
                        borderTris = new int[(8 + ((clampedResolution * 2) * 4)) * 3];
                     }
                  }
                  else if (clampedRoundness == clampedBorder) {
                     /// Roundness = Border
                     if (borderedHalfSize.x == 0f && borderedHalfSize.y == 0f) {
                        vertices = new Vector3[5 + ((clampedResolution - 1) * 4)];
                        mainTris = new int[0];
                        borderTris = new int[(clampedResolution * 4) * 3];
                     }
                     else if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
                        vertices = new Vector3[8 + (((clampedResolution - 1) * 2) * 4)];
                        mainTris = new int[0];
                        borderTris = new int[(2 + (clampedResolution * 4)) * 3];
                     }
                     else {
                        vertices = new Vector3[12 + ((clampedResolution - 1) * 4)];
                        mainTris = new int[2 * 3];
                        borderTris = new int[(8 + (clampedResolution * 4)) * 3];
                     }
                  }
                  else {
                     /// Roundness < Border
                     if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
                        vertices = new Vector3[12 + ((clampedResolution - 1) * 4)];
                        mainTris = new int[0];
                        borderTris = new int[(10 + (clampedResolution * 4)) * 3];
                     }
                     else {
                        vertices = new Vector3[16 + ((clampedResolution - 1) * 4)];
                        mainTris = new int[2 * 3];
                        borderTris = new int[(16 + (clampedResolution * 4)) * 3];
                     }
                  }
               }
            }
         }
         normals = new Vector3[vertices.Length];
         uv = new Vector2[vertices.Length];
      }
      private void MainArea() {
         if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f)
            return;

         int n = 0;
         if (clampedRoundness == 0f || clampedRoundness <= clampedBorder) {
            #region # No Roundness / Roundness <= Border
            /// No Roundness / Roundness <= Border
            vertices[0] = new Vector3(-borderedHalfSize.x, borderedHalfSize.y);
            vertices[1] = new Vector3(borderedHalfSize.x, borderedHalfSize.y);
            vertices[2] = new Vector3(borderedHalfSize.x, -borderedHalfSize.y);
            vertices[3] = new Vector3(-borderedHalfSize.x, -borderedHalfSize.y);
            skippedVertices -= 4;

            mainTris[0] = 3;
            mainTris[1] = 0;
            mainTris[2] = 1;
            mainTris[3] = 1;
            mainTris[4] = 2;
            mainTris[5] = 3;
            skippedTris -= 6;
            #endregion
         }
         else if (roundedHalfSize == Vector2.zero) {
            #region # Circle Roundness
            /// Circle Roundness
            Vector3 roundnessVector = new Vector3(0f, borderedRoundness);

            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < clampedResolution * 4; i++) {
               float rotation = roundnessStep * i;
               vertices[1 + i] = ProcUtility.RotateAxis(roundnessVector, rotation);
               skippedVertices--;
            }
            n = 0;
            for (int i = 0; i < clampedResolution * 4; i++) {
               mainTris[0 + n] = 0;
               mainTris[1 + n] = 1 + i;
               mainTris[2 + n] = 2 + i;
               n += 3;
               skippedTris -= 3;
            }
            mainTris[2 + n - 3] = 1;
            #endregion
         }
         else if (roundedHalfSize.x > 0f && roundedHalfSize.y > 0f) {
            #region # Regular Roundness
            /// Regular Roundness
            vertices[0] = new Vector3(-roundedHalfSize.x, roundedHalfSize.y);
            vertices[1] = new Vector3(roundedHalfSize.x, roundedHalfSize.y);
            vertices[2] = new Vector3(roundedHalfSize.x, -roundedHalfSize.y);
            vertices[3] = new Vector3(-roundedHalfSize.x, -roundedHalfSize.y);
            vertices[4] = new Vector3(-roundedHalfSize.x, borderedHalfSize.y);
            skippedVertices -= 5;

            Vector3 roundnessVector0 = new Vector3(0f, borderedRoundness);
            Vector3 roundnessVector1 = new Vector3(borderedRoundness, 0f);
            Vector3 roundnessVector2 = new Vector3(0f, -borderedRoundness);
            Vector3 roundnessVector3 = new Vector3(-borderedRoundness, 0f);

            n = 0;
            for (int i = 0; i < clampedResolution + 1; i++) {
               float rotation = roundnessStep * i;
               vertices[5 + n] = ProcUtility.RotateAxis(roundnessVector0, rotation) + vertices[1];
               vertices[6 + clampedResolution + n] = ProcUtility.RotateAxis(roundnessVector1, rotation) + vertices[2];
               vertices[7 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(roundnessVector2, rotation) + vertices[3];
               skippedVertices -= 3;
               if (i < clampedResolution) {
                  vertices[8 + (clampedResolution * 3) + n] = ProcUtility.RotateAxis(roundnessVector3, rotation) + vertices[0];
                  skippedVertices--;
               }
               n++;
            }

            mainTris[0] = 3;
            mainTris[1] = 0;
            mainTris[2] = 1;
            mainTris[3] = 1;
            mainTris[4] = 2;
            mainTris[5] = 3;
            
            mainTris[6] = 0;
            mainTris[7] = 4;
            mainTris[8] = 5;
            mainTris[9] = 5;
            mainTris[10] = 1;
            mainTris[11] = 0;

            n = 0;
            for (int i = 0; i < clampedResolution; i++) {
               mainTris[12 + n] = 1;
               mainTris[13 + n] = 5 + i;
               mainTris[14 + n] = 6 + i;
               n += 3;
               skippedTris -= 3;
            }

            mainTris[12 + n] = 1;
            mainTris[13 + n] = 5 + clampedResolution;
            mainTris[14 + n] = 6 + clampedResolution;
            mainTris[15 + n] = 6 + clampedResolution;
            mainTris[16 + n] = 2;
            mainTris[17 + n] = 1;

            for (int i = 0; i < clampedResolution; i++) {
               mainTris[18 + n] = 2;
               mainTris[19 + n] = 6 + clampedResolution + i;
               mainTris[20 + n] = 7 + clampedResolution + i;
               n += 3;
               skippedTris -= 3;
            }

            mainTris[18 + n] = 2;
            mainTris[19 + n] = 6 + (clampedResolution * 2);
            mainTris[20 + n] = 7 + (clampedResolution * 2);
            mainTris[21 + n] = 7 + (clampedResolution * 2);
            mainTris[22 + n] = 3;
            mainTris[23 + n] = 2;

            for (int i = 0; i < clampedResolution; i++) {
               mainTris[24 + n] = 3;
               mainTris[25 + n] = 7 + (clampedResolution * 2) + i;
               mainTris[26 + n] = 8 + (clampedResolution * 2) + i;
               n += 3;
               skippedTris -= 3;
            }

            mainTris[24 + n] = 3;
            mainTris[25 + n] = 7 + (clampedResolution * 3);
            mainTris[26 + n] = 8 + (clampedResolution * 3);
            mainTris[27 + n] = 8 + (clampedResolution * 3);
            mainTris[28 + n] = 0;
            mainTris[29 + n] = 3;
            skippedTris -= 30;

            for (int i = 0; i < clampedResolution; i++) {
               mainTris[30 + n] = 0;
               mainTris[31 + n] = 8 + (clampedResolution * 3) + i;
               mainTris[32 + n] = 9 + (clampedResolution * 3) + i;
               n += 3;
               skippedTris -= 3;
            }
            mainTris[32 + n - 3] = 4;
            #endregion
         }
         else {
            #region # Half Width / Half Height Roundness
            /// Half Width / Half Height Roundness
            Vector3 roundnessVector;
            if (roundedHalfSize.x == 0f) {
               vertices[0] = new Vector3(0f, roundedHalfSize.y);
               vertices[1] = new Vector3(0f, -roundedHalfSize.y);
               vertices[2] = new Vector3(borderedRoundness, roundedHalfSize.y);
               skippedVertices -= 3;

               roundnessVector = new Vector3(borderedRoundness, 0f);
            }
            else {
               vertices[0] = new Vector3(-roundedHalfSize.x, 0f);
               vertices[1] = new Vector3(roundedHalfSize.x, 0f);
               vertices[2] = new Vector3(-roundedHalfSize.x, borderedRoundness);
               skippedVertices -= 3;

               roundnessVector = new Vector3(0f, borderedRoundness);
            }

            n = 0;
            for (int i = 0; i < (clampedResolution * 2) + 1; i++) {
               float rotation = roundnessStep * i;
               vertices[3 + n] = ProcUtility.RotateAxis(roundnessVector, rotation) + vertices[1];
               skippedVertices--;
               if (i < clampedResolution * 2) {
                  vertices[4 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(-roundnessVector, rotation) + vertices[0];
                  skippedVertices--;
               }
               n++;
            }

            mainTris[0] = 4 + (clampedResolution * 2);
            mainTris[1] = 2;
            mainTris[2] = 3;
            mainTris[3] = 3;
            mainTris[4] = 3 + (clampedResolution * 2);
            mainTris[5] = 4 + (clampedResolution * 2);
            skippedTris -= 6;

            n = 0;
            for (int i = 0; i < clampedResolution * 2; i++) {
               mainTris[6 + n] = 1;
               mainTris[7 + n] = 3 + i;
               mainTris[8 + n] = 4 + i;
               n += 3;
               skippedTris -= 3;
            }

            for (int i = 0; i < clampedResolution * 2; i++) {
               mainTris[6 + n] = 0;
               mainTris[7 + n] = 4 + (clampedResolution * 2) + i;
               mainTris[8 + n] = 5 + (clampedResolution * 2) + i;
               n += 3;
               skippedTris -= 3;
            }
            mainTris[8 + n - 3] = 2;
            #endregion
         }
      }
      private void BorderArea() {
         if (clampedBorder == 0f || (halfSize.x == 0f || halfSize.y == 0f))
            return;

         int n = 0;
         if (clampedRoundness == 0f) {
            #region # No Roundness
            /// No Roundness
            if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
               #region # Rectangle Border
               /// Rectangle Border
               vertices[0] = new Vector3(-halfSize.x, halfSize.y);
               vertices[1] = new Vector3(halfSize.x, halfSize.y);
               vertices[2] = new Vector3(halfSize.x, -halfSize.y);
               vertices[3] = new Vector3(-halfSize.x, -halfSize.y);
               skippedVertices -= 4;

               borderTris[0] = 3;
               borderTris[1] = 0;
               borderTris[2] = 1;
               borderTris[3] = 1;
               borderTris[4] = 2;
               borderTris[5] = 3;
               skippedTris -= 6;
               #endregion
            }
            else {
               #region # Bordered Rectangle
               /// Bordered Rectangle
               if (skipMainArea) {
                  vertices[0] = new Vector3(-borderedHalfSize.x, borderedHalfSize.y);
                  vertices[1] = new Vector3(borderedHalfSize.x, borderedHalfSize.y);
                  vertices[2] = new Vector3(borderedHalfSize.x, -borderedHalfSize.y);
                  vertices[3] = new Vector3(-borderedHalfSize.x, -borderedHalfSize.y);
                  skippedVertices -= 4;
               }
               vertices[4] = new Vector3(-halfSize.x, halfSize.y);
               vertices[5] = new Vector3(halfSize.x, halfSize.y);
               vertices[6] = new Vector3(halfSize.x, -halfSize.y);
               vertices[7] = new Vector3(-halfSize.x, -halfSize.y);
               skippedVertices -= 4;

               n = 0;
               for (int i = 0; i < 4; i++) {
                  borderTris[0 + n] = i;
                  borderTris[1 + n] = 4 + i;
                  borderTris[2 + n] = 5 + i;
                  borderTris[3 + n] = 5 + i;
                  borderTris[4 + n] = 1 + i;
                  borderTris[5 + n] = i;
                  n += 6;
               }
               borderTris[2 + n - 6] = 4;
               borderTris[3 + n - 6] = 4;
               borderTris[4 + n - 6] = 0;
               skippedTris -= 24;
               #endregion
            }
            #endregion
         }
         else if (roundedHalfSize == Vector2.zero) {
            #region # Circle Roundness
            /// Circle Roundness
            if (borderedHalfSize == Vector2.zero) {
               #region # Circle Border
               /// Circle Border
               Vector3 roundnessVector = new Vector3(0f, clampedRoundness);

               vertices[0] = Vector3.zero;
               skippedVertices--;
               for (int i = 0; i < clampedResolution * 4; i++) {
                  float rotation = roundnessStep * i;
                  vertices[1 + i] = ProcUtility.RotateAxis(roundnessVector, rotation);
                  skippedVertices--;
               }
               n = 0;
               for (int i = 0; i < clampedResolution * 4; i++) {
                  borderTris[0 + n] = 0;
                  borderTris[1 + n] = 1 + i;
                  borderTris[2 + n] = 2 + i;
                  n += 3;
                  skippedTris -= 3;
               }
               borderTris[2 + n - 3] = 1;
               #endregion
            }
            else {
               #region # Bordered Circle
               /// Bordered Circle
               Vector3 roundnessVector = new Vector3(0f, clampedRoundness);
               Vector3 borderedRoundnessVector = new Vector3(0f, borderedRoundness);
               
               for (int i = 0; i < clampedResolution * 4; i++) {
                  float rotation = roundnessStep * i;
                  vertices[1 + (clampedResolution * 4) + i] = ProcUtility.RotateAxis(roundnessVector, rotation);
                  skippedVertices--;
                  if (skipMainArea) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRoundnessVector, rotation);
                     skippedVertices--;
                  }
               }

               n = 0;
               for (int i = 0; i < clampedResolution * 4; i++) {
                  borderTris[0 + n] = 1 + i;
                  borderTris[1 + n] = 1 + (clampedResolution * 4) + i;
                  borderTris[2 + n] = 2 + (clampedResolution * 4) + i;
                  borderTris[3 + n] = 2 + (clampedResolution * 4) + i;
                  borderTris[4 + n] = 2 + i;
                  borderTris[5 + n] = 1 + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[2 + n - 6] = 1 + (clampedResolution * 4);
               borderTris[3 + n - 6] = 1 + (clampedResolution * 4);
               borderTris[4 + n - 6] = 1;

               #endregion
            }
            #endregion
         }
         else if (roundedHalfSize.x > 0f && roundedHalfSize.y > 0f) {
            #region # Regular Roundness
            /// Regular Roundness
            if (clampedRoundness <= clampedBorder) {
               #region # Roundness <= Border
               /// Roundness <= Border
               int v = 0;
               if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
                  vertices[0] = new Vector3(-roundedHalfSize.x, roundedHalfSize.y);
                  vertices[1] = new Vector3(roundedHalfSize.x, roundedHalfSize.y);
                  vertices[2] = new Vector3(roundedHalfSize.x, -roundedHalfSize.y);
                  vertices[3] = new Vector3(-roundedHalfSize.x, -roundedHalfSize.y);
                  skippedVertices -= 4;
               }
               else {
                  if (skipMainArea) {
                     vertices[0] = new Vector3(-borderedHalfSize.x, borderedHalfSize.y);
                     vertices[1] = new Vector3(borderedHalfSize.x, borderedHalfSize.y);
                     vertices[2] = new Vector3(borderedHalfSize.x, -borderedHalfSize.y);
                     vertices[3] = new Vector3(-borderedHalfSize.x, -borderedHalfSize.y);
                     skippedVertices -= 4;
                  }
                  if (clampedRoundness < clampedBorder) {
                     v = 4;
                     vertices[4] = new Vector3(-roundedHalfSize.x, roundedHalfSize.y);
                     vertices[5] = new Vector3(roundedHalfSize.x, roundedHalfSize.y);
                     vertices[6] = new Vector3(roundedHalfSize.x, -roundedHalfSize.y);
                     vertices[7] = new Vector3(-roundedHalfSize.x, -roundedHalfSize.y);
                     skippedVertices -= 4;
                  }
               }
               vertices[v + 4] = new Vector3(-roundedHalfSize.x, halfSize.y);
               skippedVertices--;

               Vector3 roundnessVector0 = new Vector3(0f, clampedRoundness);
               Vector3 roundnessVector1 = new Vector3(clampedRoundness, 0f);
               Vector3 roundnessVector2 = new Vector3(0f, -clampedRoundness);
               Vector3 roundnessVector3 = new Vector3(-clampedRoundness, 0f);

               n = 0;
               for (int i = 0; i < clampedResolution + 1; i++) {
                  float rotation = roundnessStep * i;
                  vertices[v + 5 + n] = ProcUtility.RotateAxis(roundnessVector0, rotation) + vertices[v + 1];
                  vertices[v + 6 + clampedResolution + n] = ProcUtility.RotateAxis(roundnessVector1, rotation) + vertices[v + 2];
                  vertices[v + 7 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(roundnessVector2, rotation) + vertices[v + 3];
                  skippedVertices -= 3;
                  if (i < clampedResolution) {
                     vertices[v + 8 + (clampedResolution * 3) + n] = ProcUtility.RotateAxis(roundnessVector3, rotation) + vertices[v];
                     skippedVertices--;
                  }
                  n++;
               }

               n = 0;
               if (borderedHalfSize.x == 0f || borderedHalfSize.y == 0f) {
                  borderTris[0] = 3;
                  borderTris[1] = 0;
                  borderTris[2] = 1;
                  borderTris[3] = 1;
                  borderTris[4] = 2;
                  borderTris[5] = 3;
                  n += 6;
                  skippedTris -= 6;
               }
               else if (clampedRoundness < clampedBorder) {
                  for (int i = 0; i < 4; i++) {
                     borderTris[n] = i;
                     borderTris[1 + n] = 4 + i;
                     borderTris[2 + n] = 5 + i;
                     borderTris[3 + n] = 5 + i;
                     borderTris[4 + n] = 1 + i;
                     borderTris[5 + n] = i;
                     n += 6;
                  }
                  borderTris[2 + n - 6] = 4;
                  borderTris[3 + n - 6] = 4;
                  borderTris[4 + n - 6] = 0;
                  skippedTris -= 24;
               }

               borderTris[n] = v;
               borderTris[1 + n] = v + 4;
               borderTris[2 + n] = v + 5;
               borderTris[3 + n] = v + 5;
               borderTris[4 + n] = v + 1;
               borderTris[5 + n] = v;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[6 + n] = v + 1;
                  borderTris[7 + n] = v + 5 + i;
                  borderTris[8 + n] = v + 6 + i;
                  n += 3;
                  skippedTris -= 3;
               }

               borderTris[6 + n] = v + 1;
               borderTris[7 + n] = v + 5 + clampedResolution;
               borderTris[8 + n] = v + 6 + clampedResolution;
               borderTris[9 + n] = v + 6 + clampedResolution;
               borderTris[10 + n] = v + 2;
               borderTris[11 + n] = v + 1;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[12 + n] = v + 2;
                  borderTris[13 + n] = v + 6 + clampedResolution + i;
                  borderTris[14 + n] = v + 7 + clampedResolution + i;
                  n += 3;
                  skippedTris -= 3;
               }

               borderTris[12 + n] = v + 2;
               borderTris[13 + n] = v + 6 + (clampedResolution * 2);
               borderTris[14 + n] = v + 7 + (clampedResolution * 2);
               borderTris[15 + n] = v + 7 + (clampedResolution * 2);
               borderTris[16 + n] = v + 3;
               borderTris[17 + n] = v + 2;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[18 + n] = v + 3;
                  borderTris[19 + n] = v + 7 + (clampedResolution * 2) + i;
                  borderTris[20 + n] = v + 8 + (clampedResolution * 2) + i;
                  n += 3;
                  skippedTris -= 3;
               }

               borderTris[18 + n] = v + 3;
               borderTris[19 + n] = v + 7 + (clampedResolution * 3);
               borderTris[20 + n] = v + 8 + (clampedResolution * 3);
               borderTris[21 + n] = v + 8 + (clampedResolution * 3);
               borderTris[22 + n] = v;
               borderTris[23 + n] = v + 3;
               skippedTris -= 24;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[24 + n] = v;
                  borderTris[25 + n] = v + 8 + (clampedResolution * 3) + i;
                  borderTris[26 + n] = v + 9 + (clampedResolution * 3) + i;
                  n += 3;
                  skippedTris -= 3;
               }
               borderTris[26 + n - 3] = v + 4;
               #endregion
            }
            else {
               #region # Roundness > Border
               /// Roundness > Border

               Vector3 v0 = new Vector3(-roundedHalfSize.x, roundedHalfSize.y);
               Vector3 v1 = new Vector3(roundedHalfSize.x, roundedHalfSize.y);
               Vector3 v2 = new Vector3(roundedHalfSize.x, -roundedHalfSize.y);
               Vector3 v3 = new Vector3(-roundedHalfSize.x, -roundedHalfSize.y);
               Vector3 roundnessVector0 = new Vector3(0f, clampedRoundness);
               Vector3 roundnessVector1 = new Vector3(clampedRoundness, 0f);
               Vector3 roundnessVector2 = new Vector3(0f, -clampedRoundness);
               Vector3 roundnessVector3 = new Vector3(-clampedRoundness, 0f);
               Vector3 borderedRoundnessVector0 = new Vector3(0f, borderedRoundness);
               Vector3 borderedRoundnessVector1 = new Vector3(borderedRoundness, 0f);
               Vector3 borderedRoundnessVector2 = new Vector3(0f, -borderedRoundness);
               Vector3 borderedRoundnessVector3 = new Vector3(-borderedRoundness, 0f);

               if (skipMainArea) {
                  vertices[4] = new Vector3(-roundedHalfSize.x, borderedHalfSize.y);
                  skippedVertices--;
               }
               vertices[8 + (clampedResolution * 4)] = new Vector3(-roundedHalfSize.x, halfSize.y);
               skippedVertices--;

               n = 0;
               for (int i = 0; i < clampedResolution + 1; i++) {
                  float rotation = roundnessStep * i;
                  if (skipMainArea) {
                     vertices[5 + n] = ProcUtility.RotateAxis(borderedRoundnessVector0, rotation) + v1;
                     vertices[6 + clampedResolution + n] = ProcUtility.RotateAxis(borderedRoundnessVector1, rotation) + v2;
                     vertices[7 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(borderedRoundnessVector2, rotation) + v3;
                     skippedVertices -= 3;
                     if (i < clampedResolution) {
                        vertices[8 + (clampedResolution * 3) + n] = ProcUtility.RotateAxis(borderedRoundnessVector3, rotation) + v0;
                        skippedVertices--;
                     } 
                  }
                  vertices[9 + (clampedResolution * 4) + n] = ProcUtility.RotateAxis(roundnessVector0, rotation) + v1;
                  vertices[10 + (clampedResolution * 5) + n] = ProcUtility.RotateAxis(roundnessVector1, rotation) + v2;
                  vertices[11 + (clampedResolution * 6) + n] = ProcUtility.RotateAxis(roundnessVector2, rotation) + v3;
                  skippedVertices -= 3;
                  if (i < clampedResolution) {
                     vertices[12 + (clampedResolution * 7) + n] = ProcUtility.RotateAxis(roundnessVector3, rotation) + v0;
                     skippedVertices--;
                  } 
                  n++;
               }

               borderTris[0] = 4;
               borderTris[1] = 8 + (clampedResolution * 4);
               borderTris[2] = 9 + (clampedResolution * 4);
               borderTris[3] = 9 + (clampedResolution * 4);
               borderTris[4] = 5;
               borderTris[5] = 4;

               n = 0;
               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[6 + n] = 5 + i;
                  borderTris[7 + n] = 9 + (clampedResolution * 4) + i;
                  borderTris[8 + n] = 10 + (clampedResolution * 4) + i;
                  borderTris[9 + n] = 10 + (clampedResolution * 4) + i;
                  borderTris[10 + n] = 6 + i;
                  borderTris[11 + n] = 5 + i;
                  n += 6;
                  skippedTris -= 6;
               }

               borderTris[6 + n] = 5 + clampedResolution;
               borderTris[7 + n] = 9 + (clampedResolution * 5);
               borderTris[8 + n] = 10 + (clampedResolution * 5);
               borderTris[9 + n] = 10 + (clampedResolution * 5);
               borderTris[10 + n] = 6 + clampedResolution;
               borderTris[11 + n] = 5 + clampedResolution;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[12 + n] = 6 + clampedResolution + i;
                  borderTris[13 + n] = 10 + (clampedResolution * 5) + i;
                  borderTris[14 + n] = 11 + (clampedResolution * 5) + i;
                  borderTris[15 + n] = 11 + (clampedResolution * 5) + i;
                  borderTris[16 + n] = 7 + clampedResolution + i;
                  borderTris[17 + n] = 6 + clampedResolution + i;
                  n += 6;
                  skippedTris -= 6;
               }

               borderTris[12 + n] = 6 + (clampedResolution * 2);
               borderTris[13 + n] = 10 + (clampedResolution * 6);
               borderTris[14 + n] = 11 + (clampedResolution * 6);
               borderTris[15 + n] = 11 + (clampedResolution * 6);
               borderTris[16 + n] = 7 + (clampedResolution * 2);
               borderTris[17 + n] = 6 + (clampedResolution * 2);

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[18 + n] = 7 + (clampedResolution * 2) + i;
                  borderTris[19 + n] = 11 + (clampedResolution * 6) + i;
                  borderTris[20 + n] = 12 + (clampedResolution * 6) + i;
                  borderTris[21 + n] = 12 + (clampedResolution * 6) + i;
                  borderTris[22 + n] = 8 + (clampedResolution * 2) + i;
                  borderTris[23 + n] = 7 + (clampedResolution * 2) + i;
                  n += 6;
                  skippedTris -= 6;
               }

               borderTris[18 + n] = 7 + (clampedResolution * 3);
               borderTris[19 + n] = 11 + (clampedResolution * 7);
               borderTris[20 + n] = 12 + (clampedResolution * 7);
               borderTris[21 + n] = 12 + (clampedResolution * 7);
               borderTris[22 + n] = 8 + (clampedResolution * 3);
               borderTris[23 + n] = 7 + (clampedResolution * 3);
               skippedTris -= 24;

               for (int i = 0; i < clampedResolution; i++) {
                  borderTris[24 + n] = 8 + (clampedResolution * 3) + i;
                  borderTris[25 + n] = 12 + (clampedResolution * 7) + i;
                  borderTris[26 + n] = 13 + (clampedResolution * 7) + i;
                  borderTris[27 + n] = 13 + (clampedResolution * 7) + i;
                  borderTris[28 + n] = 9 + (clampedResolution * 3) + i;
                  borderTris[29 + n] = 8 + (clampedResolution * 3) + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[26 + n - 6] = 8 + (clampedResolution * 4);
               borderTris[27 + n - 6] = 8 + (clampedResolution * 4);
               borderTris[28 + n - 6] = 4;
               #endregion
            }
            #endregion
         }
         else {
            #region # Half Width / Half Height Roundness
            /// Half Width / Half Height Roundness
            if (clampedRoundness == clampedBorder) {
               #region # Roundness == Border
               /// Roundness == Border
               Vector3 roundnessVector;
               if (roundedHalfSize.x == 0f) {
                  vertices[0] = new Vector3(0f, roundedHalfSize.y);
                  vertices[1] = new Vector3(0f, -roundedHalfSize.y);
                  vertices[2] = new Vector3(halfSize.x, roundedHalfSize.y);
                  skippedVertices -= 3;

                  roundnessVector = new Vector3(clampedRoundness, 0f);
               }
               else {
                  vertices[0] = new Vector3(-roundedHalfSize.x, 0f);
                  vertices[1] = new Vector3(roundedHalfSize.x, 0f);
                  vertices[2] = new Vector3(-roundedHalfSize.x, halfSize.y);
                  skippedVertices -= 3;

                  roundnessVector = new Vector3(0f, clampedRoundness);
               }

               n = 0;
               for (int i = 0; i < (clampedResolution * 2) + 1; i++) {
                  float rotation = roundnessStep * i;
                  vertices[3 + n] = ProcUtility.RotateAxis(roundnessVector, rotation) + vertices[1];
                  skippedVertices--;
                  if (i < clampedResolution * 2) {
                     vertices[4 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(-roundnessVector, rotation) + vertices[0];
                     skippedVertices--;
                  }
                  n++;
               }

               borderTris[0] = 4 + (clampedResolution * 2);
               borderTris[1] = 2;
               borderTris[2] = 3;
               borderTris[3] = 3;
               borderTris[4] = 3 + (clampedResolution * 2);
               borderTris[5] = 4 + (clampedResolution * 2);
               skippedTris -= 6;

               n = 0;
               for (int i = 0; i < clampedResolution * 2; i++) {
                  borderTris[6 + n] = 1;
                  borderTris[7 + n] = 3 + i;
                  borderTris[8 + n] = 4 + i;
                  n += 3;
                  skippedTris -= 3;
               }

               for (int i = 0; i < clampedResolution * 2; i++) {
                  borderTris[6 + n] = 0;
                  borderTris[7 + n] = 6 + ((clampedResolution - 1) * 2) + i;
                  borderTris[8 + n] = 7 + ((clampedResolution - 1) * 2) + i;
                  n += 3;
                  skippedTris -= 3;
               }
               borderTris[8 + n - 3] = 2;
               #endregion
            }
            else {
               #region # Roundness > Border
               /// Roundness > Border
               Vector3 v0;
               Vector3 v1;
               Vector3 roundnessVector;
               Vector3 borderedRoundnessVector;
               if (roundedHalfSize.x == 0f) {
                  v0 = new Vector3(0f, roundedHalfSize.y);
                  v1 = new Vector3(0f, -roundedHalfSize.y);
                  roundnessVector = new Vector3(clampedRoundness, 0f);
                  borderedRoundnessVector = new Vector3(borderedRoundness, 0f);

                  if (skipMainArea) {
                     vertices[2] = new Vector3(borderedHalfSize.x, roundedHalfSize.y);
                     skippedVertices--;
                  }
                  vertices[4 + (clampedResolution * 4)] = new Vector3(halfSize.x, roundedHalfSize.y);
                  skippedVertices--;
               }
               else {
                  v0 = new Vector3(-roundedHalfSize.x, 0f);
                  v1 = new Vector3(roundedHalfSize.x, 0f);
                  roundnessVector = new Vector3(0f, clampedRoundness);
                  borderedRoundnessVector = new Vector3(0f, borderedRoundness);

                  if (skipMainArea) {
                     vertices[2] = new Vector3(-roundedHalfSize.x, borderedHalfSize.y);
                     skippedVertices--;
                  }
                  vertices[4 + (clampedResolution * 4)] = new Vector3(-roundedHalfSize.x, halfSize.y);
                  skippedVertices--;
               }

               n = 0;
               for (int i = 0; i < (clampedResolution * 2) + 1; i++) {
                  float rotation = roundnessStep * i;
                  if (skipMainArea) {
                     vertices[3 + n] = ProcUtility.RotateAxis(borderedRoundnessVector, rotation) + v1;
                     skippedVertices--;
                     if (i < clampedResolution * 2) {
                        vertices[4 + (clampedResolution * 2) + n] = ProcUtility.RotateAxis(-borderedRoundnessVector, rotation) + v0;
                        skippedVertices--;
                     }
                  }
                  vertices[5 + (clampedResolution * 4) + n] = ProcUtility.RotateAxis(roundnessVector, rotation) + v1;
                  skippedVertices--;
                  if (i < clampedResolution * 2) {
                     vertices[6 + (clampedResolution * 6) + n] = ProcUtility.RotateAxis(-roundnessVector, rotation) + v0;
                     skippedVertices--;
                  }
                  n++;
               }

               borderTris[0] = 2;
               borderTris[1] = 4 + (clampedResolution * 4);
               borderTris[2] = 5 + (clampedResolution * 4);
               borderTris[3] = 5 + (clampedResolution * 4);
               borderTris[4] = 3;
               borderTris[5] = 2;

               n = 0;
               for (int i = 0; i < clampedResolution * 2; i++) {
                  borderTris[6 + n] = 3 + i;
                  borderTris[7 + n] = 5 + (clampedResolution * 4) + i;
                  borderTris[8 + n] = 6 + (clampedResolution * 4) + i;
                  borderTris[9 + n] = 6 + (clampedResolution * 4) + i;
                  borderTris[10 + n] = 4 + i;
                  borderTris[11 + n] = 3 + i;
                  n += 6;
                  skippedTris -= 6;
               }

               borderTris[6 + n] = 3 + (clampedResolution * 2);
               borderTris[7 + n] = 5 + (clampedResolution * 6);
               borderTris[8 + n] = 6 + (clampedResolution * 6);
               borderTris[9 + n] = 6 + (clampedResolution * 6);
               borderTris[10 + n] = 4 + (clampedResolution * 2);
               borderTris[11 + n] = 3 + (clampedResolution * 2);
               skippedTris -= 12;

               for (int i = 0; i < clampedResolution * 2; i++) {
                  borderTris[12 + n] = 4 + (clampedResolution * 2) + i;
                  borderTris[13 + n] = 6 + (clampedResolution * 6) + i;
                  borderTris[14 + n] = 7 + (clampedResolution * 6) + i;
                  borderTris[15 + n] = 7 + (clampedResolution * 6) + i;
                  borderTris[16 + n] = 5 + (clampedResolution * 2) + i;
                  borderTris[17 + n] = 4 + (clampedResolution * 2) + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[14 + n - 6] = 4 + (clampedResolution * 4);
               borderTris[15 + n - 6] = 4 + (clampedResolution * 4);
               borderTris[16 + n - 6] = 2;
               #endregion
            }
            #endregion
         }
      }

      private void SetTextureCoordinates() {
         for (int i = 0; i < uv.Length; i++) {
            uv[i] = (vertices[i] + new Vector3(halfSize.x, halfSize.y));
            uv[i] = uv[i] / clampedSize;
         }
      }
      private void SetNormals() {
         for (int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.back;
         }
      }
      
      private void UpdateMesh() {
         GenericMath();

         Indices();
         skippedVertices = vertices.Length;
         skippedTris = mainTris.Length + borderTris.Length;

         if (!skipMainArea)
            MainArea();
         if (!skipBorderArea)
            BorderArea();

         if (skippedVertices != vertices.Length) {
            SetTextureCoordinates();
            SetNormals();
         }

         if (!filter.sharedMesh)
            filter.sharedMesh = new Mesh();

         filter.sharedMesh.name = gameObject.name + " ProceduralRectangle";
         filter.sharedMesh.subMeshCount = 2;

         filter.sharedMesh.SetTriangles(new int[] { }, 1);
         filter.sharedMesh.SetTriangles(new int[] { }, 0);
         filter.sharedMesh.uv = new Vector2[] { };
         filter.sharedMesh.normals = new Vector3[] { };
         filter.sharedMesh.vertices = new Vector3[] { };

         filter.sharedMesh.vertices = vertices;
         filter.sharedMesh.normals = normals;
         filter.sharedMesh.uv = uv;
         filter.sharedMesh.SetTriangles(mainTris, 0);
         filter.sharedMesh.SetTriangles(borderTris, 1);
         
         filter.sharedMesh.RecalculateTangents();

         needUpdate = false;
      }

      public void CreateMesh(bool forceUpdate = false)
      {
         ReadUserValues(out needUpdate);
         if (needUpdate || forceUpdate)
         {
            ClampUserValues();
            UpdateMesh();
         }
      }
   }

}
