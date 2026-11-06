Add-Type -AssemblyName System.Drawing
foreach($dir in 'C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs\store-screenshots\app-store-iphone-6.3','C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs\store-screenshots\app-store-iphone-6.9'){
  Get-ChildItem $dir -Filter '0*.png' | ForEach-Object {
    $i=[Drawing.Image]::FromFile($_.FullName); $b=New-Object Drawing.Bitmap $i.Width,$i.Height,([Drawing.Imaging.PixelFormat]::Format24bppRgb); $b.SetResolution(72,72)
    $g=[Drawing.Graphics]::FromImage($b); $g.Clear([Drawing.Color]::White); $g.DrawImage($i,0,0,$i.Width,$i.Height); $g.Dispose(); $i.Dispose()
    $tmp="$($_.FullName).tmp"; $b.Save($tmp,[Drawing.Imaging.ImageFormat]::Png); $b.Dispose(); Move-Item $tmp $_.FullName -Force } }
'ok'
