using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Island : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int islandWidth;
    public int chunkWidth;
    public int metersBetweenPoints;
    public int maxHeight;
    public Material mat;
    public bool random;
    public float WaterLevel;

    [Header("Generation")]
    public float flattening;
    public SoundSettings Settings;

    public static Dictionary<Chunk, Chunk.CombinedMeshInfo> meshInfos = new Dictionary<Chunk, Chunk.CombinedMeshInfo>();

    private int toProcess = 0;

    public Sound sound { get; private set; }

    public int numChunks { get => Mathf.CeilToInt((float)islandWidth / chunkWidth); }

    public float SandStop { get; private set; }
    public float GrassStop { get; private set; }
    public float RockStop { get; private set; }

    Transform player;

    List<Chunk> chunks = new List<Chunk>();

    private void Awake()
    {
        Vector3 offset = Vector3.zero;
        if (random)
        {
            offset.x = Random.Range(-100000, 100000);
            offset.z = Random.Range(-100000, 100000);
        }

        mat.SetFloat("MaxHeight", maxHeight);

        SandStop = mat.GetFloat("_SandStop") * maxHeight;
        GrassStop = mat.GetFloat("_GrassStop") * maxHeight;
        RockStop = mat.GetFloat("_RockStop") * maxHeight;

        player = Camera.main.transform.parent;

        sound = new Sound(maxHeight, islandWidth, flattening, Settings, offset);
        GenChunks(); 
    }
    Vector2Int lastChunk;
    bool loaded = false;
    // Start is called before the first frame update
    private void FixedUpdate()
    {
        if (toProcess != 0 && meshInfos.Count == toProcess)
        {
            foreach(var kvp in meshInfos)
            {
                kvp.Key.TerrainCallback(kvp.Value);
            }
            meshInfos.Clear();
            loaded = true;
            Movement.canMove = true;
        }

        if (loaded)
        {
            var curChunk = new Vector2Int((int)(player.position.x / chunkWidth), (int)(player.position.z / chunkWidth));
            if (curChunk != lastChunk)
            {
                foreach (var chunk in chunks)
                {
                    chunk.CalcMesh(player.position);
                }
                lastChunk = curChunk;
            }
        }
    }

    private void GenChunks()
    {
        for (int x = 0; x < numChunks; x++)
        {
            for (int z = 0; z < numChunks; z++)
            {
                var chunk = new GameObject().AddComponent<Chunk>();
                chunk.name = $"{x},{z} Chunk";
                chunk.transform.parent = transform;
                chunk.transform.position = new Vector3(x * chunkWidth - islandWidth / 2f, 0, z * chunkWidth - islandWidth / 2f);
                chunk.Setup(chunkWidth, Mathf.CeilToInt((float)chunkWidth / metersBetweenPoints), mat, sound);
                toProcess++;
                chunks.Add(chunk);
            }
        }
    }
}
