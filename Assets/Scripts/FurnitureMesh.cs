using System.Collections.Generic;
using UnityEngine;
namespace Blocklands
{
    public partial class VoxelWorld
    {
        void FurnitureMesh(Vector3Int p,Block b,List<Vector3> vertices,List<int> triangles,List<Vector3> normals,List<Color> colors,List<Vector2> uv)
        {
            void Box(Vector3 min,Vector3 size,Color color){color.a=(int)b/128f;for(int face=0;face<6;face++){int start=vertices.Count;for(int v=0;v<4;v++){vertices.Add((Vector3)p+min+Vector3.Scale(Corners[face,v],size));normals.Add(Directions[face]);colors.Add(color);uv.Add(UV[v]);}triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}}
            Color wood=new Color(.48f,.29f,.14f),c=Blocks.ColorOf(b);
            if(b==Block.RedCarpet){Box(new Vector3(.02f,.01f,.02f),new Vector3(.96f,.04f,.96f),c);return;}
            if(b==Block.Gauze){Box(new Vector3(.05f,.02f,.05f),new Vector3(.9f,.8f,.9f),c);for(int i=0;i<5;i++)Box(new Vector3(.04f,.1f+i*.13f,.03f),new Vector3(.92f,.03f,.94f),c*.85f);return;}
            float leg=b==Block.Bed?.28f:b==Block.Sofa?.25f:b==Block.Chair?.42f:b==Block.Cabinet?.1f:.6f;foreach(float x in new[]{.08f,.82f})foreach(float z in new[]{.08f,.82f})Box(new Vector3(x,0,z),new Vector3(.1f,leg,.1f),wood);
            if(b==Block.Table)Box(new Vector3(.03f,.6f,.03f),new Vector3(.94f,.12f,.94f),c);
            if(b==Block.Chair){Box(new Vector3(.1f,.42f,.1f),new Vector3(.8f,.13f,.8f),c);Box(new Vector3(.1f,.55f,.78f),new Vector3(.8f,.45f,.12f),wood);}
            if(b==Block.Bed){Box(new Vector3(.05f,.28f,.03f),new Vector3(.9f,.25f,.94f),c);Box(new Vector3(.12f,.53f,.65f),new Vector3(.76f,.12f,.23f),new Color(.93f,.88f,.75f));}
            if(b==Block.Sofa){Box(new Vector3(.03f,.25f,.05f),new Vector3(.94f,.3f,.85f),c);Box(new Vector3(.03f,.5f,.76f),new Vector3(.94f,.4f,.15f),c*.8f);foreach(float x in new[]{.03f,.84f})Box(new Vector3(x,.45f,.1f),new Vector3(.13f,.3f,.8f),c);}
            if(b==Block.Cabinet){Box(new Vector3(.06f,.1f,.08f),new Vector3(.88f,.85f,.82f),c);foreach(float x in new[]{.13f,.52f})Box(new Vector3(x,.22f,.03f),new Vector3(.34f,.6f,.06f),wood);foreach(float x in new[]{.41f,.55f})Box(new Vector3(x,.5f,.0f),new Vector3(.04f,.1f,.07f),new Color(.9f,.7f,.3f));}
        }
    }
}
