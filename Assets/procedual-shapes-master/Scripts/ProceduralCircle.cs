
using UnityEngine;

namespace ProceduralShapes {

   [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
   [ExecuteInEditMode]

   public class ProceduralCircle : MonoBehaviour {

      private MeshFilter filter;
      private float time;
      private bool needUpdate;
      private float circleStep;
      private float borderedRadius;
      private Vector2 radiusVector;
      private Vector2 borderedRadiusVector;
      private float fillStartDegree;
      private float fillEndDegree;
      private bool hasFillStartVector;
      private bool hasFillEndVector;
      private int fillExtraVertices;
      private int fillVertices;
      private int fillStartVector;
      private int fillEndVector;
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
      [SerializeField] private int[] fillTris = new int[0];
      [SerializeField] private int[] pieTris = new int[0];
      [SerializeField] private int skippedVertices;
      [SerializeField] private int skippedTris;

      public float synchronise = -1;
      public float radius = 0.5f;
      public float border = 0.1f;
      public int resolution = 18;
      public float circleOffset = 0f;
      public float fill = 0f;
      public float fillOffset = 0f;
      public float pie = 0f;
      public float pieOffset = 0f;
      public bool skipMainArea = false;
      public bool skipBorderArea = false;
      public bool skipFillArea = false;
      public bool skipPieArea = false;

      public float readRadius { get; private set; }
      public float readBorder { get; private set; }
      public int readResolution { get; private set; }
      public float readCircleOffset { get; private set; }
      public float readFill { get; private set; }
      public float readFillOffset { get; private set; }
      public float readPie { get; private set; }
      public float readPieOffset { get; private set; }

      public float clampedRadius { get; private set; }
      public float clampedBorder { get; private set; }
      public int clampedResolution { get; private set; }
      public float clampedCircleOffset { get; private set; }
      public float clampedFill { get; private set; }
      public float clampedFillOffset { get; private set; }
      public float clampedPie { get; private set; }
      public float clampedPieOffset { get; private set; }

      public bool NeedUpdate { get { return needUpdate; } }
      public Vector3[] Vertices { get { return vertices; } }
      public Vector3[] Normals { get { return normals; } }
      public Vector2[] UV { get { return uv; } }
      public int[] MainTrIs { get { return mainTris; } }
      public int[] BorderTrIs { get { return borderTris; } }
      public int[] FillTrIs { get { return fillTris; } }
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
            if (synchronise >= 0) {
               if (time < synchronise) {
                  time += Time.deltaTime;
                  return;
               }
               time = 0;
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

         if (readRadius != radius)
            valuesChanged = true;
         else if (readBorder != border)
            valuesChanged = true;
         else if (readResolution != resolution)
            valuesChanged = true;
         else if (readCircleOffset != circleOffset)
            valuesChanged = true;
         else if (readFill != fill)
            valuesChanged = true;
         else if (readFillOffset != fillOffset)
            valuesChanged = true;
         else if (readPie != pie)
            valuesChanged = true;
         else if (readPieOffset != pieOffset)
            valuesChanged = true;

         readRadius = radius;
         readBorder = border;
         readResolution = resolution;
         readCircleOffset = circleOffset;
         readFill = fill;
         readFillOffset = fillOffset;
         readPie = pie;
         readPieOffset = pieOffset;
      }
      private void ClampUserValues() {
         clampedRadius = readRadius;
         clampedBorder = readBorder;
         clampedResolution = readResolution;
         clampedCircleOffset = readCircleOffset;
         clampedFill = readFill;
         clampedFillOffset = readFillOffset;
         clampedPie = readPie;
         clampedPieOffset = readPieOffset;

         if (clampedRadius < 0f)
            clampedRadius = 0f;

         if (clampedBorder < 0f)
            clampedBorder = 0f;
         if (clampedBorder > clampedRadius)
            clampedBorder = clampedRadius;
         
         if (clampedResolution < 3)
            clampedResolution = 3;

         while (clampedCircleOffset < 0f)
            clampedCircleOffset += 360f;
         while (clampedCircleOffset > 360f)
            clampedCircleOffset -= 360f;

         if (clampedFill < 0f)
            clampedFill = 0f;
         if (clampedFill > 1f)
            clampedFill = 1f;

         while (clampedFillOffset < 0f)
            clampedFillOffset += 360f;
         while (clampedFillOffset > 360f)
            clampedFillOffset -= 360f;

         if (clampedPie < 0f || clampedFill > 0f)
            clampedPie = 0f;
         if (clampedPie > 360f)
            clampedPie = 360f;
         
         while (clampedPieOffset < 0f)
            clampedPieOffset += 360f;
         while (clampedPieOffset > 360f)
            clampedPieOffset -= 360f;
      }

      private void GenericMath() {
         circleStep = 360f / clampedResolution;
         borderedRadius = clampedRadius - clampedBorder;
      }
      private void FillMath() {
         fillStartDegree = clampedFillOffset + (Mathf.PI - Mathf.Acos(1 - 2 * clampedFill)) * 180 / Mathf.PI;
         while (fillStartDegree >= 360f) fillStartDegree -= 360f;

         fillEndDegree = clampedFillOffset + 360f - (Mathf.PI - Mathf.Acos(1 - 2 * clampedFill)) * 180 / Mathf.PI;
         while (fillEndDegree >= 360f) fillEndDegree -= 360f;

         hasFillStartVector = false;
         float cheapFillStartVector = fillStartDegree / circleStep;
         hasFillStartVector = cheapFillStartVector != Mathf.Floor(cheapFillStartVector);

         hasFillEndVector = false;
         float cheapFillEndVector = fillEndDegree / circleStep;
         hasFillEndVector = cheapFillEndVector != Mathf.Floor(cheapFillEndVector);

         fillExtraVertices = 0;
         if (hasFillStartVector) fillExtraVertices++;
         if (hasFillEndVector) fillExtraVertices++;

         fillVertices = 0;
         if (fillStartDegree < fillEndDegree)
            fillVertices = 1 + Mathf.FloorToInt(fillEndDegree / circleStep) - Mathf.FloorToInt(fillStartDegree / circleStep);
         else
            fillVertices = 1 + Mathf.FloorToInt((fillEndDegree + 360f) / circleStep) - Mathf.FloorToInt(fillStartDegree / circleStep);
         if (hasFillEndVector)
            fillVertices++;

         fillStartVector = 0;
         int fCount = 0;
         float fStart = (fillStartDegree / circleStep);
         float fEnd = (fillEndDegree / circleStep);
         if (hasFillStartVector)
            fCount++;
         if (hasFillEndVector && fStart > fEnd)
            fCount++;
         fillStartVector = Mathf.FloorToInt(fStart + fCount);

         fillEndVector = 0;
         fCount = 0;
         if (hasFillEndVector)
            fCount++;
         if (hasFillStartVector && fEnd > fStart)
            fCount++;
         fillEndVector = Mathf.FloorToInt(fEnd + fCount);
      }
      private void PieMath() {
         pieStartDegree = clampedPieOffset;
         while (pieStartDegree >= 360f) pieStartDegree -= 360f;

         pieEndDegree = clampedPieOffset + clampedPie;
         while (pieEndDegree >= 360f) pieEndDegree -= 360f;

         hasPieStartVector = false;
         float cheapPieStartVector = pieStartDegree / circleStep;
         hasPieStartVector = cheapPieStartVector != Mathf.Floor(cheapPieStartVector);

         hasPieEndVector = false;
         float cheapPieEndVector = pieEndDegree / circleStep;
         hasPieEndVector = cheapPieEndVector != Mathf.Floor(cheapPieEndVector);

         pieExtraVertices = 0;
         if (hasPieStartVector) pieExtraVertices++;
         if (hasPieEndVector) pieExtraVertices++;

         pieVertices = 0;
         if (pieStartDegree < pieEndDegree)
            pieVertices = 1 + Mathf.FloorToInt(pieEndDegree / circleStep) - Mathf.FloorToInt(pieStartDegree / circleStep);
         else
            pieVertices = 1 + Mathf.FloorToInt((pieEndDegree + 360f) / circleStep) - Mathf.FloorToInt(pieStartDegree / circleStep);
         if (hasPieEndVector)
            pieVertices++;

         pieStartVector = 0;
         int pCount = 0;
         float pStart = pieStartDegree / circleStep;
         float pEnd = (pieEndDegree / circleStep);
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
         if (clampedRadius > 0f) {
            if (clampedBorder > 0f && borderedRadius > 0f) {
               if (clampedFill > 0f && clampedFill < 1f) {
                  // Main + Border + Fill
                  vertices = new Vector3[1 + (clampedResolution * 2) + fillExtraVertices];
                  mainTris = new int[(clampedResolution - fillVertices + 1 + fillExtraVertices) * 3];
                  borderTris = new int[(clampedResolution * 2) * 3];
                  fillTris = new int[(fillVertices - 1) * 3];
                  pieTris = new int[0];
               }
               else if (clampedFill == 1f) {
                  // Border + Fill
                  vertices = new Vector3[1 + clampedResolution + (clampedResolution * 2)];
                  mainTris = new int[0];
                  borderTris = new int[(clampedResolution * 2) * 3];
                  fillTris = new int[clampedResolution * 3];
                  pieTris = new int[0];
               }
               else {
                  if (clampedPie > 0f && clampedPie < 360f) {
                     // Main + Border + Pie
                     vertices = new Vector3[1 + (clampedResolution * 2) + pieExtraVertices];
                     mainTris = new int[(clampedResolution - (pieVertices - 1) + pieExtraVertices) * 3];
                     borderTris = new int[(clampedResolution * 2) * 3];
                     fillTris = new int[0];
                     pieTris = new int[(pieVertices - 1) * 3];
                  }
                  else if (clampedPie == 360f) {
                     // Border + Pie
                     vertices = new Vector3[1 + (clampedResolution * 2)];
                     mainTris = new int[0];
                     borderTris = new int[(clampedResolution * 2) * 3];
                     fillTris = new int[0];
                     pieTris = new int[clampedResolution * 3];
                  }
                  else {
                     // Main + Border
                     vertices = new Vector3[1 + (clampedResolution * 2)];
                     mainTris = new int[clampedResolution * 3];
                     borderTris = new int[(clampedResolution * 2) * 3];
                     fillTris = new int[0];
                     pieTris = new int[0];
                  }
               }
            }
            else if (borderedRadius == 0f) {
               // Border Only
               vertices = new Vector3[1 + clampedResolution];
               mainTris = new int[0];
               borderTris = new int[clampedResolution * 3];
               fillTris = new int[0];
               pieTris = new int[0];
            }
            else {
               if (clampedFill > 0f && clampedFill < 1f) {
                  // Main + Fill
                  vertices = new Vector3[1 + clampedResolution + fillExtraVertices];
                  mainTris = new int[(clampedResolution - fillVertices + 1 + fillExtraVertices) * 3];
                  borderTris = new int[0];
                  fillTris = new int[(fillVertices - 1) * 3];
                  pieTris = new int[0];
               }
               else if (clampedFill == 1f) {
                  // Fill Only
                  vertices = new Vector3[1 + clampedResolution];
                  mainTris = new int[0];
                  borderTris = new int[0];
                  fillTris = new int[clampedResolution * 3];
                  pieTris = new int[0];
               }
               else {
                  if (clampedPie > 0f && clampedPie < 360f) {
                     // Main + Pie
                     vertices = new Vector3[1 + clampedResolution + pieExtraVertices];
                     mainTris = new int[(clampedResolution - pieVertices + 1 + pieExtraVertices) * 3];
                     borderTris = new int[0];
                     fillTris = new int[0];
                     pieTris = new int[(pieVertices - 1) * 3];
                  }
                  else if (clampedPie == 360f) {
                     // Pie Only
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[0];
                     borderTris = new int[0];
                     fillTris = new int[0];
                     pieTris = new int[clampedResolution * 3];
                  }
                  else {
                     // Main Only
                     vertices = new Vector3[1 + clampedResolution];
                     mainTris = new int[clampedResolution * 3];
                     borderTris = new int[0];
                     fillTris = new int[0];
                     pieTris = new int[0];
                  }
               }
            }
         }
         else {
            // Null
            vertices = new Vector3[0];
            mainTris = new int[0];
            borderTris = new int[0];
            fillTris = new int[0];
            pieTris = new int[0];
         }
         normals = new Vector3[vertices.Length];
         uv = new Vector2[vertices.Length];
      }
      private void MainArea() {
         if (borderedRadius == 0f || clampedFill == 1f || clampedPie == 360f)
            return;

         borderedRadiusVector = new Vector2(0f, borderedRadius);
         int r = clampedResolution;
         int n = 0;

         if (clampedPie == 0 && clampedFill == 0f) {
            #region # pie == 0f && fill == 0f
            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < r; i++) {
               float rotation = clampedCircleOffset + (circleStep * i);
               vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
         else if (clampedFill > 0f) {
            #region fill > 0f
            int arcStart = fillEndVector;
            int arcEnd = fillStartVector;
            int xr = clampedResolution + fillExtraVertices;
            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < xr; i++) {
               float rotation = clampedCircleOffset + circleStep * n;
               if (i == arcStart && hasFillEndVector) {
                  float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                  float startRotation = clampedCircleOffset + fillEndDegree;
                  arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, startRotation);
                  ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                  ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                  vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                  skippedVertices--;
               }
               else if (i == arcEnd && hasFillStartVector) {
                  float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                  float endRotation = clampedCircleOffset + fillStartDegree;
                  arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, endRotation);
                  ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                  ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                  vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                  skippedVertices--;
               }
               else {
                  n++;
                  if (arcEnd > arcStart) {
                     if (i >= arcStart && i <= arcEnd) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else if (i >= arcStart || i <= arcEnd) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                     skippedVertices--;
                  }
               }
            }
            if (clampedFill != 0.5f) {
               Vector3 arcLineA = ProcUtility.RotateAxis(borderedRadiusVector, clampedCircleOffset + clampedFillOffset);
               arcLineB = ProcUtility.RotateAxis(-borderedRadiusVector, clampedCircleOffset + clampedFillOffset);
               vertices[0] = ProcUtility.Intersection(arcLineA, arcLineB, vertices[1 + arcStart], vertices[1 + arcEnd]);
            }
            n = 0;
            for (int i = arcStart; i < arcStart + xr - fillVertices + 1; i++) {
               mainTris[0 + n] = 0;
               mainTris[1 + n] = ProcUtility.SpinInt(1, i, 1, xr);
               mainTris[2 + n] = ProcUtility.SpinInt(2, i, 1, xr);
               n += 3;
               skippedTris -= 3;
            }
            #endregion
         }
         else {
            #region # pie > 0f
            int arcStart = pieEndVector;
            int arcEnd = pieStartVector;
            int xr = clampedResolution + pieExtraVertices;
            vertices[0] = Vector3.zero;
            skippedVertices--;
            n = 0;
            for (int i = 0; i < xr; i++) {
               float rotation = clampedCircleOffset + circleStep * n;
               if (i == arcStart && hasPieEndVector) {
                  float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                  float startRotation = clampedCircleOffset + pieEndDegree;
                  arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, startRotation);
                  ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                  ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                  vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                  skippedVertices--;
               }
               else if (i == arcEnd && hasPieStartVector) {
                  float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                  float endRotation = clampedCircleOffset + pieStartDegree;
                  arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, endRotation);
                  ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                  ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                  vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                  skippedVertices--;
               }
               else {
                  n++;
                  if (arcEnd > arcStart) {
                     if (i >= arcStart && i <= arcEnd) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else if (i >= arcStart || i <= arcEnd) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
      }
      private void FillArea() {
         if (borderedRadius == 0f || clampedFill == 0f)
            return;

         borderedRadiusVector = new Vector2(0f, borderedRadius);
         int r = clampedResolution;
         int n = 0;

         if (clampedFill == 1f) {
            #region fill == 1f
            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < r; i++) {
               float rotation = clampedCircleOffset + (circleStep * i);
               vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
               skippedVertices--;
            }
            n = 0;
            for (int i = 0; i < r; i++) {
               fillTris[0 + n] = 0;
               fillTris[1 + n] = 1 + i;
               fillTris[2 + n] = 2 + i;
               n += 3;
               skippedTris -= 3;
            }
            fillTris[2 + n - 3] = 1;
            #endregion
         }
         else {
            #region fill < 1f
            int arcStart = fillStartVector;
            int arcEnd = fillEndVector;
            int xr = clampedResolution + fillExtraVertices;
            if (skipMainArea) {
               vertices[0] = Vector3.zero;
               skippedVertices--;
            }
            n = 0;
            for (int i = 0; i < xr; i++) {
               float rotation = clampedCircleOffset + circleStep * n;
               if (i == arcStart && hasFillStartVector) {
                  if (skipMainArea) {
                     float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                     float startRotation = clampedCircleOffset + fillStartDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, startRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                     vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                     skippedVertices--;
                  }
               }
               else if (i == arcEnd && hasFillEndVector) {
                  if (skipMainArea) {
                     float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                     float endRotation = clampedCircleOffset + fillEndDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, endRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else if (i >= arcStart || i <= arcEnd) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                     skippedVertices--;
                  }
               }
            }
            if (clampedFill != 0.5f && skipMainArea) {
               Vector3 arcLineA = ProcUtility.RotateAxis(borderedRadiusVector, clampedCircleOffset + clampedFillOffset);
               arcLineB = ProcUtility.RotateAxis(-borderedRadiusVector, clampedCircleOffset + clampedFillOffset);
               vertices[0] = ProcUtility.Intersection(arcLineA, arcLineB, vertices[1 + arcStart], vertices[1 + arcEnd]);
            }

            n = 0;
            for (int i = arcStart; i < arcStart + fillVertices - 1; i++) {
               fillTris[0 + n] = 0;
               fillTris[1 + n] = ProcUtility.SpinInt(1, i, 1, xr);
               fillTris[2 + n] = ProcUtility.SpinInt(2, i, 1, xr);
               n += 3;
               skippedTris -= 3;
            }
            #endregion
         }
      }
      private void PieArea() {
         if (borderedRadius == 0f || clampedPie == 0f || clampedFill > 0f)
            return;

         borderedRadiusVector = new Vector2(0f, borderedRadius);
         int r = clampedResolution;
         int n = 0;

         if (clampedPie == 360f) {
            #region # pie == 360f
            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < r; i++) {
               float rotation = clampedCircleOffset + (circleStep * i);
               vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
            #region # pie < 360f
            int arcStart = pieStartVector;
            int arcEnd = pieEndVector;
            int xr = clampedResolution + pieExtraVertices;
            if (skipMainArea) {
               vertices[0] = Vector3.zero;
               skippedVertices--;
            }
            n = 0;
            for (int i = 0; i < xr; i++) {
               float rotation = clampedCircleOffset + circleStep * n;
               if (i == arcStart && hasPieStartVector) {
                  if (skipMainArea) {
                     float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                     float startRotation = clampedCircleOffset + pieStartDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, startRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                     vertices[1 + i] = ProcUtility.Intersection(vertices[0], arcLineB, ringLineA, ringLineB);
                     skippedVertices--;
                  }
               }
               else if (i == arcEnd && hasPieEndVector) {
                  if (skipMainArea) {
                     float prevRotation = clampedCircleOffset + circleStep * (n - 1);
                     float endRotation = clampedCircleOffset + pieEndDegree;
                     arcLineB = ProcUtility.RotateAxis(borderedRadiusVector, endRotation);
                     ringLineA = ProcUtility.RotateAxis(borderedRadiusVector, prevRotation);
                     ringLineB = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else if (i >= arcStart || i <= arcEnd) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
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
      }
      private void BorderArea() {
         if (clampedRadius == 0f || clampedBorder == 0f)
            return;

         radiusVector = new Vector2(0f, clampedRadius);
         int r = clampedResolution;
         int n = 0;
         if (borderedRadius == 0f) {
            #region # Border full
            vertices[0] = Vector3.zero;
            skippedVertices--;
            for (int i = 0; i < r; i++) {
               float rotation = clampedCircleOffset + (circleStep * i);
               vertices[1 + i] = ProcUtility.RotateAxis(radiusVector, rotation);
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
            #region # Border not full
            borderedRadiusVector = new Vector2(0f, clampedRadius - clampedBorder);
            if ((clampedFill == 0f || clampedFill == 1f) && (clampedPie == 0f || clampedPie == 360f)) {
               #region # no/full fill && no/full pie
               for (int i = 0; i < r; i++) {
                  float rotation = clampedCircleOffset + (circleStep * i);
                  vertices[r + 1 + i] = ProcUtility.RotateAxis(radiusVector, rotation);
                  skippedVertices--;
                  if ((clampedFill == 0f && clampedPie == 0f && skipMainArea) || (clampedFill == 1f && skipFillArea) || (clampedPie == 360f && skipPieArea)) {
                     vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                     skippedVertices--;
                  }
               }
               n = 0;
               for (int i = 0; i < r; i++) {
                  borderTris[0 + n] = 1 + i;
                  borderTris[1 + n] = r + 1 + i;
                  borderTris[2 + n] = r + 2 + i;
                  borderTris[3 + n] = r + 2 + i;
                  borderTris[4 + n] = 2 + i;
                  borderTris[5 + n] = 1 + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[2 + n - 6] = r + 1;
               borderTris[3 + n - 6] = r + 1;
               borderTris[4 + n - 6] = 1;
               #endregion
            }
            else if (clampedFill > 0f && clampedFill < 1f) {
               #region # 0f < fill < 1f
               int arcStart = fillStartVector;
               int arcEnd = fillEndVector;
               int xr = clampedResolution + fillExtraVertices;
               n = 0;
               for (int i = 0; i < xr; i++) {
                  if (i == arcStart && hasFillStartVector)
                     continue;
                  if (i == arcEnd && hasFillEndVector)
                     continue;
                  float rotation = clampedCircleOffset + circleStep * n;
                  vertices[1 + xr + n] = ProcUtility.RotateAxis(radiusVector, rotation);
                  skippedVertices--;
                  n++;
                  if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipFillArea))
                     continue;
                  if (arcEnd > arcStart) {
                     if ((i >= arcStart && i <= arcEnd) && skipFillArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                     else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else {
                     if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                     else if ((i >= arcStart || i <= arcEnd) && skipFillArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
               }
               n = 0;
               int trail = 0;
               int lead = 0;
               for (int i = 0; i < r; i++) {
                  if (arcStart < arcEnd) {
                     if (hasFillStartVector) {
                        if (i + trail + 1 == arcStart)
                           trail++;
                        if (i + lead == arcStart)
                           lead++;
                     }
                     if (hasFillEndVector) {
                        if (i + trail + 1 == arcEnd)
                           trail++;
                        if (i + lead == arcEnd)
                           lead++;
                     }
                  }
                  else {
                     if (hasFillEndVector) {
                        if (i + trail + 1 == arcEnd)
                           trail++;
                        if (i + lead == arcEnd)
                           lead++;
                     }
                     if (hasFillStartVector) {
                        if (i + trail + 1 == arcStart)
                           trail++;
                        if (i + lead == arcStart)
                           lead++;
                     }
                  }
                  borderTris[0 + n] = 1 + lead + i;
                  borderTris[1 + n] = 1 + xr + i;
                  borderTris[2 + n] = 1 + xr + 1 + i;
                  borderTris[3 + n] = 1 + xr + 1 + i;
                  borderTris[4 + n] = 1 + trail + 1 + i;
                  borderTris[5 + n] = 1 + lead + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[2 + n - 6] = 1 + xr;
               borderTris[3 + n - 6] = 1 + xr;
               borderTris[4 + n - 6] = 1;
               #endregion
            }
            else {
               #region # 0f < pie < 360f
               int arcStart = pieStartVector;
               int arcEnd = pieEndVector;
               int xr = clampedResolution + pieExtraVertices;
               n = 0;
               for (int i = 0; i < xr; i++) {
                  if (i == arcStart && hasPieStartVector)
                     continue;
                  if (i == arcEnd && hasPieEndVector)
                     continue;
                  float rotation = clampedCircleOffset + circleStep * n;
                  vertices[1 + xr + n] = ProcUtility.RotateAxis(radiusVector, rotation);
                  skippedVertices--;
                  n++;
                  if ((i == arcStart || i == arcEnd) && (!skipMainArea || !skipPieArea))
                     continue;
                  if (arcEnd > arcStart) {
                     if ((i >= arcStart && i <= arcEnd) && skipPieArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                     else if ((i <= arcStart || i >= arcEnd) && skipMainArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
                  else {
                     if ((i <= arcStart && i >= arcEnd) && skipMainArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                     else if ((i >= arcStart || i <= arcEnd) && skipPieArea) {
                        vertices[1 + i] = ProcUtility.RotateAxis(borderedRadiusVector, rotation);
                        skippedVertices--;
                     }
                  }
               }
               n = 0;
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
                  borderTris[0 + n] = 1 + lead + i;
                  borderTris[1 + n] = 1 + xr + i;
                  borderTris[2 + n] = 1 + xr + 1 + i;
                  borderTris[3 + n] = 1 + xr + 1 + i;
                  borderTris[4 + n] = 1 + trail + 1 + i;
                  borderTris[5 + n] = 1 + lead + i;
                  n += 6;
                  skippedTris -= 6;
               }
               borderTris[2 + n - 6] = 1 + xr;
               borderTris[3 + n - 6] = 1 + xr;
               borderTris[4 + n - 6] = 1;
               #endregion
            }
            #endregion
         }
      }

      private void SetTextureCoordinates() {
         for (int i = 0; i < uv.Length; i++) {
            uv[i] = (vertices[i] + new Vector3(clampedRadius, clampedRadius));
            uv[i] = new Vector2(uv[i].x / (2 * clampedRadius), uv[i].y / (2 * clampedRadius));
         }
      }
      private void SetNormals() {
         for (int i = 0; i < normals.Length; i++) {
            normals[i] = Vector3.back;
         }
      }
      
      private void UpdateMesh() {
         GenericMath();
         if (clampedFill > 0f && clampedFill < 360f)
            FillMath();
         else if (clampedPie > 0f && clampedPie < 360f)
            PieMath();

         Indices();
         skippedVertices = vertices.Length;
         skippedTris = mainTris.Length + fillTris.Length + pieTris.Length + borderTris.Length;

         if (!skipMainArea)
            MainArea();
         if (!skipFillArea)
            FillArea();
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

         filter.sharedMesh.Clear();

         filter.sharedMesh.name = gameObject.name + " ProceduralCircle";
         filter.sharedMesh.subMeshCount = 4;

         if (vertices.Length > 0) {
            filter.sharedMesh.vertices = vertices;
            filter.sharedMesh.normals = normals;
            filter.sharedMesh.uv = uv;
            filter.sharedMesh.SetTriangles(mainTris, 0);
            filter.sharedMesh.SetTriangles(borderTris, 1);
            filter.sharedMesh.SetTriangles(fillTris, 2);
            filter.sharedMesh.SetTriangles(pieTris, 3);

            filter.sharedMesh.RecalculateTangents();
         }

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