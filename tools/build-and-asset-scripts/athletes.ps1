Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @"
using System; using System.Collections.Generic; using System.Drawing; using System.Drawing.Drawing2D; using System.Drawing.Imaging; using System.IO;
public static class Athletes {
  static Color Black=Color.FromArgb(20,20,20), Purple=Color.FromArgb(123,31,255), Gold=Color.FromArgb(253,191,80), Cream=Color.FromArgb(255,246,222);
  const float O=14f;
  public static Bitmap Base(string[] files){
    var list=new List<byte[]>(); int W=0,H=0;
    foreach(var f in files){ using(var b=new Bitmap(f)){ if(W==0){W=b.Width;H=b.Height;} if(b.Width!=W||b.Height!=H) continue;
      using(var c=new Bitmap(W,H,PixelFormat.Format32bppArgb)){ using(var g=Graphics.FromImage(c)) g.DrawImage(b,0,0,W,H);
        var d=c.LockBits(new Rectangle(0,0,W,H),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb); var arr=new byte[d.Stride*H]; System.Runtime.InteropServices.Marshal.Copy(d.Scan0,arr,0,arr.Length); c.UnlockBits(d); list.Add(arr);} } }
    var res=new Bitmap(W,H,PixelFormat.Format32bppArgb); var rd=res.LockBits(new Rectangle(0,0,W,H),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
    var outArr=new byte[rd.Stride*H]; int n=list.Count; var tmp=new byte[n];
    for(int p=0;p<outArr.Length;p+=4){ int best=0,bc=-1; for(int k=0;k<n;k++){ var a=list[k]; int al=a[p+3]; int c=Math.Min(a[p],Math.Min(a[p+1],a[p+2])); int comp=(c*al+255*(255-al))/255; if(comp>bc){bc=comp;best=k;} } var q=list[best]; outArr[p]=q[p];outArr[p+1]=q[p+1];outArr[p+2]=q[p+2];outArr[p+3]=q[p+3]; }
    System.Runtime.InteropServices.Marshal.Copy(outArr,0,rd.Scan0,outArr.Length); res.UnlockBits(rd); return res;
  }
  static Pen P(Color c,float w){var p=new Pen(c,w);p.StartCap=p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;return p;}
  static void Limb(Graphics g,Color fill,float w,params float[] pts){
    var pp=new PointF[pts.Length/2]; for(int i=0;i<pp.Length;i++)pp[i]=new PointF(pts[2*i],pts[2*i+1]);
    using(var a=P(Black,w+2*O)) g.DrawLines(a,pp); using(var b=P(fill,w)) g.DrawLines(b,pp);
  }
  static void Circle(Graphics g,Color fill,float x,float y,float r){
    using(var br=new SolidBrush(Black)) g.FillEllipse(br,x-r-O,y-r-O,2*(r+O),2*(r+O)); using(var b2=new SolidBrush(fill)) g.FillEllipse(b2,x-r,y-r,2*r,2*r);
  }
  static void Poly(Graphics g,Color fill,params float[] pts){
    var pp=new PointF[pts.Length/2]; for(int i=0;i<pp.Length;i++)pp[i]=new PointF(pts[2*i],pts[2*i+1]);
    using(var b=new SolidBrush(fill)) g.FillPolygon(b,pp); using(var p=P(Black,O)) g.DrawPolygon(p,pp);
  }
  static void Runner(Graphics g,float ox,float oy,float s){
    Func<float,float> X=v=>ox+v*s; Func<float,float> Y=v=>oy+v*s;
    // slim, long-limbed runner: thin limbs, small head, gold shoes, hands and headband
    Limb(g,Purple,40*s,X(-5),Y(-125),X(-100),Y(-45),X(-170),Y(-125));
    Limb(g,Purple,50*s,X(-10),Y(100),X(-115),Y(190),X(-245),Y(165));
    Limb(g,Purple,72*s,X(22),Y(-150),X(-10),Y(50));
    Limb(g,Gold,78*s,X(-10),Y(50),X(-8),Y(98));
    Limb(g,Purple,50*s,X(0),Y(100),X(110),Y(190),X(92),Y(340));
    Circle(g,Gold,X(-255),Y(165),30*s); Circle(g,Gold,X(92),Y(352),30*s);
    Limb(g,Purple,40*s,X(38),Y(-125),X(140),Y(-75),X(205),Y(-175));
    Circle(g,Gold,X(-172),Y(-128),22*s); Circle(g,Gold,X(207),Y(-178),22*s);
    Circle(g,Gold,X(58),Y(-232),50*s);
    using(var eb=new SolidBrush(Black)){ g.FillEllipse(eb,X(72),Y(-244),13*s,17*s); g.FillEllipse(eb,X(46),Y(-244),13*s,17*s);} 
    using(var m=P(Black,8*s)) g.DrawArc(m,X(44),Y(-232),38*s,26*s,20,140);
    Limb(g,Purple,14*s,X(18),Y(-270),X(98),Y(-276));
  }
  static void Bolt(Graphics g,float x,float y,float sc){
    Poly(g,Gold,x+40*sc,y,x-40*sc,y+120*sc,x+5*sc,y+120*sc,x-45*sc,y+260*sc,x+70*sc,y+90*sc,x+22*sc,y+90*sc,x+60*sc,y);
  }
  public static void Boxes(Bitmap bs,string path){
    using(var bmp=new Bitmap(bs)) using(var g=Graphics.FromImage(bmp)){ g.SmoothingMode=SmoothingMode.AntiAlias;
      // box around the whole body, its right wall torn open and folded outwards
      Poly(g,Cream,600,560,1220,560,1220,1330,600,1330);
      Poly(g,Gold,600,560,1220,560,1160,640,660,640);
      Poly(g,Gold,1220,560,1220,1330,1340,1270,1340,500);
      Runner(g,1085,900,1.15f);
      Poly(g,Gold,1220,1330,1340,1270,1340,1330);
      bmp.Save(path,ImageFormat.Png);}
  }
  public static void Flash(Bitmap bs,string path){
    using(var bmp=new Bitmap(bs)) using(var g=Graphics.FromImage(bmp)){ g.SmoothingMode=SmoothingMode.AntiAlias;
      Bolt(g,720,520,1.1f); Bolt(g,1330,540,1.0f);
      Runner(g,1020,830,1.2f);
      // lightning striking down at the feet
      Bolt(g,830,1110,1.15f); Bolt(g,1100,1130,1.15f);
      Limb(g,Gold,22,640,1380,1420,1380);
      bmp.Save(path,ImageFormat.Png);}
  }
  static void Bubble(Graphics g,Color fill,float cx,float cy,float rx,float ry,bool tail){
    if(tail){ Poly(g,fill,cx-rx*0.55f,cy+ry*0.75f,cx-rx*0.95f,cy+ry*1.55f,cx-rx*0.05f,cy+ry*0.97f); }
    using(var br=new SolidBrush(Black)) g.FillEllipse(br,cx-rx-O,cy-ry-O,2*(rx+O),2*(ry+O));
    using(var b2=new SolidBrush(fill)) g.FillEllipse(b2,cx-rx,cy-ry,2*rx,2*ry);
  }
  public static void Says(Bitmap bs,string path){
    using(var bmp=new Bitmap(bs)) using(var g=Graphics.FromImage(bmp)){ g.SmoothingMode=SmoothingMode.AntiAlias;
      Bubble(g,Purple,1320,610,185,140,false);
      Bubble(g,Gold,900,880,420,310,true);
      // heart in the big bubble, three dots in the small one
      var pts=new List<float>(); for(int i=0;i<64;i++){ double a=i*2*Math.PI/64; double x=16*Math.Pow(Math.Sin(a),3); double y=13*Math.Cos(a)-5*Math.Cos(2*a)-2*Math.Cos(3*a)-Math.Cos(4*a); pts.Add(900+(float)x*15); pts.Add(850-(float)y*15); }
      Poly(g,Purple,pts.ToArray());
      using(var db=new SolidBrush(Black)){ for(int k=0;k<3;k++) g.FillEllipse(db,1280+k*66,586,40,40); }
      using(var sp=P(Black,34)){ g.DrawLine(sp,1380,470,1440,420); g.DrawLine(sp,1420,540,1500,520); }
      using(var sq=P(Gold,16)){ g.DrawLine(sq,1380,470,1440,420); g.DrawLine(sq,1420,540,1500,520); }
      bmp.Save(path,ImageFormat.Png);}
  }
  public static void Chain(Bitmap bs,string path){
    using(var bmp=new Bitmap(bs)) using(var g=Graphics.FromImage(bmp)){ g.SmoothingMode=SmoothingMode.AntiAlias;
      Limb(g,Purple,130,950,1110,920,1330); Limb(g,Purple,130,1100,1110,1130,1330);
      Poly(g,Purple,760,780,1290,780,1130,1110,920,1110);
      Poly(g,Gold,920,1110,1130,1110,1150,1200,900,1200);
      // heavy arms pulling outwards, large biceps, fists
      Limb(g,Purple,150,800,830,690,720,650,590); Limb(g,Purple,150,1250,830,1360,720,1400,590);
      Circle(g,Purple,720,740,88); Circle(g,Purple,1330,740,88);
      Circle(g,Gold,650,570,70); Circle(g,Gold,1400,570,70);
      Circle(g,Gold,1026,660,92);
      using(var eb=new SolidBrush(Black)){ g.FillEllipse(eb,978,630,26,32); g.FillEllipse(eb,1048,630,26,32);} 
      using(var br=P(Black,12)){ g.DrawLine(br,960,610,1004,628); g.DrawLine(br,1092,610,1048,628); g.DrawLine(br,985,708,1067,708);} 
      // chain between the fists, torn apart in the middle
      float[] left={770,860}; float[] right={1190,1280};
      foreach(float x in left){ using(var p=P(Black,30)) g.DrawEllipse(p,x-70,520,140,84); using(var q=P(Gold,12)) g.DrawEllipse(q,x-70,520,140,84);} 
      foreach(float x in right){ using(var p=P(Black,30)) g.DrawEllipse(p,x-70,520,140,84); using(var q=P(Gold,12)) g.DrawEllipse(q,x-70,520,140,84);} 
      using(var sp=P(Black,34)) { g.DrawLine(sp,1026,470,1026,420); g.DrawLine(sp,980,500,940,470); g.DrawLine(sp,1072,500,1112,470); g.DrawLine(sp,980,620,940,650); g.DrawLine(sp,1072,620,1112,650);} 
      using(var sq=P(Gold,16)) { g.DrawLine(sq,1026,470,1026,420); g.DrawLine(sq,980,500,940,470); g.DrawLine(sq,1072,500,1112,470); g.DrawLine(sq,980,620,940,650); g.DrawLine(sq,1072,620,1112,650);} 
      bmp.Save(path,ImageFormat.Png);}
  }
}
"@
$dir='C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\frontend\Assets\Resources\Achievements\Extra'
$files=@('all-star','boxes-star','flash-star','stars-60','stars-120','tip-born','tip-dream','tip-emotions','tip-focus','tip-fun','tip-mindset','tip-myth','tip-senses','tip-retrieval','memory-athlete','tip-major-system','tip-memory-palace','video-intro','focus-chain-max','focus-boxes-max') | ForEach-Object { "$dir\$_.png" }
$base=[Athletes]::Base($files)
$out=$args[0]; New-Item -ItemType Directory -Force $out | Out-Null
[Athletes]::Boxes($base,"$out\athlete-one-heart-boxes.png"); [Athletes]::Flash($base,"$out\athlete-one-heart-flash.png"); [Athletes]::Chain($base,"$out\athlete-one-heart-chain.png"); [Athletes]::Says($base,"$out\athlete-one-heart-says.png"); $base.Save("$out\_base.png"); 'done'









