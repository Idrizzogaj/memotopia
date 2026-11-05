Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @"
using System; using System.Drawing; using System.Drawing.Imaging;
public static class Refit {
  static int Dist(Color c){ int a=c.A; int r=(c.R*a+255*(255-a))/255,g=(c.G*a+255*(255-a))/255,b=(c.B*a+255*(255-a))/255; return 255-Math.Min(r,Math.Min(g,b)); }
  public static string Run(string inp,string outp,float scale,int cx,int cy,int fx,int fy){
    var bmp=new Bitmap(inp); int W=bmp.Width,H=bmp.Height;
    var d=new Bitmap(W,H,PixelFormat.Format32bppArgb); using(var g=Graphics.FromImage(d)) g.DrawImage(bmp,0,0,W,H); bmp.Dispose();
    Color fill=d.GetPixel(fx,fy);
    int minx=W,miny=H,maxx=0,maxy=0;
    for(int y=0;y<H;y++)for(int x=0;x<W;x++){var c=d.GetPixel(x,y); if(Dist(c)>70){if(x<minx)minx=x;if(x>maxx)maxx=x;if(y<miny)miny=y;if(y>maxy)maxy=y;}}
    minx=Math.Max(0,minx-6);miny=Math.Max(0,miny-6);maxx=Math.Min(W-1,maxx+6);maxy=Math.Min(H-1,maxy+6); int w=maxx-minx+1,h=maxy-miny+1;
    var content=new Bitmap(w,h,PixelFormat.Format32bppArgb);
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=d.GetPixel(minx+x,miny+y);int dist=Dist(c);
      double a=Math.Max(0,Math.Min(1,(dist-25)/60.0)); content.SetPixel(x,y,Color.FromArgb((int)(a*255),c.R,c.G,c.B));
      if(dist>25) d.SetPixel(minx+x,miny+y,fill);}
    int nw=(int)(w*scale),nh=(int)(h*scale);
    using(var g=Graphics.FromImage(d)){g.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
      g.DrawImage(content,new Rectangle(cx-nw/2,cy-nh/2,nw,nh),0,0,w,h,GraphicsUnit.Pixel);}
    d.Save(outp,ImageFormat.Png); return W+"x"+H+" content "+w+"x"+h+" -> "+nw+"x"+nh;
  }
}
"@
[Refit]::Run($args[0],$args[1],0.8,1026,937,1026,1066)

