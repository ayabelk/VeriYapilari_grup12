FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["front_End/front_End.csproj", "front_End/"]
RUN dotnet restore "./front_End/front_End.csproj"
COPY . .
WORKDIR "/src/front_End"
RUN dotnet publish "./front_End.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
RUN printf 'events{}\nhttp{\ninclude mime.types;\nserver{\nlisten 80;\nindex index.html;\nroot /usr/share/nginx/html;\nlocation / {\ntry_files $uri $uri/ /index.html;\n}\n}\n}' > /etc/nginx/nginx.conf