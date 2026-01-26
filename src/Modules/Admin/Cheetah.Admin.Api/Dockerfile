FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Build.props", "src/"]

# Core projects
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.Core.CQRS/Cheetah.Core.CQRS.csproj", "src/Cheetah.Core.CQRS/"]
COPY ["src/Cheetah.Core.Security/Cheetah.Core.Security.csproj", "src/Cheetah.Core.Security/"]
COPY ["src/Cheetah.AspNetCore/Cheetah.AspNetCore.csproj", "src/Cheetah.AspNetCore/"]
COPY ["src/Cheetah.AspNetCore.Contracts/Cheetah.AspNetCore.Contracts.csproj", "src/Cheetah.AspNetCore.Contracts/"]

# Backend projects
COPY ["src/Cheetah.Backend.CQRS/Cheetah.Backend.CQRS.csproj", "src/Cheetah.Backend.CQRS/"]
COPY ["src/Cheetah.Backend.Endpoints/Cheetah.Backend.Endpoints.csproj", "src/Cheetah.Backend.Endpoints/"]
COPY ["src/Cheetah.OpenApi/Cheetah.OpenApi.csproj", "src/Cheetah.OpenApi/"]
COPY ["src/Cheetah.Scalar/Cheetah.Scalar.csproj", "src/Cheetah.Scalar/"]

# Mapping projects
COPY ["src/Cheetah.Mapping.Core/Cheetah.Mapping.Core.csproj", "src/Cheetah.Mapping.Core/"]
COPY ["src/Cheetah.Mapping.Mapster/Cheetah.Mapping.Mapster.csproj", "src/Cheetah.Mapping.Mapster/"]

# Generators and Analyzers
COPY ["src/Cheetah.Analyzers/Cheetah.Analyzers.csproj", "src/Cheetah.Analyzers/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]
COPY ["src/Cheetah.Generators.Endpoints/Cheetah.Generators.Endpoints.csproj", "src/Cheetah.Generators.Endpoints/"]

# Admin module projects
COPY ["src/Modules/Admin/Cheetah.Admin.Api/Cheetah.Admin.Api.csproj", "src/Modules/Admin/Cheetah.Admin.Api/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Api/Cheetah.Admin.Modules.Clients.Api.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Api/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Application/Cheetah.Admin.Modules.Clients.Application.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Application/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Contracts/Cheetah.Admin.Modules.Clients.Contracts.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Contracts/"]

RUN dotnet restore "src/Modules/Admin/Cheetah.Admin.Api/Cheetah.Admin.Api.csproj"
COPY . .
WORKDIR "/src/src/Modules/Admin/Cheetah.Admin.Api"
RUN dotnet build "./Cheetah.Admin.Api.csproj" -c $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cheetah.Admin.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.Admin.Api.dll"]
