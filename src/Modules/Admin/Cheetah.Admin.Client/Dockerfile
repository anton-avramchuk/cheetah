FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Modules/Admin/Cheetah.Admin.Client/Cheetah.Admin.Client.csproj -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY docker/nginx/admin.conf /etc/nginx/conf.d/default.conf
