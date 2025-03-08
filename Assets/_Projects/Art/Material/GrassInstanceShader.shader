Shader "Custom/GrassInstanceShader"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Cull Off       // Ensure both sides render
            ZWrite Off     // Disable depth writing for transparency
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<float4> _PositionBuffer; // Buffer holding grass positions

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Get relative grass position from buffer
                float3 localPos = _PositionBuffer[v.instanceID].xyz;

                // Add back the terrain world position
                float3 worldPos = localPos;

                // Get proper billboarding vectors
                float3 camRight = normalize(float3(UNITY_MATRIX_V[0].x, 0, UNITY_MATRIX_V[0].z));
                float3 camUp = float3(0, 1, 0);
    
                // Correct offset for billboarding
                float3 offset = v.vertex.x * camRight + v.vertex.y * camUp;
                worldPos += offset;

                // Convert to clip space
                o.vertex = UnityWorldToClipPos(float4(worldPos, 1.0));
                o.uv = v.uv;
                return o;
            }

            sampler2D _BaseMap;
            float4 _Color;

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_BaseMap, i.uv) * _Color;

                // Ensure transparency works properly
                clip(col.a - 0.1); // Adjust threshold to prevent white quads

                return col;
            }

            ENDHLSL
        }
    }
}