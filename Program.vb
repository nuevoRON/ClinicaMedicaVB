Imports System
Imports System.Windows.Forms

Module Program
    <STAThread>
    Public Sub Main()
        ApplicationConfiguration.Initialize()
        Database.Initialize()
        Application.Run(New LoginForm())
    End Sub
End Module