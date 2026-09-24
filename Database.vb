Imports Microsoft.Data.Sqlite
Imports System.IO

Public Module Database
    Public ReadOnly DbPath As String =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clinica.db")

    Public Function Connection() As SqliteConnection
        Dim cn = New SqliteConnection($"Data Source={DbPath}")
        cn.Open()
        Return cn
    End Function

    Public Sub Initialize()
        Using cn = Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "
CREATE TABLE IF NOT EXISTS Usuarios(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Usuario TEXT NOT NULL UNIQUE,
    Clave TEXT NOT NULL,
    Rol TEXT NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS Pacientes(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Identidad TEXT NOT NULL UNIQUE,
    Nombre TEXT NOT NULL,
    Apellidos TEXT NOT NULL,
    FechaNacimiento TEXT,
    Sexo TEXT,
    Telefono TEXT,
    Direccion TEXT,
    ContactoEmergencia TEXT,
    TelefonoEmergencia TEXT,
    Alergias TEXT,
    Antecedentes TEXT,
    FechaRegistro TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS Medicos(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Especialidad TEXT,
    Colegiado TEXT,
    Telefono TEXT,
    Activo INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS Citas(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PacienteId INTEGER NOT NULL,
    MedicoId INTEGER,
    FechaHora TEXT NOT NULL,
    Motivo TEXT,
    Estado TEXT NOT NULL DEFAULT 'Pendiente',
    Observaciones TEXT,
    FOREIGN KEY(PacienteId) REFERENCES Pacientes(Id),
    FOREIGN KEY(MedicoId) REFERENCES Medicos(Id)
);
CREATE TABLE IF NOT EXISTS Consultas(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PacienteId INTEGER NOT NULL,
    MedicoId INTEGER,
    CitaId INTEGER,
    FechaHora TEXT NOT NULL,
    Motivo TEXT,
    Presion TEXT,
    Temperatura TEXT,
    FrecuenciaCardiaca TEXT,
    Saturacion TEXT,
    Peso TEXT,
    Diagnostico TEXT,
    Tratamiento TEXT,
    Observaciones TEXT,
    FOREIGN KEY(PacienteId) REFERENCES Pacientes(Id),
    FOREIGN KEY(MedicoId) REFERENCES Medicos(Id)
);
CREATE TABLE IF NOT EXISTS Recetas(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConsultaId INTEGER NOT NULL,
    Fecha TEXT NOT NULL,
    Medicamento TEXT NOT NULL,
    Dosis TEXT,
    Frecuencia TEXT,
    Duracion TEXT,
    Indicaciones TEXT,
    FOREIGN KEY(ConsultaId) REFERENCES Consultas(Id)
);
INSERT OR IGNORE INTO Usuarios(Usuario, Clave, Rol) VALUES('admin','admin123','Administrador');
INSERT OR IGNORE INTO Medicos(Nombre, Especialidad, Colegiado) VALUES('Médico Demo','Medicina General','DEMO-001');
"
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Sub BackupDatabase(destino As String)
        If String.IsNullOrWhiteSpace(destino) Then
            Throw New ArgumentException("Debe indicar el destino de la copia.", NameOf(destino))
        End If

        Dim destinoCompleto = Path.GetFullPath(destino)
        Dim carpeta = Path.GetDirectoryName(destinoCompleto)

        If Not String.IsNullOrWhiteSpace(carpeta) AndAlso Not Directory.Exists(carpeta) Then
            Directory.CreateDirectory(carpeta)
        End If

        If destinoCompleto.Equals(Path.GetFullPath(DbPath), StringComparison.OrdinalIgnoreCase) Then
            Throw New IOException("El destino no puede ser la base de datos activa.")
        End If

        Using origen As SqliteConnection = Connection()
            Using destinoCn As New SqliteConnection($"Data Source={destinoCompleto}")
                destinoCn.Open()
                origen.BackupDatabase(destinoCn)
            End Using
        End Using
    End Sub

    Public Function CarpetaCopias() As String
        Dim ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups")
        If Not Directory.Exists(ruta) Then Directory.CreateDirectory(ruta)
        Return ruta
    End Function

    Public Function CrearCopiaAutomatica() As String
        If Not File.Exists(DbPath) Then Return String.Empty

        Dim destino = Path.Combine(CarpetaCopias(), $"clinica_auto_{DateTime.Now:yyyyMMdd_HHmmss}.db")
        BackupDatabase(destino)
        Return destino
    End Function

    Public Sub CrearCopiaDiariaSiCorresponde()
        If Not File.Exists(DbPath) Then Return

        Dim carpeta = CarpetaCopias()
        Dim archivos = Directory.GetFiles(carpeta, "clinica_auto_*.db")

        If archivos.Length = 0 Then
            CrearCopiaAutomatica()
            Return
        End If

        Dim ultimaFecha As DateTime = DateTime.MinValue

        For Each archivo In archivos
            Dim fecha = File.GetLastWriteTime(archivo)
            If fecha > ultimaFecha Then ultimaFecha = fecha
        Next

        If ultimaFecha.Date < DateTime.Now.Date Then
            CrearCopiaAutomatica()
        End If
    End Sub
End Module