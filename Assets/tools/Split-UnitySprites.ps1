param(
    [string]$InputPath,
    [string]$OutputPath,
    [switch]$Overwrite,
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$nyhRoot = Split-Path -Parent $scriptRoot

if ([string]::IsNullOrWhiteSpace($InputPath)) {
    $InputPath = Join-Path $nyhRoot "assets"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $nyhRoot "split_sprites"
}

function ConvertTo-SafeFileName {
    param([string]$Name)

    $invalid = [System.IO.Path]::GetInvalidFileNameChars()
    $result = $Name
    foreach ($char in $invalid) {
        $result = $result.Replace($char, "_")
    }

    return $result
}

function Add-SpriteIfComplete {
    param(
        [System.Collections.Generic.List[object]]$Sprites,
        [hashtable]$Sprite
    )

    if ($null -eq $Sprite) {
        return
    }

    $required = @("Name", "X", "Y", "Width", "Height")
    foreach ($key in $required) {
        if (-not $Sprite.ContainsKey($key)) {
            return
        }
    }

    if ($Sprite.Width -le 0 -or $Sprite.Height -le 0) {
        return
    }

    [void]$Sprites.Add([pscustomobject]@{
        Name = [string]$Sprite.Name
        X = [int][math]::Round([double]$Sprite.X)
        Y = [int][math]::Round([double]$Sprite.Y)
        Width = [int][math]::Round([double]$Sprite.Width)
        Height = [int][math]::Round([double]$Sprite.Height)
    })
}

function Get-UnityMetaSprites {
    param([string]$MetaPath)

    $sprites = [System.Collections.Generic.List[object]]::new()
    $inSprites = $false
    $inRect = $false
    $current = $null

    foreach ($line in [System.IO.File]::ReadLines($MetaPath)) {
        if (-not $inSprites) {
            if ($line -match "^\s+sprites:\s*$") {
                $inSprites = $true
            }
            continue
        }

        if ($line -match "^\s*-\s*serializedVersion:\s*\d+\s*$") {
            Add-SpriteIfComplete -Sprites $sprites -Sprite $current
            $current = @{}
            $inRect = $false
            continue
        }

        if ($null -eq $current) {
            continue
        }

        if ($line -match "^\s*name:\s*(.+?)\s*$") {
            $current.Name = $Matches[1]
            continue
        }

        if ($line -match "^\s*rect:\s*$") {
            $inRect = $true
            continue
        }

        if ($line -match "^\s*alignment:\s*") {
            $inRect = $false
            continue
        }

        if ($inRect) {
            if ($line -match "^\s*x:\s*(-?\d+(?:\.\d+)?)\s*$") {
                $current.X = [double]$Matches[1]
            } elseif ($line -match "^\s*y:\s*(-?\d+(?:\.\d+)?)\s*$") {
                $current.Y = [double]$Matches[1]
            } elseif ($line -match "^\s*width:\s*(-?\d+(?:\.\d+)?)\s*$") {
                $current.Width = [double]$Matches[1]
            } elseif ($line -match "^\s*height:\s*(-?\d+(?:\.\d+)?)\s*$") {
                $current.Height = [double]$Matches[1]
            }
        }
    }

    Add-SpriteIfComplete -Sprites $sprites -Sprite $current
    return $sprites
}

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$TargetPath
    )

    $baseFull = [System.IO.Path]::GetFullPath($BasePath).TrimEnd("\", "/") + [System.IO.Path]::DirectorySeparatorChar
    $targetFull = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = [System.Uri]::new($baseFull)
    $targetUri = [System.Uri]::new($targetFull)
    return [System.Uri]::UnescapeDataString($baseUri.MakeRelativeUri($targetUri).ToString()).Replace("/", [System.IO.Path]::DirectorySeparatorChar)
}

function Get-PngFiles {
    param([string]$Path)

    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        if ([System.IO.Path]::GetExtension($Path) -ieq ".png") {
            return @(Get-Item -LiteralPath $Path)
        }
        throw "InputPath is a file, but it is not a PNG: $Path"
    }

    if (Test-Path -LiteralPath $Path -PathType Container) {
        return @(Get-ChildItem -LiteralPath $Path -Recurse -File -Filter "*.png")
    }

    throw "InputPath does not exist: $Path"
}

$inputItem = Get-Item -LiteralPath $InputPath
$basePath = if ($inputItem.PSIsContainer) { $inputItem.FullName } else { Split-Path -Parent $inputItem.FullName }
$pngFiles = Get-PngFiles -Path $inputItem.FullName
$processedSheets = 0
$writtenSprites = 0
$skippedSheets = 0

foreach ($png in $pngFiles) {
    $metaPath = "$($png.FullName).meta"
    if (-not (Test-Path -LiteralPath $metaPath -PathType Leaf)) {
        $skippedSheets++
        continue
    }

    $sprites = @(Get-UnityMetaSprites -MetaPath $metaPath)
    if ($sprites.Count -eq 0) {
        $skippedSheets++
        continue
    }

    $relative = Get-RelativePath -BasePath $basePath -TargetPath $png.FullName
    $relativeDir = Split-Path -Parent $relative
    $sheetName = [System.IO.Path]::GetFileNameWithoutExtension($png.Name)
    $sheetOutputDir = if ([string]::IsNullOrWhiteSpace($relativeDir)) {
        Join-Path $OutputPath $sheetName
    } else {
        Join-Path $OutputPath (Join-Path $relativeDir $sheetName)
    }

    Write-Host ("{0}: {1} sprites" -f $relative, $sprites.Count)
    $processedSheets++

    if ($DryRun) {
        continue
    }

    if (-not (Test-Path -LiteralPath $sheetOutputDir -PathType Container)) {
        [void][System.IO.Directory]::CreateDirectory($sheetOutputDir)
    }

    $bitmap = $null
    try {
        $bitmap = [System.Drawing.Bitmap]::FromFile($png.FullName)

        foreach ($sprite in $sprites) {
            $cropX = $sprite.X
            $cropY = $bitmap.Height - $sprite.Y - $sprite.Height
            $cropRect = [System.Drawing.Rectangle]::new($cropX, $cropY, $sprite.Width, $sprite.Height)

            if ($cropRect.Left -lt 0 -or $cropRect.Top -lt 0 -or $cropRect.Right -gt $bitmap.Width -or $cropRect.Bottom -gt $bitmap.Height) {
                Write-Warning ("Skipping out-of-bounds sprite {0} in {1}" -f $sprite.Name, $png.FullName)
                continue
            }

            $safeName = ConvertTo-SafeFileName -Name $sprite.Name
            $destination = Join-Path $sheetOutputDir "$safeName.png"
            if ((Test-Path -LiteralPath $destination -PathType Leaf) -and -not $Overwrite) {
                continue
            }

            $piece = $bitmap.Clone($cropRect, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $piece.Save($destination, [System.Drawing.Imaging.ImageFormat]::Png)
                $writtenSprites++
            } finally {
                $piece.Dispose()
            }
        }
    } finally {
        if ($null -ne $bitmap) {
            $bitmap.Dispose()
        }
    }
}

Write-Host ""
Write-Host ("Processed sheets: {0}" -f $processedSheets)
Write-Host ("Written sprites:   {0}" -f $writtenSprites)
Write-Host ("Skipped sheets:    {0}" -f $skippedSheets)
Write-Host ("Output:            {0}" -f ([System.IO.Path]::GetFullPath($OutputPath)))
