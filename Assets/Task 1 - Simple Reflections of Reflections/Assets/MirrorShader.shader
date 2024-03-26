/* This shader projects the view of a reflection camera through a mirror
 * such that the texture is cropped to only what is visible through the mirror.
 *
 * Based of the code from:
 * https://wiki.unity3d.com/index.php/MirrorReflection4
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2021, University of Canterbury
 * Written by Adrian Clark
 */

Shader "Custom/MirrorShader"
{
    Properties
    {
        _MainTex ("", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
 
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct v2f
            {
                float4 refl : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            v2f vert(float4 pos : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos (pos);
                o.refl = ComputeScreenPos (o.pos);
                return o;
            }

            sampler2D _MainTex;
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 refl = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.refl));
                return refl;
            }
            ENDCG
        }
    }
}