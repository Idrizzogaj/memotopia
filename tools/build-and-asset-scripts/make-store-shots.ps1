Add-Type -AssemblyName System.Drawing
$root='C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs\store-screenshots'
$items=@(
 @('20261106_134858_0000.png','Train your brain.|Challenge your friends.'),
 @('20261106_134557_0000.png','Memory and Focus|in one app'),
 @('20261106_134649_0000.png','NEW Focus:|fast 90-second games'),
 @('20261106_134751_0000.png','Train your attention|green and red'),
 @('20261106_134823_0000.png','20 levels in every game|with stars and XP'),
 @('20261106_135034_0000.png','NEW The Deck:|all 52 cards as pictures'),
 @('20261106_134803_0000.png','NEW Says:|repeat the sequence'),
 @('20261106_134936_0000.png','Follow your progress|levels, stars and XP')
)
function Round($g,$img,$x,$y,$w,$h,$r){
  $p=New-Object Drawing.Drawing2D.GraphicsPath; $d=$r*2
  $p.AddArc($x,$y,$d,$d,180,90); $p.AddArc($x+$w-$d,$y,$d,$d,270,90); $p.AddArc($x+$w-$d,$y+$h-$d,$d,$d,0,90); $p.AddArc($x,$y+$h-$d,$d,$d,90,90); $p.CloseFigure()
  $g.SetClip($p); $g.DrawImage($img,$x,$y,$w,$h); $g.ResetClip()
  $pen=New-Object Drawing.Pen([Drawing.Color]::FromArgb(255,255,255,255),10); $g.DrawPath($pen,$p)
}
function Make($W,$H,$shotW,$top,$capSize,$outDir){
  New-Item -ItemType Directory -Force $outDir | Out-Null; $n=0
  foreach($it in $items){ $n++
    $src=[Drawing.Image]::FromFile("$root\raw\$($it[0])")
    $bmp=New-Object Drawing.Bitmap $W,$H; $g=[Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode='AntiAlias'; $g.InterpolationMode='HighQualityBicubic'; $g.TextRenderingHint='AntiAliasGridFit'
    $br=New-Object Drawing.Drawing2D.LinearGradientBrush((New-Object Drawing.Rectangle 0,0,$W,$H),[Drawing.Color]::FromArgb(255,123,31,255),[Drawing.Color]::FromArgb(255,70,20,180),90)
    $g.FillRectangle($br,0,0,$W,$H)
    $font=New-Object Drawing.Font('Segoe UI',$capSize,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
    $sf=New-Object Drawing.StringFormat; $sf.Alignment='Center'; $sf.LineAlignment='Center'
    $lines=$it[1] -split '\|'; $lh=$capSize*1.25
    for($i=0;$i -lt $lines.Count;$i++){ $rect=New-Object Drawing.RectangleF 0,($top*0.15+$i*$lh),$W,$lh; $g.DrawString($lines[$i],$font,[Drawing.Brushes]::White,$rect,$sf) }
    $shotH=[int]($shotW*$src.Height/$src.Width); $x=[int](($W-$shotW)/2); $y=$H-$shotH-[int]($H*0.035)
    Round $g $src $x $y $shotW $shotH ([int]($shotW*0.06))
    $bmp.Save("$outDir\{0:D2}.png" -f $n,[Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose(); $src.Dispose()
  }
}
Make 1206 2622 960 470 102 "$root\app-store-iphone-6.3"
Make 1080 2160 760 330 86 "$root\google-play-phone"
'done'


