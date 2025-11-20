# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj e restaurar dependências
COPY *.csproj ./
RUN dotnet restore

# Copiar resto do projeto
COPY . .

# Build da aplicação
RUN dotnet publish -c Release -o /app

# Imagem final
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Copiar arquivos compilados
COPY --from=build /app .

# Copiar appsettings.json (necessário em runtime)
COPY appsettings.json .

ENTRYPOINT ["dotnet", "stock-quote-alert.dll"]
