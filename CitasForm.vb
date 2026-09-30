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
    Private ReadOnly ColorBorde As Color = Color.FromArgb(210, 216, 224)

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
        paciente.SetBounds(22, 84, 340, 32)
        paciente.DropDownStyle = ComboBoxStyle.DropDownList
        paciente.FlatStyle = FlatStyle.Flat
        paciente.BackColor = Color.White
        paciente.Font = New Font("Segoe UI", 9.5F)
        panelRegistro.Controls.Add(paciente)

        CrearEtiqueta(panelRegistro, "Médico", 22, 126)
        medico.SetBounds(22, 148, 340, 32)
        medico.DropDownStyle = ComboBoxStyle.DropDownList
        medico.FlatStyle = FlatStyle.Flat
        medico.BackColor = Color.White
        medico.Font = New Font("Segoe UI", 9.5F)
        panelRegistro.Controls.Add(medico)

        CrearEtiqueta(panelRegistro, "Fecha y hora", 22, 190)
        fecha.SetBounds(22, 212, 340, 32)
        fecha.Format = DateTimePickerFormat.Custom
        fecha.CustomFormat = "dd/MM/yyyy HH:mm"
        fecha.ShowUpDown = True
        fecha.Font = New Font("Segoe UI", 9.5F)
        panelRegistro.Controls.Add(fecha)

        CrearEtiqueta(panelRegistro, "Motivo de la cita", 22, 254)
        motivo.SetBounds(22, 276, 340, 64)
        motivo.Multiline = True
        motivo.ScrollBars = ScrollBars.Vertical
        motivo.Font = New Font("Segoe UI", 9.5F)
        motivo.BorderStyle = BorderStyle.FixedSingle
        motivo.BackColor = Color.White
        panelRegistro.Controls.Add(motivo)

        CrearEtiqueta(panelRegistro, "Estado", 22, 350)
        estado.SetBounds(22, 372, 340, 32)
        estado.DropDownStyle = ComboBoxStyle.DropDownList
        estado.FlatStyle = FlatStyle.Flat
        estado.BackColor = Color.White
        estado.Font = New Font("Segoe UI", 9.5F)
        estado.Items.AddRange({"Pendiente", "Confirmada", "Atendida", "Cancelada"})
        estado.SelectedIndex = 0
        panelRegistro.Controls.Add(estado)

        btn.Text = "Guardar cita"
        btn.SetBounds(22, 430, 340, 42)
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
        grid.GridColor = ColorBorde
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
            MessageBox.Show("Seleccione un paciente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information)
            paciente.Focus()
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
        fecha.Value = DateTime.Now.AddMinutes(30)
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
