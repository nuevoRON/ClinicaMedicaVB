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
End Module