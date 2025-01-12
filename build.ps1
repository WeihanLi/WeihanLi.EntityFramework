[string]$SCRIPT = '.\build\build.cs'
 
# Install dotnet tool
dotnet tool update --global dotnet-execute

Write-Host "dotnet-exec $SCRIPT --args $ARGS" -ForegroundColor GREEN
 
dotnet-exec $SCRIPT --args $ARGS
