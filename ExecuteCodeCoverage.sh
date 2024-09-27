cd ~/source/repos/BookingSolution

# This only needs to be installed once (globally), if installed it fails silently: 
#dotnet tool install -g dotnet-reportgenerator-globaltool

find -name TestResults -exec rm -r {} \; #Delete all historical TestResults
rm -r ./coveragereport/ #Delete the coveragereport folder

dotnet test ./BookingSolution.sln --collect:"XPlat Code Coverage"

# Generate the Code Coverage HTML Report, ignoring EF migrations
reportgenerator -reports:"./**/coverage.cobertura.xml" -targetdir:"./coveragereport" -reporttypes:Html -classfilters:-Data.Migrations.*

start ./coveragereport/index.html