FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /src
COPY . .
WORKDIR /src/src
RUN dotnet restore Explorer.API/Explorer.API.csproj
RUN dotnet build Explorer.API/Explorer.API.csproj -c Release

FROM build as publish
RUN dotnet publish Explorer.API/Explorer.API.csproj -c Release -o /app/publish

ENV ASPNETCORE_URLS=http://+:80
FROM base AS final
COPY --from=publish /app .
WORKDIR /app/publish
CMD ["dotnet", "Explorer.API.dll"]

FROM build as migration-base
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 7.*

FROM migration-base AS execute-migration

ENV STARTUP_PROJECT=Explorer.API
ENV MIGRATION=init
ENV DATABASE_SCHEMA=""
ENV DATABASE_HOST=""
ENV DATABASE_PASSWORD=""
ENV DATABASE_USERNAME=""

ENV STAKEHOLDERS_TARGET_PROJECT=Explorer.Stakeholders.Infrastructure

ENV PAYMENTS_TARGET_PROJECT=Explorer.Payments.Infrastructure


CMD PATH="$PATH:/root/.dotnet/tools" \
    dotnet-ef migrations add "${MIGRATION}-stakeholders" \
        -s "${STARTUP_PROJECT}/${STARTUP_PROJECT}.csproj" \
        -p "Modules/Stakeholders/${STAKEHOLDERS_TARGET_PROJECT}/${STAKEHOLDERS_TARGET_PROJECT}.csproj" \
        -c "StakeholdersContext" \
        --configuration Release && \
    PATH="$PATH:/root/.dotnet/tools" \   
    dotnet-ef database update "${MIGRATION}-stakeholders" \
        -s "${STARTUP_PROJECT}/${STARTUP_PROJECT}.csproj" \
        -p "Modules/Stakeholders/${STAKEHOLDERS_TARGET_PROJECT}/${STAKEHOLDERS_TARGET_PROJECT}.csproj" \
        -c "StakeholdersContext" \
        --configuration Release && \
    dotnet-ef migrations add "${MIGRATION}-payments" \
        -s "${STARTUP_PROJECT}/${STARTUP_PROJECT}.csproj" \
        -p "Modules/Payments/${PAYMENTS_TARGET_PROJECT}/${PAYMENTS_TARGET_PROJECT}.csproj" \
        -c "PaymentsContext" \
    --configuration Release && \
    dotnet-ef database update "${MIGRATION}-payments" \
        -s "${STARTUP_PROJECT}/${STARTUP_PROJECT}.csproj" \
        -p "Modules/Payments/${PAYMENTS_TARGET_PROJECT}/${PAYMENTS_TARGET_PROJECT}.csproj" \
        -c "PaymentsContext" \
    --configuration Release 