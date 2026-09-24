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
        Dim btnUsu As Button = CrearBoton("Usuarios",600,210)
        Dim btnCerrar As Button = CrearBoton("Cerrar sesión",600,300)

        AddHandler btnPac.Click, Sub() New PacientesForm().ShowDialog()
        AddHandler btnCit.Click, Sub() New CitasForm().ShowDialog()
        AddHandler btnCon.Click, Sub() New ConsultasForm().ShowDialog()
        AddHandler btnMed.Click, Sub() New MedicosForm().ShowDialog()
        AddHandler btnRec.Click, Sub() New RecetasForm().ShowDialog()
        AddHandler btnExp.Click, Sub() New ExpedienteForm().ShowDialog()
        AddHandler btnRep.Click, Sub() New ReportesForm().ShowDialog()
        AddHandler btnUsu.Click, Sub() New UsuariosForm().ShowDialog()
        AddHandler btnCerrar.Click, Sub() Close()

        ' Permisos por rol
        If Not rol.Equals("Administrador",StringComparison.OrdinalIgnoreCase) Then
            btnUsu.Enabled=False
            btnMed.Enabled=False
        End If

        If rol.Equals("Recepción",StringComparison.OrdinalIgnoreCase) Then
            btnCon.Enabled=False
            btnExp.Enabled=False
            btnRec.Enabled=False
            btnRep.Enabled=False
        End If

        Controls.AddRange({titulo,sesion,btnPac,btnCit,btnCon,btnRep,btnMed,btnExp,btnRec,btnUsu,btnCerrar})
    End Sub

    Private Function CrearBoton(t As String,x As Integer,y As Integer) As Button
        Return New Button With {.Text=t,.Left=x,.Top=y,.Width=150,.Height=55}
    End Function

End Class