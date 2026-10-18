Shader "Blocklands/TsunamiWater"
{
 Properties {_Color("Water",Color)=(0.1,0.55,0.7,0.58)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} LOD 150 Cull Off ZWrite Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade
 #pragma target 3.0
 fixed4 _Color;struct Input {float3 worldPos;};
 void surf(Input i,inout SurfaceOutputStandard o){float a=sin(i.worldPos.x*.22+_Time.y*1.6);float b=cos(i.worldPos.z*.28-_Time.y*2.1);float ripple=a*b;float foam=smoothstep(.73,.98,ripple);o.Albedo=lerp(_Color.rgb,float3(.78,.95,1),foam*.65);o.Emission=_Color.rgb*.15+foam*.1;o.Normal=normalize(float3(a*.12,b*.09,1));o.Smoothness=.82;o.Metallic=.15;o.Alpha=_Color.a+foam*.12;}
 ENDCG
 }
 FallBack "Transparent/Diffuse"
}
