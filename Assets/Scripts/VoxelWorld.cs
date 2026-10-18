using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Blocklands
{
    public partial class VoxelWorld : MonoBehaviour
    {
        public const int Width=96, Height=80, Depth=96, ChunkSize=16, Seed=731;
        public const int WaterLevel=12;
        public const int MinY=-272,VerticalSize=Height-MinY;
        readonly Block[,,] cells=new Block[Width,VerticalSize,Depth];
        readonly Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
        public readonly Dictionary<Vector3Int,Block> Edits=new Dictionary<Vector3Int,Block>();
        Material ground,water;
        public Vector3 Spawn => Region==0?new Vector3(28.5f,GroundHeight(28,28)+3,28.5f):new Vector3(30.5f,RegionHeight(30,12)+3,12.5f);
        static readonly Vector3Int[] Directions={Vector3Int.right,Vector3Int.left,Vector3Int.up,Vector3Int.down,Vector3Int.forward,Vector3Int.back};
        // Counter-clockwise when viewed from outside (cross products point outward).
        static readonly Vector3[,] Corners={
            {new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(1,1,1),new Vector3(1,0,1)},
            {new Vector3(0,0,1),new Vector3(0,1,1),new Vector3(0,1,0),new Vector3(0,0,0)},
            {new Vector3(0,1,1),new Vector3(1,1,1),new Vector3(1,1,0),new Vector3(0,1,0)},
            {new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,0,1),new Vector3(0,0,1)},
            {new Vector3(1,0,1),new Vector3(1,1,1),new Vector3(0,1,1),new Vector3(0,0,1)},
            {new Vector3(0,0,0),new Vector3(0,1,0),new Vector3(1,1,0),new Vector3(1,0,0)}
        };
        static readonly Vector2[] UV={new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};
        class Chunk { public GameObject Root; public Mesh Solid,Water; }

        public static bool InBounds(Vector3Int p) => p.x>=0 && p.x<Width && p.y>=MinY && p.y<Height && p.z>=0 && p.z<Depth;
        public Block Get(Vector3Int p) => InBounds(p)?cells[p.x,p.y-MinY,p.z]:Block.Air;
        Dictionary<Vector3Int,Block> houseDraft;
        void Put(int x,int y,int z,Block b) {var p=new Vector3Int(x,y,z);if(houseDraft!=null){houseDraft[p]=b;return;}if(InBounds(p))cells[x,y-MinY,z]=b;}
        public Dictionary<Vector3Int,Block> PlanHouse(int x,int z){houseDraft=new Dictionary<Vector3Int,Block>();try{House(x,z);return houseDraft;}finally{houseDraft=null;}}
        public static int GroundHeight(int x,int z)
        {
            float hills=Mathf.PerlinNoise((x+Seed)*.035f,(z+Seed)*.035f)*10;
            float detail=Mathf.PerlinNoise(x*.11f+12,z*.11f+29)*3;
            float lake=Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,z),new Vector2(17,18))/13)*10;
            int h=Mathf.RoundToInt(12+hills+detail-lake);
            // A safe, flat spawn meadow with the cabin immediately ahead.
            if(x>=27 && x<=42 && z>=27 && z<=44) h=18;
            return Mathf.Clamp(h,5,29);
        }
        public void Initialize()
        {
            PrepareMaterials();Generate(); RebuildAll();
        }
        void PrepareMaterials()
        {
            var shader=Resources.Load<Shader>("BlockSurface");
            var lakeShader=Resources.Load<Shader>("LakeSurface");
            if(shader==null || lakeShader==null) throw new InvalidOperationException("Missing Blocklands shaders in Assets/Resources.");
            ground=new Material(shader);water=new Material(lakeShader);
        }
        public System.Collections.IEnumerator InitializeForLoading(System.Action<float,string> report)
        {
            PrepareMaterials();report(0,"正在生成山林、河流与矿层");yield return null;Generate();report(.1f,"地形数据已生成");yield return null;
            int done=0,total=(Width/ChunkSize)*(Depth/ChunkSize);
            for(int x=0;x<Width/ChunkSize;x++)for(int z=0;z<Depth/ChunkSize;z++){Rebuild(x,z);done++;report(.1f+.9f*done/total,"正在构筑方块地形  "+done+" / "+total);yield return null;}
            Physics.SyncTransforms();
        }
        public void Generate(int region=0)
        {
            Region=region;Array.Clear(cells,0,cells.Length);Edits.Clear();FixtureStates.Clear();
            GenerateConnectedUnderground();
            if(region!=0){GenerateRegion();return;}
            for(int x=0;x<Width;x++) for(int z=0;z<Depth;z++)
            {
                int h=GroundHeight(x,z);
                for(int y=0;y<=h;y++) Put(x,y,z,y==0?Block.Bedrock:y==h?(h<=WaterLevel+1?Block.Sand:Block.Grass):y<h-3?Block.Stone:Block.Dirt);
                for(int y=h+1;y<=WaterLevel;y++) Put(x,y,z,Block.Water);
            }
            var random=new System.Random(Seed);
            for(int i=0;i<90;i++)
            {
                int x=random.Next(3,61),z=random.Next(3,61),h=GroundHeight(x,z);
                if(h<=WaterLevel+1 || (x>=24&&x<=45&&z>=24&&z<=47))continue;
                for(int y=1;y<=5;y++)Put(x,h+y,z,Block.Wood);
                for(int dx=-2;dx<=2;dx++)for(int dz=-2;dz<=2;dz++)for(int dy=3;dy<=6;dy++)
                    if(Mathf.Abs(dx)+Mathf.Abs(dz)+(dy==6?1:0)<4 && Get(new Vector3Int(x+dx,h+dy,z+dz))==Block.Air)Put(x+dx,h+dy,z+dz,Block.Leaves);
            }
            BuildCabin();
        }
        void BuildCabin()
        {
            for(int x=33;x<=40;x++)for(int z=35;z<=42;z++)
            {
                Put(x,19,z,Block.Planks);
                for(int y=20;y<=24;y++)
                {
                    bool wall=x==33||x==40||z==35||z==42;
                    bool door=z==35&&(x==36||x==37)&&y<=22;
                    bool window=(x==33||x==40)&&z>=38&&z<=39&&y>=21&&y<=22;
                    Put(x,y,z,wall&&!door?(window?Block.Glass:Block.Planks):Block.Air);
                }
                Put(x,25,z,Block.Brick);
            }
            // A stepped gable, log posts and flower boxes distinguish the cabin from a box.
            for(int x=32;x<=41;x++)for(int z=34;z<=43;z++)
            {
                int rise=Mathf.Min(x-32,41-x);
                Put(x,25+rise/2,z,Block.Brick);
            }
            foreach(int x in new[]{33,40})foreach(int z in new[]{35,42})for(int y=20;y<=24;y++)Put(x,y,z,Block.Wood);
            for(int z=38;z<=39;z++){Put(32,20,z,Block.Wood);Put(32,21,z,Block.Leaves);Put(41,20,z,Block.Wood);Put(41,21,z,Block.Leaves);}
            for(int z=30;z<35;z++)for(int x=36;x<=37;x++)Put(x,19,z,Block.Planks);
            Put(31,19,31,Block.Workbench);
        }
        public bool Edit(Vector3Int p,Block b,bool rebuild=true)
        {
            if(!InBounds(p)||p.y==0||!(b==Block.Air||b==Block.Water||b==Block.Lava||Blocks.Buildable(b))||Get(p)==b)return false;
            FixtureStates.Remove(p);cells[p.x,p.y-MinY,p.z]=b;Edits[p]=b;
            if(rebuild)
            {
                Rebuild(p.x/ChunkSize,p.z/ChunkSize);
                if(p.x%ChunkSize==0)Rebuild(p.x/ChunkSize-1,p.z/ChunkSize);
                if(p.x%ChunkSize==ChunkSize-1)Rebuild(p.x/ChunkSize+1,p.z/ChunkSize);
                if(p.z%ChunkSize==0)Rebuild(p.x/ChunkSize,p.z/ChunkSize-1);
                if(p.z%ChunkSize==ChunkSize-1)Rebuild(p.x/ChunkSize,p.z/ChunkSize+1);
            }
            return true;
        }
        public bool OpenDepthHole(Vector3Int p,bool rebuild=true){if(!InBounds(p)||p.y!=0||Get(p)==Block.Air)return false;cells[p.x,-MinY,p.z]=Block.Air;Edits[p]=Block.Air;if(rebuild)RebuildAll();return true;}
        public void RebuildAll(){for(int x=0;x<Width/ChunkSize;x++)for(int z=0;z<Depth/ChunkSize;z++)Rebuild(x,z);Physics.SyncTransforms();}
        void Rebuild(int cx,int cz)
        {
            if(cx<0||cz<0||cx>=Width/ChunkSize||cz>=Depth/ChunkSize)return;
            var key=new Vector2Int(cx,cz);
            if(!chunks.TryGetValue(key,out Chunk chunk))
            {
                chunk=new Chunk{Root=new GameObject("Chunk "+cx+","+cz)};
                chunk.Root.transform.SetParent(transform,false);
                chunk.Root.AddComponent<MeshFilter>();chunk.Root.AddComponent<MeshRenderer>().sharedMaterial=ground;
                chunk.Root.AddComponent<MeshCollider>();
                var lake=new GameObject("Water");lake.transform.SetParent(chunk.Root.transform,false);
                lake.AddComponent<MeshFilter>();lake.AddComponent<MeshRenderer>().sharedMaterial=water;
                chunks.Add(key,chunk);
            }
            var solid=MakeMesh(cx,cz,false);var wet=MakeMesh(cx,cz,true);
            var collider=chunk.Root.GetComponent<MeshCollider>();collider.sharedMesh=null;
            chunk.Root.GetComponent<MeshFilter>().sharedMesh=solid;collider.sharedMesh=solid;
            chunk.Root.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh=wet;
            Dispose(chunk.Solid);Dispose(chunk.Water);chunk.Solid=solid;chunk.Water=wet;
            RebuildFixtures(chunk.Root.transform,cx,cz);
        }
        Mesh MakeMesh(int cx,int cz,bool isWater)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var normals=new List<Vector3>();var colors=new List<Color>();var uv=new List<Vector2>();
            for(int x=cx*ChunkSize;x<(cx+1)*ChunkSize;x++)for(int z=cz*ChunkSize;z<(cz+1)*ChunkSize;z++)for(int y=MinY;y<Height;y++)
            {
                var p=new Vector3Int(x,y,z);Block b=Get(p);
                if(b==Block.Air||(b==Block.Water)!=isWater)continue;
                if(Region==6&&MuseumRelic.IsAnchor(p))continue;
                if(Blocks.Fixture(b))continue;
                if(Blocks.Furniture(b)){FurnitureMesh(p,b,vertices,triangles,normals,colors,uv);continue;}
                for(int face=0;face<6;face++)
                {
                    Block neighbor=Get(p+Directions[face]);
                    // Thin bamboo cannot hide an entire neighbouring cube face.
                    bool occludes=Blocks.Solid(neighbor)&&!Blocks.Furniture(neighbor)&&!(Region==6&&MuseumRelic.IsAnchor(p+Directions[face]))&&(neighbor!=Block.Bamboo||(b==Block.Bamboo&&(face==2||face==3)));
                    if(occludes||(isWater&&neighbor==Block.Water))continue;
                    int start=vertices.Count;Color color=Blocks.ColorOf(b,face);color.a=(int)b/128f;
                    for(int v=0;v<4;v++){Vector3 corner=Corners[face,v];if(b==Block.Bamboo){corner.x=.5f+(corner.x-.5f)*.3f;corner.z=.5f+(corner.z-.5f)*.3f;}vertices.Add((Vector3)p+corner);normals.Add(Directions[face]);colors.Add(color);uv.Add(UV[v]);}
                    triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);triangles.Add(start);triangles.Add(start+2);triangles.Add(start+3);
                }
            }
            var mesh=new Mesh{name="Voxel chunk",indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
        }
        static void Dispose(UnityEngine.Object o){if(o==null)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
        void OnDestroy(){foreach(var c in chunks.Values){Dispose(c.Solid);Dispose(c.Water);}Dispose(ground);Dispose(water);}
    }
}
