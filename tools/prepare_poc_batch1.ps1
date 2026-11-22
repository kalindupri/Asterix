# Export authored images and produce the batch contact sheet / tiling QA.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$probe = [System.Drawing.Bitmap]::new(1,1); $probe.Dispose()
$drawingRefs = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -match '^(System.Drawing|System.Private.Windows)' } |
    ForEach-Object { $_.Location } | Select-Object -Unique
Add-Type -ReferencedAssemblies $drawingRefs -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
public static class PocBatchExport {
    static Rectangle Bounds(Bitmap im) {
        int l=im.Width,t=im.Height,r=0,b=0;
        for(int y=0;y<im.Height;y++) for(int x=0;x<im.Width;x++) {
            if(im.GetPixel(x,y).A<16) continue;
            l=Math.Min(l,x); t=Math.Min(t,y); r=Math.Max(r,x+1); b=Math.Max(b,y+1);
        }
        if(r<=l||b<=t) throw new Exception("Empty art");
        return new Rectangle(l,t,r-l,b-t);
    }
    static Graphics Setup(Bitmap image) {
        var g=Graphics.FromImage(image);
        g.InterpolationMode=InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode=PixelOffsetMode.HighQuality;
        return g;
    }
    static void Draw(Graphics g, Image image, Rectangle dst, Rectangle src) {
        using(var attr=new ImageAttributes()) {
            attr.SetWrapMode(WrapMode.TileFlipXY);
            g.DrawImage(image,dst,src.X,src.Y,src.Width,src.Height,GraphicsUnit.Pixel,attr);
        }
    }
    static Color Mix(Color a,Color b,double t) {
        double alpha=a.A*(1-t)+b.A*t;
        if(alpha<0.5) return Color.Transparent;
        return Color.FromArgb((int)Math.Round(alpha),
            (int)Math.Round((a.R*a.A*(1-t)+b.R*b.A*t)/alpha),
            (int)Math.Round((a.G*a.A*(1-t)+b.G*b.A*t)/alpha),
            (int)Math.Round((a.B*a.A*(1-t)+b.B*b.A*t)/alpha));
    }
    static void Periodic(Bitmap im,bool horizontal) {
        int span=horizontal?im.Width:im.Height, rows=horizontal?im.Height:im.Width;
        int band=Math.Max(8,Math.Min(64,span/16));
        for(int row=0;row<rows;row++) for(int i=0;i<band;i++) {
            int xa=horizontal?i:row, ya=horizontal?row:i;
            int xb=horizontal?span-1-i:row, yb=horizontal?row:span-1-i;
            Color a=im.GetPixel(xa,ya),b=im.GetPixel(xb,yb),mid=Mix(a,b,.5);
            double t=1.0-(double)i/(band-1); t=t*t*(3-2*t);
            im.SetPixel(xa,ya,Mix(a,mid,t)); im.SetPixel(xb,yb,Mix(b,mid,t));
        }
    }
    public static void Export(string input,string output,int w,int h,string mode,string tile,int surface) {
        using(var source=new Bitmap(input)) using(var result=new Bitmap(w,h,PixelFormat.Format32bppArgb)) {
            Rectangle src=new Rectangle(0,0,source.Width,source.Height);
            Rectangle dst=new Rectangle(0,0,w,h);
            if(mode=="prop"||mode=="icon") {
                src=Bounds(source);
                if(mode=="icon") dst=new Rectangle(3,3,w-6,h-6);
            }
            if(mode=="bottom") {
                var bounds=Bounds(source); src=new Rectangle(0,0,source.Width,bounds.Bottom);
            }
            using(var g=Setup(result)) {
                g.CompositingMode=CompositingMode.SourceCopy; g.Clear(Color.Transparent);
                if(surface>0) {
                    Draw(g,source,new Rectangle(0,0,w,64),new Rectangle(0,0,source.Width,surface));
                    Draw(g,source,new Rectangle(0,64,w,h-64),new Rectangle(0,surface,source.Width,source.Height-surface));
                } else Draw(g,source,dst,src);
            }
            // Canonicalise transparent RGB, and restore full coverage in authored solid regions.
            for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
                Color c=result.GetPixel(x,y);
                if(c.A==0) result.SetPixel(x,y,Color.Transparent);
                else if(c.A>=250) result.SetPixel(x,y,Color.FromArgb(255,c.R,c.G,c.B));
            }
            // Narrow premultiplied edge reconciliation only; authored centre remains unchanged.
            if(tile.Contains("x")) Periodic(result,true);
            if(tile.Contains("y")) Periodic(result,false);
            result.Save(output,ImageFormat.Png);
        }
    }
    static void Checker(Graphics g,int w,int h) {
        using(var a=new SolidBrush(Color.FromArgb(53,65,67)))
        using(var b=new SolidBrush(Color.FromArgb(66,77,78))) {
            g.FillRectangle(a,0,0,w,h);
            for(int y=0;y<h;y+=16) for(int x=0;x<w;x+=16)
                if(((x/16+y/16)&1)==0) g.FillRectangle(b,x,y,16,16);
        }
    }
    public static void TilePreview(string input,string output) {
        using(var im=new Bitmap(input)) {
            double scale=Math.Min(384.0/im.Width,240.0/im.Height);
            int w=Math.Max(1,(int)(im.Width*scale)),h=Math.Max(1,(int)(im.Height*scale));
            using(var result=new Bitmap(w*3,h*3)) using(var g=Setup(result)) {
                Checker(g,result.Width,result.Height);
                for(int y=0;y<3;y++) for(int x=0;x<3;x++)
                    Draw(g,im,new Rectangle(x*w,y*h,w,h),new Rectangle(0,0,im.Width,im.Height));
                result.Save(output,ImageFormat.Png);
            }
        }
    }
    public static void Contact(string[] inputs,string[] labels,string output) {
        const int w=384,h=240;
        using(var result=new Bitmap(w*4,h*((inputs.Length+3)/4))) using(var g=Setup(result))
        using(var font=new Font("Segoe UI",10)) using(var text=new SolidBrush(Color.White)) {
            Checker(g,result.Width,result.Height);
            for(int i=0;i<inputs.Length;i++) using(var im=new Bitmap(inputs[i])) {
                int x=(i%4)*w,y=(i/4)*h;
                double scale=Math.Min((w-20.0)/im.Width,(h-42.0)/im.Height);
                int iw=(int)(im.Width*scale),ih=(int)(im.Height*scale);
                Draw(g,im,new Rectangle(x+(w-iw)/2,y+30+(h-36-ih)/2,iw,ih),new Rectangle(0,0,im.Width,im.Height));
                g.DrawString(labels[i],font,text,x+8,y+6);
            }
            result.Save(output,ImageFormat.Png);
        }
    }
}
'@
$batchRoot = Split-Path $PSScriptRoot -Parent
$batchData = Get-Content (Join-Path $batchRoot 'docs/art-source/poc-batch-1/generation.json') -Raw | ConvertFrom-Json
$previewPath = Join-Path $batchRoot 'docs/art-source/poc-batch-1/tiling-previews'
New-Item -ItemType Directory -Force $previewPath | Out-Null
$images = @(); $labels = @()
foreach ($asset in $batchData.assets) {
    $sourceFile = if ($asset.selectedSource) { $asset.selectedSource } else { $asset.source }
    $outputFile = Join-Path $batchRoot $asset.output
    New-Item -ItemType Directory -Force (Split-Path $outputFile -Parent) | Out-Null
    [PocBatchExport]::Export((Join-Path $batchRoot $sourceFile),$outputFile,$asset.size[0],$asset.size[1],$asset.exportMode,$asset.tile,$asset.sourceSurfaceY)
    if ($asset.tile) { [PocBatchExport]::TilePreview($outputFile,(Join-Path $previewPath ($asset.name+'.png'))) }
    $images += $outputFile; $labels += "$($asset.name)  $($asset.size[0])x$($asset.size[1])"
    Write-Output "Exported $($asset.name)"
}
[PocBatchExport]::Contact([string[]]$images,[string[]]$labels,(Join-Path $batchRoot 'docs/art-source/poc-batch-1/contact-sheet.png'))
