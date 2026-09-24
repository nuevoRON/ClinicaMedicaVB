Imports System
Imports System.Windows.Forms

Module Program
    <STAThread>
    Public Sub Main()
        ApplicationConfiguration.Initialize()
        Database.Initialize()

        Try
            Database.CrearCopiaDiariaSiCorresponde()
        Catch
            ' Un fallo de respaldo no debe impedir el arranque de la clínica.
        End Try

        AddHandler Application.ApplicationExit, AddressOf RealizarCopiaAlCerrar
        Application.Run(New LoginForm())
    End Sub

    Private Sub RealizarCopiaAlCerrar(sender As Object, e As EventArgs)
        Try
            Database.CrearCopiaAutomatica()
        Catch
            ' El cierre de la aplicación no debe bloquearse por un fallo de respaldo.
        End Try
    End Sub
End Module