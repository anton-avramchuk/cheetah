FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Api/Cheetah.Admin.Api.csproj", "src/Modules/Admin/Cheetah.Admin.Api/"]
COPY ["src/Cheetah.Analyzers/Cheetah.Analyzers.csproj", "src/Cheetah.Analyzers/"]
COPY ["Cheetah.ServiceDefaults/Cheetah.ServiceDefaults.csproj", "Cheetah.ServiceDefaults/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.AspNetCore/Cheetah.AspNetCore.csproj", "src/Cheetah.AspNetCore/"]
COPY ["src/Cheetah.Core.Security/Cheetah.Core.Security.csproj", "src/Cheetah.Core.Security/"]
COPY ["src/Cheetah.Scalar/Cheetah.Scalar.csproj", "src/Cheetah.Scalar/"]
COPY ["src/Cheetah.OpenApi/Cheetah.OpenApi.csproj", "src/Cheetah.OpenApi/"]
RUN dotnet restore "src/Modules/Admin/Cheetah.Admin.Api/Cheetah.Admin.Api.csproj"
COPY . .
WORKDIR "/src/src/Modules/Admin/Cheetah.Admin.Api"
RUN dotnet build "./Cheetah.Admin.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cheetah.Admin.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cheetah.Admin.Api.dll"]
