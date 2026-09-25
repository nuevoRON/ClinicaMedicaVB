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
CREATE TABLE IF NOT EXISTS RegistroAcciones(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FechaHora TEXT NOT NULL,
    Usuario TEXT NOT NULL,
    Rol TEXT NOT NULL,
    Accion TEXT NOT NULL,
    Modulo TEXT NOT NULL,
    Detalle TEXT
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
";
                cmd.ExecuteNonQuery()
            End Using

            Using adminCmd = cn.CreateCommand()
                adminCmd.CommandText = "INSERT OR IGNORE INTO Usuarios(Usuario, Clave, Rol) VALUES($u,$c,$r)"
                adminCmd.Parameters.AddWithValue("$u", "admin")
                adminCmd.Parameters.AddWithValue("$c", SecurityHelper.HashPassword("admin123"))
                adminCmd.Parameters.AddWithValue("$r", "Administrador")
                adminCmd.ExecuteNonQuery()
            End Using

            Using medicoCmd = cn.CreateCommand()
                medicoCmd.CommandText = "INSERT OR IGNORE INTO Medicos(Nombre, Especialidad, Colegiado) VALUES($n,$e,$c)"
                medicoCmd.Parameters.AddWithValue("$n", "Médico Demo")
                medicoCmd.Parameters.AddWithValue("$e", "Medicina General")
                medicoCmd.Parameters.AddWithValue("$c", "DEMO-001")
                medicoCmd.ExecuteNonQuery()
            End Using
"
                cmd.ExecuteNonQuery()
            End Using

            Using migracion = cn.CreateCommand()
                migracion.CommandText = "SELECT Id, Clave FROM Usuarios WHERE Clave NOT LIKE 'PBKDF2$%'"
                Using rd = migracion.ExecuteReader()
                    Dim pendientes As New List(Of Tuple(Of Integer, String))()
                    While rd.Read()
                        pendientes.Add(Tuple.Create(rd.GetInt32(0), rd.GetString(1)))
                    End While
                    rd.Close()
                    For Each item In pendientes
                        Using upd = cn.CreateCommand()
                            upd.CommandText = "UPDATE Usuarios SET Clave=$c WHERE Id=$id"
                            upd.Parameters.AddWithValue("$c", SecurityHelper.HashPassword(item.Item2))
                            upd.Parameters.AddWithValue("$id", item.Item1)
                            upd.ExecuteNonQuery()
                        End Using
                    Next
                End Using
            End Using
        End Using
    End Sub

    Public Sub RegistrarAccion(usuario As String, rol As String, accion As String, modulo As String, detalle As String)
        Try
            Using cn = Connection()
                Using cmd = cn.CreateCommand()
                    cmd.CommandText = "INSERT INTO RegistroAcciones(FechaHora,Usuario,Rol,Accion,Modulo,Detalle) VALUES($f,$u,$r,$a,$m,$d)"
                    cmd.Parameters.AddWithValue("$f", DateTime.Now.ToString("s"))
                    cmd.Parameters.AddWithValue("$u", If(String.IsNullOrWhiteSpace(usuario), "SISTEMA", usuario))
                    cmd.Parameters.AddWithValue("$r", If(String.IsNullOrWhiteSpace(rol), "SISTEMA", rol))
                    cmd.Parameters.AddWithValue("$a", accion)
                    cmd.Parameters.AddWithValue("$m", modulo)
                    cmd.Parameters.AddWithValue("$d", If(detalle, ""))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch
        End Try
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
        Dim cfg = BackupSettings.Cargar()
        Dim ruta = cfg.ObtenerCarpetaDestino()
        If Not Directory.Exists(ruta) Then Directory.CreateDirectory(ruta)
        Return ruta
    End Function

    Public Function CrearCopiaAutomatica() As String
        If Not File.Exists(DbPath) Then Return String.Empty

        Dim destino = Path.Combine(CarpetaCopias(), $"clinica_auto_{DateTime.Now:yyyyMMdd_HHmmss}.db")
        BackupDatabase(destino)
        LimpiarCopiasAntiguas()
        Return destino
    End Function

    Public Sub CrearCopiaDiariaSiCorresponde()
        If Not File.Exists(DbPath) Then Return

        Dim cfg = BackupSettings.Cargar()
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

        If (DateTime.Now.Date - ultimaFecha.Date).Days >= Math.Max(1, cfg.IntervaloDias) Then
            CrearCopiaAutomatica()
        Else
            LimpiarCopiasAntiguas()
        End If
    End Sub

    Public Sub LimpiarCopiasAntiguas()
        Dim cfg = BackupSettings.Cargar()
        Dim limite = Math.Max(1, cfg.CopiasMaximas)
        Dim carpeta = CarpetaCopias()

        Dim archivos = Directory.GetFiles(carpeta, "clinica_auto_*.db") _
            .OrderByDescending(Function(f) File.GetLastWriteTime(f)) _
            .ToList()

        If archivos.Count <= limite Then Return

        For i = limite To archivos.Count - 1
            Try
                File.Delete(archivos(i))
            Catch
            End Try
        Next
    End Sub
End Module