@"
# Usar la imagen oficial de .NET 9 SDK para construir la app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copiar los archivos del proyecto
COPY . .

# Restaurar dependencias y publicar
RUN dotnet restore "./LAB14-Mijael_Juy/LAB14-Mijael_Juy.csproj"
RUN dotnet publish "./LAB14-Mijael_Juy/LAB14-Mijael_Juy.csproj" -c Release -o /app/publish

# Crear la imagen final para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto que usa Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Comando para iniciar la app
ENTRYPOINT ["dotnet", "LAB14-Mijael_Juy.dll"]
"@ | Out-File -FilePath Dockerfile -Encoding UTF8
```

Una vez que ejecutes eso, no olvides subirlo a GitHub con estos tres comandos para que Render pueda encontrarlo:

```powershell
git add .
git commit -m "Agregando Dockerfile"
git push