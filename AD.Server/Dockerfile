FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

COPY ["AD.Server.csproj", "./"]
RUN dotnet restore "AD.Server.csproj"

COPY . .
RUN dotnet publish "AD.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5264
EXPOSE 5264
ENTRYPOINT ["dotnet", "AD.Server.dll"]
