FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
COPY ["./dopaminator-backend/dopaminator-backend.csproj", "./"]
RUN dotnet restore
COPY ./dopaminator-backend .
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image as the final image
FROM base AS release
WORKDIR /app
COPY --from=builder /app/publish .
RUN ls -la
ENTRYPOINT ["dotnet", "/app/dopaminator-backend.dll"]
