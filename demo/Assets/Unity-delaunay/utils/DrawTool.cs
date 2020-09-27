using System.Collections.Generic;
using System.Linq;
using Delaunay.Geo;
using UnityEngine;

namespace Delaunay
{
    public class DrawTool
    {
        public static GameObject DrawPolygon(List<Vector2> list,Color color)
        {
            // Create Vector2 vertices
            var vertices2D = list.ToArray();

            var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);
 
            // Use the triangulator to get indices for creating triangles
            var triangulator = new Triangulator(vertices2D);
            var indices =  triangulator.Triangulate();
		
            // Generate a color for each vertex
            var colors = Enumerable.Range(0, vertices3D.Length)
                .Select(i => color)
                .ToArray();

            // Create the mesh
            var mesh = new Mesh {
                vertices = vertices3D,
                triangles = indices,
                colors = colors
            };
		
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
 
            var go = new GameObject();
            
            // Set up game object with mesh;
            var meshRenderer = go.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
		
            var filter = go.gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            return go;
        }
    }
}