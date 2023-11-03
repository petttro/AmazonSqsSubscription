FROM mcr.microsoft.com/dotnet/sdk:6.0.408

ARG ARTIFACTORY_USER
ARG ARTIFACTORY_PASSWORD
ARG PACKAGE_VERSION
ARG ASSEMBLY_VERSION
ARG BUILD_CONFIGURATION
ARG ARTIFACTORY_URL

WORKDIR /build

COPY test test
COPY src src
COPY deploy deploy
COPY .git .git
COPY *.sln ./
COPY StyleCop.ruleset ./
COPY logo.png ./
COPY NuGet.config ./

RUN dotnet restore && dotnet build -c "$BUILD_CONFIGURATION" -p:Version="$PACKAGE_VERSION" -p:AssemblyVersion="$ASSEMBLY_VERSION"

RUN dotnet pack -o Result --include-source -p:SymbolPackageFormat=snupkg --no-build /p:configuration="$BUILD_CONFIGURATION" -p:AssemblyVersion="$ASSEMBLY_VERSION" -p:Version="$PACKAGE_VERSION"

RUN cd deploy && \
    bash -e replace-in-xml nuget.config.in nuget.config \
      @REPO@ "$ARTIFACTORY_URL" \
      @USERNAME@ "$ARTIFACTORY_USER" \
      @PASSWORD@ "$ARTIFACTORY_PASSWORD" && \
    dotnet nuget push ../Result/*.nupkg -s repo

