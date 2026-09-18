FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["BlackburnCaravanServices/BlackburnCaravanServices.csproj", "BlackburnCaravanServices/"]
RUN dotnet restore "BlackburnCaravanServices/BlackburnCaravanServices.csproj"

COPY . .

WORKDIR "/src/BlackburnCaravanServices"
RUN dotnet publish "BlackburnCaravanServices.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BlackburnCaravanServices.dll"]