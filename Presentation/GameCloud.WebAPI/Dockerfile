﻿# Stage 1: Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Presentation/GameCloud.WebAPI/GameCloud.WebAPI.csproj", "Presentation/GameCloud.WebAPI/"]
RUN dotnet restore "Presentation/GameCloud.WebAPI/GameCloud.WebAPI.csproj"
COPY . .
WORKDIR "/src/Presentation/GameCloud.WebAPI"
RUN dotnet build "GameCloud.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GameCloud.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameCloud.WebAPI.dll"]
