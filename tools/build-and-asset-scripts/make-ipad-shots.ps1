Add-Type -AssemblyName System.Drawing
$root='C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs\store-screenshots'
$out="$root\app-store-ipad-13"; New-Item -ItemType Directory -Force $out | Out-Null
$W=2064;$H=2752
$sets=@(
 @('Train your brain.|Challenge your friends.','20261106_134858_0000.png','20261106_134557_0000.png'),
 @('NEW Focus:|fast 90-second games','20261106_134649_0000.png','20261106_134751_0000.png'),
 @('NEW Says and NEW The Deck|memory with pictures','20261106_134803_0000.png','20261106_135034_0000.png'),
 @('20 levels in every game|follow your progress','20261106_134823_0000.png','20261106_134936_0000.png')
)
function RoundImg($g,$img,$x,$y,$w,$h,$r){
  $p=New-Object Drawing.Drawing2D.GraphicsPath; $d=$r*2
  $p.AddArc($x,$y,$d,$d,180,90); $p.AddArc($x+$w-$d,$y,$d,$d,270,90); $p.AddArc($x+$w-$d,$y+$h-$d,$d,$d,0,90); $p.AddArc($x,$y+$h-$d,$d,$d,90,90); $p.CloseFigure()
  $g.SetClip($p); $g.DrawImage($img,$x,$y,$w,$h); $g.ResetClip(); $pen=New-Object Drawing.Pen([Drawing.Color]::White,10); $g.DrawPath($pen,$p)
}
$n=0
foreach($s in $sets){ $n++
  $bmp=New-Object Drawing.Bitmap $W,$H,([Drawing.Imaging.PixelFormat]::Format24bppRgb); $bmp.SetResolution(72,72)
  $g=[Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode='AntiAlias'; $g.InterpolationMode='HighQualityBicubic'; $g.TextRenderingHint='AntiAliasGridFit'
  $br=New-Object Drawing.Drawing2D.LinearGradientBrush((New-Object Drawing.Rectangle 0,0,$W,$H),[Drawing.Color]::FromArgb(255,123,31,255),[Drawing.Color]::FromArgb(255,70,20,180),90); $g.FillRectangle($br,0,0,$W,$H)
  $font=New-Object Drawing.Font('Segoe UI',150,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel); $sf=New-Object Drawing.StringFormat; $sf.Alignment='Center'; $sf.LineAlignment='Center'
  $lines=$s[0] -split '\|'; for($i=0;$i -lt $lines.Count;$i++){ $g.DrawString($lines[$i],$font,[Drawing.Brushes]::White,(New-Object Drawing.RectangleF 0,(120+$i*190),$W,190),$sf) }
  $sw=900; $gap=70; $total=2*$sw+$gap; $x0=[int](($W-$total)/2)
  for($k=0;$k -lt 2;$k++){ $img=[Drawing.Image]::FromFile("$root\raw\$($s[1+$k])"); $sh=[int]($sw*$img.Height/$img.Width); $y=$H-$sh-110
    RoundImg $g $img ($x0+$k*($sw+$gap)) $y $sw $sh 60; $img.Dispose() }
  $bmp.Save("$out\{0:D2}.png" -f $n,[Drawing.Imaging.ImageFormat]::Png); $g.Dispose(); $bmp.Dispose()
}
'done'
