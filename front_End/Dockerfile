# Bu aşama, VS'den hızlı modda çalıştırıldığında kullanılır
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# Oluşturma (Build) aşaması
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["front_End/front_End.csproj", "front_End/"]
RUN dotnet restore "./front_End/.csproj"
COPY . .
WORKDIR "/src/front_End"
RUN dotnet build "./front_End.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Yayınlama aşaması
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./front_End/front_End.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final aşaması: Sadece gerekli dosyaları alır
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "front_End.dll"]]