using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EarClipperLib.Sample
{
    [Serializable]
    public class BaseMesh
    {
        public Vector3[] points;
    }
    [Serializable]
    public class Hole
    {
        public Vector3[] points;
    }
    public class ExampleWall : MonoBehaviour
    {
        [Header("Gizmos")]
        public bool drawBasePoints = true;
        public bool drawHoles = true;
        public bool drawWireMesh = false;

        [Header("Settings")]
        public bool flipFaces = false;

        [SerializeField]
        BaseMesh myBaseMesh;

        [SerializeField]
        List<Hole> myHoles = new();

        public void GenerateMesh()
        {
            if (myHoles.Count > 0) CreateMeshWithHoles();
            else CreateMesh();
        }

        private void CreateMesh()
        {
            // Define the vertices of a simple polygon.
            // IMPORTANT: The points must be ordered in **clockwise** order for proper triangulation.
            Vector3[] points = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(-1f, 0, 0),
                new Vector3(-1f, 1f, 0),
                new Vector3(0, 1f, 0)
            };

            // Create a new instance of the EarClipper triangulator with the defined points.
            EarClipper earClipper = new(points);

            // Generate triangle indices using the ear clipping algorithm.
            int[] triangles = earClipper.Triangulate();

            // Create a new Unity mesh and assign the vertices and triangles.
            Mesh mesh = new();
            mesh.vertices = points;
            mesh.triangles = triangles;

            // Recalculate normals and bounds to ensure correct rendering and physics interaction.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Assign the generated mesh to this object's MeshFilter component.
            GetComponent<MeshFilter>().mesh = mesh;
        }


        private void CreateMeshWithHoles()
        {
            // Define the outer boundary of the polygon.
            // Must be ordered in **clockwise** direction.
            Vector3[] points = myBaseMesh.points;

            // Define the holes inside the polygon.
            // Each hole must be defined in **counter-clockwise** order.
            List<Vector3[]> holes = new();
            myHoles.ForEach(h => holes.Add(h.points));

            // Create an EarClipper instance that will automatically merge the holes with the outer shape.
            // The merged result is returned as a new array of points.
            EarClipper earClipper = new(points, holes, out var merged);

            // Create a Unity Mesh and assign the merged vertices.
            Mesh mesh = new();
            mesh.vertices = merged;

            // Generate triangle indices from the merged polygon structure.
            // Set 'flipFaces' to true if you want to invert the triangle winding order (If you want to flip the face of your mesh).
            mesh.triangles = earClipper.Triangulate(flipFaces);

            // Generate UVs for the mesh (implementation of GenerateUV is user-defined).
            mesh.uv = GenerateUV(merged);

            // Recalculate normals and bounds for correct lighting and rendering.
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Assign the mesh to the MeshFilter component of this GameObject.
            GetComponent<MeshFilter>().mesh = mesh;
        }

        Vector2[] GenerateUV(Vector3[] points)
        {
            Vector2[] uvs = new Vector2[points.Length];

            float minX = points.Min(p => p.x);
            float maxX = points.Max(p => p.x);
            float minY = points.Min(p => p.y);
            float maxY = points.Max(p => p.y);

            float sizeX = maxX - minX;
            float sizeY = maxY - minY;

            for (int i = 0; i < points.Length; i++)
            {
                float u = (points[i].x - minX) / sizeX;
                float v = (points[i].y - minY) / sizeY;
                uvs[i] = new Vector2(u, v);
            }

            return uvs;
        }

        void OnDrawGizmos()
        {
            if (drawBasePoints) DrawPoints(myBaseMesh.points);
            if (drawHoles) myHoles.ForEach(h => DrawPoints(h.points));
            if (drawWireMesh) DrawMesh();
        }
        void DrawMesh()
        {
            Gizmos.color = Color.gray;
            if (TryGetComponent<MeshFilter>(out var mf))
            {
                if (mf.sharedMesh != null) Gizmos.DrawWireMesh(mf.sharedMesh);
            }
        }
        void DrawPoints(Vector3[] points)
        {
            var colors = GenerateColors(points.Length);

            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.color = colors[i];
                Gizmos.DrawSphere(points[i], 0.05f);
            }
        }

        Color[] GenerateColors(int count)
        {
            Color[] colors = new Color[count];

            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                colors[i] = Color.Lerp(Color.red, Color.green, t);
            }

            return colors;
        }
    }
}

