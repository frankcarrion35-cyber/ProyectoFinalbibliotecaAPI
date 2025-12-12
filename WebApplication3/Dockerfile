# Usa una imagen base de .NET SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Establece el directorio de trabajo
WORKDIR /app

# Copia los archivos del proyecto
COPY ["ProyectoFinalbibliotecaAPI/ProyectoFinalbibliotecaAPI.csproj", "ProyectoFinalbibliotecaAPI/"]

# Restaura las dependencias
RUN dotnet restore "ProyectoFinalbibliotecaAPI/ProyectoFinalbibliotecaAPI.csproj"

# Copia todo el código fuente
COPY . .

# Publica el proyecto (compilación en modo release)
RUN dotnet publish "ProyectoFinalbibliotecaAPI/ProyectoFinalbibliotecaAPI.csproj" -c Release -o /app/publish

# Usa la imagen base de .NET Runtime para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Copia los archivos publicados
COPY --from=build /app/publish .

# Define el comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "ProyectoFinalbibliotecaAPI.dll"]
