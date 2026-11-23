Shader "UI/SagaMapProgress" {
 Properties {
  [PerRendererData] _MainTex("Sprite",2D)="white" {}
  _StencilComp("Stencil Comparison",Float)=8
  _Stencil("Stencil ID",Float)=0
  _StencilOp("Stencil Operation",Float)=0
  _StencilWriteMask("Stencil Write Mask",Float)=255
  _StencilReadMask("Stencil Read Mask",Float)=255
  _ColorMask("Color Mask",Float)=15
 }
 SubShader {
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
  Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 local:TEXCOORD1;};
   sampler2D _MainTex;float4 _ClipRect;float4 _Cells[20];float4 _Charge;
   v2f vert(appdata v){v2f o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {
    float2 chargeDelta=(i.uv-_Charge.xy)*float2(1,1.5);
    float influence=saturate(1-length(chargeDelta)/.13)*_Charge.z;
    float2 jitter=float2(sin(_Charge.w*27),cos(_Charge.w*31))*.0012*influence;
    fixed4 c=tex2D(_MainTex,i.uv+jitter)*i.color;
    // Composite the unchanged approved white-backed artwork over the UI backdrop.
    c.a*=1-smoothstep(.94,.995,min(c.r,min(c.g,c.b)));
    float nearest=100;float unlocked=0;
    [unroll] for(int n=0;n<20;n++){
     float2 delta=(i.uv-_Cells[n].xy)*float2(1,1.5);float distance=dot(delta,delta);
     if(distance<nearest){nearest=distance;unlocked=_Cells[n].z;}
    }
    float grey=dot(c.rgb,float3(.2126,.7152,.0722));
    c.rgb=lerp(lerp(float3(grey,grey,grey),float3(1,1,1),.22),c.rgb,unlocked);
    #ifdef UNITY_UI_CLIP_RECT
     c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
    #endif
    return c;
   }
  ENDCG }
 }
}
