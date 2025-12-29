FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props for package versions
COPY ["src/Directory.Build.props", "src/"]

# Copy solution file
COPY ["Cheetah.slnx", "./"]

# Copy all .csproj files to preserve Docker layer caching
COPY ["src/Cheetah.Crm/Cheetah.Crm.csproj", "src/Cheetah.Crm/"]
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.Core.CQRS/Cheetah.Core.CQRS.csproj", "src/Cheetah.Core.CQRS/"]
COPY ["src/Cheetah.Core.Events/Cheetah.Core.Events.csproj", "src/Cheetah.Core.Events/"]
COPY ["src/Cheetah.Core.Domain/Cheetah.Core.Domain.csproj", "src/Cheetah.Core.Domain/"]
COPY ["src/Cheetah.Core.DataAccess/Cheetah.Core.DataAccess.csproj", "src/Cheetah.Core.DataAccess/"]
COPY ["src/Cheetah.Core.EntityFramework/Cheetah.Core.EntityFramework.csproj", "src/Cheetah.Core.EntityFramework/"]
COPY ["src/Cheetah.Core.EntityFramework.PostgreSql/Cheetah.Core.EntityFramework.PostgreSql.csproj", "src/Cheetah.Core.EntityFramework.PostgreSql/"]
COPY ["src/Cheetah.Core.EntityFramework.MySql/Cheetah.Core.EntityFramework.MySql.csproj", "src/Cheetah.Core.EntityFramework.MySql/"]
COPY ["src/Cheetah.Core.EntityFramework.MsSql/Cheetah.Core.EntityFramework.MsSql.csproj", "src/Cheetah.Core.EntityFramework.MsSql/"]
COPY ["src/Cheetah.Core.Tenants/Cheetah.Core.Tenants.csproj", "src/Cheetah.Core.Tenants/"]
COPY ["src/Cheetah.Core.Cache/Cheetah.Core.Cache.csproj", "src/Cheetah.Core.Cache/"]
COPY ["src/Cheetah.AspNetCore/Cheetah.AspNetCore.csproj", "src/Cheetah.AspNetCore/"]
COPY ["src/Cheetah.Backend.CQRS/Cheetah.Backend.CQRS.csproj", "src/Cheetah.Backend.CQRS/"]
COPY ["src/Cheetah.Backend.Events.Redis/Cheetah.Backend.Events.Redis.csproj", "src/Cheetah.Backend.Events.Redis/"]
COPY ["src/Cheetah.Backend.Redis/Cheetah.Backend.Redis.csproj", "src/Cheetah.Backend.Redis/"]
COPY ["src/Cheetah.Mapping.Core/Cheetah.Mapping.Core.csproj", "src/Cheetah.Mapping.Core/"]
COPY ["src/Cheetah.Mapping.Mapster/Cheetah.Mapping.Mapster.csproj", "src/Cheetah.Mapping.Mapster/"]
COPY ["src/Cheetah.OpenApi/Cheetah.OpenApi.csproj", "src/Cheetah.OpenApi/"]
COPY ["src/Cheetah.Scalar/Cheetah.Scalar.csproj", "src/Cheetah.Scalar/"]
COPY ["src/Cheetah.Generators.Core/Cheetah.Generators.Core.csproj", "src/Cheetah.Generators.Core/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.Domain/Cheetah.Tenants.Domain.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.Domain/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.DataAccess/Cheetah.Tenants.DataAccess.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.DataAccess/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.Application/Cheetah.Tenants.Application.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.Application/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.Api/Cheetah.Tenants.Api.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.Api/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.Shared/Cheetah.Tenants.Shared.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.Shared/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.Client/Cheetah.Tenants.Client.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.Client/"]
COPY ["src/Cheetah.Tenants/Cheetah.Tenants.ApiClient/Cheetah.Tenants.ApiClient.csproj", "src/Cheetah.Tenants/Cheetah.Tenants.ApiClient/"]

# Restore NuGet packages
RUN dotnet restore "src/Cheetah.Crm/Cheetah.Crm.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/Cheetah.Crm"
RUN dotnet build "./Cheetah.Crm.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cheetah.Crm.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.Crm.dll"]
