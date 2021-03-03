
using UnityEngine;

namespace ProceduralShapes {

   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   [ExecuteInEditMode]

   public class ProceduralRing : MonoBehaviour {

      private MeshFilter filter;
      private bool needUpdate;
      private float ringStep;
      private float ringWidth;
      private float borderedRingWidth;
      private Vector2 outerVector;
      private Vector2 innerVector;
      private Vector2 borderedOuterVector;
      private Vector2 borderedInnerVector;
      private float pieStartDegree;
      private float pieEndDegree;
      private bool hasPieStartVector;
      private bool hasPieEndVector;
      private int pieExtraVertices;
      private int pieVertices;
      private int pieStartVector;
      private int pieEndVector;
      private Vector2 arcLineB;
      private Vector2 ringLineA;
      private Vector2 ringLineB;

      [SerializeField] private Vector3[] vertices = new Vector3[0];
      [SerializeField] private Vector3[] normals = new Vector3[0];
      [SerializeField] private Vector2[] uv = new Vector2[0];
      [SerializeField] private int[] mainTris = new int[0];
      [SerializeField] private int[] borderTris = new int[0];
      [SerializeField] private int[] pieTris = new int[0];
      [SerializeField] private int skippedVertices;
      [SerializeField] private int skippedTris;

      public bool immediateUpdate = false;
      public float outerRadius = 0.5f;
      public float innerRadius = 0.2f;
      public float outerBorder = 0.08f;
      public float innerBorder = 0.08f;
      public int resolution = 18;
      public float ringOffset = 0f;
      public float pie = 0f;
      public float pieOffset = 0f;
      public bool skipMainArea = false;
      public bool skipBorderArea = false;
      public bool skipPieArea = false;

      public float readOuterRadius { get; private set; }
      public float readInnerRadius { get; private set; }
      public float readOuterBorder { get; private set; }
      public float readInnerBorder { get; private set; }
      public float readPie { get; private set; }
      public int readResolution { get; private set; }
      public float readRingOffset { get; private set; }
      public float readPieOffset { get; private set; }

      public float clampedOuterRadius { get; private set; }
      public float clampedInnerRadius { get; private set; }
      public float clampedOuterBorder { get; private set; }
      public float clampedInnerBorder { get; private set; }
      public int clampedResolution { get; private set; }
      public float clampedRingOffset { get; private set; }
      public float clampedPie { get; private set; }
      public float clampedPieOffset { get; private set; }

      public bool NeedUpdate { get { return needUpdate; } }
      public Vector3[] Vertices { get { return vertices; } }
      public Vector3[] Normals { get { return normals; } }
      public Vector2[] UV { get { return uv; } }
      public int[] MainTrIs { get { return mainTris; } }
      public int[] BorderTrIs { get { return borderTris; } }
      public int[] PieTrIs { get { return pieTris; } }
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

         if (readOuterRadius != outerRadius)
            valuesChanged = true;
         else if (readInnerRadius != innerRadius)
            valuesChanged = true;
         else if (readOuterBorder != outerBorder)
            valuesChanged = true;
         else if (readInnerBorder != innerBorder)
            valuesChanged = true;
         else if (readPie != pie)
            valuesChanged = true;
         else if (readResolution != resolution)
            valuesChanged = true;
         else if (readRingOffset != ringOffset)
            valuesChanged = true;
         else if (readPieOffset != pieOffset)
            valuesChanged = true;

         readOuterRadius = outerRadius;
         readInnerRadius = innerRadius;
         readOuterBorder = outerBorder;
         readInnerBorder = innerBorder;
         readPie = pie;
         readResolution = resolution;
         readRingOffset = ringOffset;
         readPieOffset = pieOffset;
      }
      private void ClampUserValues() {
         clampedOuterRadius = readOuterRadius;
         clampedInnerRadius = readInnerRadius;
         clampedOuterBorder = readOuterBorder;
         clampedInnerBorder = readInnerBorder;
         clampedPie = readPie;
         clampedResolution = readResolution;
         clampedRingOffset = readRingOffset;
         clampedPieOffset = readPieOffset;

         if (clampedOuterRadius < 0f)
            clampedOuterRadius = 0f;
         
         if (clampedInnerRadius < 0f)
            clampedInnerRadius = 0f;
         if (clampedInnerRadius > clampedOuterRadius)
            clampedInnerRadius = clampedOuterRadius;

         ringWidth = clampedOuterRadius - clampedInnerRadius;
         
         if (clampedOuterBorder < 0f)
            clampedOuterBorder = 0f;
         if (clampedOuterBorder > ringWidth)
            clampedOuterBorder = ringWidth;
         if (clampedInnerBorder < 0f)
            clampedInnerBorder = 0f;
         if (clampedInnerBorder > ringWidth - clampedOuterBorder)
            clampedInnerBorder = ringWidth - clampedOuterBorder;

         if (clampedPie < 0f)
            clampedPie = 0f;
         if (clampedPie > 360f)
            clampedPie = 360f;

         if (clampedResolution < 3)
            clampedResolution = 3;

         while (clampedRingOffset < 0f)
            clampedRingOffset += 360f;
         while (clampedRingOffset > 360f)
            clampedRingOffset -= 360f;

         while (clampedPieOffset < 0f)
            clampedPieOffset += 360f;
         while (clampedPieOffset > 360f)
            clampedPieOffset -= 360f;
      }

      private void GenericMath() {
         ringStep = 360f / clampedResolution;
         borderedRingWidth = ringWidth - clampedOuterBorder - clampedInnerBorder;
         // ringWidth set in ClampUserValues()
      }
      private void PieMath() {
         pieStartDegree = clampedPieOffset;
         while (pieStartDegree >= 360f) pieStartDegree -= 360f;

         pieEndDegree = clampedPieOffset + clampedPie;
         while (pieEndDegree >= 360f) pieEndDegree -= 360f;

         hasPieStartVector = false;
         float cheapPieStartVector = pieStartDegree / ringStep;
         hasPieStartVector = cheapPieStartVector != Mathf.Floor(cheapPieStartVector);

         hasPieEndVector = false;
         float cheapPieEndVector = pieEndDegree / ringStep;
         hasPieEndVector = cheapPieEndVector != Mathf.Floor(cheapPieEndVector);

         pieExtraVertices = 0;
         if (hasPieStartVector) pieExtraVertices++;
         if (hasPieEndVector) pieExtraVertices++;

         pieVertices = 0;
         if (pieStartDegree < pieEndDegree)
            pieVertices = 1 + Mathf.FloorToInt(pieEndDegree / ringStep) - Mathf.FloorToInt(pieStartDegree / ringStep);
         else
            pieVertices = 1 + Mathf.FloorToInt((pieEndDegree + 360f) / ringStep) - Mathf.FloorToInt(pieStartDegree / ringStep);
         if (hasPieEndVector)
            pieVertices++;

         pieStartVector = 0;
         int pCount = 0;
         float pStart = pieStartDegree / ringStep;
         float pEnd = (pieEndDegree / ringStep);
         if (hasPieStartVector)
            pCount++;
         if (hasPieEndVector && pStart > pEnd)
            pCount++;
         pieStartVector = Mathf.FloorToInt(pStart + pCount);

         pieEndVector = 0;
         pCount = 0;
         if (hasPieEndVector)
            pCount++;
         if (hasPieStartVector && pEnd > pStart)
            pCount++;
         pieEndVector = Mathf.FloorToInt(pEnd + pCount);
      }
      private void Indices() {
         if (ringWidth == 0f) {
            vertices = new Vector3[0];
            mainTris = new int[0];
            pieTris = new int[0];
            borderTris = new int[0];
         }
         else {
            if (clampedPie == 0f) {
               /// No Pie
               if (clampedInnerRadius == 0f) {
                  /// Inner Radius == 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[clampedResolution * 3];
                     pieTris = new int[0];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[clampedResolution * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[1 + clampedResolution * 3];
                        mainTris = new int[(clampedResolution * 2) * 3];
                        pieTris = new int[0];
                        borderTris = new int[(clampedResolution * 3) * 3];
                     }
                     else if (clampedInnerBorder > 0f) {
                        /// Only Inner Border
                        vertices = new Vector3[1 + clampedResolution * 2];
                        mainTris = new int[(clampedResolution * 2) * 3];
                        pieTris = new int[0];
                        borderTris = new int[clampedResolution * 3];
                     }
                     else {
                        /// Only Outer Border
                        vertices = new Vector3[1 + clampedResolution * 2];
                        mainTris = new int[clampedResolution * 3];
                        pieTris = new int[0];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
               else {
                  /// Inner Radius > 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[clampedResolution * 2];
                     mainTris = new int[(clampedResolution * 2) * 3];
                     pieTris = new int[0];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[clampedResolution * 2];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[(clampedResolution * 2) * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[clampedResolution * 4];
                        mainTris = new int[(clampedResolution * 2) * 3];
                        pieTris = new int[0];
                        borderTris = new int[(clampedResolution * 4) * 3];
                     }
                     else {
                        /// Only Outer Border / Only Inner Border
                        vertices = new Vector3[clampedResolution * 3];
                        mainTris = new int[(clampedResolution * 2) * 3];
                        pieTris = new int[0];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
            }
            else if (clampedPie == 360f) {
               /// Full Pie
               if (clampedInnerRadius == 0f) {
                  /// Inner Radius == 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[0];
                     pieTris = new int[clampedResolution * 3];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[clampedResolution * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[1 + clampedResolution * 3];
                        mainTris = new int[0];
                        pieTris = new int[(clampedResolution * 2) * 3];
                        borderTris = new int[(clampedResolution * 3) * 3];
                     }
                     else if (clampedInnerBorder > 0f) {
                        /// Only Inner Border
                        vertices = new Vector3[1 + clampedResolution * 2];
                        mainTris = new int[0];
                        pieTris = new int[(clampedResolution * 2) * 3];
                        borderTris = new int[clampedResolution * 3];
                     }
                     else {
                        /// Only Outer Border
                        vertices = new Vector3[1 + clampedResolution * 2];
                        mainTris = new int[0];
                        pieTris = new int[clampedResolution * 3];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
               else {
                  /// Inner Radius > 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[clampedResolution * 2];
                     mainTris = new int[0];
                     pieTris = new int[(clampedResolution * 2) * 3];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[clampedResolution * 2];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[(clampedResolution * 2) * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[clampedResolution * 4];
                        mainTris = new int[0];
                        pieTris = new int[(clampedResolution * 2) * 3];
                        borderTris = new int[(clampedResolution * 4) * 3];
                     }
                     else {
                        /// Only Outer Border / Only Inner Border
                        vertices = new Vector3[clampedResolution * 3];
                        mainTris = new int[0];
                        pieTris = new int[(clampedResolution * 2) * 3];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
            }
            else {
               /// With not full Pie
               if (clampedInnerRadius == 0f) {
                  /// Inner Radius == 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[1 + clampedResolution + pieExtraVertices];
                     mainTris = new int[(clampedResolution - pieVertices + 1 + pieExtraVertices) * 3];
                     pieTris = new int[(pieVertices - 1) * 3];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[clampedResolution * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[1 + (clampedResolution + pieExtraVertices) * 2 + clampedResolution];
                        mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                        pieTris = new int[((pieVertices - 1) * 2) * 3];
                        borderTris = new int[(clampedResolution * 3) * 3];
                     }
                     else if (clampedInnerBorder > 0f) {
                        /// Only Inner Border
                        vertices = new Vector3[1 + (clampedResolution + pieExtraVertices) * 2];
                        mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                        pieTris = new int[((pieVertices - 1) * 2) * 3];
                        borderTris = new int[clampedResolution * 3];
                     }
                     else {
                        /// Only Outer Border
                        vertices = new Vector3[1 + clampedResolution * 2 + pieExtraVertices];
                        mainTris = new int[(clampedResolution - pieVertices + 1 + pieExtraVertices) * 3];
                        pieTris = new int[(pieVertices - 1) * 3];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
               else {
                  /// Inner Radius > 0f
                  if (ringWidth == borderedRingWidth) {
                     /// No Border
                     vertices = new Vector3[(clampedResolution + pieExtraVertices) * 2];
                     mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                     pieTris = new int[((pieVertices - 1) * 2) * 3];
                     borderTris = new int[0];
                  }
                  else if (borderedRingWidth == 0f) {
                     /// Full Border
                     vertices = new Vector3[clampedResolution * 2];
                     mainTris = new int[0];
                     pieTris = new int[0];
                     borderTris = new int[(clampedResolution * 2) * 3];
                  }
                  else {
                     if (clampedInnerBorder > 0f && clampedOuterBorder > 0f) {
                        /// Inner Border + Outer Border
                        vertices = new Vector3[(clampedResolution + pieExtraVertices) * 2 + clampedResolution * 2];
                        mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                        pieTris = new int[((pieVertices - 1) * 2) * 3];
                        borderTris = new int[(clampedResolution * 4) * 3];
                     }
                     else if (clampedInnerBorder > 0f) {
                        /// Only Inner Border
                        vertices = new Vector3[(clampedResolution + pieExtraVertices) * 2 + clampedResolution];
                        mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                        pieTris = new int[((pieVertices - 1) * 2) * 3];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                     else {
                        /// Only Outer Border
                        vertices = new Vector3[(clampedResolution + pieExtraVertices) * 2 + clampedResolution];
                        mainTris = new int[((clampedResolution - pieVertices + 1 + pieExtraVertices) * 2) * 3];
                        pieTris = new int[((pieVertices - 1) * 2) * 3];
                        borderTris = new int[(clampedResolution * 2) * 3];
                     }
                  }
               }
            }
         }
         normals = new Vector3[vertices.Length];
         uv = new Vector2[vertices.Length];
      }
      private void MainArea() {
         if (borderedRingWidth == 0f || clampedPie == 360f)
            return;

         borderedInnerVector = new Vector2(0f, clampedInnerRadius + clampedInnerBorder);
         borderedOuterVector = new Vector2(0f, clampedOuterRadius - clampedOuterBorder);
         int r = clampedResolution;
         int n = 0;

         if (clampedPie == 0) {
            #region # pie == 0f
            if (borderedInnerVector.y == 0f) {
               #region # innerRadius == 0f && no innerBorder && pie == 0f
               vertices[0] = Vector3.zero;
               skippedVertices--;
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                  skippedVertices--;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  mainTris[0 + n] = 0;
                  mainTris[1 + n] = 1 + i;
                  mainTris[2 + n] = 2 + i;
                  n += 3;
                  skippedTris -= 3;
               }
               mainTris[2 + n - 3] = 1;
               #endregion
            }
            else {
               #region # innerRadius > 0f / with innerBorder && pie == 0f
               int v = 1;
               if (clampedInnerRadius > 0f && clampedInnerBorder > 0f)
                  v = r;
               else if (clampedInnerRadius > 0f)
                  v = 0;
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                  vertices[v + r + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                  skippedVertices -= 2;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  mainTris[0 + n] = v + i;
                  mainTris[1 + n] = v + r + i;
                  mainTris[2 + n] = v + r + 1 + i;
                  mainTris[3 + n] = v + r + 1 + i;
                  mainTris[4 + n] = v + 1 + i;
                  mainTris[5 + n] = v + i;
                  n += 6;
                  skippedTris -= 6;
               }
               mainTris[2 + n - 6] = v + r;
               mainTris[3 + n - 6] = v + r;
               mainTris[4 + n - 6] = v;
               #endregion
            }
            #endregion
         }
         else {
            #region # pie > 0f
            int arcStart = pieEndVector;
            int arcEnd = pieStartVector;
            int xr = clampedResolution + pieExtraVertices;
            if (borderedInnerVector.y == 0f) {
               #region # innerRadius == 0f && no innerBorder && pie not full
               vertices[0] = Vector3.zero;
               skippedVertices--;
               n = 0;
               for (int i = 0; i < xr; i++) {
                  float rotation = clampedRingOffset + ringStep * n;
                  if (i == arcStart && hasPieEndVector) {
                     float prevRotation = clampedRingOffset + ringStep * (n - 1);
                     float startRotation = clampedRingOffset + pieEndDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedOuterVector, startRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                     vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                     skippedVertices--;
                  }
                  else if (i == arcEnd && hasPieStartVector) {
                     float prevRotation = clampedRingOffset + ringStep * (n - 1);
                     float endRotation = clampedRingOffset + pieStartDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedOuterVector, endRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                     vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                     skippedVertices--;
                  }
                  else {
                     n++;
                     if (arcEnd > arcStart) {
                        if (i >= arcStart && i <= arcEnd) {
                           vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices--;
                        }
                     }
                     else if (i >= arcStart || i <= arcEnd) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        skippedVertices--;
                     }
                  }
               }
               n = 0;
               for (int i = arcStart; i < arcStart + xr - pieVertices + 1; i++) {
                  mainTris[0 + n] = 0;
                  mainTris[1 + n] = ProcUtility.SpinInt(1, i, 1, xr);
                  mainTris[2 + n] = ProcUtility.SpinInt(2, i, 1, xr);
                  n += 3;
                  skippedTris -= 3;
               }
               #endregion
            }
            else {
               #region # innerRadius > 0f / With innerBorder && pie not full
               int v = 1;
               if (clampedInnerRadius > 0f && clampedInnerBorder > 0f)
                  v = r;
               else if (clampedInnerRadius > 0f)
                  v = 0;

               for (int i = 0; i < xr; i++) {
                  float rotation = clampedRingOffset + ringStep * n;
                  if (i == arcStart && hasPieEndVector) {
                     float prevRotation = clampedRingOffset + ringStep * (n - 1);
                     float startRotation = clampedRingOffset + pieEndDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedInnerVector, startRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedInnerVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                     vertices[v + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                     arcLineB = ProcUtility.RotateAxis(borderedOuterVector, startRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                     vertices[v + xr + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                     skippedVertices -= 2;
                  }
                  else if (i == arcEnd && hasPieStartVector) {
                     float prevRotation = clampedRingOffset + ringStep * (n - 1);
                     float endRotation = clampedRingOffset + pieStartDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedInnerVector, endRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedInnerVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                     vertices[v + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                     arcLineB = ProcUtility.RotateAxis(borderedOuterVector, endRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                     vertices[v + xr + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                     skippedVertices -= 2;
                  }
                  else {
                     if (arcEnd > arcStart) {
                        if (i >= arcStart && i <= arcEnd) {
                           vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                           vertices[v + xr + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices -= 2;
                        }
                     }
                     else if (i >= arcStart || i <= arcEnd) {
                        vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                        vertices[v + xr + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        skippedVertices -= 2;
                     }
                     n++;
                  }
               }
               n = 0;
               for (int i = arcStart; i < arcStart + xr - pieVertices + 1; i++) {
                  mainTris[0 + n] = v + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  mainTris[1 + n] = v + xr + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  mainTris[2 + n] = v + xr + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  mainTris[3 + n] = v + xr + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  mainTris[4 + n] = v + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  mainTris[5 + n] = v + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  n += 6;
                  skippedTris -= 6;
               }
               #endregion
            }
            #endregion
         }
      }
      private void PieArea() {
         if (borderedRingWidth == 0f || clampedPie == 0f)
            return;

         borderedInnerVector = new Vector2(0f, clampedInnerRadius + clampedInnerBorder);
         borderedOuterVector = new Vector2(0f, clampedOuterRadius - clampedOuterBorder);
         int r = clampedResolution;
         int n = 0;

         if (clampedPie == 360f) {
            #region # pie == 360f
            if (borderedInnerVector.y == 0f) {
               #region # innerRadius == 0f && no innerBorder && pie full
               vertices[0] = Vector3.zero;
               skippedVertices--;
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                  skippedVertices--;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  pieTris[0 + n] = 0;
                  pieTris[1 + n] = 1 + i;
                  pieTris[2 + n] = 2 + i;
                  n += 3;
                  skippedTris -= 3;
               }
               pieTris[2 + n - 3] = 1;
               #endregion
            }
            else {
               #region # innerRadius > 0f / with innerBorder && pie full
               int v = 1;
               if (clampedInnerRadius > 0f && clampedInnerBorder > 0f)
                  v = r;
               else if (clampedInnerRadius > 0f)
                  v = 0;
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                  vertices[v + r + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                  skippedVertices -= 2;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  pieTris[0 + n] = v + i;
                  pieTris[1 + n] = v + r + i;
                  pieTris[2 + n] = v + r + 1 + i;
                  pieTris[3 + n] = v + r + 1 + i;
                  pieTris[4 + n] = v + 1 + i;
                  pieTris[5 + n] = v + i;
                  n += 6;
                  skippedTris -= 6;
               }
               pieTris[2 + n - 6] = v + r;
               pieTris[3 + n - 6] = v + r;
               pieTris[4 + n - 6] = v;
               #endregion
            }
            #endregion
         }
         else {
            #region # pie < 360f
            int arcStart = pieStartVector;
            int arcEnd = pieEndVector;
            int xr = clampedResolution + pieExtraVertices;
            if (borderedInnerVector.y == 0f) {
               #region # innerRadius == 0f && no innerBorder && pie not full
               if (skipMainArea) {
                  vertices[0] = Vector3.zero;
                  skippedVertices--;
               }
               n = 0;
               for (int i = 0; i < xr; i++) {
                  float rotation = clampedRingOffset + ringStep * n;
                  if (i == arcStart && hasPieStartVector) {
                     if (skipMainArea) {
                        float prevRotation = clampedRingOffset + ringStep * (n - 1);
                        float startRotation = clampedRingOffset + pieStartDegree;
                        arcLineB = ProcUtility.RotateAxis(borderedOuterVector, startRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                        skippedVertices--;
                     }
                  }
                  else if (i == arcEnd && hasPieEndVector) {
                     if (skipMainArea) {
                        float prevRotation = clampedRingOffset + ringStep * (n - 1);
                        float endRotation = clampedRingOffset + pieEndDegree;
                        arcLineB = ProcUtility.RotateAxis(borderedOuterVector, endRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                        skippedVertices--;
                     }
                  }
                  else {
                     n++;
                     if ((i == arcStart || i == arcEnd) && !skipMainArea)
                        continue;
                     if (arcEnd > arcStart) {
                        if (i >= arcStart && i <= arcEnd) {
                           vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices--;
                        }
                     }
                     else if (i >= arcStart || i <= arcEnd) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        skippedVertices--;
                     }
                  }
               }
               n = 0;
               for (int i = arcStart; i < arcStart + pieVertices - 1; i++) {
                  pieTris[0 + n] = 0;
                  pieTris[1 + n] = ProcUtility.SpinInt(1, i, 1, xr);
                  pieTris[2 + n] = ProcUtility.SpinInt(2, i, 1, xr);
                  n += 3;
                  skippedTris -= 3;
               }
               #endregion
            }
            else {
               #region # innerRadius > 0f / With innerBorder && pie not full
               int v = 1;
               if (clampedInnerRadius > 0f && clampedInnerBorder > 0f)
                  v = r;
               else if (clampedInnerRadius > 0f)
                  v = 0;

               for (int i = 0; i < xr; i++) {
                  float rotation = clampedRingOffset + ringStep * n;
                  if (i == arcStart && hasPieStartVector) {
                     if (skipMainArea) {
                        float prevRotation = clampedRingOffset + ringStep * (n - 1);
                        float startRotation = clampedRingOffset + pieStartDegree;
                        arcLineB = ProcUtility.RotateAxis(borderedInnerVector, startRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedInnerVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                        vertices[v + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                        arcLineB = ProcUtility.RotateAxis(borderedOuterVector, startRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        vertices[v + xr + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                        skippedVertices -= 2;
                     }
                  }
                  else if (i == arcEnd && hasPieEndVector) {
                     if (skipMainArea) {
                        float prevRotation = clampedRingOffset + ringStep * (n - 1);
                        float endRotation = clampedRingOffset + pieEndDegree;
                        arcLineB = ProcUtility.RotateAxis(borderedInnerVector, endRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedInnerVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                        vertices[v + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                        arcLineB = ProcUtility.RotateAxis(borderedOuterVector, endRotation);
                        ringLineA = ProcUtility.RotateAxis(borderedOuterVector, prevRotation);
                        ringLineB = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        vertices[v + xr + i] = ProcUtility.Intersection(Vector3.zero, arcLineB, ringLineA, ringLineB);
                        skippedVertices -= 2;
                     }
                  }
                  else {
                     n++;
                     if ((i == arcStart || i == arcEnd) && !skipMainArea)
                        continue;
                     if (arcEnd > arcStart) {
                        if (i >= arcStart && i <= arcEnd) {
                           vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                           vertices[v + xr + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices -= 2;
                        }
                     }
                     else if (i >= arcStart || i <= arcEnd) {
                        vertices[v + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                        vertices[v + xr + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                        skippedVertices -= 2;
                     }
                  }
               }
               n = 0;
               for (int i = arcStart; i < arcStart + pieVertices - 1; i++) {
                  pieTris[0 + n] = v + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  pieTris[1 + n] = v + xr + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  pieTris[2 + n] = v + xr + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  pieTris[3 + n] = v + xr + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  pieTris[4 + n] = v + ProcUtility.SpinInt(1, i, 0, xr - 1);
                  pieTris[5 + n] = v + ProcUtility.SpinInt(0, i, 0, xr - 1);
                  n += 6;
                  skippedTris -= 6;
               }
               #endregion
            }
            #endregion
         }
      }
      private void BorderArea() {
         if (ringWidth == 0f || (clampedOuterBorder == 0f && clampedInnerBorder == 0f))
            return;
         
         innerVector = new Vector2(0f, clampedInnerRadius);
         outerVector = new Vector2(0f, clampedOuterRadius);
         int r = clampedResolution;
         int n = 0;

         if (borderedRingWidth == 0f) {
            #region # Border full
            if (clampedInnerRadius == 0f) {
               #region # innerRadius == 0f
               vertices[0] = Vector3.zero;
               skippedVertices--;
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[1 + i] = ProcUtility.RotateAxis(outerVector, rotation);
                  skippedVertices--;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
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
               #region # innerRadius > 0f
               for (int i = 0; i < r; i++) {
                  float rotation = clampedRingOffset + (ringStep * i);
                  vertices[i] = ProcUtility.RotateAxis(innerVector, rotation);
                  vertices[r + i] = ProcUtility.RotateAxis(outerVector, rotation);
                  skippedVertices -= 2;
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  borderTris[0 + n] = i;
                  borderTris[1 + n] = r + i;
                  borderTris[2 + n] = r + 1 + i;
                  borderTris[3 + n] = r + 1 + i;
                  borderTris[4 + n] = 1 + i;
                  borderTris[5 + n] = i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[2 + n - 6] = r;
               borderTris[3 + n - 6] = r;
               borderTris[4 + n - 6] = 0;
               #endregion
            }
            #endregion
         }
         else {
            #region # Border not full
            borderedInnerVector = new Vector2(0f, clampedInnerRadius + clampedInnerBorder);
            borderedOuterVector = new Vector2(0f, clampedOuterRadius - clampedOuterBorder);
            int xr = clampedResolution + pieExtraVertices;
            int v = 0;
            int b = 0;
            if (clampedPie == 0f || clampedPie == 360f) {
               if (clampedInnerRadius == 0f) {
                  #region # innerRadius == 0f  &&  pie == 0f / pie == 360f
                  if (clampedInnerBorder > 0f) {
                     v = r;
                     vertices[0] = Vector3.zero;
                     skippedVertices--;
                     if ((clampedPie == 0f && skipMainArea) || (clampedPie == 360f && skipPieArea)) {
                        for (int i = 0; i < r; i++) {
                           float rotation = clampedRingOffset + ringStep * i;
                           vertices[1 + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                           skippedVertices--;
                        }
                     }

                     for (int i = 0; i < r; i++) {
                        borderTris[0 + b] = 0;
                        borderTris[1 + b] = 1 + i;
                        borderTris[2 + b] = 2 + i;
                        b += 3;
                        skippedTris -= 3;
                     }
                     borderTris[2 + b - 3] = 1;
                  }
                  if (clampedOuterBorder > 0f) {
                     for (int i = 0; i < r; i++) {
                        float rotation = clampedRingOffset + (ringStep * i);
                        vertices[v + r + 1 + i] = ProcUtility.RotateAxis(outerVector, rotation);
                        skippedVertices--;
                        if ((clampedPie == 0f && skipMainArea) || (clampedPie == 360f && skipPieArea)) {
                           vertices[v + 1 + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices--;
                        }
                     }

                     for (int i = 0; i < r; i++) {
                        borderTris[0 + b] = v + 1 + i;
                        borderTris[1 + b] = v + r + 1 + i;
                        borderTris[2 + b] = v + r + 2 + i;
                        borderTris[3 + b] = v + r + 2 + i;
                        borderTris[4 + b] = v + 2 + i;
                        borderTris[5 + b] = v + 1 + i;
                        b += 6;
                        skippedTris -= 6;
                     }
                     borderTris[2 + b - 6] = v + r + 1;
                     borderTris[3 + b - 6] = v + r + 1;
                     borderTris[4 + b - 6] = v + 1;
                  }
                  #endregion
               }
               else {
                  #region # innerRadius > 0f  &&  pie == 0f / pie == 360f
                  if (clampedInnerBorder > 0f) {
                     v = r;
                     for (int i = 0; i < r; i++) {
                        float rotation = clampedRingOffset + (ringStep * i);
                        vertices[i] = ProcUtility.RotateAxis(innerVector, rotation);
                        skippedVertices--;
                        if ((clampedPie == 0f && skipMainArea) || (clampedPie == 360f && skipPieArea)) {
                           vertices[r + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                           skippedVertices--;
                        }
                     }
                     for (int i = 0; i < r; i++) {
                        borderTris[0 + b] = i;
                        borderTris[1 + b] = r + i;
                        borderTris[2 + b] = r + 1 + i;
                        borderTris[3 + b] = r + 1 + i;
                        borderTris[4 + b] = 1 + i;
                        borderTris[5 + b] = i;
                        b += 6;
                        skippedTris -= 6;
                     }
                     borderTris[2 + b - 6] = r;
                     borderTris[3 + b - 6] = r;
                     borderTris[4 + b - 6] = 0;
                  }
                  if (clampedOuterBorder > 0f) {
                     for (int i = 0; i < r; i++) {
                        float rotation = clampedRingOffset + (ringStep * i);
                        vertices[v + (r * 2) + i] = ProcUtility.RotateAxis(outerVector, rotation);
                        skippedVertices--;
                        if ((clampedPie == 0f && skipMainArea) || (clampedPie == 360f && skipPieArea)) {
                           vertices[v + r + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                           skippedVertices--;
                        }
                     }
                     for (int i = 0; i < r; i++) {
                        borderTris[0 + b] = v + r + i;
                        borderTris[1 + b] = v + (r * 2) + i;
                        borderTris[2 + b] = v + (r * 2) + 1 + i;
                        borderTris[3 + b] = v + (r * 2) + 1 + i;
                        borderTris[4 + b] = v + r + 1 + i;
                        borderTris[5 + b] = v + r + i;
                        b += 6;
                        skippedTris -= 6;
                     }
                     borderTris[2 + b - 6] = v + (r * 2);
                     borderTris[3 + b - 6] = v + (r * 2);
                     borderTris[4 + b - 6] = v + r;
                  }
                  #endregion
               }
            }
            else {
               int arcStart = pieStartVector;
               int arcEnd = pieEndVector;
               if (clampedInnerRadius == 0f) {
                  #region # innerRadius == 0f  &&  pie != 0f / pie != 360f
                  if (clampedInnerBorder > 0f) {
                     vertices[0] = Vector3.zero;
                     skippedVertices--;
                     v = xr;
                     n = 0;
                     for (int i = 0; i < xr; i++) {
                        if (i == arcStart && hasPieStartVector)
                           continue;
                        if (i == arcEnd && hasPieEndVector)
                           continue;
                        float rotation = clampedRingOffset + ringStep * n;
                        n++;
                        if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipPieArea))
                           continue;
                        if (arcEnd > arcStart) {
                           if ((i >= arcStart && i <= arcEnd) && skipPieArea) {
                              vertices[1 + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                              vertices[1 + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                        }
                        else {
                           if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                              vertices[1 + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i >= arcStart || i <= arcEnd) && skipPieArea) {
                              vertices[1 + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                        }
                     }
                     b = 0;
                     int trail = 0;
                     int lead = 0;
                     for (int i = 0; i < r; i++) {
                        if (arcStart < arcEnd) {
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                        }
                        else {
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                        }
                        borderTris[0 + b] = 0;
                        borderTris[1 + b] = lead + 1 + i;
                        borderTris[2 + b] = trail + 2 + i;
                        b += 3;
                        skippedTris -= 3;
                     }
                     borderTris[2 + b - 3] = 1;
                  }
                  if (clampedOuterBorder > 0f) {
                     n = 0;
                     for (int i = 0; i < xr; i++) {
                        if (i == arcStart && hasPieStartVector)
                           continue;
                        if (i == arcEnd && hasPieEndVector)
                           continue;
                        float rotation = clampedRingOffset + ringStep * n;
                        vertices[1 + v + xr + n] = ProcUtility.RotateAxis(outerVector, rotation);
                        skippedVertices--;
                        n++;
                        if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipPieArea))
                           continue;
                        if (arcEnd > arcStart) {
                           if ((i >= arcStart && i <= arcEnd) && skipPieArea) {
                              vertices[1 + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                              vertices[1 + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                        }
                        else {
                           if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                              vertices[1 + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i >= arcStart || i <= arcEnd) && skipPieArea) {
                              vertices[1 + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }  
                        }
                     }
                     int trail = 0;
                     int lead = 0;
                     for (int i = 0; i < r; i++) {
                        if (arcStart < arcEnd) {
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                        }
                        else {
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                        }
                        borderTris[0 + b] = 1 + v + lead + i;
                        borderTris[1 + b] = 1 + v + xr + i;
                        borderTris[2 + b] = 1 + v + xr + 1 + i;
                        borderTris[3 + b] = 1 + v + xr + 1 + i;
                        borderTris[4 + b] = 1 + v + trail + 1 + i;
                        borderTris[5 + b] = 1 + v + lead + i;
                        b += 6;
                        skippedTris -= 6;
                     }
                     borderTris[2 + b - 6] = 1 + v + xr;
                     borderTris[3 + b - 6] = 1 + v + xr;
                     borderTris[4 + b - 6] = 1 + v;
                  }
                  #endregion
               }
               else {
                  #region # innerRadius > 0f  &&  pie != 0f / pie != 360f
                  v = pieExtraVertices;
                  if (clampedInnerBorder > 0f) {
                     v = xr;
                     n = 0;
                     for (int i = 0; i < xr; i++) {
                        if (i == arcStart && hasPieStartVector)
                           continue;
                        if (i == arcEnd && hasPieEndVector)
                           continue;
                        float rotation = clampedRingOffset + ringStep * n;
                        vertices[n] = ProcUtility.RotateAxis(innerVector, rotation);
                        skippedVertices--;
                        n++;
                        if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipPieArea))
                           continue;
                        if (arcEnd > arcStart) {
                           if ((i >= arcStart && i <= arcEnd) && skipPieArea) {
                              vertices[r + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                              vertices[r + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                        }
                        else {
                           if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                              vertices[r + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i >= arcStart || i <= arcEnd) && skipPieArea) {
                              vertices[r + i] = ProcUtility.RotateAxis(borderedInnerVector, rotation);
                              skippedVertices--;
                           }
                        }
                     }
                     b = 0;
                     int trail = 0;
                     int lead = 0;
                     for (int i = 0; i < r; i++) {
                        if (arcStart < arcEnd) {
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                        }
                        else {
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                        }
                        borderTris[0 + b] = i;
                        borderTris[1 + b] = r + lead + i;
                        borderTris[2 + b] = r + trail + 1 + i;
                        borderTris[3 + b] = r + trail + 1 + i;
                        borderTris[4 + b] = 1 + i;
                        borderTris[5 + b] = i;
                        skippedTris -= 6;
                        b += 6;
                     }
                     borderTris[2 + b - 6] = r;
                     borderTris[3 + b - 6] = r;
                     borderTris[4 + b - 6] = 0;
                  }
                  if (clampedOuterBorder > 0f) {
                     n = 0;
                     for (int i = 0; i < xr; i++) {
                        if (i == arcStart && hasPieStartVector)
                           continue;
                        if (i == arcEnd && hasPieEndVector)
                           continue;
                        float rotation = clampedRingOffset + ringStep * n;
                        vertices[r + v + xr + n] = ProcUtility.RotateAxis(outerVector, rotation);
                        skippedVertices--;
                        n++;
                        if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipPieArea))
                           continue;
                        if (arcEnd > arcStart) {
                           if ((i >= arcStart && i <= arcEnd) && skipPieArea) {
                              vertices[r + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                              vertices[r + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                        }
                        else {
                           if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                              vertices[r + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                           else if ((i >= arcStart || i <= arcEnd) && skipPieArea) {
                              vertices[r + v + i] = ProcUtility.RotateAxis(borderedOuterVector, rotation);
                              skippedVertices--;
                           }
                        }
                     }
                     int trail = 0;
                     int lead = 0;
                     for (int i = 0; i < r; i++) {
                        if (arcStart < arcEnd) {
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                        }
                        else {
                           if (hasPieEndVector) {
                              if (i + trail + 1 == arcEnd)
                                 trail++;
                              if (i + lead == arcEnd)
                                 lead++;
                           }
                           if (hasPieStartVector) {
                              if (i + trail + 1 == arcStart)
                                 trail++;
                              if (i + lead == arcStart)
                                 lead++;
                           }
                        }
                        borderTris[0 + b] = r + v + lead + i;
                        borderTris[1 + b] = r + v + xr + i;
                        borderTris[2 + b] = r + v + xr + 1 + i;
                        borderTris[3 + b] = r + v + xr + 1 + i;
                        borderTris[4 + b] = r + v + trail + 1 + i;
                        borderTris[5 + b] = r + v + lead + i;
                        b += 6;
                        skippedTris -= 6;
                     }
                     borderTris[2 + b - 6] = r + v + xr;
                     borderTris[3 + b - 6] = r + v + xr;
                     borderTris[4 + b - 6] = r + v;

                  }
                  #endregion
               }
            }
            #endregion
         }
      }

      private void SetTextureCoordinates() {
         uv = new Vector2[vertices.Length];
         for (int i = 0; i < uv.Length; i++) {
            uv[i] = (vertices[i] + new Vector3(clampedOuterRadius, clampedOuterRadius));
            uv[i] = new Vector2(uv[i].x / (2 * clampedOuterRadius), uv[i].y / (2 * clampedOuterRadius));
         }
      }
      private void SetNormals() {
         normals = new Vector3[vertices.Length];
         for (int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.back;
         }
      }
      
      private void UpdateMesh() {
         GenericMath();
         if (clampedPie > 0f && clampedPie < 360f)
            PieMath();

         Indices();
         skippedVertices = vertices.Length;
         skippedTris = mainTris.Length + pieTris.Length + borderTris.Length;

         if (!skipMainArea)
            MainArea();
         if (!skipPieArea)
            PieArea();
         if (!skipBorderArea)
            BorderArea();

         if (skippedVertices != vertices.Length) {
            SetTextureCoordinates();
            SetNormals();
         }

         if (!filter.sharedMesh)
            filter.sharedMesh = new Mesh();

         filter.sharedMesh.name = gameObject.name + " ProceduralRing";
         filter.sharedMesh.subMeshCount = 3;

         filter.sharedMesh.SetTriangles(new int[] { }, 2);
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
         filter.sharedMesh.SetTriangles(pieTris, 2);

         //filter.sharedMesh.RecalculateBounds();
         filter.sharedMesh.RecalculateTangents();

         needUpdate = false;
      }

      public void CreateMesh(bool forceUpdate = false) {
         ReadUserValues(out needUpdate);
         if (needUpdate || forceUpdate) {
            ClampUserValues();
            UpdateMesh();
         }
      }
   }

}
