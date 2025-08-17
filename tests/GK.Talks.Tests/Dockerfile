FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

COPY ["tests/GK.Talks.Tests/GK.Talks.Tests.csproj", "tests/GK.Talks.Tests/"]
COPY ["src/GK.Talks.Api/GK.Talks.Api.csproj", "src/GK.Talks.Api/"]
COPY ["src/GK.Talks.Application/GK.Talks.Application.csproj", "src/GK.Talks.Application/"]
COPY ["src/GK.Talks.Domain/GK.Talks.Domain.csproj", "src/GK.Talks.Domain/"]
COPY ["src/GK.Talks.Infrastructure/GK.Talks.Infrastructure.csproj", "src/GK.Talks.Infrastructure/"]

RUN dotnet restore "tests/GK.Talks.Tests/GK.Talks.Tests.csproj"

COPY . .

WORKDIR "/src/tests/GK.Talks.Tests"

ENTRYPOINT ["dotnet", "test", "--no-restore", "--logger:console;verbosity=detailed", "--results-directory:/testresults"]

VOLUME ["/testresults"]