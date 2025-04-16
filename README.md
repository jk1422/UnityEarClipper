# 🔺 UnityEarClipper – Polygon Triangulation for Unity

**UnityEarClipper** is a lightweight and efficient triangulation algorithm for generating meshes in Unity.  
Unlike other implementations that often rely on high-precision math to mitigate floating-point round-off errors, UnityEarClipper takes a different approach:  

It projects and flattens 3D vertices into 2D space before performing triangulation.

This makes it simpler and more performant, making it suitable for use during runtime.

![Triangles](https://github.com/user-attachments/assets/b4ce99c5-7595-4f7d-95f6-ae1b9e3eb0a4)

## 🛠️ How To Use

Using UnityEarClipper in your Unity project is simple. Here's some examples:

### Example 1: Create a simple plane

```csharp
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

mesh.RecalculateNormals();
mesh.RecalculateBounds();

// Assign the generated mesh to this object's MeshFilter component.
GetComponent<MeshFilter>().mesh = mesh;
   ```

### Example 2: Create a plane with a hole
   ```c#
// Define the outer boundary of the polygon.
// Must be ordered in **clockwise** direction.
Vector3[] outer = new Vector3[] {
    new Vector3(1f, 0f, 0f),
    new Vector3(-1f, 0f, 0f),
    new Vector3(-1f, 2f, 0f),
    new Vector3(1f, 2f, 0f)
};

// Define a hole inside the polygon.
// Each hole must be ordered in **counter-clockwise** direction.
Vector3[] hole1 = new Vector3[] {
    new Vector3(-0.25f, 0.75f, 0f),
    new Vector3(0.25f, 0.75f, 0f),
    new Vector3(0.25f, 1.25f, 0f),
    new Vector3(-0.25f, 1.25f, 0f)
};

// Add holes to a list
List<Vector3[]> holes = new() { hole1 };

// Create an EarClipper instance that merges the outer shape with the holes.
// The merged polygon will be returned in the 'merged' output variable.
EarClipper earClipper = new(outer, holes, out var merged);

// Create a Unity Mesh and assign the merged vertices.
Mesh mesh = new();
mesh.vertices = merged; // Note that we now use the merged result.

// Generate triangle indices.
// Pass 'true' to flip the triangle winding order if needed.
mesh.triangles = earClipper.Triangulate(true);

mesh.RecalculateNormals();
mesh.RecalculateBounds();

// Assign the generated mesh to this object's MeshFilter component.
GetComponent<MeshFilter>().mesh = mesh;

   ```


> [!TIP]  
> ⭐ If you find this useful, don't forget to star it! It not only helps others find it, but also gives me a little ego boost 😊
