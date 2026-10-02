Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

Public Class CitasForm
    Inherits Form

    Private paciente As New ComboBox()
    Private medico As New ComboBox()
    Private fecha As New DateTimePicker()
    Private hora As New DateTimePicker()
    Private motivo As New TextBox()
    Private estado As New ComboBox()
    Private grid As New DataGridView()
    Private btnGuardar As New Button()
    Private btnEditar As New Button()
    Private btnNueva As New Button()
    Private btnCancelar As New Button()
    Private citaEditandoId As Integer = 0
    Private citaEditandoFechaHora As DateTime = DateTime.MinValue

    Private ReadOnly ColorFondo As Color = Color.FromArgb(245, 247, 250)
    Private ReadOnly ColorPanel As Color = Color.White
    Private ReadOnly ColorPrimario As Color = Color.FromArgb(35, 99, 160)
    Private ReadOnly ColorTexto As Color = Color.FromArgb(45, 55, 72)
    Private ReadOnly ColorBorde As Color = Color.FromArgb(185, 195, 207)
    Private ReadOnly ColorBordeActivo As Color = Color.FromArgb(35, 99, 160)

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Citas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Citas", "Citas")
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub(sender As Object, e As EventArgs) Me.Close()
            Return
        End If

        Text = "Clínica Médica - Gestión de citas"
        Width = 1160
        Height = 720
        MinimumSize = New Size(1050, 650)
        StartPosition = FormStartPosition.CenterParent
        BackColor = ColorFondo
        Font = New Font("Segoe UI", 9.0F)
        Padding = New Padding(18)

        ConstruirInterfaz()
        CargarCombos()
        fecha.MinDate = DateTime.Today
        fecha.MaxDate = DateTime.Today.AddYears(5)
        hora.Format = DateTimePickerFormat.Custom
        hora.CustomFormat = "HH:mm"
        hora.ShowUpDown = True
        EstablecerFechaHoraInicial()
        Cargar()
        ActualizarBotones()
    End Sub

    Private Sub ConstruirInterfaz()
        Dim titulo As New Label With {.Text = "Gestión de citas", .AutoSize = True, .Font = New Font("Segoe UI Semibold", 18.0F), .ForeColor = ColorTexto, .Location = New Point(18, 12)}
        Controls.Add(titulo)
        Dim subtitulo As New Label With {.Text = "Registre, modifique y actualice el estado de las citas.", .AutoSize = True, .Font = New Font("Segoe UI", 9.0F), .ForeColor = Color.FromArgb(100, 110, 125), .Location = New Point(20, 45)}
        Controls.Add(subtitulo)

        Dim panelRegistro As New Panel With {.BackColor = ColorPanel, .Location = New Point(18, 78), .Size = New Size(390, 555), .BorderStyle = BorderStyle.FixedSingle}
        Controls.Add(panelRegistro)

        Dim encabezado As New Label With {.Text = "Nueva cita", .AutoSize = True, .Font = New Font("Segoe UI Semibold", 12.0F), .ForeColor = ColorPrimario, .Location = New Point(22, 18)}
        panelRegistro.Controls.Add(encabezado)

        CrearEtiqueta(panelRegistro, "Paciente", 22, 62)
        CrearCampoEnmarcado(panelRegistro, paciente, 22, 84, 340, 34)
        paciente.DropDownStyle = ComboBoxStyle.DropDownList
        paciente.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Médico", 22, 126)
        CrearCampoEnmarcado(panelRegistro, medico, 22, 148, 340, 34)
        medico.DropDownStyle = ComboBoxStyle.DropDownList
        medico.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Fecha", 22, 190)
        CrearCampoEnmarcado(panelRegistro, fecha, 22, 212, 162, 34)
        fecha.Format = DateTimePickerFormat.Custom
        fecha.CustomFormat = "dd/MM/yyyy"
        fecha.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Hora", 200, 190)
        CrearCampoEnmarcado(panelRegistro, hora, 200, 212, 162, 34)
        hora.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Motivo de la cita", 22, 264)
        CrearCampoEnmarcado(panelRegistro, motivo, 22, 286, 340, 64)
        motivo.Multiline = True
        motivo.ScrollBars = ScrollBars.Vertical
        motivo.Font = New Font("Segoe UI", 9.5F)

        CrearEtiqueta(panelRegistro, "Estado", 22, 364)
        CrearCampoEnmarcado(panelRegistro, estado, 22, 386, 340, 34)
        estado.DropDownStyle = ComboBoxStyle.DropDownList
        estado.Font = New Font("Segoe UI", 9.5F)
        estado.Items.AddRange({"Pendiente", "Confirmada", "Atendida", "Cancelada"})
        estado.SelectedIndex = 0

        btnGuardar.Text = "Guardar cita"
        btnGuardar.SetBounds(22, 438, 162, 40)
        btnGuardar.BackColor = ColorPrimario
        btnGuardar.ForeColor = Color.White
        btnGuardar.FlatStyle = FlatStyle.Flat
        btnGuardar.FlatAppearance.BorderSize = 0
        btnGuardar.Font = New Font("Segoe UI Semibold", 9.5F)
        panelRegistro.Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click, AddressOf Guardar

        btnCancelar.Text = "Cancelar edición"
        btnCancelar.SetBounds(200, 438, 162, 40)
        btnCancelar.BackColor = Color.White
        btnCancelar.ForeColor = ColorTexto
        btnCancelar.FlatStyle = FlatStyle.Flat
        btnCancelar.FlatAppearance.BorderColor = ColorBorde
        btnCancelar.Font = New Font("Segoe UI Semibold", 9.0F)
        panelRegistro.Controls.Add(btnCancelar)
        AddHandler btnCancelar.Click, Sub(sender As Object, e As EventArgs) LimpiarEdicion()

        btnNueva.Text = "Nueva cita"
        btnNueva.SetBounds(22, 490, 162, 38)
        btnNueva.BackColor = Color.FromArgb(28, 112, 91)
        btnNueva.ForeColor = Color.White
        btnNueva.FlatStyle = FlatStyle.Flat
        btnNueva.FlatAppearance.BorderSize = 0
        panelRegistro.Controls.Add(btnNueva)
        AddHandler btnNueva.Click, Sub(sender As Object, e As EventArgs) LimpiarEdicion()

        btnEditar.Text = "Modificar seleccionada"
        btnEditar.SetBounds(200, 490, 162, 38)
        btnEditar.BackColor = Color.FromArgb(75, 85, 99)
        btnEditar.ForeColor = Color.White
        btnEditar.FlatStyle = FlatStyle.Flat
        btnEditar.FlatAppearance.BorderSize = 0
        panelRegistro.Controls.Add(btnEditar)
        AddHandler btnEditar.Click, AddressOf CargarSeleccionada

        Dim panelListado As New Panel With {
            .BackColor = ColorPanel,
            .Location = New Point(426, 78),
            .Size = New Size(Width - 462, 555),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .BorderStyle = BorderStyle.FixedSingle
        }
        Controls.Add(panelListado)

        Dim tituloListado As New Label With {.Text = "Citas registradas", .AutoSize = True, .Font = New Font("Segoe UI Semibold", 12.0F), .ForeColor = ColorTexto, .Location = New Point(18, 18)}
        panelListado.Controls.Add(tituloListado)

        grid.SetBounds(18, 52, panelListado.Width - 36, panelListado.Height - 70)
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoGenerateColumns = False
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
        grid.GridColor = Color.FromArgb(210, 216, 224)
        grid.RowHeadersVisible = False
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersHeight = 38
        grid.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.FromArgb(238, 242, 247), .ForeColor = ColorTexto, .Font = New Font("Segoe UI Semibold", 9.0F), .Alignment = DataGridViewContentAlignment.MiddleLeft}
        grid.DefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.White, .ForeColor = ColorTexto, .SelectionBackColor = Color.FromArgb(220, 235, 250), .SelectionForeColor = Color.FromArgb(25, 55, 85), .Font = New Font("Segoe UI", 9.0F)}
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 253)
        grid.RowTemplate.Height = 32
        AgregarColumnaTexto("Id", "Id", 0, False, 40)
        AgregarColumnaTexto("PacienteId", "PacienteId", 0, False, 40)
        AgregarColumnaTexto("MedicoId", "MedicoId", 0, False, 40)
        AgregarColumnaTexto("Paciente", "Paciente", 120)
        AgregarColumnaTexto("Medico", "Médico", 105)
        AgregarColumnaTexto("Fecha", "Fecha", 75)
        AgregarColumnaTexto("Hora", "Hora", 55)
        AgregarColumnaTexto("Motivo", "Motivo", 110)
        AgregarColumnaTexto("Estado", "Estado", 75)
        panelListado.Controls.Add(grid)
        AddHandler grid.CellDoubleClick, AddressOf CargarSeleccionada
        AddHandler grid.SelectionChanged, Sub(sender As Object, e As EventArgs) ActualizarBotones()
    End Sub

    Private Sub AgregarColumnaTexto(nombre As String, encabezado As String, peso As Integer, Optional visible As Boolean = True, Optional ancho As Integer = 50)
        Dim col As New DataGridViewTextBoxColumn With {.Name = nombre, .HeaderText = encabezado, .DataPropertyName = nombre, .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = Math.Max(1, peso), .Visible = visible}
        If Not visible Then col.Width = ancho
        grid.Columns.Add(col)
    End Sub

    Private Sub CrearEtiqueta(contenedor As Control, texto As String, x As Integer, y As Integer)
        contenedor.Controls.Add(New Label With {.Text = texto, .AutoSize = True, .Location = New Point(x, y), .ForeColor = ColorTexto, .Font = New Font("Segoe UI Semibold", 9.0F)})
    End Sub

    Private Sub CrearCampoEnmarcado(contenedor As Control, control As Control, x As Integer, y As Integer, ancho As Integer, alto As Integer)
        Dim marco As New Panel With {.BackColor = ColorBorde, .Location = New Point(x, y), .Size = New Size(ancho, alto), .Padding = New Padding(1)}
        control.Dock = DockStyle.Fill
        control.Margin = New Padding(0)
        control.BackColor = Color.White
        If TypeOf control Is TextBox Then DirectCast(control, TextBox).BorderStyle = BorderStyle.None
        marco.Controls.Add(control)
        contenedor.Controls.Add(marco)
        AddHandler control.Enter, Sub(sender As Object, e As EventArgs) marco.BackColor = ColorBordeActivo
        AddHandler control.Leave, Sub(sender As Object, e As EventArgs) marco.BackColor = ColorBorde
    End Sub

    Private Sub EstablecerFechaHoraInicial()
        Dim propuesta = DateTime.Now.AddMinutes(30)
        Dim minutosRedondeados = ((propuesta.Minute + 29) \ 30) * 30
        If minutosRedondeados >= 60 Then
            propuesta = New DateTime(propuesta.Year, propuesta.Month, propuesta.Day, propuesta.Hour, 0, 0).AddHours(1)
        Else
            propuesta = New DateTime(propuesta.Year, propuesta.Month, propuesta.Day, propuesta.Hour, minutosRedondeados, 0)
        End If
        If propuesta < DateTime.Now Then propuesta = DateTime.Now.AddHours(1)
        fecha.Value = propuesta.Date
        hora.Value = propuesta
    End Sub

    Private Sub CargarCombos()
        paciente.Items.Clear()
        medico.Items.Clear()
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT Id,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos,Nombre"
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

    Private Function ObtenerFechaHoraSeleccionada() As DateTime
        Return fecha.Value.Date.Add(hora.Value.TimeOfDay)
    End Function

    Private Sub Guardar(sender As Object, e As EventArgs)
        If paciente.SelectedItem Is Nothing Then
            MessageBox.Show("Seleccione un paciente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information)
            paciente.Focus()
            Return
        End If

        Dim fechaHora = ObtenerFechaHoraSeleccionada()
        If citaEditandoId = 0 AndAlso fechaHora < DateTime.Now Then
            MessageBox.Show("La fecha y hora de una nueva cita no puede ser anterior al momento actual.", "Fecha no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If citaEditandoId > 0 AndAlso fechaHora < DateTime.Now AndAlso fechaHora <> citaEditandoFechaHora Then
            MessageBox.Show("La nueva fecha y hora no puede quedar en el pasado.", "Fecha no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim p = DirectCast(paciente.SelectedItem, ComboItem)
        Dim mid As Object = DBNull.Value
        If medico.SelectedItem IsNot Nothing Then mid = DirectCast(medico.SelectedItem, ComboItem).Id

        Using cn = Database.Connection()
            Using validar = cn.CreateCommand()
                validar.CommandText = "SELECT COUNT(*) FROM Citas WHERE FechaHora=$f AND COALESCE(MedicoId,0)=COALESCE($m,0) AND Estado<>'Cancelada' AND Id<>$id"
                validar.Parameters.AddWithValue("$f", fechaHora.ToString("s"))
                validar.Parameters.AddWithValue("$m", mid)
                validar.Parameters.AddWithValue("$id", citaEditandoId)
                If Convert.ToInt32(validar.ExecuteScalar()) > 0 Then
                    MessageBox.Show("Ya existe una cita activa para ese médico en la fecha y hora seleccionadas.", "Conflicto de horario", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
            End Using
            Using cmd = cn.CreateCommand()
                If citaEditandoId = 0 Then
                    cmd.CommandText = "INSERT INTO Citas(PacienteId,MedicoId,FechaHora,Motivo,Estado) VALUES($p,$m,$f,$mo,$e)"
                Else
                    cmd.CommandText = "UPDATE Citas SET PacienteId=$p,MedicoId=$m,FechaHora=$f,Motivo=$mo,Estado=$e WHERE Id=$id"
                    cmd.Parameters.AddWithValue("$id", citaEditandoId)
                End If
                cmd.Parameters.AddWithValue("$p", p.Id)
                cmd.Parameters.AddWithValue("$m", mid)
                cmd.Parameters.AddWithValue("$f", fechaHora.ToString("s"))
                cmd.Parameters.AddWithValue("$mo", motivo.Text.Trim())
                cmd.Parameters.AddWithValue("$e", estado.Text)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        Dim accion = If(citaEditandoId = 0, "Registro de cita", "Modificación de cita")
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, accion, "Citas", "PacienteId=" & p.Id.ToString())
        MessageBox.Show(If(citaEditandoId = 0, "Cita registrada correctamente.", "Cita modificada correctamente."), "Clínica Médica", MessageBoxButtons.OK, MessageBoxIcon.Information)
        LimpiarEdicion()
        Cargar()
    End Sub

    Private Sub CargarSeleccionada(sender As Object, e As EventArgs)
        If grid.SelectedRows.Count = 0 Then Return
        Dim fila = grid.SelectedRows(0)
        citaEditandoId = Convert.ToInt32(fila.Cells("Id").Value)
        citaEditandoFechaHora = ObtenerFechaHoraPorId(citaEditandoId)
        SeleccionarComboPorId(paciente, Convert.ToInt32(fila.Cells("PacienteId").Value))
        If fila.Cells("MedicoId").Value Is Nothing OrElse fila.Cells("MedicoId").Value Is DBNull.Value Then
            medico.SelectedIndex = -1
        Else
            SeleccionarComboPorId(medico, Convert.ToInt32(fila.Cells("MedicoId").Value))
        End If
        Dim fh = citaEditandoFechaHora
        fecha.MinDate = New DateTime(1753, 1, 1)
        fecha.Value = fh.Date
        hora.Value = fh
        motivo.Text = If(fila.Cells("Motivo").Value, "").ToString()
        estado.SelectedItem = fila.Cells("Estado").Value.ToString()
        btnGuardar.Text = "Guardar cambios"
        ActualizarBotones()
    End Sub

    Private Function ObtenerFechaHoraPorId(id As Integer) As DateTime
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT FechaHora FROM Citas WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id", id)
                Dim valor = cmd.ExecuteScalar()
                If valor Is Nothing OrElse valor Is DBNull.Value Then Return DateTime.Now
                Return DateTime.Parse(valor.ToString())
            End Using
        End Using
    End Function

    Private Sub SeleccionarComboPorId(cmb As ComboBox, id As Integer)
        For i = 0 To cmb.Items.Count - 1
            If DirectCast(cmb.Items(i), ComboItem).Id = id Then
                cmb.SelectedIndex = i
                Return
            End If
        Next
        cmb.SelectedIndex = -1
    End Sub

    Private Sub LimpiarEdicion()
        citaEditandoId = 0
        citaEditandoFechaHora = DateTime.MinValue
        EstablecerFechaHoraInicial()
        fecha.MinDate = DateTime.Today
        paciente.SelectedIndex = -1
        medico.SelectedIndex = -1
        motivo.Clear()
        estado.SelectedIndex = 0
        btnGuardar.Text = "Guardar cita"
        ActualizarBotones()
    End Sub

    Private Sub ActualizarBotones()
        btnEditar.Enabled = grid.SelectedRows.Count > 0
        btnCancelar.Enabled = citaEditandoId > 0
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT C.Id,C.PacienteId,C.MedicoId,P.Nombre||' '||P.Apellidos AS Paciente,COALESCE(M.Nombre,'') AS Medico,substr(C.FechaHora,1,10) AS Fecha,substr(C.FechaHora,12,5) AS Hora,COALESCE(C.Motivo,'') AS Motivo,COALESCE(C.Estado,'Pendiente') AS Estado FROM Citas C JOIN Pacientes P ON P.Id=C.PacienteId LEFT JOIN Medicos M ON M.Id=C.MedicoId ORDER BY C.FechaHora DESC"
                Using rd = cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource = Nothing
        grid.DataSource = dt
        ActualizarBotones()
    End Sub

    Private Class ComboItem
        Public Property Id As Integer
        Public Property Nombre As String
        Public Sub New(i As Integer, n As String)
            Id = i : Nombre = n
        End Sub
        Public Overrides Function ToString() As String
            Return Nombre
        End Function
    End Class
End Class
