using UnityEngine;
namespace Blocklands {public class CrystalVeinDetail:MonoBehaviour {public Sandbox Game;public Vector3Int Cell;void Update(){if(Game==null||Game.World.Region!=2||Game.World.Get(Cell)!=Blocks.CrystalOre)Destroy(gameObject);}}}
