FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore JobProcessor.sln

WORKDIR /src/JobProcessor.JobsWorker
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "JobProcessor.JobsWorker.dll"]