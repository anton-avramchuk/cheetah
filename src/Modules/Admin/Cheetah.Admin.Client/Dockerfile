FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Directory.Build.props", "src/"]
COPY ["src/Modules/Admin/Cheetah.Admin.Client/Cheetah.Admin.Client.csproj", "src/Modules/Admin/Cheetah.Admin.Client/"]
COPY ["src/Cheetah.Analyzers/Cheetah.Analyzers.csproj", "src/Cheetah.Analyzers/"]
COPY ["src/Cheetah.Core/Cheetah.Core.csproj", "src/Cheetah.Core/"]
COPY ["src/Cheetah.Generators.Module/Cheetah.Generators.Module.csproj", "src/Cheetah.Generators.Module/"]
COPY ["src/Cheetah.Generators.Application/Cheetah.Generators.Application.csproj", "src/Cheetah.Generators.Application/"]
RUN dotnet restore "src/Modules/Admin/Cheetah.Admin.Client/Cheetah.Admin.Client.csproj"
COPY . .
WORKDIR "/src/src/Modules/Admin/Cheetah.Admin.Client"
RUN dotnet publish "./Cheetah.Admin.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/Modules/Admin/Cheetah.Admin.Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 8080
