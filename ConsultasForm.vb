Imports Microsoft.Data.Sqlite

Public Class ConsultasForm
    Inherits Form
    Private paciente As New ComboBox()
    Private medico As New ComboBox()
    Private motivo As New TextBox()
    Private diagnostico As New TextBox()
    Private tratamiento As New TextBox()
    Private observaciones As New TextBox()
    Private presion As New TextBox()
    Private temperatura As New TextBox()
    Private frecuencia As New TextBox()
    Private saturacion As New TextBox()
    Private peso As New TextBox()
    Private btn As New Button()
    Private grid As New DataGridView()

    Public Sub New()
        Id=i : Nombre=n : End Sub
        Public Overrides Function ToString() As String : Return Nombre : End Function
    End Class
End Class