FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Modules/Identity/Crm.Identity.Api/Crm.Identity.Api.csproj", "src/Modules/Identity/Crm.Identity.Api/"]
COPY ["src/Cheetah.Analyzers/Cheetah.Analyzers.csproj", "src/Cheetah.Analyzers/"]
COPY ["Cheetah.ServiceDefaults/Cheetah.ServiceDefaults.csproj", "Cheetah.ServiceDefaults/"]
COPY ["src/Cheetah.AspNetCore/Cheetah.AspNetCore.csproj", "src/Cheetah.AspNetCore/"]
COPY ["src/Cheetah.Core.Domain/Cheetah.Core.Domain.csproj", "src/Cheetah.Core.Domain/"]
COPY ["src/Cheetah.Core.Events/Cheetah.Core.Events.csproj", "src/Cheetah.Core.Events/"]
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Core.Security/Cheetah.Core.Security.csproj", "src/Cheetah.Core.Security/"]
COPY ["src/Cheetah.AspNetCore.Contracts/Cheetah.AspNetCore.Contracts.csproj", "src/Cheetah.AspNetCore.Contracts/"]
COPY ["src/Cheetah.Contracts/Cheetah.Contracts.csproj", "src/Cheetah.Contracts/"]
COPY ["src/Cheetah.Backend.CQRS/Cheetah.Backend.CQRS.csproj", "src/Cheetah.Backend.CQRS/"]
COPY ["src/Cheetah.Core.CQRS/Cheetah.Core.CQRS.csproj", "src/Cheetah.Core.CQRS/"]
COPY ["src/Cheetah.Backend.Endpoints/Cheetah.Backend.Endpoints.csproj", "src/Cheetah.Backend.Endpoints/"]
COPY ["src/Cheetah.Backend.Events.Redis/Cheetah.Backend.Events.Redis.csproj", "src/Cheetah.Backend.Events.Redis/"]
COPY ["src/Cheetah.Backend.Redis/Cheetah.Backend.Redis.csproj", "src/Cheetah.Backend.Redis/"]
COPY ["src/Cheetah.Mapping.Mapster/Cheetah.Mapping.Mapster.csproj", "src/Cheetah.Mapping.Mapster/"]
COPY ["src/Cheetah.Mapping.Core/Cheetah.Mapping.Core.csproj", "src/Cheetah.Mapping.Core/"]
COPY ["src/Cheetah.Scalar/Cheetah.Scalar.csproj", "src/Cheetah.Scalar/"]
COPY ["src/Cheetah.OpenApi/Cheetah.OpenApi.csproj", "src/Cheetah.OpenApi/"]
COPY ["src/Modules/Identity/Crm.Identity.Application/Crm.Identity.Application.csproj", "src/Modules/Identity/Crm.Identity.Application/"]
COPY ["src/Cheetah.Core.Grid/Cheetah.Core.Grid.csproj", "src/Cheetah.Core.Grid/"]
COPY ["src/Cheetah.Core.EntityFramework/Cheetah.Core.EntityFramework.csproj", "src/Cheetah.Core.EntityFramework/"]
COPY ["src/Cheetah.Core.DataAccess/Cheetah.Core.DataAccess.csproj", "src/Cheetah.Core.DataAccess/"]
COPY ["src/Cheetah.Core.Specification/Cheetah.Core.Specification.csproj", "src/Cheetah.Core.Specification/"]
COPY ["src/Modules/Identity/Crm.Identity.Contracts/Crm.Identity.Contracts.csproj", "src/Modules/Identity/Crm.Identity.Contracts/"]
COPY ["src/Modules/Identity/Crm.Identity.DataAccess/Crm.Identity.DataAccess.csproj", "src/Modules/Identity/Crm.Identity.DataAccess/"]
COPY ["src/Cheetah.Core.EntityFramework.PostgreSql/Cheetah.Core.EntityFramework.PostgreSql.csproj", "src/Cheetah.Core.EntityFramework.PostgreSql/"]
COPY ["src/Modules/Identity/Crm.Identity.Domain/Crm.Identity.Domain.csproj", "src/Modules/Identity/Crm.Identity.Domain/"]
COPY ["src/Modules/Identity/Crm.Identity.DomainEvents/Crm.Identity.DomainEvents.csproj", "src/Modules/Identity/Crm.Identity.DomainEvents/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]
COPY ["src/Cheetah.Generators.Endpoints/Cheetah.Generators.Endpoints.csproj", "src/Cheetah.Generators.Endpoints/"]
RUN dotnet restore "src/Modules/Identity/Crm.Identity.Api/Crm.Identity.Api.csproj"
COPY . .
WORKDIR "/src/src/Modules/Identity/Crm.Identity.Api"
RUN dotnet build "./Crm.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Crm.Identity.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crm.Identity.Api.dll"]
