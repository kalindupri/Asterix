# Export the generated cut-outs into the template canvas and occupied bounds.
# Shapes and paint are authored with image_gen; this only trims and resamples PNGs.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$drawingProbe = [System.Drawing.Bitmap]::new(1, 1)
$drawingProbe.Dispose()
$drawingAssemblies = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -match '^(System.Drawing|System.Private.Windows)' } |
    ForEach-Object { $_.Location } | Select-Object -Unique
Add-Type -ReferencedAssemblies $drawingAssemblies -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
public static class BrennaPartExport {
    public static void Export(string source, string output, int w, int h, int[] box) {
        using (var input = new Bitmap(source)) {
            int left=input.Width, top=input.Height, right=0, bottom=0;
            for (int y=0;y<input.Height;y++) for (int x=0;x<input.Width;x++) {
                if (input.GetPixel(x,y).A < 32) continue;
                left=Math.Min(left,x); top=Math.Min(top,y);
                right=Math.Max(right,x+1); bottom=Math.Max(bottom,y+1);
            }
            if (left>=right || top>=bottom) throw new Exception("Empty part: "+source);
            using (var result = new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
                using (var g=Graphics.FromImage(result)) {
                    g.Clear(Color.Transparent);
                    g.CompositingMode=CompositingMode.SourceCopy;
                    g.InterpolationMode=InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode=PixelOffsetMode.HighQuality;
                    g.DrawImage(input,new Rectangle(box[0],box[1],box[2]-box[0],box[3]-box[1]),
                        left,top,right-left,bottom-top,GraphicsUnit.Pixel);
                }
                result.Save(output,ImageFormat.Png);
            }
        }
    }
}
'@
$repoRoot = Split-Path $PSScriptRoot -Parent
$manifestPath = Join-Path $repoRoot 'docs/art-source/brenna-cutout-v1/generation.json'
$partManifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
foreach ($part in $partManifest.parts) {
    $sourcePath = if ($part.selectedSource) { $part.selectedSource } else { $part.source }
    [BrennaPartExport]::Export((Join-Path $repoRoot $sourcePath), (Join-Path $repoRoot $part.output),
        $part.size[0], $part.size[1], [int[]]$part.exportBounds)
    Write-Output "Exported $($part.id) ($($part.size[0]) x $($part.size[1]))"
}
