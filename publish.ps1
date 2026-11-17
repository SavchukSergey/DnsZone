$project = "DnsZone"
$projectFile = "./$project/$project.csproj"
$configuration = "Release"
$version = ([xml](Get-Content $projectFile)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1

$source="https://api.nuget.org/v3/index.json"
$keyFile = if (Test-Path nuget.key) { "nuget.key" } else { "../nuget.key" }
$key = Get-Content $keyFile

& dotnet restore $projectFile
& dotnet build $projectFile -c $configuration

& dotnet test "./$project.Tests\$project.Tests.csproj"
if (!$?) { Exit };

& dotnet pack $projectFile -c $configuration -p:PackageVersion=$version
& dotnet nuget push "./$project/bin/$configuration/$project.$version.nupkg" --source $source --api-key $key
