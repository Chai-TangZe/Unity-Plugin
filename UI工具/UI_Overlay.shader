// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:True,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:33445,y:33058,varname:node_9361,prsc:2|custl-816-OUT,clip-2961-A;n:type:ShaderForge.SFN_Color,id:3206,x:32517,y:33030,ptovrint:False,ptlb:shaningCollor,ptin:_shaningCollor,varname:node_3206,prsc:2,glob:False,taghide:False,taghdr:True,tagprd:False,tagnsco:False,tagnrm:False,c1:0.4044118,c2:0,c3:0,c4:1;n:type:ShaderForge.SFN_Time,id:7126,x:32091,y:33108,varname:node_7126,prsc:2;n:type:ShaderForge.SFN_Sin,id:8662,x:32454,y:33206,varname:node_8662,prsc:2|IN-5074-OUT;n:type:ShaderForge.SFN_Multiply,id:2534,x:32756,y:33131,varname:node_2534,prsc:2|A-3206-RGB,B-8108-OUT;n:type:ShaderForge.SFN_Tex2d,id:2961,x:32988,y:33353,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_2961,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Add,id:816,x:33175,y:33189,varname:node_816,prsc:2|A-2961-RGB,B-2534-OUT;n:type:ShaderForge.SFN_Slider,id:7117,x:32095,y:33430,ptovrint:False,ptlb:shaingSpeed,ptin:_shaingSpeed,varname:node_7117,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:7.264957,max:10;n:type:ShaderForge.SFN_Multiply,id:5074,x:32315,y:33184,varname:node_5074,prsc:2|A-7126-T,B-7117-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:8108,x:32604,y:33306,varname:node_8108,prsc:2,min:0,max:1|IN-8662-OUT;proporder:3206-2961-7117;pass:END;sub:END;*/

Shader "UI/UI_Overlay"
{
	Properties
	{
		[PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}

		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}
	
	SubShader
	{
		LOD 100

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
				};
	
				struct v2f
				{
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					fixed4 color : COLOR;
				};
	
				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Color;
				fixed4 _TextureSampleAdd;
				
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.color = v.color * _Color;
#ifdef UNITY_HALF_TEXEL_OFFSET
					o.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif

					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
					 clip (col.a - 0.01);
					return col;
				}
			ENDCG
		}
	}
}
