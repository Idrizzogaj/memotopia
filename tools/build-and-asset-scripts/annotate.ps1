Add-Type -AssemblyName System.Drawing
$src='C:\Users\IdrZog_MEM\.claude\projects\C--Users-IdrZog-MEM-GitProjects-vnv-tools\f4b79332-35c9-4545-b65d-d11d080ca211\tool-results\mcp-Claude_Browser-blob-1793973619026-uki3f2.jpg'
$out='C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs\store-screenshots\GUIDE-ladda-upp-skarmbilder.png'
$img=[Drawing.Image]::FromFile($src); $k=2; $W=$img.Width*$k; $H=$img.Height*$k
$bmp=New-Object Drawing.Bitmap $W,$H; $g=[Drawing.Graphics]::FromImage($bmp); $g.SmoothingMode='AntiAlias'; $g.InterpolationMode='HighQualityBicubic'; $g.TextRenderingHint='AntiAliasGridFit'
$g.DrawImage($img,0,0,$W,$H)
$red=[Drawing.Color]::FromArgb(255,220,20,40); $pen=New-Object Drawing.Pen($red,6); $pen.CustomEndCap=New-Object Drawing.Drawing2D.AdjustableArrowCap(6,6)
$font=New-Object Drawing.Font('Segoe UI',34,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel); $bb=New-Object Drawing.SolidBrush $red; $wb=[Drawing.Brushes]::White
function Label($x,$y,$n,$text){ $g.FillEllipse($bb,$x-26,$y-26,52,52); $f=New-Object Drawing.Font('Segoe UI',34,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel); $g.DrawString("$n",$f,$wb,$x-12,$y-22); $g.DrawString($text,$font,$bb,$x+36,$y-24) }
# 1: tab already chosen (iPhone with Dynamic Island)
$g.DrawRectangle((New-Object Drawing.Pen($red,5)),(180*$k),(168*$k),(120*$k),(34*$k))
Label (150*$k) (150*$k) 1 ('R'+[char]228+'tt flik (redan vald)')
# 2: Remove All
$g.DrawRectangle((New-Object Drawing.Pen($red,5)),(440*$k),(357*$k),(55*$k),(16*$k))
$g.DrawLine($pen,(470*$k),(468*$k),(466*$k),(380*$k)); Label (370*$k) (478*$k) 2 'Klicka Remove All (tar bort gamla)'
# 3: Upload
$g.DrawRectangle((New-Object Drawing.Pen($red,5)),(406*$k),(357*$k),(32*$k),(16*$k))
$g.DrawLine($pen,(330*$k),(436*$k),(414*$k),(382*$k)); Label (60*$k) (445*$k) 3 ('Sedan Upload: v'+[char]228+'lj de 8 bilderna')
$bmp.Save($out,[Drawing.Imaging.ImageFormat]::Png)
'saved'
