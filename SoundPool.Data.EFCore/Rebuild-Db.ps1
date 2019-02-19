dotnet ef database drop --force
Remove-Item -r ./Migrations
dotnet ef migrations add initial
dotnet ef database update