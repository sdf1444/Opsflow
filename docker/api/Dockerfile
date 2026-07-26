FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /src

COPY backend/*.sln ./
COPY backend/ .

RUN dotnet restore
RUN dotnet build -c Debug

WORKDIR /src/OpsFlow.Api

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls=http://0.0.0.0:8080"]
