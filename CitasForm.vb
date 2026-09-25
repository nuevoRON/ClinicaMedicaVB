Imports Microsoft.Data.Sqlite

Public Class CitasForm
    Inherits Form
    Private paciente As New ComboBox()
    Private medico As New ComboBox()
    Private fecha As New DateTimePicker()
    Private motivo As New TextBox()
    Private estado As New ComboBox()
    Private grid As New DataGridView()
    Private btn As New Button()

    Public Sub New()
        Id=i : Nombre=n : End Sub
        Public Overrides Function ToString() As String : Return Nombre : End Function
    End Class
End Class