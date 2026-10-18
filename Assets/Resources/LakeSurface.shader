Shader "Blocklands/LakeSurface"
{
    Properties { _Color("Water", Color)=(.15,.57,.71,.7) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float3 worldPos; float3 viewDir; };
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            float wave=sin(IN.worldPos.x*2+_Time.y)*cos(IN.worldPos.z*1.8+_Time.y*.7);
            float fresnel=pow(1-saturate(dot(normalize(IN.viewDir),float3(0,0,1))),3);
            o.Albedo=lerp(_Color.rgb,float3(.65,.82,.9),fresnel*.5)+wave*.025;
            o.Smoothness=.85; o.Metallic=.15; o.Alpha=_Color.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
