#
#   This script is building the nuget packages for you to import and test in a dev environment
#   for building apps. Run once to create the version file. Then set the version you want to use
#   run script and the packages is copied to ./packages folder.
#   In your app dev environment,
#   - change version in csproj file
#   - Do a "dotnet restore -s [path_to_package_folder]"
#
#   RUN THIS FILE FROM PROJECT ROOT!
#

# Create the version file if not exists
if ((Test-Path ./scripts/version.txt) -eq $False) {
    New-Item -Path ./scripts -Name "version.txt" -ItemType "file" -Value "0.0.1-beta"
}

if ((Test-Path ./packages) -eq $False) {
    New-Item . -Name "packages" -ItemType "directory"
}
# Remove all current nuget packages
Get-ChildItem  ./src -file -recurse "*.nupkg" | Remove-Item

# Get version to be used
$version = Get-Content ./scripts/version.txt

# Pack
dotnet pack -p:PackageVersion=$version

# Copy the two app packages
Get-ChildItem  ./src/App -file -recurse "*.nupkg" | Copy-Item -Destination "./packages"
Get-ChildItem  ./src/DaemonRunner -file -recurse "*.nupkg" | Copy-Item -Destination "./packages"
Get-ChildItem  ./src/Daemon -file -recurse "*.nupkg" | Copy-Item -Destination "./packages"

