using UnityEngine;
using UnityEditor;
using System.IO;

namespace LowPolyWater
{
    public class GeneratePlane : ScriptableWizard
    {
        public string objectName = "Plane";           //Optional name that can given to created plane gameobject

        public float PlaneSize = 32;
        public float MetersBetweenPoints = 5;

        public Material material;           //By default, it is assigned to 'LowPolyWaterMaterial' in the editor

        int vertPerChunk = 254;

        [MenuItem("GameObject/LowPoly Water/Generate Water Plane...")]
        static void CreateWizard()
        {
            DisplayWizard("Generate Water Plane", typeof(GeneratePlane));
        }

        private void OnWizardCreate()
        {
            GameObject container = new GameObject();
            container.AddComponent<SetHeight>();
            if (string.IsNullOrEmpty(objectName))
            {
                container.name = "Plane";
            }
            else
            {
                container.name = objectName;
            }

            int totalVert = Mathf.CeilToInt(PlaneSize / MetersBetweenPoints);
            int numChunks = Mathf.CeilToInt(totalVert / (float)vertPerChunk);

            var mesh = GenMesh(vertPerChunk);

            for(int x = 0; x<numChunks; x++)
            {
                for (int y = 0; y < numChunks; y++)
                {
                    var chunk = GenSingleChunk(x, y, mesh);
                    chunk.transform.parent = container.transform;
                    chunk.transform.position = new Vector3(x - numChunks / 2, 0, y - numChunks / 2) * vertPerChunk * MetersBetweenPoints;
                    if(numChunks % 2 != 0) 
                        chunk.transform.position -= new Vector3(1, 0, 1) * vertPerChunk * MetersBetweenPoints / 2;
                }
            }
        }

        private Mesh GenMesh(int vertCount)
        {
            Mesh m = new Mesh();

            int width = vertCount;
            int height = vertCount;
            int count = 0;

            Vector3[] verticesHigh = new Vector3[(width + 1) * (height + 1)];
            for (float d = 0; d <= height; d++)
            {
                for (float w = 0; w <= width; w++)
                {
                    verticesHigh[count] = new Vector3(MetersBetweenPoints * w, 0, MetersBetweenPoints * d);
                    count++;
                }
            }

            // Defining triangles.

            int[] triangles = new int[width * height * 2 * 3]; // 2 - polygon per quad, 3 - corners per polygon
            for (int d = 0; d < height; d++)
            {
                for (int w = 0; w < width; w++)
                {
                    // quad triangles index.
                    int ti = (d * (width) + w) * 6; // 6 - polygons per quad * corners per polygon
                                                    // First tringle
                    triangles[ti] = (d * (width + 1)) + w;
                    triangles[ti + 1] = ((d + 1) * (width + 1)) + w;
                    triangles[ti + 2] = ((d + 1) * (width + 1)) + w + 1;
                    // Second triangle
                    triangles[ti + 3] = (d * (width + 1)) + w;
                    triangles[ti + 4] = ((d + 1) * (width + 1)) + w + 1;
                    triangles[ti + 5] = (d * (width + 1)) + w + 1;
                }
            }
            /*
            Vector3[] flatShaded = new Vector3[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                flatShaded[i] = verticesHigh[triangles[i]];
                triangles[i] = i;
            }*/

            //Update the mesh properties (vertices, UVs, triangles, normals etc.)
            m.vertices = verticesHigh;
            m.triangles = triangles;
            m.RecalculateNormals();

            //Update mesh
            m.RecalculateBounds();

            return m;
        }

        private GameObject GenSingleChunk(int xPos, int yPos, Mesh mesh)
        {
            //Create an empty gamobject
            GameObject plane = new GameObject();

            //If user hasn't assigned a name, by default object name is 'Plane'
            if (string.IsNullOrEmpty(objectName))
            {
                plane.name = "Plane" + xPos + "x" + yPos;
            }
            else
            {
                plane.name = objectName + xPos + "x" + yPos;
            }

            //Create Mesh Filter and Mesh Renderer components
            MeshFilter meshFilter = plane.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshRenderer meshRenderer = plane.AddComponent((typeof(MeshRenderer))) as MeshRenderer;
            meshRenderer.sharedMaterial = material;

            meshFilter.sharedMesh = mesh;

            return plane;
        }
    }
}
