Imports System.Drawing

Public Class MainForm
    Inherits Form
    Private usuario As String
    Private rol As String

    Public Sub New(usuario As String, rol As String)
        Me.usuario=usuario : Me.rol=rol
        Text = "Clínica Médica - Panel principal"
        Width=900 : Height=600
        StartPosition=FormStartPosition.CenterScreen
        Dim titulo As New Label With {.Text="SISTEMA DE GESTIÓN DE CLÍNICA MÉDICA",.Font=New Font("Segoe UI",18,FontStyle.Bold),.AutoSize=True,.Left=30,.Top=25}
        Dim sesion As New Label With {.Text=$"Usuario: {usuario} | Rol: {rol}",.AutoSize=True,.Left=32,.Top=70}
        Dim btnPac As Button = CrearBoton("Pacientes",30,120)
        Dim btnCit As Button = CrearBoton("Citas",220,120)
        Dim btnCon As Button = CrearBoton("Consultas",410,120)
        Dim btnRep As Button = CrearBoton("Reportes",600,120)
        Dim btnMed As Button = CrearBoton("Médicos",30,210)
        Dim btnExp As Button = CrearBoton("Expediente clínico",220,210)
        Dim btnRec As Button = CrearBoton("Recetas",410,210)
        Dim btnCerrar As Button = CrearBoton("Cerrar sesión",600,210)
        AddHandler btnPac.Click, Sub() New PacientesForm().ShowDialog()
        AddHandler btnCit.Click, Sub() New CitasForm().ShowDialog()
        AddHandler btnCon.Click, Sub() New ConsultasForm().ShowDialog()
        AddHandler btnMed.Click, Sub() New MedicosForm().ShowDialog()
        AddHandler btnRec.Click, Sub() New RecetasForm().ShowDialog()
        AddHandler btnExp.Click, Sub() New ExpedienteForm().ShowDialog()
        AddHandler btnRep.Click, AddressOf Reportes
        AddHandler btnCerrar.Click, Sub() Close()
        Controls.AddRange({titulo,sesion,btnPac,btnCit,btnCon,btnRep,btnMed,btnExp,btnRec,btnCerrar})
    End Sub

    Private Function CrearBoton(t As String,x As Integer,y As Integer) As Button
        Return New Button With {.Text=t,.Left=x,.Top=y,.Width=150,.Height=55}
    End Function

    Private Sub Reportes(sender As Object,e As EventArgs)
        Dim totalPac As Integer
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT COUNT(*) FROM Pacientes"
                totalPac=Convert.ToInt32(cmd.ExecuteScalar())
            End Using
        End Using
        MessageBox.Show($"Pacientes registrados: {totalPac}" & Environment.NewLine & "Módulo de reportes preparado para ampliarse.","Reportes")
    End Sub
End Class