$project = "DnsZone"
$projectFile = "./$project/$project.csproj"
$configuration = "Release"
$version = "1.2.1"

$source="https://api.nuget.org/v3/index.json"
$key = Get-Content nuget.key

& dotnet restore $projectFile
& dotnet build $projectFile -c $configuration

& dotnet test "./$project.Tests\$project.Tests.csproj"
if (!$?) { Exit };

& dotnet pack $projectFile -c $configuration -p:PackageVersion=$version
& dotnet nuget push "./$project/bin/$configuration/$project.$version.nupkg" --source $source --api-key $key
