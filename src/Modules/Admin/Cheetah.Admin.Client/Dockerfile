FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Build.props", "src/"]

# Core projects
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.Contracts/Cheetah.Contracts.csproj", "src/Cheetah.Contracts/"]
COPY ["src/Cheetah.Frontend.Navigation/Cheetah.Frontend.Navigation.csproj", "src/Cheetah.Frontend.Navigation/"]
COPY ["src/Cheetah.Blazor.Components/Cheetah.Blazor.Components.csproj", "src/Cheetah.Blazor.Components/"]

# Generators and Analyzers
COPY ["src/Cheetah.Analyzers/Cheetah.Analyzers.csproj", "src/Cheetah.Analyzers/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]

# Admin module projects
COPY ["src/Modules/Admin/Cheetah.Admin.Client/Cheetah.Admin.Client.csproj", "src/Modules/Admin/Cheetah.Admin.Client/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Frontend/Cheetah.Admin.Modules.Clients.Frontend.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Frontend/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Api.Client/Cheetah.Admin.Modules.Clients.Api.Client.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Api.Client/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Modules.Clients.Contracts/Cheetah.Admin.Modules.Clients.Contracts.csproj", "src/Modules/Admin/Cheetah.Admin.Modules.Clients.Contracts/"]

RUN dotnet restore "src/Modules/Admin/Cheetah.Admin.Client/Cheetah.Admin.Client.csproj"
COPY . .
WORKDIR "/src/src/Modules/Admin/Cheetah.Admin.Client"
RUN dotnet publish "./Cheetah.Admin.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/Modules/Admin/Cheetah.Admin.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 8080
