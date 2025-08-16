[string]$SCRIPT = '.\build\build.cs'
 
# Install dotnet tool
dotnet tool install --global dotnet-execute --prerelease

Write-Host "dotnet-exec $SCRIPT --args $ARGS" -ForegroundColor GREEN
 
dotnet-exec $SCRIPT --args $ARGS
