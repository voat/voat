# Voat — multi-stage build for .NET 10.
#
# Build:  docker build -t voat .
# Run     see docker-compose.yml (app + PostgreSQL + Caddy) or INSTALL.md
#
# The image runs as the non-root "app" user that ships with the aspnet image.

# ---- Stage 1: build -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Full source tree (see .dockerignore for what is left out). The solution mixes
# C# and F# projects plus two vendored source projects under src/external, so
# the whole tree is copied and Voat.UI is published with its transitive graph.
COPY src/ src/

# /p:PublicSign=false: the vendored OpenGraph-Net project enables PublicSign on
# non-Windows, which the .NET 10 SDK rejects (CS8102) even with its .snk present.
# Nothing requires a strong-named OpenGraph-Net (SignAssembly is false), so the
# signing switch is pinned off instead of editing vendored source.
RUN dotnet publish src/Voat.UI/Voat.UI.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:PublicSign=false

# ---- Stage 2: runtime ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# libgssapi-krb5-2: Npgsql probes for the GSSAPI (Kerberos) library on first
# database connection and logs a scary "Cannot load library libgssapi_krb5.so.2"
# when it is missing. Harmless (password auth works regardless) but noisy in
# production logs; installing it also enables Kerberos auth for those who use it.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Writable locations the app needs at runtime:
#   /app/wwwroot/storage                      - uploaded avatars & thumbnails
#   /home/app/.aspnet/DataProtection-Keys     - auth cookie / antiforgery keys
# Both are named volumes in docker-compose.yml. The .keep files make the
# directories non-empty so a fresh named volume inherits their ownership.
RUN mkdir -p /app/wwwroot/storage/avatars /app/wwwroot/storage/thumbs \
        /home/app/.aspnet/DataProtection-Keys \
    && touch /home/app/.aspnet/DataProtection-Keys/.keep \
    && chown -R app:app /app /home/app/.aspnet

USER app
ENV HOME=/home/app \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "Voat.UI.dll"]
