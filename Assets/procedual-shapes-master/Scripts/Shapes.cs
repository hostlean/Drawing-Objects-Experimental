
using System;
using UnityEngine;

namespace ProceduralShapes {

   [Serializable] public enum Direction {
      /// <summary>
      /// Positive Z
      /// </summary>
      Forward = 0,
      /// <summary>
      /// Negative Z
      /// </summary>
      Back = 1,
      /// <summary>
      /// Positive X
      /// </summary>
      Right = 2,
      /// <summary>
      /// Negative X
      /// </summary>
      Left = 3,
      /// <summary>
      /// Positive Y
      /// </summary>
      Up = 4,
      /// <summary>
      /// Negative Y
      /// </summary>
      Down = 5
   }

   public class ProcUtility {

      public static Vector2 RotateVector(Vector2 v, float degrees) {
         float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
         float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

         float tx = v.x;
         float ty = v.y;
         v.x = (cos * tx) + (sin * ty);
         v.y = (cos * ty) - (sin * tx);
         return v;
      }
      public static Vector3 RotateAxis(Vector3 v, float degrees, Direction direction = Direction.Forward) {
         Vector2 result;
         switch (direction) {
            default:
            case Direction.Forward:
               result = v;
               break;
            case Direction.Back:
               result = new Vector2(-v.x, v.y);
               break;
            case Direction.Left:
               result = new Vector2(v.z, v.y);
               break;
            case Direction.Right:
               result = new Vector2(-v.z, v.y);
               break;
            case Direction.Down:
               result = new Vector2(v.x, -v.z);
               break;
            case Direction.Up:
               result = new Vector2(v.x, v.z);
               break;
         }
         result = RotateVector(result, degrees);
         switch (direction) {
            default:
            case Direction.Forward:
               break;
            case Direction.Back:
               result = new Vector3(-result.x, result.y, v.z);
               break;
            case Direction.Left:
               result = new Vector3(v.x, result.y, result.x);
               break;
            case Direction.Right:
               result = new Vector3(v.x, result.y, -result.x);
               break;
            case Direction.Down:
               result = new Vector3(result.x, v.y, -result.y);
               break;
            case Direction.Up:
               result = new Vector3(result.x, v.y, result.y);
               break;
         }
         return result;
      }
      public static Vector2 Intersection(Vector2 lineApointA, Vector2 lineApointB, Vector2 lineBpointA, Vector2 lineBpointB) {
         float A1 = lineApointB.y - lineApointA.y;
         float B1 = lineApointA.x - lineApointB.x;
         float C1 = A1 * lineApointA.x + B1 * lineApointA.y;

         float A2 = lineBpointB.y - lineBpointA.y;
         float B2 = lineBpointA.x - lineBpointB.x;
         float C2 = A2 * lineBpointA.x + B2 * lineBpointA.y;

         float delta = A1 * B2 - A2 * B1;
         if (delta == 0) throw new System.Exception("Procedural Shapes: Lines are parallel!");

         return new Vector2(
             (B2 * C1 - B1 * C2) / delta,
             (A1 * C2 - A2 * C1) / delta
         );
      }

      public static int SpinInt(int start, int spin, int min, int max) {
         if (spin == 0)
            return start;
         if (min == max)
            return min;

         int nmin = min;
         int nmax = max;
         if (min > max) {
            nmin = max;
            nmax = min;
            spin = -spin;
         }
         if (start < nmin)
            start = nmin;
         if (start > nmax)
            start = nmax;

         int result = start + spin;
         if (result > nmin && result < nmax)
            return result;

         result = start;
         if (spin > 0) {
            for (int i = 0; i < spin; i++) {
               result++;
               if (result > nmax)
                  result = nmin;
            }
         }
         else {
            for (int i = 0; i > spin; i--) {
               result--;
               if (result < nmin)
                  result = nmax;
            }
         }
         return result;
      }
      public static float SpinFloat(float start, float spin, float min, float max, float step) {
         if (spin == 0)
            return start;
         if (min == max)
            return min;

         float nmin = min;
         float nmax = max;
         if (min > max) {
            nmin = max;
            nmax = min;
            spin = -spin;
         }
         if (start < nmin)
            start = nmin;
         if (start > nmax)
            start = nmax;

         float result = start + spin;
         if (result > nmin && result < nmax)
            return result;

         result = start;
         if (spin > 0) {
            for (float i = 0; i < spin; i += step) {
               result += step;
               if (result > nmax)
                  result = nmin + (result - nmax);
            }
         }
         else {
            for (float i = 0; i > spin; i -= step) {
               result -= step;
               if (result < nmin)
                  result = nmax - (nmin - result);
            }
         }
         return result;
      }

      public static Vector3[] CornyArc(float start, float size, float offset, float radius, int resolution, Vector3 center, Quaternion rotation) {
         Vector3[] vertices = new Vector3[0];
         if (size <= 0f || radius <= 0f)
            return vertices;

         while (start < 0f)
            start += 360f;
         while (start >= 360f)
            start -= 360f;
         while (size < 0f)
            size += 360f;
         while (size >= 360f)
            size -= 360f;
         while (offset < 0f)
            offset += 360f;
         while (offset >= 360f)
            offset -= 360f;

         float cornerStep = 360f / resolution;
         float arcStartDegree = start;
         float arcEndDegree = start + size;
         if (arcEndDegree >= 360f)
            arcEndDegree -= 360f;

         bool arcStartOffCorner = false;
         float cheapCornersToStart = arcStartDegree / cornerStep;
         arcStartOffCorner = !Mathf.Approximately(cheapCornersToStart, Mathf.Round(cheapCornersToStart));
         
         bool arcEndOffCorner = false;
         float cheapCornersToEnd = arcEndDegree / cornerStep;
         arcEndOffCorner = !Mathf.Approximately(cheapCornersToEnd, Mathf.Round(cheapCornersToEnd));

         int arcVertices = 0;
         int cornersToStart = Mathf.FloorToInt(cheapCornersToStart);
         int cornersToEnd = Mathf.FloorToInt(cheapCornersToEnd);
         if (arcStartDegree < arcEndDegree)
            arcVertices = 1 + cornersToEnd - cornersToStart;
         else
            arcVertices = 1 + cornersToEnd + resolution - cornersToStart;
         if (arcEndOffCorner)
            arcVertices++;

         Vector3 radiusVector = new Vector3(0f, radius);
         Vector3 arcLineA, arcLineB, cornerLineA, cornerLineB;
         vertices = new Vector3[arcVertices];

         if (arcStartOffCorner) {
            float prevCornerDeg = offset + cornerStep * cornersToStart;
            float nextCornerDeg = offset + cornerStep * (cornersToStart + 1);
            float startDeg = offset + arcStartDegree;

            arcLineA = Vector3.zero;
            arcLineB = RotateAxis(radiusVector, startDeg);
            cornerLineA = RotateAxis(radiusVector, prevCornerDeg);
            cornerLineB = RotateAxis(radiusVector, nextCornerDeg);
            vertices[0] = Intersection(arcLineA, arcLineB, cornerLineA, cornerLineB);
            vertices[0] = rotation * vertices[0];
            vertices[0] += center;
         }
         else {
            vertices[0] = RotateAxis(radiusVector, offset + arcStartDegree);
            vertices[0] = rotation * vertices[0];
            vertices[0] += center;
         }
         int n = cornersToStart + 1;
         int v = 1;
         while (v < arcVertices - 1) {
            float cornerDeg = offset + cornerStep * n;
            vertices[v] = RotateAxis(radiusVector, cornerDeg);
            vertices[v] = rotation * vertices[v];
            vertices[v] += center;
            n++;
            v++;
         }
         if (v < arcVertices) {
            if (arcEndOffCorner) {
               float prevCornerDeg = offset + cornerStep * cornersToEnd;
               float nextCornerDeg = offset + cornerStep * (cornersToEnd + 1);
               float endDeg = offset + arcEndDegree;

               arcLineA = Vector3.zero;
               arcLineB = RotateAxis(radiusVector, endDeg);
               cornerLineA = RotateAxis(radiusVector, prevCornerDeg);
               cornerLineB = RotateAxis(radiusVector, nextCornerDeg);
               vertices[v] = Intersection(arcLineA, arcLineB, cornerLineA, cornerLineB);
               vertices[v] = rotation * vertices[v];
               vertices[v] += center;
            }
            else {
               vertices[v] = RotateAxis(radiusVector, offset + arcEndDegree);
               vertices[v] = rotation * vertices[v];
               vertices[v] += center;
            }
         }
         return vertices;
      }
   }

   /*public class ProcDirection {

      public Direction key;
      public Vector3 vector3 { get {
         switch (key) {
            default:
            case Direction.Back:
               return Vector3.back;
            case Direction.Forward:
               return Vector3.forward;
            case Direction.Left:
               return Vector3.left;
            case Direction.Right:
               return Vector3.right;
            case Direction.Down:
               return Vector3.down;
            case Direction.Up:
               return Vector3.up;
         }
      } }
      public Quaternion quaternion { get {
         switch (key) {
            default:
            case Direction.Back:
               return Quaternion.LookRotation(Vector3.back, Vector3.up);
            case Direction.Forward:
               return Quaternion.LookRotation(Vector3.forward, Vector3.up);
            case Direction.Left:
               return Quaternion.LookRotation(Vector3.left, Vector3.up);
            case Direction.Right:
               return Quaternion.LookRotation(Vector3.right, Vector3.up);
            case Direction.Down:
               return Quaternion.LookRotation(Vector3.down, Vector3.back);
            case Direction.Up:
               return Quaternion.LookRotation(Vector3.up, Vector3.forward);
         }
      } }
      public ProcDirection up { get {
            switch (key) {
               default:
               case Direction.Back:
               case Direction.Forward:
               case Direction.Left:
               case Direction.Right:
                  return new ProcDirection(Direction.Up);
               case Direction.Down:
                  return new ProcDirection(Direction.Back);
               case Direction.Up:
                  return new ProcDirection(Direction.Forward);
            }
         } }
      public ProcDirection right { get {
         switch (key) {
            default:
            case Direction.Back:
               return new ProcDirection(Direction.Left);
            case Direction.Forward:
               return new ProcDirection(Direction.Right);
            case Direction.Left:
               return new ProcDirection(Direction.Back);
            case Direction.Right:
               return new ProcDirection(Direction.Forward);
            case Direction.Down:
               return new ProcDirection(Direction.Right);
            case Direction.Up:
               return new ProcDirection(Direction.Right);
         }
      } }
      public ProcDirection inverse { get {
            switch (key) {
               default:
               case Direction.Back:
                  return new ProcDirection(Direction.Forward);
               case Direction.Forward:
                  return new ProcDirection(Direction.Back);
               case Direction.Left:
                  return new ProcDirection(Direction.Right);
               case Direction.Right:
                  return new ProcDirection(Direction.Left);
               case Direction.Down:
                  return new ProcDirection(Direction.Up);
               case Direction.Up:
                  return new ProcDirection(Direction.Down);
            }
         } }

      public ProcDirection() {
         key = Direction.Forward;
      }
      public ProcDirection(Direction key) {
         this.key = key;
      }

      public static Vector3 Swap(Vector3 point, Direction from, Direction to) {
         switch (from) {
            default:
            case Direction.Back:
               switch (to) {
                  default:
                  case Direction.Back:
                     return point;
                  case Direction.Forward:
                     return new Vector3(-point.x, point.y, -point.z);
                  case Direction.Left:
                     return new Vector3(point.z, point.y, -point.x);
                  case Direction.Right:
                     return new Vector3(-point.z, point.y, point.x);
                  case Direction.Down:
                     return new Vector3(point.x, -point.z, point.y);
                  case Direction.Up:
                     return new Vector3(point.x, point.z, -point.y);
               }
            case Direction.Forward:
               switch (to) {
                  default:
                  case Direction.Back:
                     return new Vector3(-point.x, point.y, -point.z);
                  case Direction.Forward:
                     return point;
                  case Direction.Left:
                     return new Vector3(-point.z, point.y, point.x);
                  case Direction.Right:
                     return new Vector3(point.z, point.y, -point.x);
                  case Direction.Down:
                     return new Vector3(-point.x, point.z, -point.y);
                  case Direction.Up:
                     return new Vector3(-point.x, -point.z, point.y);
               }
            case Direction.Left:
               switch (to) {
                  default:
                  case Direction.Back:
                     return new Vector3(-point.z, point.y, point.x);
                  case Direction.Forward:
                     return new Vector3(point.z, point.y, -point.x);
                  case Direction.Left:
                     return point;
                  case Direction.Right:
                     return new Vector3(-point.z, point.y, -point.z);
                  case Direction.Down:
                     return new Vector3(-point.z, point.x, -point.y);
                  case Direction.Up:
                     return new Vector3(-point.z, -point.x, -point.y);
               }
            case Direction.Right:
               switch (to) {
                  default:
                  case Direction.Back:
                     return new Vector3(point.z, point.y, -point.x);
                  case Direction.Forward:
                     return new Vector3(-point.z, point.y, point.x);
                  case Direction.Left:
                     return new Vector3(-point.x, point.y, -point.z);
                  case Direction.Right:
                     return point;
                  case Direction.Down:
                     return new Vector3(point.z, -point.x, -point.y);
                  case Direction.Up:
                     return new Vector3(point.z, point.x, point.y);
               }
            case Direction.Down:
               switch (to) {
                  default:
                  case Direction.Back:
                     return new Vector3(point.x, -point.y, -point.z);
                  case Direction.Forward:
                     return new Vector3(-point.x, -point.z, -point.y);
                  case Direction.Left:
                     return new Vector3(point.y, -point.z, -point.x);
                  case Direction.Right:
                     return new Vector3(-point.y, -point.z, point.x);
                  case Direction.Down:
                     return point;
                  case Direction.Up:
                     return new Vector3(point.x, -point.y, -point.z);
               }
            case Direction.Up:
               switch (to) {
                  default:
                  case Direction.Back:
                     return new Vector3(point.x, point.z, -point.y);
                  case Direction.Forward:
                     return new Vector3(-point.x, point.z, -point.y);
                  case Direction.Left:
                     return new Vector3(-point.y, point.z, -point.x);
                  case Direction.Right:
                     return new Vector3(point.y, point.z, point.x);
                  case Direction.Down:
                     return new Vector3(point.x, -point.z, -point.y);
                  case Direction.Up:
                     return point;
               }
         }
      }
      public static Vector3 Swap(Vector3 point, ProcDirection from, ProcDirection to) {
         return Swap(point, from.key, to.key);
      }
   }*/

}