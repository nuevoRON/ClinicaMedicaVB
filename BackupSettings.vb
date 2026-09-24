Imports System.IO
Imports System.Text.Json

Public Class BackupSettings
    Public Property IntervaloDias As Integer = 1
    Public Property CopiasMaximas As Integer = 10
    Public Property CarpetaDestino As String = ""

    Private Shared ReadOnly ConfigPath As String =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup_config.json")

    Public Shared Function Cargar() As BackupSettings
        Try
            If File.Exists(ConfigPath) Then
                Dim json = File.ReadAllText(ConfigPath)
                Dim cfg = JsonSerializer.Deserialize(Of BackupSettings)(json)
                If cfg IsNot Nothing Then Return cfg
            End If
        Catch
        End Try

        Return New BackupSettings()
    End Function

    Public Sub Guardar()
        Dim json = JsonSerializer.Serialize(Me, New JsonSerializerOptions With {.WriteIndented = True})
        File.WriteAllText(ConfigPath, json)
    End Sub

    Public Function ObtenerCarpetaDestino() As String
        If String.IsNullOrWhiteSpace(CarpetaDestino) Then
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups")
        End If

        Return CarpetaDestino
    End Function
End Class