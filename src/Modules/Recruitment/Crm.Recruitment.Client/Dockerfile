FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Modules/Recruitment/Crm.Recruitment.Client/Crm.Recruitment.Client.csproj -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY docker/nginx/recruitment.conf /etc/nginx/conf.d/default.conf
