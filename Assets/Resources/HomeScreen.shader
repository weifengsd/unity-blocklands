Shader "Blocklands/HomeScreen" {
 Properties { _MainTex("Screen",2D)="black"{} }
 SubShader { Tags {"RenderType"="Opaque"} Pass { Lighting Off Cull Off SetTexture [_MainTex] { combine texture } } }
}
