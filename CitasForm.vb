Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class CitasForm
    Inherits Form

    Private paciente As New ComboBox()
    Private medico As New ComboBox()
    Private fecha As New DateTimePicker()
    Private motivo As New TextBox()
    Private estado As New ComboBox()
    Private grid As New DataGridView()
    Private btn As New Button()

    Private ReadOnly ColorFondo As Color = Color.FromArgb(245, 247, 250)
    Private ReadOnly ColorPanel As Color = Color.White
    Private ReadOnly ColorPrimario As Color = Color.FromArgb(35, 99, 160)
    Private ReadOnly ColorTexto As Color = Color.FromArgb(45, 55, 72)
    Private ReadOnly ColorBorde As Color = Color.FromArgb(185, 195, 207)
    Private ReadOnly ColorBordeActivo As Color = Color.FromArgb(35, 99, 160)

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Citas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Citas", "Citas")
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)

            AddHandler Me.Load,
                Sub(sender As Object, e As EventArgs)
                    Me.Close()
                End Sub
            Return
        End If

        Text = "Clínica Médica - Gestión de citas"
        Width = 1080
        Height = 680
        MinimumSize = New Size(980, 620)
        StartPosition = FormStartPosition.CenterParent
        BackColor = ColorFondo
        Font = New Font("Segoe UI", 9.0F)
        Padding = New Padding(18)

        ConstruirInterfaz()
        CargarCombos()

        ' Permite programar citas desde hoy hasta cinco años en el futuro.
        fecha.MinDate = DateTime.Today
        fecha.MaxDate = DateTime.Today.AddYears(5)
        fecha.Value = ObtenerProximaFechaDisponible()

        Cargar()
    End Sub

    Private Sub ConstruirInterfaz()
        Dim titulo As New Label With {
            .Text = "Gestión de citas",
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 18.0F),
            .ForeColor = ColorTexto,
            .Location = New Point(18, 12)
        }
        Controls.Add(titulo)

        Dim subtitulo As New Label With {
            .Text = "Registre y consulte las citas programadas de la clínica.",
            .AutoSize = True,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(100, 110, 125),
            .Location = New Point(20, 45)
        }
        Controls.Add(subtitulo)

        Dim panelRegistro As New Panel With {
            .BackColor = ColorPanel,
            .Location = New Point(18, 78),
            .Size = New Size(390, 510),
            .BorderStyle = BorderStyle.FixedSingle
        }
        Controls.Add(panelRegistro)

        Dim encabezado As New Label With {
            .Text = "Nueva cita",
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 12.0F),
            .ForeColor = ColorPrimario,
            .Location = New Point(22, 18)
        }
        panelRegistro.Controls.Add(encabezado)

        CrearEtiqueta(panelRegistro, "Paciente", 22, 62)
        CrearCampoEnmarcado(panelRegistro, paciente, 22, 84, 340, 34)
        paciente.DropDownStyle = ComboBoxStyle.DropDownList
        paciente.FlatStyle = FlatStyle.Flat
        paciente.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Médico", 22, 126)
        CrearCampoEnmarcado(panelRegistro, medico, 22, 148, 340, 34)
        medico.DropDownStyle = ComboBoxStyle.DropDownList
        medico.FlatStyle = FlatStyle.Flat
        medico.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Fecha y hora", 22, 190)
        CrearCampoEnmarcado(panelRegistro, fecha, 22, 212, 340, 34)
        fecha.Format = DateTimePickerFormat.Custom
        fecha.CustomFormat = "dd/MM/yyyy HH:mm"
        fecha.ShowUpDown = False
        fecha.Font = New Font("Segoe UI", 9.5F)
        fecha.MinDate = DateTime.Today
        fecha.MaxDate = DateTime.Today.AddYears(5)
        fecha.Value = ObtenerProximaFechaDisponible()

        Dim ayudaFecha As New Label With {
            .Text = "Seleccione una fecha futura en el calendario.",
            .AutoSize = True,
            .Font = New Font("Segoe UI", 8.0F),
            .ForeColor = Color.FromArgb(105, 115, 130),
            .Location = New Point(24, 247)
        }
        panelRegistro.Controls.Add(ayudaFecha)

        CrearEtiqueta(panelRegistro, "Motivo de la cita", 22, 274)
        CrearCampoEnmarcado(panelRegistro, motivo, 22, 296, 340, 64)
        motivo.Multiline = True
        motivo.ScrollBars = ScrollBars.Vertical
        motivo.Font = New Font("Segoe UI", 9.5F)
        motivo.Padding = New Padding(5, 4, 5, 4)

        CrearEtiqueta(panelRegistro, "Estado", 22, 374)
        CrearCampoEnmarcado(panelRegistro, estado, 22, 396, 340, 34)
        estado.DropDownStyle = ComboBoxStyle.DropDownList
        estado.FlatStyle = FlatStyle.Flat
        estado.Font = New Font("Segoe UI", 9.5F)
        estado.Items.AddRange({"Pendiente", "Confirmada", "Atendida", "Cancelada"})
        estado.SelectedIndex = 0

        btn.Text = "Guardar cita"
        btn.SetBounds(22, 454, 340, 42)
        btn.BackColor = ColorPrimario
        btn.ForeColor = Color.White
        btn.FlatStyle = FlatStyle.Flat
        btn.FlatAppearance.BorderSize = 0
        btn.Font = New Font("Segoe UI Semibold", 10.0F)
        btn.Cursor = Cursors.Hand
        panelRegistro.Controls.Add(btn)
        AddHandler btn.Click, AddressOf Guardar

        Dim panelListado As New Panel With {
            .BackColor = ColorPanel,
            .Location = New Point(426, 78),
            .Size = New Size(Width - 462, 510),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .BorderStyle = BorderStyle.FixedSingle
        }
        Controls.Add(panelListado)

        Dim tituloListado As New Label With {
            .Text = "Citas registradas",
            .AutoSize = True,
            .Font = New Font("Segoe UI Semibold", 12.0F),
            .ForeColor = ColorTexto,
            .Location = New Point(18, 18)
        }
        panelListado.Controls.Add(tituloListado)

        grid.SetBounds(18, 52, panelListado.Width - 36, panelListado.Height - 70)
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        grid.GridColor = Color.FromArgb(210, 216, 224)
        grid.RowHeadersVisible = False
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersHeight = 38
        grid.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {
            .BackColor = Color.FromArgb(238, 242, 247),
            .ForeColor = ColorTexto,
            .Font = New Font("Segoe UI Semibold", 9.0F),
            .Alignment = DataGridViewContentAlignment.MiddleLeft
        }
        grid.DefaultCellStyle = New DataGridViewCellStyle With {
            .BackColor = Color.White,
            .ForeColor = ColorTexto,
            .SelectionBackColor = Color.FromArgb(220, 235, 250),
            .SelectionForeColor = Color.FromArgb(25, 55, 85),
            .Font = New Font("Segoe UI", 9.0F),
            .Padding = New Padding(4, 2, 4, 2)
        }
        grid.RowTemplate.Height = 32
        panelListado.Controls.Add(grid)
    End Sub

    Private Sub CrearEtiqueta(contenedor As Control, texto As String, x As Integer, y As Integer)
        Dim etiqueta As New Label With {
            .Text = texto,
            .AutoSize = True,
            .Location = New Point(x, y),
            .ForeColor = ColorTexto,
            .Font = New Font("Segoe UI Semibold", 9.0F)
        }
        contenedor.Controls.Add(etiqueta)
    End Sub

    Private Sub CrearCampoEnmarcado(contenedor As Control, control As Control,
                                    x As Integer, y As Integer, ancho As Integer, alto As Integer)

        Dim marco As New Panel With {
            .BackColor = ColorBorde,
            .Location = New Point(x, y),
            .Size = New Size(ancho, alto),
            .Padding = New Padding(1),
            .Tag = control
        }

        control.Dock = DockStyle.Fill
        control.Margin = New Padding(0)
        control.BackColor = Color.White

        If TypeOf control Is TextBox Then
            DirectCast(control, TextBox).BorderStyle = BorderStyle.None
        ElseIf TypeOf control Is ComboBox Then
            DirectCast(control, ComboBox).FlatStyle = FlatStyle.Flat
        End If

        marco.Controls.Add(control)
        contenedor.Controls.Add(marco)

        AddHandler control.Enter,
            Sub(sender As Object, e As EventArgs)
                marco.BackColor = ColorBordeActivo
            End Sub

        AddHandler control.Leave,
            Sub(sender As Object, e As EventArgs)
                marco.BackColor = ColorBorde
            End Sub
    End Sub

    Private Function ObtenerProximaFechaDisponible() As DateTime
        Dim propuesta = DateTime.Now.AddMinutes(30)

        ' Redondea a intervalos de 30 minutos para facilitar la programación.
        Dim minutos = propuesta.Minute
        Dim minutosRedondeados = ((minutos + 29) \ 30) * 30

        If minutosRedondeados >= 60 Then
            propuesta = New DateTime(propuesta.Year, propuesta.Month, propuesta.Day,
                                     propuesta.Hour, 0, 0).AddHours(1)
        Else
            propuesta = New DateTime(propuesta.Year, propuesta.Month, propuesta.Day,
                                     propuesta.Hour, minutosRedondeados, 0)
        End If

        If propuesta < DateTime.Now Then
            propuesta = DateTime.Now.AddHours(1)
        End If

        If propuesta < DateTime.Today Then
            propuesta = DateTime.Today
        End If

        Return propuesta
    End Function

    Private Sub CargarCombos()
        paciente.Items.Clear()
        medico.Items.Clear()

        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT Id,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos"
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        paciente.Items.Add(New ComboItem(CInt(rd("Id")), rd("Nombre").ToString()))
                    End While
                End Using
            End Using

            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT Id,Nombre FROM Medicos WHERE Activo=1 ORDER BY Nombre"
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        medico.Items.Add(New ComboItem(CInt(rd("Id")), rd("Nombre").ToString()))
                    End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(sender As Object, e As EventArgs)
        If paciente.SelectedItem Is Nothing Then
            MessageBox.Show("Seleccione un paciente.", "Validación",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            paciente.Focus()
            Return
        End If

        If fecha.Value < DateTime.Now Then
            MessageBox.Show("La fecha y hora de la cita no puede ser anterior al momento actual.",
                            "Fecha no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            fecha.Focus()
            Return
        End If

        Dim p = DirectCast(paciente.SelectedItem, ComboItem)
        Dim mid As Object = DBNull.Value

        If medico.SelectedItem IsNot Nothing Then
            mid = DirectCast(medico.SelectedItem, ComboItem).Id
        End If

        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "INSERT INTO Citas(PacienteId,MedicoId,FechaHora,Motivo,Estado) VALUES($p,$m,$f,$mo,$e)"
                cmd.Parameters.AddWithValue("$p", p.Id)
                cmd.Parameters.AddWithValue("$m", mid)
                cmd.Parameters.AddWithValue("$f", fecha.Value.ToString("s"))
                cmd.Parameters.AddWithValue("$mo", motivo.Text.Trim())
                cmd.Parameters.AddWithValue("$e", estado.Text)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        Database.RegistrarAccion(
            Session.CurrentUser,
            Session.CurrentRole,
            "Registro de cita",
            "Citas",
            "PacienteId=" & p.Id.ToString()
        )

        MessageBox.Show("Cita registrada correctamente.", "Clínica Médica",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)

        motivo.Clear()
        estado.SelectedIndex = 0
        fecha.Value = ObtenerProximaFechaDisponible()
        Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()

        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText =
                    "SELECT C.Id, " &
                    "P.Nombre||' '||P.Apellidos AS Paciente, " &
                    "M.Nombre AS Medico, C.FechaHora, C.Motivo, C.Estado " &
                    "FROM Citas C " &
                    "JOIN Pacientes P ON P.Id=C.PacienteId " &
                    "LEFT JOIN Medicos M ON M.Id=C.MedicoId " &
                    "ORDER BY C.FechaHora DESC"

                Using rd = cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using

        grid.DataSource = dt

        If grid.Columns.Contains("Id") Then
            grid.Columns("Id").HeaderText = "ID"
            grid.Columns("Id").FillWeight = 35
        End If

        If grid.Columns.Contains("Paciente") Then
            grid.Columns("Paciente").HeaderText = "Paciente"
            grid.Columns("Paciente").FillWeight = 115
        End If

        If grid.Columns.Contains("Medico") Then
            grid.Columns("Medico").HeaderText = "Médico"
            grid.Columns("Medico").FillWeight = 105
        End If

        If grid.Columns.Contains("FechaHora") Then
            grid.Columns("FechaHora").HeaderText = "Fecha y hora"
            grid.Columns("FechaHora").FillWeight = 85
        End If

        If grid.Columns.Contains("Motivo") Then
            grid.Columns("Motivo").HeaderText = "Motivo"
            grid.Columns("Motivo").FillWeight = 115
        End If

        If grid.Columns.Contains("Estado") Then
            grid.Columns("Estado").HeaderText = "Estado"
            grid.Columns("Estado").FillWeight = 75
        End If
    End Sub

    Private Class ComboItem
        Public Property Id As Integer
        Public Property Nombre As String

        Public Sub New(i As Integer, n As String)
            Id = i
            Nombre = n
        End Sub

        Public Overrides Function ToString() As String
            Return Nombre
        End Function
    End Class
End Class
