using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public partial class Sandbox
{
    Transform glassShelterRoot;
    // The human selected this exact old ground house, not all player homes.
    public bool ConvertLegacyShelter(OwnedHome h)
    {
        if(saveBlocked||h==null||h.realm||h.region!=1||World.Region!=1||!h.legacy||h.x!=28||h.y!=14||h.z!=50||h.secretRoomVersion!=1||h.transparentHouseVersion!=0)return false;
        var changes=new List<Vector3Int>();var counts=new int[Blocks.Count];
        foreach(var e in World.PlanHouse(h.x,h.z))if(e.Key.y>=h.y+2&&(e.Value==Block.Planks||e.Value==Block.Wood||e.Value==Block.Brick||e.Value==Block.Glass)){
            bool shell=e.Key.x==h.x||e.Key.x==h.x+9||e.Key.z==h.z||e.Key.z==h.z+9||e.Key.y>=h.y+7;
            if(shell&&World.Get(e.Key)==e.Value){changes.Add(e.Key);counts[(int)e.Value]++;}
        }
        if(h.storedItems==null)h.storedItems=new int[Blocks.Count];for(int i=1;i<counts.Length;i++)if(counts[i]>9999-h.storedItems[i]){Notice="先腾出储物柜空间，才能回收旧木屋的墙与屋顶。";return false;}
        foreach(var p in changes)World.Edit(p,Block.Air,false);for(int i=1;i<counts.Length;i++)h.storedItems[i]+=counts[i];h.transparentHouseVersion=1;World.RebuildAll();return true;
    }
    public void BuildTransparentLegacyHomes()
    {
        if(glassShelterRoot!=null){glassShelterRoot.gameObject.SetActive(false);Destroy(glassShelterRoot.gameObject);}glassShelterRoot=null;
        if(World.Region!=1)return;RegisterLegacyVillageHome();
        foreach(var h in OwnedHomes){ConvertLegacyShelter(h);if(h.region!=1||h.transparentHouseVersion!=1)continue;
            if(glassShelterRoot==null){glassShelterRoot=new GameObject("旧木屋替换为透明玻璃房").transform;glassShelterRoot.SetParent(villagePeople.transform,false);}
            var root=new GameObject("透明地面房 · 原密室保留").transform;root.SetParent(glassShelterRoot,false);root.position=new Vector3(h.x,h.y,h.z);var art=root.gameObject.AddComponent<RoyalArt>();
            void Pane(string name,Vector3 p,Vector3 size){var t=art.Piece(name,p,size,new Color(.55f,.87f,.96f,.16f),PrimitiveType.Cube,true);var shader=Resources.Load<Shader>("GlassPane");var old=t.GetComponent<Renderer>().sharedMaterial;var m=new Material(shader);m.color=new Color(.55f,.87f,.96f,.16f);t.GetComponent<Renderer>().sharedMaterial=m;t.gameObject.AddComponent<GlassShelterMaterial>().Owned=m;}
            var frame=new Color(.81f,.87f,.89f);foreach(float x in new[]{.5f,9.5f}){Pane("可透视玻璃侧墙",new Vector3(x,4.5f,5),new Vector3(.09f,5,9.9f));foreach(float z in new[]{.5f,9.5f})art.Piece("细银色玻璃框",new Vector3(x,4.5f,z),new Vector3(.13f,5,.13f),frame);}
            Pane("可透视玻璃后墙",new Vector3(5,4.5f,9.5f),new Vector3(9,5,.09f));
            // Front retains the original door opening.
            foreach(float x in new[]{2f,8f})Pane("玻璃门旁墙",new Vector3(x,4.5f,.5f),new Vector3(3,5,.09f));
            Pane("门上玻璃",new Vector3(5,6,.5f),new Vector3(3,2,.09f));Pane("透明玻璃屋顶",new Vector3(5,7.06f,5),new Vector3(9.9f,.09f,9.9f));
            foreach(float z in new[]{.5f,9.5f})art.Piece("屋顶细框",new Vector3(5,7.1f,z),new Vector3(10,.12f,.12f),frame);
            PlaceSign(root,"透明小屋 · 九层密室、保险箱和监控都保留",new Vector3(5,7.6f,.4f));
        }Physics.SyncTransforms();
    }
}
public class GlassShelterMaterial:MonoBehaviour {public Material Owned;void OnDestroy(){if(Owned!=null)Destroy(Owned);}}
}
