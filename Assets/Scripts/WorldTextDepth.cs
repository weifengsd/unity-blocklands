using UnityEngine;
namespace Blocklands {
    [RequireComponent(typeof(TextMesh))] public class WorldTextDepth:MonoBehaviour {
        TextMesh label;Material material;
        void OnEnable(){label=GetComponent<TextMesh>();var shader=Resources.Load<Shader>("WorldText");if(shader==null||label.font==null)return;material=new Material(shader);material.mainTexture=label.font.material.mainTexture;GetComponent<Renderer>().sharedMaterial=material;Font.textureRebuilt+=Rebuild;}
        void Rebuild(Font font){if(label!=null&&label.font==font&&material!=null)material.mainTexture=font.material.mainTexture;}
        void OnDisable(){Font.textureRebuilt-=Rebuild;if(material!=null)Destroy(material);material=null;}
    }
}
