using System;using System.Collections.Generic;using UnityEngine;
namespace Blocklands {
 [Serializable] public class FallenResident {public int id;public Vector3 position;}
 public partial class Sandbox {
  public readonly List<FallenResident> FallenResidents=new List<FallenResident>();
  public void RememberFallenResidents(){if(World.Region!=1||villagePeople==null)return;FallenResidents.Clear();foreach(var v in villagePeople.GetComponentsInChildren<Villager>())if(!v.IsFriend&&v.transform.position.y<0)FallenResidents.Add(new FallenResident{id=v.ResidentId,position=v.transform.position});}
  void RestoreFallenResidents(){if(World.Region!=1)return;foreach(var record in FallenResidents)foreach(var v in villagePeople.GetComponentsInChildren<Villager>())if(v.ResidentId==record.id)v.RestoreUnderground(record.position);}
 }
}
