[string]$SCRIPT = '.\build\build.cs'
 
# Install dotnet tool
dotnet tool install --global dotnet-execute

Write-Host "dotnet-exec $SCRIPT $ARGS --debug" -ForegroundColor GREEN
 
dotnet-exec $SCRIPT $ARGS
