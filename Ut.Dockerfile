FROM mcr.microsoft.com/dotnet/sdk:6.0.408

ARG BUILD_CONFIGURATION
ARG PACKAGE_VERSION

WORKDIR /build

COPY test test
COPY src/ src
COPY .git .git
COPY *.sln ./
COPY StyleCop.ruleset ./
COPY NuGet.config ./

RUN dotnet restore && dotnet build --configuration Release

CMD find /build/test/ -mindepth 1 -maxdepth 1 -type d -name "*.Test" | xargs -P $(grep -c ^processor /proc/cpuinfo) -i sh -c  'cd "{}" && dotnet test -c Release --no-build -r /mnt/test/ --logger "trx;LogFileName=$(basename {}).trx"'
