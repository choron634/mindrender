// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MiT/Alpha Blend"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass
        {
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off Lighting Off ZWrite Off
//            Blend One Zero

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed4 _Color;

            struct appdata
            {
                half4 vertex : POSITION;
                half2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD;
            };

            v2f vert( appdata v )
            {
                v2f o;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.uv = v.texcoord;
                return o;
            }

            half4 frag( v2f i ) : COLOR
            {
                fixed4 tex = tex2D( _MainTex, i.uv ) * _Color;
                return tex;
            }

            ENDCG
        }
    }

    Fallback "Diffuse"
}