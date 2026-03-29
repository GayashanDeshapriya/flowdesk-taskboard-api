# Stage 1: runtime dependencies
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 2: build and publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowDesk.TaskBoard.Api/FlowDesk.TaskBoard.Api.csproj", "FlowDesk.TaskBoard.Api/"]
COPY ["FlowDesk.TaskBoard.Application/FlowDesk.TaskBoard.Application.csproj", "FlowDesk.TaskBoard.Application/"]
COPY ["FlowDesk.TaskBoard.Domain/FlowDesk.TaskBoard.Domain.csproj", "FlowDesk.TaskBoard.Domain/"]
COPY ["FlowDesk.TaskBoard.Infrastructure/FlowDesk.TaskBoard.Infrastructure.csproj", "FlowDesk.TaskBoard.Infrastructure/"]
RUN dotnet restore "FlowDesk.TaskBoard.Api/FlowDesk.TaskBoard.Api.csproj"
COPY . .
WORKDIR /src/FlowDesk.TaskBoard.Api
RUN dotnet publish "FlowDesk.TaskBoard.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: final image	
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FlowDesk.TaskBoard.Api.dll"]