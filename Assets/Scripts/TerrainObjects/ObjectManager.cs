using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class ObjectManager : MonoBehaviour
{
    public bool drawQuad;
    public bool instantiate;

    [Header("Object Settings")]
    public int ObjectCount;
    public float loadDistance;
    public float viewDistance = 150;

    [Header("Density")]
    public float SandDensity;
    public float grassDensity;
    public float rockDensity;

    [Serializable]
    public class TypeReference
    {
        public GameObject obj;
        public float weight = 1;
        public bool sand;
        public bool grass;
        public bool rock;

        public MeshRenderer mr { get; private set; }
        public MeshFilter mf { get; private set; }

        public void Process()
        {
            mr = obj.GetComponentInChildren<MeshRenderer>();
            mf = obj.GetComponentInChildren<MeshFilter>();
        }
    }

    public List<TypeReference> types;

    private Quadtree<ObjectReference> quadtree;
    private Island island;
    private Rect bounds;

    private Transform player;

    private static ObjectManager objectInstance;

    private Vector2 noiseOffset;
    // Start is called before the first frame update
    void Start()
    {
        Setup();

        if (instantiate)
        {
            if (objectInstance != null && objectInstance != this)
                throw new InvalidOperationException("Second instantiate object manager detected");
            objectInstance = this;
        }

        foreach (var obj in types)
        {
            obj.Process(); 
        }
    }

    private bool ValidForHeight(TypeReference type, float height)
    {
        return  (height > island.WaterLevel && height < island.SandStop && type.sand) ||
                (height > island.SandStop && height < island.GrassStop && type.grass) ||
                (height > island.GrassStop && height < island.RockStop && type.rock);
    }

    private bool DensityCalc(Vector3 pos)
    {
        var density = pos.y < island.SandStop ? SandDensity :
                        pos.y < island.GrassStop ? grassDensity :
                            pos.y < island.RockStop ? rockDensity : 0;

        return Mathf.PerlinNoise(pos.x / 100 + noiseOffset.x, pos.z/ 100 + noiseOffset.y) < UnityEngine.Random.Range(0, density);
    }

    private void Setup()
    {
        noiseOffset = new Vector2(UnityEngine.Random.Range(-10000f, 10000f), UnityEngine.Random.Range(-10000f, 10000f));
        player = Camera.main.transform.parent;
        island = GameObject.Find("Island").GetComponent<Island>();
        bounds = new Rect(Vector2.one * -island.islandWidth / 2, Vector2.one * island.islandWidth);

        quadtree = new Quadtree<ObjectReference>(bounds);
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        for(int i = 0; i<ObjectCount; i++)
        {
            var newPos = bounds.position + new Vector2(UnityEngine.Random.Range(0, bounds.width), UnityEngine.Random.Range(0, bounds.height));
            
            var height = island.sound.GetHeight(new Vector3(newPos.x, 0, newPos.y));

            if (height < island.WaterLevel) continue;

            if (!DensityCalc(new Vector3(newPos.x, height, newPos.y))) continue;

            var validTypes = types.Where(t => ValidForHeight(t, height)).ToList();

            if (validTypes.Count == 0) continue;

            var typeRef = validTypes.RandomElementByWeight(t => t.weight);

            int type = types.IndexOf(typeRef);

            var reference = new ObjectReference(newPos, type, height, typeRef.obj.transform.localScale);
            quadtree.Insert(reference);
        }
    }

    IEnumerable<IGrouping<int, ObjectReference>> toRender;

    private void Update()
    {
        if(toRender != null)
        {
            foreach (var group in toRender)
            {
                Render(group.Key, group);
            }
        }
    }
     
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 playerPos = Utilities.Vector3ToXZ(player.position);

        var toRender = quadtree.FindClosest(playerPos + Utilities.Vector3ToXZ(player.forward) * viewDistance * 3/4, viewDistance);

        if (instantiate)
        {
            var closest = quadtree.FindClosest(playerPos, loadDistance);
            RemoveAllChildren();
            foreach (var obj in closest)
            {
                toRender.Remove(obj);
                var spawned = Instantiate(types[obj.Type].obj, obj.WorldLocation, obj.Rotation, transform);
                spawned.transform.localScale = obj.Scale;
            }
        }

        this.toRender = toRender.GroupBy(r => r.Type);
    }

    private void RemoveAllChildren()
    {
        for (var i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void Render(int type, IEnumerable<ObjectReference> references)
    {
        for (int i = 0; i < Mathf.CeilToInt(references.Count() / 1023f); i++)
        {
            var batch = references.Skip(i * 1023).Take(1023);
            var meshFilter = types[type].mf;
            var meshRenderer = types[type].mr;
            Graphics.DrawMeshInstanced(meshFilter.sharedMesh, 0, meshRenderer.sharedMaterial, batch.Select(r => r.matrix).ToArray());
        }
    }

    private void OnDrawGizmos()
    {
        if (drawQuad && Application.isPlaying)
        {
            quadtree.Display();
            Vector2 playerPos = new Vector2(player.position.x, player.position.z);
            var playerBounds = new Rect(playerPos - Vector2.one * 5, Vector2.one * 5 * 2);
            Gizmos.DrawWireCube(new Vector3(playerBounds.center.x, 0, playerBounds.center.y) + Vector3.up * 100, new Vector3(playerBounds.size.x, 0, playerBounds.size.y));
        }
    }

    public static void RemoveObject(Vector3 pos)
    {
        objectInstance.quadtree.Remove(Utilities.Vector3ToXZ(pos));
    }

    public class ObjectReference : Trackable
    {
        private Vector2 location;
        public Vector2 Location() 
        {
            return location;
        }

        public int Type { get; private set; }

        public float height { get; private set; }

        public Matrix4x4 matrix { get; private set; }

        public Quaternion Rotation { get; private set; }

        public Vector3 Scale { get; private set; }

        public Vector3 WorldLocation { get; private set; }

        public ObjectReference(Vector2 loc, int type, float height, Vector3 scale)
        {
            location = loc;
            Type = type;
            this.height = height;
            Rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
            Scale = scale * UnityEngine.Random.Range(0.8f, 1.2f);
            WorldLocation = new Vector3(loc.x, height, loc.y);

            matrix = Matrix4x4.TRS(WorldLocation, Rotation, Scale);
        }
    }
}
