using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer mr;
    MeshCollider mc;
    Sound sound;
    Vector2 Vector3ToXZ(Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }

    public void CalcMesh(Vector3 playerPos)
    {
        if(Vector2.Distance(Vector3ToXZ(playerPos), Vector3ToXZ(transform.position)) < 150)
        {
            meshFilter.sharedMesh = highMesh;
        }
        else
        {
            meshFilter.sharedMesh = lowMesh;
        }
    }

    // Start is called before the first frame update
    public void Setup(int sideLen, int vertexCount, Material mat, Sound sound)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = mat;
        mc = gameObject.AddComponent<MeshCollider>();
        this.sound = sound;
        startTerrainGenThread(vertexCount, sideLen);
    }

    Mesh highMesh;
    Mesh lowMesh;
    public void TerrainCallback(CombinedMeshInfo info)
    {
        lowMesh = getMesh(info.low);
        highMesh = getMesh(info.high);
        mc.sharedMesh = lowMesh;
    }

    private void startTerrainGenThread(int vertexCount, int sideLength)
    {
        var pos = transform.position;
        ThreadStart threadStart = delegate
        {
            GenCombinedMesh(vertexCount, sideLength, pos);
        };
        new Thread(threadStart).Start();
    }

    Mesh getMesh(MeshInfo info)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = info.vertices;
        mesh.triangles = info.triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void GenCombinedMesh(int vertexCount, int sideLen, Vector3 chunkPos)
    {
        lock (Island.meshInfos)
        {
            Island.meshInfos.Add(this, new CombinedMeshInfo(meshGen(vertexCount, sideLen, chunkPos), meshGen(vertexCount / 2, sideLen, chunkPos)));
        }
    }

    private MeshInfo meshGen(int vertexCount, int sideLen, Vector3 chunkPos)
    {

        int width = vertexCount;
        int height = vertexCount;

        float metersBetweenVerts = sideLen / (float)vertexCount;
        // Creating a mesh object.
        int count = 0;

        var rand = new System.Random(Thread.CurrentThread.ManagedThreadId);

        Vector3[] verticesHigh = new Vector3[(width + 1) * (height + 1)];
        for (float d = 0; d <= height; d++)
        {
            for (float w = 0; w <= width; w++)
            {
                Vector3 randoffset = Vector3.zero;

                if (d != 0 && d != height && w != 0 && w != width)
                {
                    randoffset = new Vector3((float)rand.NextDouble() * 0.75f * metersBetweenVerts, 0, (float)rand.NextDouble() * 0.75f * metersBetweenVerts);
                }

                Vector3 newPos = new Vector3(metersBetweenVerts * w, 0, metersBetweenVerts * d) + randoffset;
                
                newPos.y = sound.GetHeight(newPos + chunkPos);
                
                verticesHigh[count] = newPos;
                count++;
            }
        }

        // Defining triangles.

        int[] trianglesHigh = new int[width * height * 2 * 3]; // 2 - polygon per quad, 3 - corners per polygon
        for (int d = 0; d < height; d++)
        {
            for (int w = 0; w < width; w++)
            {
                // quad triangles index.
                int ti = (d * (width) + w) * 6; // 6 - polygons per quad * corners per polygon
                                                // First tringle
                trianglesHigh[ti] = (d * (width + 1)) + w;
                trianglesHigh[ti + 1] = ((d + 1) * (width + 1)) + w;
                trianglesHigh[ti + 2] = ((d + 1) * (width + 1)) + w + 1;
                // Second triangle
                trianglesHigh[ti + 3] = (d * (width + 1)) + w;
                trianglesHigh[ti + 4] = ((d + 1) * (width + 1)) + w + 1;
                trianglesHigh[ti + 5] = (d * (width + 1)) + w + 1;
            }
        }
        

        Vector3[] flatShaded = new Vector3[trianglesHigh.Length];

        for(int i = 0; i<trianglesHigh.Length; i++)
        {
            flatShaded[i] = verticesHigh[trianglesHigh[i]];
            trianglesHigh[i] = i;
        }

        return new MeshInfo(flatShaded, trianglesHigh);
    }

    public class MeshInfo
    {
        public Vector3[] vertices;
        public int[] triangles;

        public MeshInfo(Vector3[] vertices, int[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
    }

    public class CombinedMeshInfo
    {
        public MeshInfo high;
        public MeshInfo low;

        public CombinedMeshInfo(MeshInfo high, MeshInfo low)
        {
            this.low = low;
            this.high = high;
        }
    }
}
