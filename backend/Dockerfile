FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/StoryJudge.Api/StoryJudge.Api.csproj", "src/StoryJudge.Api/"]
COPY ["src/StoryJudge.Core/StoryJudge.Core.csproj", "src/StoryJudge.Core/"]
COPY ["src/StoryJudge.Infrastructure/StoryJudge.Infrastructure.csproj", "src/StoryJudge.Infrastructure/"]
RUN dotnet restore "src/StoryJudge.Api/StoryJudge.Api.csproj"

# Copy everything and build
COPY . .
RUN dotnet publish "src/StoryJudge.Api/StoryJudge.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run expects port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StoryJudge.Api.dll"]
