$outputDir = [IO.Path]::Combine($pwd, "bin/Output");
$files = @(
    "manifest.xml",
    "bin\Debug\TorchEconomy.dll",
    "bin\Debug\TorchEconomy.pdb",
    "bin\Debug\Dapper.dll",
    "bin\Debug\Google.Protobuf.dll",
    "bin\Debug\Mono.Data.Sqlite.dll",
    "bin\Debug\Newtonsoft.Json.dll"
)
$version = "v1.0.0"
 
# Check output directory exists
New-Item -ItemType Directory -Force -Path $outputDir

# Create the archive
foreach($file in $files) {
    $filePath = [IO.Path]::Combine($pwd,$file)
    $destinationPath = [IO.Path]::Combine($outputDir, "TorchEconomy_" + $version + ".zip")
    Compress-Archive -Path $filePath -Update -DestinationPath $destinationPath
}