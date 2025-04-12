using System.Collections.Generic;
using System.Linq;
using EarClipperLib;
using UnityEngine;
namespace EarClipperLib.Sample
{
    public class Plane
    {
        public Vector3[] verts;
        public bool flipFaces;
        public Plane(Vector3[] verts, bool flipFaces = false)
        {
            this.verts = verts;
            this.flipFaces = flipFaces;
        }
    }
    public class ExampleCube : MonoBehaviour
    {
        List<Plane> planes = new();

        public void GenerateCube()
        {
            CreatePlanes();
        }
        void CreatePlanes()
        {
            planes.Add(new Plane(new Vector3[] { new Vector3(0.4f, 0.8f, 0), new Vector3(-0.4f, 0.8f, 0), new Vector3(-0.4f, 0.8f, 0.4f), new Vector3(0.4f, 0.8f, 0.4f) })); // Top plane
            planes.Add(new Plane(new Vector3[] { new Vector3(0.4f, 0, 0), new Vector3(-0.4f, 0, 0), new Vector3(-0.4f, 0.8f, 0), new Vector3(0.4f, 0.8f, 0) })); // Front plane
            planes.Add(new Plane(new Vector3[] { new Vector3(0.4f, 0, 0.4f), new Vector3(-0.4f, 0, 0.4f), new Vector3(-0.4f, 0.8f, 0.4f), new Vector3(0.4f, 0.8f, 0.4f) }, true)); // Back plane
            planes.Add(new Plane(new Vector3[] { new Vector3(-0.4f, 0, 0), new Vector3(-0.4f, 0, 0.4f), new Vector3(-0.4f, 0.8f, 0.4f), new Vector3(-0.4f, 0.8f, 0) })); // Left plane
            planes.Add(new Plane(new Vector3[] { new Vector3(0.4f, 0, 0), new Vector3(0.4f, 0, 0.4f), new Vector3(0.4f, 0.8f, 0.4f), new Vector3(0.4f, 0.8f, 0) }, true)); // Right plane
            planes.Add(new Plane(new Vector3[] { new Vector3(0.4f, 0, 0), new Vector3(-0.4f, 0, 0), new Vector3(-0.4f, 0, 0.4f), new Vector3(0.4f, 0, 0.4f) }, true)); // Bottom plane


            List<Mesh> meshes = new(6);

            foreach (var plane in planes)
            {
                meshes.Add(MeshFromPlane(plane));
            }



            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i];
                combine[i].transform = transform.localToWorldMatrix;
            }

            Mesh mesh = new();
            mesh.CombineMeshes(combine, true);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().mesh = mesh;

            transform.position = new Vector3(-1f, 0.6f, 0);
        }

        Mesh MeshFromPlane(Plane plane)
        {
            EarClipper earClipper = new(plane.verts);

            Mesh mesh = new();
            mesh.vertices = plane.verts;
            mesh.triangles = earClipper.Triangulate(plane.flipFaces);

            return mesh;
        }

    }
}
