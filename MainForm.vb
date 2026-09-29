Imports System.Drawing

Public Class MainForm
    Inherits Form
    Private usuario As String
    Private rol As String

    Public Sub New(usuario As String, rol As String)
        Me.usuario = usuario
        Me.rol = rol
        Text = "Clínica Médica - Panel principal"
        Width = 900
        Height = 600
        StartPosition = FormStartPosition.CenterScreen

        Dim titulo As New Label With {.Text = "SISTEMA DE GESTIÓN DE CLÍNICA MÉDICA", .Font = New Font("Segoe UI", 18, FontStyle.Bold), .AutoSize = True, .Left = 30, .Top = 25}
        Dim sesion As New Label With {.Text = $"Usuario: {usuario} | Rol: {rol}", .AutoSize = True, .Left = 32, .Top = 70}

        Dim btnPac As Button = CrearBoton("Pacientes", 30, 120)
        Dim btnCit As Button = CrearBoton("Citas", 220, 120)
        Dim btnCon As Button = CrearBoton("Consultas", 410, 120)
        Dim btnRep As Button = CrearBoton("Reportes", 600, 120)
        Dim btnMed As Button = CrearBoton("Médicos", 30, 210)
        Dim btnExp As Button = CrearBoton("Expediente clínico", 220, 210)
        Dim btnRec As Button = CrearBoton("Recetas", 410, 210)
        Dim btnUsu As Button = CrearBoton("Usuarios", 600, 210)
        Dim btnBack As Button = CrearBoton("Copias seguridad", 30, 300)
        Dim btnBit As Button = CrearBoton("Bitácora", 220, 300)
        Dim btnCerrar As Button = CrearBoton("Cerrar sesión", 600, 300)

        AddHandler btnPac.Click, Sub()
                                      Using f As New PacientesForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnCit.Click, Sub()
                                      Using f As New CitasForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnCon.Click, Sub()
                                      Using f As New ConsultasForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnMed.Click, Sub()
                                      Using f As New MedicosForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnRec.Click, Sub()
                                      Using f As New RecetasForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnExp.Click, Sub()
                                      Using f As New ExpedienteForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnRep.Click, Sub()
                                      Using f As New ReportesForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnUsu.Click, Sub()
                                      Using f As New UsuariosForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnBack.Click, Sub()
                                      Using f As New BackupForm()
                                          f.ShowDialog()
                                      End Using
                                  End Sub
        AddHandler btnBit.Click, Sub()
                                     Using f As New BitacoraForm()
                                         f.ShowDialog()
                                     End Using
                                 End Sub
        AddHandler btnCerrar.Click, Sub()
                                         Close()
                                     End Sub

        If Not rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) Then
            btnUsu.Enabled = False
            btnMed.Enabled = False
            btnBack.Enabled = False
            btnBit.Enabled = False
        End If

        If rol.Equals("Recepción", StringComparison.OrdinalIgnoreCase) Then
            btnCon.Enabled = False
            btnExp.Enabled = False
            btnRec.Enabled = False
            btnRep.Enabled = False
        End If

        Controls.AddRange({titulo, sesion, btnPac, btnCit, btnCon, btnRep, btnMed, btnExp, btnRec, btnUsu, btnBack, btnBit, btnCerrar})
    End Sub

    Private Function CrearBoton(t As String, x As Integer, y As Integer) As Button
        Return New Button With {.Text = t, .Left = x, .Top = y, .Width = 150, .Height = 55}
    End Function
End Class