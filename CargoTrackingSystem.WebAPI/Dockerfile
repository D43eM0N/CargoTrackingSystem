FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Import all Solution content
COPY . .

# Reloading Dependencies on WebAPI
RUN dotnet restore "CargoTrackingSystem.WebAPI/CargoTrackingSystem.WebAPI.csproj"

# Release Mode 
WORKDIR "/src/CargoTrackingSystem.WebAPI"
RUN dotnet publish "CargoTrackingSystem.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# Ports
EXPOSE 8080
EXPOSE 8081
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CargoTrackingSystem.WebAPI.dll"]