FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV MINOMBRE_ES=Jesus
ENV MINOMBRE_ES2=Jesus2

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["HelloWorld.csproj", "./"]
RUN dotnet restore "HelloWorld.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "HelloWorld.csproj" -c ${configuration} -o /app/build

RUN ls

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "HelloWorld.csproj" -c ${configuration} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "HelloWorld.dll" ]