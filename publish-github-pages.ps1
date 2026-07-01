$ErrorActionPreference = "Stop"

$ProjectUrl = "http://127.0.0.1:5077"
$OutputDir = "docs"

$Pages = @(
    @{ Url = "/"; Output = "index.html" },
    @{ Url = "/Servicing/Servicing"; Output = "servicing.html" },
    @{ Url = "/Repairs/Repairs"; Output = "repairs.html" },
    @{ Url = "/CaravansForSale/CaravansForSale"; Output = "caravans-for-sale.html" },
    @{ Url = "/SellYourCaravan/SellYourCaravan"; Output = "sell-your-caravan.html" },
    @{ Url = "/Valeting/Valeting"; Output = "valeting.html" },
    @{ Url = "/Contact/Contact"; Output = "contact.html" }
)

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDir | Out-Null

Copy-Item "BlackburnCaravanServices\wwwroot\*" $OutputDir -Recurse -Force

$CssPath = Join-Path $OutputDir "css\site.css"

if (Test-Path $CssPath) {
    $Css = Get-Content $CssPath -Raw

    $Css = $Css `
        -replace 'url\("/images/', 'url("../images/' `
        -replace "url\('/images/", "url('../images/" `
        -replace 'url\(/images/', 'url(../images/'

    Set-Content -Path $CssPath -Value $Css -Encoding UTF8
}

foreach ($Page in $Pages) {
    $FullUrl = "$ProjectUrl$($Page.Url)"
    $OutputPath = Join-Path $OutputDir $Page.Output

    Write-Host "Exporting $FullUrl -> $OutputPath"

    $Html = Invoke-WebRequest -Uri $FullUrl -UseBasicParsing | Select-Object -ExpandProperty Content

    $Html = $Html `
        -replace 'href="/"', 'href="index.html"' `
        -replace 'href="/Servicing/Servicing"', 'href="servicing.html"' `
        -replace 'href="/Repairs/Repairs"', 'href="repairs.html"' `
        -replace 'href="/CaravansForSale/CaravansForSale"', 'href="caravans-for-sale.html"' `
        -replace 'href="/SellYourCaravan/SellYourCaravan"', 'href="sell-your-caravan.html"' `
        -replace 'href="/Valeting/Valeting"', 'href="valeting.html"' `
        -replace 'href="/Contact/Contact"', 'href="contact.html"' `
        -replace 'href="/css/', 'href="css/' `
        -replace 'href="/lib/', 'href="lib/' `
        -replace 'href="/favicon.ico"', 'href="favicon.ico"' `
        -replace 'src="/js/', 'src="js/' `
        -replace 'src="/lib/', 'src="lib/' `
        -replace 'src="/images/', 'src="images/' `
        -replace 'url\("/images/', 'url("../images/'

    Set-Content -Path $OutputPath -Value $Html -Encoding UTF8
}

New-Item -Path ".\docs\.nojekyll" -ItemType File -Force | Out-Null

Write-Host "Static site exported to /docs"