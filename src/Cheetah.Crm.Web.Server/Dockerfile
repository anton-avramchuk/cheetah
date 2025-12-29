# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props for version management
COPY ["src/Directory.Build.props", "src/"]

# Copy project files
COPY ["src/Cheetah.Crm.Web.Server/Cheetah.Crm.Web.csproj", "src/Cheetah.Crm.Web.Server/"]
COPY ["src/Cheetah.Crm.Web.Client/Cheetah.Crm.Web.Client.csproj", "src/Cheetah.Crm.Web.Client/"]

# Restore dependencies
RUN dotnet restore "src/Cheetah.Crm.Web.Server/Cheetah.Crm.Web.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/src/Cheetah.Crm.Web.Server"
RUN dotnet build "./Cheetah.Crm.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "./Cheetah.Crm.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.Crm.Web.dll"]
