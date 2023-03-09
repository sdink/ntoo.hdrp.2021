Shader "Kinect/ThreeDMovieShader"
{
    Properties
	{
		_ColorTex("_ColorTex", 2D) = "white" {}
	}

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		ZWrite On
        Cull Off    
       
        Pass
        {
            CGPROGRAM

			#pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

			float2 _TexRes;
			float _MinDepth;
			float _MaxDepth;
			float _DepthScale;

#ifdef SHADER_API_D3D11
			StructuredBuffer<uint> _DepthMap;
#endif
			sampler2D _ColorTex;
			float4 _ColorTex_ST;

            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 vertexPos : TEXCOORD1;
            };

         
			float getDepthAt(float2 uv)
			{
				uint dx = (uint)(uv.x * _TexRes.x);
				uint dy = (uint)(uv.y * _TexRes.y);
				uint di = (dx + dy * _TexRes.x);

				uint depth2 = 0;
#ifdef SHADER_API_D3D11
				depth2 = _DepthMap[di >> 1];
#endif
				uint depth = di & 1 != 0 ? depth2 >> 16 : depth2 & 0xffff;
				float fDepth = (float)depth * 0.001;

				return (fDepth > _MinDepth && fDepth < _MaxDepth ? fDepth : _MaxDepth) *_DepthScale;
			}

			v2f vert (appdata_base v)
            {
                v2f o;
				o.uv = TRANSFORM_TEX(v.texcoord, _ColorTex);  // v.uv;

				float depth = getDepthAt(o.uv.xy);
				v.vertex = float4(v.vertex.xy, depth, 1);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertexPos = v.vertex;
             
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col;
				col = tex2D(_ColorTex, i.uv);
				//float d = (i.vertexPos.z - _MinDepth) / (_MaxDepth - _MinDepth);
				//col = float4(0, 0, d, 1);

                return col;
            }

            ENDCG
        }
    }
}
