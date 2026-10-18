Shader "Blocklands/ActorSurface"
{
    Properties { _Color("Color",Color)=(1,1,1,1) }
    SubShader {
        Tags {"RenderType"="Opaque"}
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float2 uv_MainTex; };
        void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=_Color.rgb;o.Emission=_Color.rgb*.08;o.Smoothness=.27;o.Metallic=.08;o.Alpha=1;}
        ENDCG
    }
    FallBack "Diffuse"
}
