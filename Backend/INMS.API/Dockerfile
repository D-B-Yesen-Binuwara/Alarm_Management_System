FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY INMS.sln ./
COPY INMS.API/INMS.API.csproj INMS.API/
COPY INMS.Application/INMS.Application.csproj INMS.Application/
COPY INMS.Domain/INMS.Domain.csproj INMS.Domain/
COPY INMS.Infrastructure/INMS.Infrastructure.csproj INMS.Infrastructure/
RUN dotnet restore INMS.API/INMS.API.csproj

COPY . .
RUN dotnet publish INMS.API/INMS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "INMS.API.dll"]
