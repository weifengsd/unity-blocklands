Shader "Blocklands/BlockSurface"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        struct Input { float2 uv_MainTex; float4 tint; };
        void vert(inout appdata_full v, out Input o) { UNITY_INITIALIZE_OUTPUT(Input,o); o.tint=v.color; }
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float2 cell=floor(IN.uv_MainTex*16);
            float noise=frac(sin(dot(cell,float2(12.9898,78.233)))*43758.5453);
            float2 edge=min(IN.uv_MainTex,1-IN.uv_MainTex);
            float seam=lerp(.87,1,step(.008,min(edge.x,edge.y)));
            float3 tint=IN.tint.rgb;
            float kind=round(IN.tint.a*128);
            if(kind==77){float crystal=step(.73,noise)*step(.14,frac(IN.uv_MainTex.x*5+IN.uv_MainTex.y*3));tint=lerp(float3(.44,.48,.53)*(.8+.25*noise),float3(.18,.72,.98),crystal);o.Emission=float3(.06,.4,.68)*crystal*.55;}
            if(kind==79){tint=lerp(float3(.18,.12,.2),float3(.56,.44,.65),step(.8,noise));o.Emission=tint*.1;}
            if(kind==68){tint*=.9+noise*.18;o.Emission=tint*.13;o.Smoothness=.8;}
            if(kind==1 && IN.tint.g<.6 && IN.uv_MainTex.y>.82+noise*.05) tint=float3(.38,.66,.27);
            if(kind==4) seam*=lerp(.78,1,step(.12,frac(IN.uv_MainTex.x*5+noise*.15)));
            if(kind==7) seam*=lerp(.68,1,step(.05,frac(IN.uv_MainTex.y*4)));
            if(kind==8) {
                float row=floor(IN.uv_MainTex.y*4);
                float mortar=min(frac(IN.uv_MainTex.y*4),frac(IN.uv_MainTex.x*2+row*.5));
                tint=lerp(float3(.55,.51,.44),tint,step(.06,mortar));
            }
            if(kind==12) {
                float2 grid=frac(IN.uv_MainTex*3);
                float gridLine=step(.075,min(grid.x,grid.y));
                tint=lerp(float3(.2,.12,.07),float3(.68,.46,.26),gridLine);
            }
            if(kind==16) tint=lerp(tint,float3(.88,.66,.43),step(.76,noise));
            if(kind==23||kind==27||kind==28||kind==29||kind==31){float2 tile=frac(IN.uv_MainTex*2);seam*=lerp(.6,1,step(.04,min(tile.x,tile.y)));}
            if(kind==30){float2 book=frac(IN.uv_MainTex*float2(7,3));tint=lerp(float3(.24,.13,.08),lerp(float3(.26,.5,.62),float3(.7,.32,.2),frac(floor(IN.uv_MainTex.x*7)*.37)),step(.13,book.x)*step(.15,book.y));}
            if(kind==25||kind==26) o.Emission=tint*.55;
            if(kind==37)o.Emission=tint*.35;
            if(kind==45){float flow=.5+.5*sin(IN.uv_MainTex.x*8+sin(IN.uv_MainTex.y*6)+_Time.y*.6);tint=lerp(float3(.63,.12,.015),float3(.92,.37,.04),flow*.7+noise*.08);o.Emission=tint*.45;}
            if(kind==36)tint*=lerp(.65,1.3,step(.8,noise));
            if(kind==76){float soft=.92+.08*noise;tint=lerp(float3(.82,.9,.98),float3(.99,.995,1),soft);seam=1;o.Smoothness=.12;}
            if(kind==38||kind==43||kind==44)seam*=lerp(.7,1,step(.12,frac(IN.uv_MainTex.y*(kind==38?5:3))));
            #if defined(UNITY_COLORSPACE_GAMMA)
            o.Albedo=tint*(.95+noise*.1)*seam;
            #else
            o.Albedo=GammaToLinearSpace(tint)*(.95+noise*.1)*seam;
            #endif
            o.Metallic=0;
            o.Smoothness=.08;
            if(kind==68){o.Albedo=IN.tint.rgb*(.94+.06*step(IN.uv_MainTex.x,IN.uv_MainTex.y));o.Emission=IN.tint.rgb*.08;o.Smoothness=.75;o.Metallic=.12;}
            o.Alpha=1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
