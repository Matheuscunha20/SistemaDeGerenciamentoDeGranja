# Receita para montar o AgroArca num container (usado na hospedagem).
# Montar:  docker build -t agroarca .

# 1) Compila e publica
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Primeiro só os projetos, para aproveitar o cache dos pacotes
COPY SistemaGranja.Domain/SistemaGranja.Domain.csproj SistemaGranja.Domain/
COPY SistemaGranja.Data/SistemaGranja.Data.csproj SistemaGranja.Data/
COPY SistemaGranja.Services/SistemaGranja.Services.csproj SistemaGranja.Services/
COPY SistemaGranja.Web/SistemaGranja.Web.csproj SistemaGranja.Web/
RUN dotnet restore SistemaGranja.Web/SistemaGranja.Web.csproj

COPY SistemaGranja.Domain/ SistemaGranja.Domain/
COPY SistemaGranja.Data/ SistemaGranja.Data/
COPY SistemaGranja.Services/ SistemaGranja.Services/
COPY SistemaGranja.Web/ SistemaGranja.Web/
RUN dotnet publish SistemaGranja.Web/SistemaGranja.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# 2) Imagem final, só com o necessário para rodar
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DataProtection__Pasta=/app/chaves \
    TZ=America/Sao_Paulo

# Pasta das chaves de login (montar um volume aqui para não perder ao atualizar)
RUN mkdir -p /app/chaves && chown app:app /app/chaves

COPY --from=build /app/publish .

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SistemaGranja.Web.dll"]
