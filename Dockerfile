FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY BookLibwithSub.API/BookLibwithSub.API.csproj BookLibwithSub.API/
COPY BookLibwithSub.Repo/BookLibwithSub.Repo.csproj BookLibwithSub.Repo/
COPY BookLibwithSub.Service/BookLibwithSub.Service.csproj BookLibwithSub.Service/

RUN dotnet restore BookLibwithSub.API/BookLibwithSub.API.csproj

COPY . .

WORKDIR /src/BookLibwithSub.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookLibwithSub.API.dll"]
