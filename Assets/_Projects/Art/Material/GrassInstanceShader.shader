Shader "Custom/OptimizedGrassInstanceShader"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _WindSpeed ("Wind Speed", Range(0, 10)) = 1
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.1
        _LODDistance1 ("LOD Distance 1", Float) = 50
        _LODDistance2 ("LOD Distance 2", Float) = 1000
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            StructuredBuffer<float4> _PositionBuffer;

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
                float distanceFade : TEXCOORD1;
            };

            sampler2D _BaseMap;
            float4 _Color;
            float _WindSpeed;
            float _WindStrength;
            float _LODDistance1;
            float _LODDistance2;

            v2f vert(appdata v)
            {
                v2f o;

                // Get instance position
                float3 worldPos = _PositionBuffer[v.instanceID].xyz;
                
                // Calculate distance to camera
                float distanceToCamera = length(worldPos - _WorldSpaceCameraPos);
                
                // LOD System
                float3 camRight, camUp;
                float vertexHeight = v.vertex.y;
                
                // LOD0 - Close distance: Full billboard with animation
                if (distanceToCamera < _LODDistance1) {
                    // Wind animation
                    float wind = sin(_Time.y * _WindSpeed + worldPos.x * 0.1) * _WindStrength;
                    
                    // Apply wind to top vertices only (based on Y position)
                    worldPos.x += wind * saturate(v.vertex.y);
                    
                    // Full billboarding vectors
                    camRight = normalize(float3(UNITY_MATRIX_V[0].x, 0, UNITY_MATRIX_V[0].z));
                    camUp = float3(0, 1, 0);
                }
                // LOD1 - Medium distance: Simplified animation
                else if (distanceToCamera < _LODDistance2) {
                    // Simplified billboarding
                    camRight = normalize(float3(UNITY_MATRIX_V[0].x, 0, UNITY_MATRIX_V[0].z));
                    camUp = float3(0, 1, 0);
                    
                    // Reduce vertex height slightly
                    vertexHeight *= 0.9;
                }
                // LOD2 - Far distance: Static billboarding, reduced height
                else {
                    // Even more simplified
                    camRight = normalize(float3(UNITY_MATRIX_V[0].x, 0, UNITY_MATRIX_V[0].z));
                    camUp = float3(0, 1, 0);
                    
                    // Reduce vertex height significantly
                    vertexHeight *= 0.7;
                }
                
                // Apply billboard offset with LOD adjustments
                float3 offset = v.vertex.x * camRight + vertexHeight * camUp;
                worldPos += offset;
                
                // Calculate fade based on distance (for smoother LOD transitions)
                o.distanceFade = 1.0 - saturate((distanceToCamera - _LODDistance2) / 10.0);

                // Convert to clip space
                o.vertex = UnityWorldToClipPos(float4(worldPos, 1.0));
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_BaseMap, i.uv) * _Color;
                
                // Apply distance fade for smooth LOD transition
                col.a *= i.distanceFade;
                
                // Discard pixels below alpha threshold
                clip(col.a - 0.1);
                
                return col;
            }
            ENDHLSL
        }
    }
}