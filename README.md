# Clínica Médica - VB.NET

Proyecto inicial funcional para Visual Studio 2022/2026 usando VB.NET Windows Forms y SQLite.

## Requisitos
- Windows
- Visual Studio con desarrollo de escritorio .NET
- .NET 8 SDK

## Ejecutar
1. Abra `ClinicaMedicaVB.vbproj` en Visual Studio.
2. Restaure los paquetes NuGet.
3. Ejecute con F5.
4. La base `clinica.db` se crea automáticamente.

## Usuario inicial
- Usuario: `admin`
- Contraseña: `admin123`

Cambie esta contraseña antes de usar el sistema en un entorno real.

## Módulos incluidos
- Inicio de sesión
- Pacientes
- Médicos
- Citas
- Consultas
- Base de datos local SQLite
- Reporte básico de cantidad de pacientes

## Próximas ampliaciones recomendadas
- Expediente clínico completo y búsqueda por paciente
- Recetas imprimibles
- Reportes PDF
- Usuarios y permisos granulares
- Auditoría de cambios
- Copias de seguridad automáticas
- Cifrado/protección de datos
- Instalador para Windows
- Migración a SQL Server si habrá varios equipos conectados simultáneamente

**Importante:** este proyecto es una base técnica. Para uso clínico real deben añadirse controles de seguridad, privacidad, respaldo, auditoría y validación de requisitos legales aplicables.