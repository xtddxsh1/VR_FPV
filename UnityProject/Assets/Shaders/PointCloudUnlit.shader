Shader "Unlit/PointCloudUnlit"
{
    Properties { _PointSize ("Point Size (pixels)", Float) = 4.0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            ZWrite On ZTest LEqual Cull Off
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<uint4> _Points;
            float _PointSize;

            struct v2f { float4 pos:SV_POSITION; float4 col:COLOR0; float psize:PSIZE; };

            float3 unpackPos(uint4 v){ return float3(asfloat(v.x), asfloat(v.y), asfloat(v.z)); }
            float4 unpackColor(uint u){
                float r=((u>>0)&255)/255.0;
                float g=((u>>8)&255)/255.0;
                float b=((u>>16)&255)/255.0;
                float a=((u>>24)&255)/255.0;
                return float4(r,g,b,a);
            }

            v2f vert(uint id:SV_VertexID){
                v2f o;
                uint4 raw = _Points[id];
                float3 wp = unpackPos(raw);
                o.pos = UnityObjectToClipPos(float4(wp,1));
                o.col = unpackColor(raw.w);
                o.psize = _PointSize;
                return o;
            }

            fixed4 frag(v2f i):SV_Target { return i.col; }
            ENDCG
        }
    }
}