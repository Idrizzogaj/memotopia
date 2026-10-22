Shader "UI/MemotopiaTheme" {
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white" {} _ThemeColor("Theme",Color)=(.48,.12,1,1) }
 SubShader {
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   sampler2D _MainTex;fixed4 _ThemeColor;
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv);float chroma=max(c.r,max(c.g,c.b))-min(c.r,min(c.g,c.b));float light=dot(c.rgb,float3(.2126,.7152,.0722))/.76;c.rgb=lerp(c.rgb,saturate(_ThemeColor.rgb*light),step(.08,chroma));return c*i.color;}
  ENDCG }
 }
}
