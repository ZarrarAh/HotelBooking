# --- Build stage: full SDK, compiles and publishes the API ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only project files first so `dotnet restore` is cached until dependencies change.
COPY global.json ./
COPY src/HotelBooking.Domain/HotelBooking.Domain.csproj src/HotelBooking.Domain/
COPY src/HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj src/HotelBooking.Infrastructure/
COPY src/HotelBooking.Api/HotelBooking.Api.csproj src/HotelBooking.Api/
RUN dotnet restore src/HotelBooking.Api/HotelBooking.Api.csproj

COPY src/ src/
RUN dotnet publish src/HotelBooking.Api/HotelBooking.Api.csproj -c Release -o /app/publish --no-restore

# --- Runtime stage: small ASP.NET-only image ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
# Built-in non-root user provided by the .NET images.
USER $APP_UID
ENTRYPOINT ["dotnet", "HotelBooking.Api.dll"]
