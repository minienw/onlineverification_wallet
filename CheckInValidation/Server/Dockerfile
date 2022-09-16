#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CheckInValidation/Server/CheckInValidation.Server.csproj", "CheckInValidation/Server/"]
COPY ["CheckInValidation/Shared/CheckInValidation.Shared.csproj", "CheckInValidation/Shared/"]
COPY ["CheckInValidation/Client/CheckInValidation.Client.csproj", "CheckInValidation/Client/"]
RUN dotnet restore "CheckInValidation/Server/CheckInValidation.Server.csproj"
COPY . .
WORKDIR "/src/CheckInValidation/Server"
RUN dotnet build "CheckInValidation.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CheckInValidation.Server.csproj" -c Release -o /app/publish

FROM base AS final
RUN addgroup --group walletrunnergroup --gid 2000 \
&& adduser \    
    --uid 1000 \
    --gid 2000 \
    "walletrunner" 

RUN chown walletrunner:walletrunnergroup  /app
USER walletrunner:walletrunnergroup 

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CheckInValidation.Server.dll"]