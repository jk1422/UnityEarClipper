using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace EarClipperLib
{
    public struct Triangle
    {
        public int prev;
        public int origin;
        public int next;

        public Triangle(int origin, int prev, int next)
        {
            this.origin = origin;
            this.prev = prev;
            this.next = next;
        }
    }

    public class EarClipper
    {
        public Vector2[] points2D;
        public List<Triangle> triangles = new();

        public EarClipper(Vector3[] points)
        {
            this.points2D = ProjectVerts(points);
        }
        public EarClipper(Vector3[] points, List<Vector3[]> holes, out Vector3[] merged)
        {
            List<Vector3> merge = new();

            merge.AddRange(points);

            foreach (var hole in holes)
            {
                merge = MergeHoleIntoPolygon(merge.ToArray(), hole);
            }
            merged = merge.ToArray();
            this.points2D = ProjectVerts(merged);
        }

        private List<Vector3> MergeHoleIntoPolygon(Vector3[] outer, Vector3[] hole)
        {
            int holeIndex = FindLeftMostPoint(hole);
            Vector3 holePoint = hole[holeIndex];

            int outerIndex = FindClosestVisiblePoint(holePoint, outer);

            List<Vector3> merged = new();

            for (int i = 0; i <= outerIndex; i++)
                merged.Add(outer[i]);

            for (int i = 0; i < hole.Length; i++)
                merged.Add(hole[(holeIndex + i) % hole.Length]);

            merged.Add(hole[holeIndex]);
            merged.Add(outer[outerIndex]);


            for (int i = outerIndex + 1; i < outer.Length; i++)
                merged.Add(outer[i]);

            return merged;
        }

        private int FindLeftMostPoint(Vector3[] poly)
        {
            int index = 0;
            float minX = poly[0].x;

            for (int i = 1; i < poly.Length; i++)
            {
                if (poly[i].x < minX)
                {
                    minX = poly[i].x;
                    index = i;
                }
            }

            return index;
        }

        private int FindClosestVisiblePoint(Vector3 from, Vector3[] poly)
        {
            int index = 0;
            float minDist = float.MaxValue;

            for (int i = 0; i < poly.Length; i++)
            {
                float dist = Vector3.Distance(from, poly[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    index = i;
                }
            }

            return index;
        }

        public int[] Triangulate(bool flipped = false)
        {
            List<int> remaining = Enumerable.Range(0, points2D.Length).ToList();

            while (remaining.Count > 3)
            {
                bool earClipped = false;

                for (int i = 0; i < remaining.Count; i++)
                {
                    int prevIndex = (i - 1 + remaining.Count) % remaining.Count;
                    int nextIndex = (i + 1) % remaining.Count;

                    int prev = remaining[prevIndex];
                    int curr = remaining[i];
                    int next = remaining[nextIndex];

                    if (IsConvex(points2D[prev], points2D[curr], points2D[next]) &&
                        NoPointWithin(prev, curr, next, remaining))
                    {
                        triangles.Add(new Triangle(curr, prev, next));
                        remaining.RemoveAt(i);
                        earClipped = true;
                        break;
                    }
                }

                if (!earClipped)
                {
                    Debug.LogWarning("Failed to proceed. Ensure points are ordered clockwise and holes are counter clockwise.");
                    break;
                }
            }

            if (remaining.Count == 3)
            {
                triangles.Add(new Triangle(remaining[1], remaining[0], remaining[2]));
            }

            List<int> result = new();
            foreach (var tri in triangles)
            {
                result.Add(tri.prev);
                result.Add(tri.origin);
                result.Add(tri.next);
            }
            var res = result.ToArray();
            if (flipped) return res.Reverse().ToArray();
            return res;
        }

        private bool IsConvex(Vector2 prev, Vector2 curr, Vector2 next)
        {
            Vector2 dir1 = (curr - prev).normalized;
            Vector2 dir2 = (next - curr).normalized;
            float cross = dir1.x * dir2.y - dir1.y * dir2.x;
            return cross > 0f;
        }

        private bool NoPointWithin(int prev, int curr, int next, List<int> remaining)
        {
            Vector2 a = points2D[prev];
            Vector2 b = points2D[curr];
            Vector2 c = points2D[next];

            foreach (int i in remaining)
            {
                if (i == prev || i == curr || i == next) continue;

                // Skippa punkter som är praktiskt taget lika som a, b eller c
                if (Vector2.Distance(points2D[i], a) < 0.0001f ||
                    Vector2.Distance(points2D[i], b) < 0.0001f ||
                    Vector2.Distance(points2D[i], c) < 0.0001f)
                {
                    continue;
                }

                if (PointInTriangle(points2D[i], a, b, c)) return false;
            }

            return true;
        }

        private bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float dX = p.x - c.x;
            float dY = p.y - c.y;
            float dX21 = c.x - b.x;
            float dY12 = b.y - c.y;
            float D = dY12 * (a.x - c.x) + dX21 * (a.y - c.y);
            float s = dY12 * dX + dX21 * dY;
            float t = (c.y - a.y) * dX + (a.x - c.x) * dY;

            if (D < 0)
                return s <= 0 && t <= 0 && s + t >= D;
            return s >= 0 && t >= 0 && s + t <= D;
        }

        private Vector2[] ProjectVerts(Vector3[] input)
        {
            Vector3 dim = GetDimensions(input);
            float shortest = Mathf.Min(dim.x, dim.y, dim.z);

            Vector3 direction = (shortest == dim.x) ? Vector3.right :
                                (shortest == dim.y) ? Vector3.down : Vector3.forward;

            Quaternion rot = Quaternion.FromToRotation(direction, Vector3.up);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rot);

            Vector2[] result = new Vector2[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                var projected = rotationMatrix.MultiplyPoint3x4(input[i]);
                projected.y = 0;
                result[i] = new Vector2(projected.x, projected.z);
            }
            return result;
        }

        private Vector3 GetDimensions(Vector3[] points)
        {
            float minX = points[0].x, maxX = points[0].x;
            float minY = points[0].y, maxY = points[0].y;
            float minZ = points[0].z, maxZ = points[0].z;

            foreach (var p in points)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
            }

            return new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        }
    }

}