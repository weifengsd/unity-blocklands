using System.Collections.Generic;
using UnityEngine;

namespace Blocklands
{
    // Code-native model: curved tiled neck, ivory horns and bat wings inspired by our icon.
    public class DragonSculpt:MonoBehaviour
    {
        readonly List<Material> materials=new List<Material>();readonly List<Mesh> meshes=new List<Mesh>();
        Material purple,edge,belly,horn,glow;public Transform LeftWing,RightWing;
        public void Build()=>Build(new Color(.3f,.055f,.48f),new Color(.49f,.12f,.67f),new Color(.17f,.045f,.25f),new Color(.93f,.72f,.47f),new Color(1,.16f,1));
        public void Build(Color bodyColor,Color edgeColor,Color bellyColor,Color hornColor,Color glowColor)
        {
            purple=Mat(bodyColor);edge=Mat(edgeColor);belly=Mat(bellyColor);horn=Mat(hornColor);glow=Mat(glowColor,true);
            Cube(transform,new Vector3(0,.65f,-.5f),new Vector3(2.5f,1.65f,3.5f),purple);
            for(int i=0;i<9;i++){
                float t=i/8f;Vector3 p=new Vector3(0,1+t*3,1+t*2.1f-.45f*t*t);
                float s=Mathf.Lerp(1.7f,1.05f,t);Cube(transform,p,new Vector3(s,.6f,s*.82f),i%2==0?purple:edge).localRotation=Quaternion.Euler(-20+t*20,0,0);
                Cube(transform,p+new Vector3(0,0,s*.39f),new Vector3(s*.68f,.3f,.12f),belly);
                foreach(int side in new[]{-1,1})Cube(transform,p+new Vector3(side*s*.5f,.08f,0),new Vector3(.13f,.38f,.42f),edge);
            }
            Cube(transform,new Vector3(0,4.25f,2.85f),new Vector3(1.85f,1.25f,1.6f),purple);
            Cube(transform,new Vector3(0,3.95f,3.8f),new Vector3(1.15f,.65f,1.3f),edge);
            Cube(transform,new Vector3(0,3.61f,3.95f),new Vector3(1.1f,.14f,1.05f),belly);
            foreach(int side in new[]{-1,1}){
                Cube(transform,new Vector3(side*.72f,4.25f,3.65f),new Vector3(.42f,.28f,.12f),glow);
                Cube(transform,new Vector3(side*.75f,4.52f,3.55f),new Vector3(.6f,.22f,.48f),edge);
                Cube(transform,new Vector3(side*.35f,4.07f,4.46f),new Vector3(.17f,.1f,.055f),belly);
                for(int i=0;i<5;i++)Cube(transform,new Vector3(side*(.65f+i*.095f),4.8f+i*.29f,2.6f-i*.2f),new Vector3(.4f-i*.055f,.4f,.4f-i*.055f),horn);
                for(int leg=0;leg<2;leg++){
                    Vector3 p=new Vector3(side*1.2f,-.4f,-1.5f+leg*2.4f);Cube(transform,p,new Vector3(.55f,1.1f,.7f),purple);
                    for(int c=0;c<3;c++)Cube(transform,p+new Vector3((c-1)*.2f,-.6f,.28f),new Vector3(.15f,.25f,.65f),horn);
                }
            }
            for(int i=0;i<17;i++){
                float t=i/16f;Vector3 p=new Vector3(Mathf.Sin(t*2.5f)*2,.6f+t*t*2,-2-t*6);float s=1.25f*(1-t)+.18f;
                Cube(transform,p,new Vector3(s,s,.65f),i%2==0?purple:edge).localRotation=Quaternion.Euler(-t*25,Mathf.Cos(t*2.5f)*-25,0);
                Cube(transform,p+Vector3.up*s*.6f,new Vector3(s*.3f,.35f,.25f),horn);
            }
            LeftWing=Wing(-1);RightWing=Wing(1);
        }
        Material Mat(Color c,bool unlit=false){var m=new Material(Resources.Load<Shader>(unlit?"Selection":"ActorSurface"));m.color=c;materials.Add(m);return m;}
        Transform Cube(Transform parent,Vector3 p,Vector3 s,Material m){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;Destroy(g.GetComponent<Collider>());return g.transform;}
        Transform Wing(int sign)
        {
            var root=new GameObject("Sculpted bat wing").transform;root.SetParent(transform,false);root.localPosition=new Vector3(sign*1.1f,1.35f,-.2f);
            Vector3[] points={Vector3.zero,new Vector3(sign*3,3,0),new Vector3(sign*7,2,-1),new Vector3(sign*5,.6f,-2.7f),new Vector3(sign*5.2f,.3f,-4.4f),new Vector3(sign*3.1f,-.1f,-3.5f),new Vector3(sign*1.9f,-.2f,-5),new Vector3(0,0,-2)};
            var mesh=new Mesh();var verts=new List<Vector3>(points);verts.AddRange(points);mesh.vertices=verts.ToArray();var tris=new List<int>();int npoints=points.Length;for(int i=1;i<npoints-1;i++){tris.AddRange(new[]{0,i,i+1,npoints,npoints+i+1,npoints+i});}mesh.triangles=tris.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var membrane=new GameObject("Purple wing membrane");membrane.transform.SetParent(root,false);membrane.AddComponent<MeshFilter>().sharedMesh=mesh;membrane.AddComponent<MeshRenderer>().sharedMaterial=purple;
            foreach(int end in new[]{1,2,4,6,7}){Vector3 v=points[end];int n=Mathf.CeilToInt(v.magnitude/.45f);for(int j=0;j<n;j++)Cube(root,Vector3.Lerp(Vector3.zero,v,j/(float)n),Vector3.one*.29f,edge);}
            return root;
        }
        void OnDestroy(){foreach(var m in materials)Destroy(m);foreach(var m in meshes)Destroy(m);}
    }
}
