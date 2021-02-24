// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hsinpa/SnakeShaderEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        //X = Point to hide, Y = Point to appear; Ignore other two
        _Constraint("Constraint", Vector) = (0.0, 0.0, 0.0, 0.0)

        _Color("Color", Color) = (0, 0, 0, 1)

        _FresnelPower("Power", Range(0, 1)) = 0
        _FresnelColor("Fresnel Color", Color) = (0, 0, 0, 1)

		[Toggle(USE_DISPLAY_CONSTRAINT)] _UseDisplayContraint("Use Display Constraint", Float) = 0

    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma multi_compile_fwdbase
            #pragma vertex vert
            #pragma fragment frag

            #include "Lighting.cginc"
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal   : NORMAL;    // The vertex normal in model space.
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float lightStrength : TEXCOORD2;
                float R : TEXCOORD3;

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _UseDisplayContraint;
            uniform float4 _Constraint;

            uniform float _FresnelPower;
            uniform float4 _FresnelColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                float3 normWorld = normalize(mul(unity_ObjectToWorld, float4(v.normal,1))).xyz;

                float3 I = normalize(o.worldPos - _WorldSpaceCameraPos.xyz);
                o.R = mul(v.normal, I) + 1 * 0.5;

                half nl = max(0, dot(v.normal, _WorldSpaceLightPos0.xyz));

                o.lightStrength = nl;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                col = col * i.lightStrength * _Color + (i.R * _FresnelPower * _FresnelColor);

                if (_UseDisplayContraint == 1) {
                    if (i.worldPos.z <= _Constraint.y && i.worldPos.z >= _Constraint.x)
                        col.a = 1;
                    else
                        discard;                
                }

                return col;
            }
            ENDCG
        }
    }
}
