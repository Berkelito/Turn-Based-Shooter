Shader "Unlit/BlockShader"
{
    Properties
    {
        _Size("Size", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct Block
            {
                float3 StartPos;
                float4x4 Mat;
    
                int3 GridPos;

                float3 LowColor;
                float3 HighColor;
                float4 Col;

                float TimeOffset;
            };

            
            StructuredBuffer<Block> data;

            float _Size;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
            };

            v2f vert (appdata v, const uint instance_id: SV_InstanceID)
            {
                v2f o;

                v.vertex.xyz /= _Size;

                float4 pos = UnityObjectToClipPos(mul(data[instance_id].Mat, v.vertex));

                if (data[instance_id].Col.x == 0 && data[instance_id].Col.y == 0 && data[instance_id].Col.z == 0)
                {
                    pos.w = 0.0 / 0.0; //making it not render
                }
                o.vertex = pos;
                o.color = data[instance_id].Col;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
