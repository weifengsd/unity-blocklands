using System.Collections.Generic;using UnityEngine;
namespace Blocklands {public static class ConnectedSaveUpgrade {
 public static bool Apply(SavedWorld s){if(s.continuousVersion>=1)return false;s.continuousVersion=1;bool legacy=s.region==5||s.region>=7&&s.region<=10;int previous=s.region;if(s.regions==null)s.regions=new List<SavedRegion>();
  List<SavedBlock> root=s.region==0?s.edits:null;SavedRegion rootRecord=null;foreach(var r in s.regions)if(r.id==0){root=r.edits;rootRecord=r;break;}if(root==null)root=new List<SavedBlock>();
  if(legacy){s.regions.Add(new SavedRegion{id=s.region,edits=s.edits});if(rootRecord!=null)s.regions.Remove(rootRecord);s.region=0;s.edits=root;s.position=new Vector3(31.5f,previous==5?-40:previous==7?-71:previous==8?-102:previous==9?-135.3f:-174,30.5f);s.flying=previous==9;}
  var taken=new HashSet<Vector3Int>();foreach(var b in root)taken.Add(new Vector3Int(b.x,b.y,b.z));foreach(var r in s.regions){int offset=r.id==5?-60:r.id==7?-86:r.id==8?-120:r.id==9?-165:r.id==10?-189:0;if(offset==0)continue;foreach(var b in r.edits){var p=new Vector3Int(b.x,b.y+offset,b.z);if(p.y<0&&VoxelWorld.InBounds(p)&&taken.Add(p))root.Add(new SavedBlock{x=p.x,y=p.y,z=p.z,id=b.id});}}
  if(!legacy&&s.region!=0&&rootRecord==null&&root.Count>0)s.regions.Add(new SavedRegion{id=0,edits=root});return legacy&&previous==9;
 }
}}
