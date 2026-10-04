### RTeset bazy 
on path "/Storage/Magazine v2"

dotnet ef database drop --force \
  --project MagazineAPI/MagazineAPIInfrastructure \
  --startup-project MagazineAPI/MagazineAPI \
  --context AppDbContext

dotnet ef database drop --force \
  --project MagazineAPI/MagazineAPIInfrastructure \
  --startup-project MagazineAPI/MagazineAPI \
  --context HistoryDbContext

dotnet ef database update \
  --project MagazineAPI/MagazineAPIInfrastructure \
  --startup-project MagazineAPI/MagazineAPI \
  --context AppDbContext

dotnet ef database update \
  --project MagazineAPI/MagazineAPIInfrastructure \
  --startup-project MagazineAPI/MagazineAPI \
  --context HistoryDbContext

dotnet run --project MagazineAPI/MagazineAPI