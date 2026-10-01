Imports System.Drawing
Imports System.Windows.Forms

Public Class MainForm
    Inherits Form

    Private usuario As String
    Private rol As String

    Private ReadOnly Fondo As Color = Color.FromArgb(244, 247, 251)
    Private ReadOnly Azul As Color = Color.FromArgb(35, 99, 160)
    Private ReadOnly Tinta As Color = Color.FromArgb(42, 54, 72)

    Public Sub New(usuario As String, rol As String)
        Me.usuario = usuario
        Me.rol = rol

        Text = "Clínica Médica - Panel principal"
        ClientSize = New Size(940, 650)
        MinimumSize = New Size(820, 600)
        StartPosition = FormStartPosition.CenterScreen
        BackColor = Fondo
        Font = New Font("Segoe UI", 9.0F)

        Dim encabezado As New Panel With {
            .Dock = DockStyle.Top,
            .Height = 132,
            .BackColor = Color.FromArgb(25, 55, 88),
            .Padding = New Padding(30, 20, 30, 15)
        }

        Dim titulo As New Label With {
            .Text = "CLÍNICA MÉDICA",
            .Font = New Font("Segoe UI Semibold", 21.0F),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Location = New Point(28, 22)
        }

        Dim subtitulo As New Label With {
            .Text = "Sistema de gestión y administración clínica",
            .Font = New Font("Segoe UI", 10.0F),
            .ForeColor = Color.FromArgb(210, 225, 240),
            .AutoSize = True,
            .Location = New Point(31, 62)
        }

        Dim sesion As New Label With {
            .Text = $"Sesión: {usuario}   |   Perfil: {rol}",
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.White,
            .AutoSize = True,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
            .Location = New Point(650, 38)
        }

        encabezado.Controls.AddRange({titulo, subtitulo, sesion})
        Controls.Add(encabezado)

        Dim etiquetaModulos As New Label With {
            .Text = "Módulos del sistema",
            .Font = New Font("Segoe UI Semibold", 14.0F),
            .ForeColor = Tinta,
            .AutoSize = True,
            .Location = New Point(34, 154)
        }
        Controls.Add(etiquetaModulos)

        Dim contenedor As New FlowLayoutPanel With {
            .Location = New Point(28, 190),
            .Size = New Size(870, 350),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Fondo,
            .WrapContents = True,
            .AutoScroll = True,
            .Padding = New Padding(4),
            .FlowDirection = FlowDirection.LeftToRight
        }
        Controls.Add(contenedor)

        Dim btnPac = CrearBoton("Pacientes", "Registro y búsqueda")
        Dim btnCit = CrearBoton("Citas", "Programación y seguimiento")
        Dim btnCon = CrearBoton("Consultas", "Atención médica")
        Dim btnRep = CrearBoton("Reportes", "Consultas e impresión")
        Dim btnMed = CrearBoton("Médicos", "Directorio profesional")
        Dim btnExp = CrearBoton("Expediente clínico", "Historial del paciente")
        Dim btnRec = CrearBoton("Recetas", "Registro e impresión")
        Dim btnUsu = CrearBoton("Usuarios", "Cuentas y permisos")
        Dim btnBack = CrearBoton("Copias de seguridad", "Respaldo y restauración")
        Dim btnBit = CrearBoton("Bitácora", "Registro de actividad")

        AddHandler btnPac.Click, Sub()
                                     Using f As New PacientesForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnCit.Click, Sub()
                                     Using f As New CitasForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnCon.Click, Sub()
                                     Using f As New ConsultasForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnMed.Click, Sub()
                                     Using f As New MedicosForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnRec.Click, Sub()
                                     Using f As New RecetasForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnExp.Click, Sub()
                                     Using f As New ExpedienteForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnRep.Click, Sub()
                                     Using f As New ReportesForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnUsu.Click, Sub()
                                     Using f As New UsuariosForm()
                                         f.ShowDialog(Me)
                                     End Using
                                 End Sub
        AddHandler btnBack.Click, Sub()
                                      Using f As New BackupForm()
                                          f.ShowDialog(Me)
                                      End Using
                                  End Sub
        AddHandler btnBit.Click, Sub()
                                     Using f As New BitacoraForm()
                                         f.ShowDialog(Me)
                                     End Using
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

        contenedor.Controls.AddRange({btnPac, btnCit, btnCon, btnRep, btnMed, btnExp, btnRec, btnUsu, btnBack, btnBit})

        Dim btnCerrar As New Button With {
            .Text = "Cerrar sesión",
            .Size = New Size(170, 40),
            .Location = New Point(34, 566),
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Left,
            .BackColor = Color.White,
            .ForeColor = Azul,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI Semibold", 9.5F),
            .Cursor = Cursors.Hand
        }
        btnCerrar.FlatAppearance.BorderColor = Azul
        btnCerrar.FlatAppearance.BorderSize = 1
        AddHandler btnCerrar.Click, Sub() Close()
        Controls.Add(btnCerrar)
    End Sub

    Private Function CrearBoton(titulo As String, descripcion As String) As Button
        Dim b As New Button With {
            .Text = titulo & Environment.NewLine & descripcion,
            .Size = New Size(202, 82),
            .Margin = New Padding(7),
            .BackColor = Color.White,
            .ForeColor = Tinta,
            .FlatStyle = FlatStyle.Flat,
            .Font = New Font("Segoe UI Semibold", 10.0F),
            .TextAlign = ContentAlignment.MiddleCenter,
            .Cursor = Cursors.Hand
        }
        b.FlatAppearance.BorderColor = Color.FromArgb(205, 216, 228)
        b.FlatAppearance.BorderSize = 1
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(231, 241, 251)
        Return b
    End Function
End Class
