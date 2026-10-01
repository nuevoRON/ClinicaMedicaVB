Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Drawing

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
    Private btnGuardar As New Button()
    Private btnEditar As New Button()
    Private btnNueva As New Button()
    Private btnCancelar As New Button()
    Private grid As New DataGridView()
    Private consultaEditandoId As Integer = 0

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Consultas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Consultas", "Consultas")
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub(sender As Object, e As EventArgs) Me.Close()
            Return
        End If

        Text = "Consultas médicas"
        Width = 1180
        Height = 760
        MinimumSize = New Size(1080, 680)
        StartPosition = FormStartPosition.CenterParent
        BackColor = Color.FromArgb(245, 247, 250)
        Font = New Font("Segoe UI", 9.0F)

        ConstruirInterfaz()
        CargarCombos()
        Cargar()
        LimpiarEdicion()
    End Sub

    Private Sub ConstruirInterfaz()
        Controls.Add(New Label With {.Text = "Gestión de consultas médicas", .Left = 18, .Top = 12, .AutoSize = True, .Font = New Font("Segoe UI Semibold", 18), .ForeColor = Color.FromArgb(45, 55, 72)})
        Controls.Add(New Label With {.Text = "Registre nuevas consultas y edite la información clínica guardada.", .Left = 20, .Top = 45, .AutoSize = True, .ForeColor = Color.FromArgb(100, 110, 125)})

        Dim panel As New Panel With {.BackColor = Color.White, .Location = New Point(18, 78), .Size = New Size(440, 600), .BorderStyle = BorderStyle.FixedSingle}
        Controls.Add(panel)
        panel.Controls.Add(New Label With {.Text = "Datos de la consulta", .Left = 20, .Top = 16, .AutoSize = True, .Font = New Font("Segoe UI Semibold", 12), .ForeColor = Color.FromArgb(35, 99, 160)})

        Dim y = 52
        AddField(panel, "Paciente", paciente, y, True) : y += 40
        AddField(panel, "Médico", medico, y, True) : y += 40
        AddField(panel, "Motivo", motivo, y) : y += 40
        AddField(panel, "Presión", presion, y) : y += 40
        AddField(panel, "Temperatura", temperatura, y) : y += 40
        AddField(panel, "Frec. cardiaca", frecuencia, y) : y += 40
        AddField(panel, "Saturación", saturacion, y) : y += 40
        AddField(panel, "Peso", peso, y) : y += 40
        AddField(panel, "Diagnóstico", diagnostico, y) : y += 40
        AddField(panel, "Tratamiento", tratamiento, y) : y += 40
        AddField(panel, "Observaciones", observaciones, y) : y += 40

        btnGuardar.Text = "Guardar consulta"
        btnGuardar.SetBounds(20, 520, 190, 38)
        EstiloBoton(btnGuardar, Color.FromArgb(35, 99, 160), Color.White)
        panel.Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click, AddressOf Guardar

        btnCancelar.Text = "Cancelar edición"
        btnCancelar.SetBounds(225, 520, 190, 38)
        EstiloBoton(btnCancelar, Color.White, Color.FromArgb(45, 55, 72))
        panel.Controls.Add(btnCancelar)
        AddHandler btnCancelar.Click, Sub(sender As Object, e As EventArgs) LimpiarEdicion()

        btnNueva.Text = "Nueva consulta"
        btnNueva.SetBounds(20, 565, 190, 32)
        EstiloBoton(btnNueva, Color.FromArgb(28, 112, 91), Color.White)
        panel.Controls.Add(btnNueva)
        AddHandler btnNueva.Click, Sub(sender As Object, e As EventArgs) LimpiarEdicion()

        btnEditar.Text = "Editar seleccionada"
        btnEditar.SetBounds(225, 565, 190, 32)
        EstiloBoton(btnEditar, Color.FromArgb(75, 85, 99), Color.White)
        panel.Controls.Add(btnEditar)
        AddHandler btnEditar.Click, AddressOf CargarSeleccionada

        Dim listado As New Panel With {.BackColor = Color.White, .Location = New Point(476, 78), .Size = New Size(Width - 512, 600), .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right, .BorderStyle = BorderStyle.FixedSingle}
        Controls.Add(listado)
        listado.Controls.Add(New Label With {.Text = "Consultas registradas", .Left = 18, .Top = 16, .AutoSize = True, .Font = New Font("Segoe UI Semibold", 12), .ForeColor = Color.FromArgb(45, 55, 72)})

        grid.SetBounds(18, 52, listado.Width - 36, listado.Height - 70)
        grid.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoGenerateColumns = False
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.RowHeadersVisible = False
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersHeight = 38
        grid.ColumnHeadersDefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.FromArgb(238, 242, 247), .ForeColor = Color.FromArgb(45, 55, 72), .Font = New Font("Segoe UI Semibold", 9)}
        grid.DefaultCellStyle = New DataGridViewCellStyle With {.BackColor = Color.White, .ForeColor = Color.FromArgb(45, 55, 72), .SelectionBackColor = Color.FromArgb(220, 235, 250), .SelectionForeColor = Color.FromArgb(25, 55, 85)}
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 253)
        grid.RowTemplate.Height = 32
        AddColumn("Id", "Id", False, 40)
        AddColumn("PacienteId", "PacienteId", False, 40)
        AddColumn("MedicoId", "MedicoId", False, 40)
        AddColumn("Paciente", "Paciente", True, 120)
        AddColumn("FechaHora", "Fecha y hora", True, 90)
        AddColumn("Diagnostico", "Diagnóstico", True, 120)
        AddColumn("Tratamiento", "Tratamiento", True, 120)
        listado.Controls.Add(grid)
        AddHandler grid.CellDoubleClick, AddressOf CargarSeleccionada
        AddHandler grid.SelectionChanged, Sub(sender As Object, e As EventArgs) btnEditar.Enabled = grid.SelectedRows.Count > 0
    End Sub

    Private Sub AddColumn(nombre As String, encabezado As String, visible As Boolean, peso As Integer)
        grid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = nombre, .HeaderText = encabezado, .DataPropertyName = nombre, .Visible = visible, .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, .FillWeight = peso})
    End Sub

    Private Sub EstiloBoton(b As Button, fondo As Color, texto As Color)
        b.BackColor = fondo
        b.ForeColor = texto
        b.FlatStyle = FlatStyle.Flat
        b.FlatAppearance.BorderSize = 0
        b.Font = New Font("Segoe UI Semibold", 9)
    End Sub

    Private Sub AddField(t As String, c As Control, y As Integer, Optional combo As Boolean = False)
        ControlsAddLabel(t, y)
        c.SetBounds(150, y, 270, 28)
        If TypeOf c Is TextBox Then
            DirectCast(c, TextBox).BorderStyle = BorderStyle.FixedSingle
        ElseIf TypeOf c Is ComboBox Then
            DirectCast(c, ComboBox).DropDownStyle = ComboBoxStyle.DropDownList
        End If
        DirectCast(c.Parent, Control)
        ' El control se agrega al panel desde el llamador mediante la referencia del panel.
    End Sub

    Private Sub ControlsAddLabel(t As String, y As Integer)
        ' Se reemplaza por etiquetas en el panel de registro mediante el método auxiliar.
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
                cmd.CommandText = "SELECT Id,Nombre FROM Medicos ORDER BY Nombre"
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
            MessageBox.Show("Seleccione paciente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Dim p = DirectCast(paciente.SelectedItem, ComboItem)
        Dim mid As Object = DBNull.Value
        If medico.SelectedItem IsNot Nothing Then mid = DirectCast(medico.SelectedItem, ComboItem).Id

        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                If consultaEditandoId = 0 Then
                    cmd.CommandText = "INSERT INTO Consultas(PacienteId,MedicoId,FechaHora,Motivo,Presion,Temperatura,FrecuenciaCardiaca,Saturacion,Peso,Diagnostico,Tratamiento,Observaciones) VALUES($p,$m,$f,$mo,$pr,$te,$fc,$sa,$pe,$di,$tr,$ob)"
                    cmd.Parameters.AddWithValue("$f", DateTime.Now.ToString("s"))
                Else
                    cmd.CommandText = "UPDATE Consultas SET PacienteId=$p,MedicoId=$m,Motivo=$mo,Presion=$pr,Temperatura=$te,FrecuenciaCardiaca=$fc,Saturacion=$sa,Peso=$pe,Diagnostico=$di,Tratamiento=$tr,Observaciones=$ob WHERE Id=$id"
                    cmd.Parameters.AddWithValue("$id", consultaEditandoId)
                End If
                cmd.Parameters.AddWithValue("$p", p.Id)
                cmd.Parameters.AddWithValue("$m", mid)
                cmd.Parameters.AddWithValue("$mo", motivo.Text.Trim())
                cmd.Parameters.AddWithValue("$pr", presion.Text.Trim())
                cmd.Parameters.AddWithValue("$te", temperatura.Text.Trim())
                cmd.Parameters.AddWithValue("$fc", frecuencia.Text.Trim())
                cmd.Parameters.AddWithValue("$sa", saturacion.Text.Trim())
                cmd.Parameters.AddWithValue("$pe", peso.Text.Trim())
                cmd.Parameters.AddWithValue("$di", diagnostico.Text.Trim())
                cmd.Parameters.AddWithValue("$tr", tratamiento.Text.Trim())
                cmd.Parameters.AddWithValue("$ob", observaciones.Text.Trim())
                cmd.ExecuteNonQuery()
            End Using
        End Using

        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, If(consultaEditandoId = 0, "Registro de consulta", "Modificación de consulta"), "Consultas", "PacienteId=" & p.Id.ToString())
        MessageBox.Show(If(consultaEditandoId = 0, "Consulta registrada.", "Consulta modificada correctamente."), "Clínica Médica", MessageBoxButtons.OK, MessageBoxIcon.Information)
        LimpiarEdicion()
        Cargar()
    End Sub

    Private Sub CargarSeleccionada(sender As Object, e As EventArgs)
        If grid.SelectedRows.Count = 0 Then Return
        Dim id = Convert.ToInt32(grid.SelectedRows(0).Cells("Id").Value)
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT PacienteId,MedicoId,Motivo,Presion,Temperatura,FrecuenciaCardiaca,Saturacion,Peso,Diagnostico,Tratamiento,Observaciones FROM Consultas WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id", id)
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then Return
                    consultaEditandoId = id
                    SeleccionarCombo(paciente, rd("PacienteId"))
                    If rd("MedicoId") Is DBNull.Value Then medico.SelectedIndex = -1 Else SeleccionarCombo(medico, rd("MedicoId"))
                    motivo.Text = ValorTexto(rd("Motivo"))
                    presion.Text = ValorTexto(rd("Presion"))
                    temperatura.Text = ValorTexto(rd("Temperatura"))
                    frecuencia.Text = ValorTexto(rd("FrecuenciaCardiaca"))
                    saturacion.Text = ValorTexto(rd("Saturacion"))
                    peso.Text = ValorTexto(rd("Peso"))
                    diagnostico.Text = ValorTexto(rd("Diagnostico"))
                    tratamiento.Text = ValorTexto(rd("Tratamiento"))
                    observaciones.Text = ValorTexto(rd("Observaciones"))
                End Using
            End Using
        End Using
        btnGuardar.Text = "Guardar cambios"
        btnCancelar.Enabled = True
    End Sub

    Private Function ValorTexto(v As Object) As String
        If v Is Nothing OrElse v Is DBNull.Value Then Return ""
        Return v.ToString()
    End Function

    Private Sub SeleccionarCombo(cmb As ComboBox, valor As Object)
        If valor Is Nothing OrElse valor Is DBNull.Value Then cmb.SelectedIndex = -1 : Return
        Dim id = Convert.ToInt32(valor)
        For i = 0 To cmb.Items.Count - 1
            If DirectCast(cmb.Items(i), ComboItem).Id = id Then cmb.SelectedIndex = i : Return
        Next
        cmb.SelectedIndex = -1
    End Sub

    Private Sub LimpiarEdicion()
        consultaEditandoId = 0
        paciente.SelectedIndex = -1
        medico.SelectedIndex = -1
        motivo.Clear() : diagnostico.Clear() : tratamiento.Clear() : observaciones.Clear()
        presion.Clear() : temperatura.Clear() : frecuencia.Clear() : saturacion.Clear() : peso.Clear()
        btnGuardar.Text = "Guardar consulta"
        btnCancelar.Enabled = False
        btnEditar.Enabled = grid.SelectedRows.Count > 0
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT C.Id,C.PacienteId,C.MedicoId,P.Nombre||' '||P.Apellidos AS Paciente,C.FechaHora,COALESCE(C.Diagnostico,'') AS Diagnostico,COALESCE(C.Tratamiento,'') AS Tratamiento FROM Consultas C JOIN Pacientes P ON P.Id=C.PacienteId ORDER BY C.FechaHora DESC"
                Using rd = cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource = Nothing
        grid.DataSource = dt
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