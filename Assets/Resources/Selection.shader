Shader "Blocklands/Selection" {
    Properties { _Color("Color",Color)=(1,.91,.62,1) }
    SubShader { Tags { "Queue"="Transparent" } Pass {
        ZWrite Off
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        float4 _Color;
        struct v2f { float4 pos:SV_POSITION; };
        v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); return o; }
        fixed4 frag(v2f i):SV_Target { return _Color; }
        ENDCG
    }}
}
