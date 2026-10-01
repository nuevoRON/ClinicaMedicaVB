Imports Microsoft.Data.Sqlite
Imports System.Drawing

Public Class PacientesForm
    Inherits Form
    Private operador As String = Environment.UserName

    Private grid As New DataGridView()
    Private txtIdentidad As New TextBox()
    Private txtNombre As New TextBox()
    Private txtApellidos As New TextBox()
    Private txtFecha As New DateTimePicker()
    Private txtSexo As New ComboBox()
    Private txtTelefono As New TextBox()
    Private txtDireccion As New TextBox()
    Private txtAlergias As New TextBox()
    Private txtAntecedentes As New TextBox()
    Private txtBuscar As New TextBox()
    Private cmbCampo As New ComboBox()
    Private btnGuardar As New Button()
    Private btnExpediente As New Button()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Pacientes") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Pacientes", "Pacientes")
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text = "Gestión de pacientes" : Width = 1200 : Height = 760 : MinimumSize = New Size(1050, 680) : StartPosition = FormStartPosition.CenterParent
        BackColor = Color.FromArgb(244, 247, 251)
        Font = New Font("Segoe UI", 9.0F)
        Dim encabezado As New Panel With {.BackColor = Color.FromArgb(24, 40, 70)}
        encabezado.SetBounds(0, 0, 1200, 68)
        Dim titulo As New Label With {.Text = "GESTIÓN DE PACIENTES", .ForeColor = Color.White, .Font = New Font("Segoe UI", 16, FontStyle.Bold), .AutoSize = True, .Location = New Point(22, 10)}
        Dim subtitulo As New Label With {.Text = "Registro y consulta de información del paciente", .ForeColor = Color.FromArgb(205, 216, 230), .Font = New Font("Segoe UI", 9), .AutoSize = True, .Location = New Point(24, 42)}
        encabezado.Controls.AddRange({titulo, subtitulo})
        Controls.Add(encabezado)

        Dim y=92
        AddField("Identidad",txtIdentidad,y) : ConfigurarSoloDigitos(txtIdentidad) : y+=38
        AddField("Nombre",txtNombre,y) : y+=38
        AddField("Apellidos",txtApellidos,y) : y+=38
        txtFecha.SetBounds(150,y,220,30)
        Controls.Add(New Label With {.Text="Fecha nacimiento",.Left=20,.Top=y+5,.Width=120})
        Controls.Add(txtFecha) : y+=38
        txtSexo.Items.AddRange({"Femenino","Masculino","Otro"})
        txtSexo.SetBounds(150,y,220,30)
        Controls.Add(New Label With {.Text="Sexo",.Left=20,.Top=y+5,.Width=120}) : Controls.Add(txtSexo) : y+=38
        AddField("Teléfono",txtTelefono,y) : ConfigurarSoloDigitos(txtTelefono) : y+=38
        AddField("Dirección",txtDireccion,y) : y+=38
        AddField("Alergias",txtAlergias,y) : y+=38
        AddField("Antecedentes",txtAntecedentes,y) : y+=38

        btnGuardar.Text="Guardar paciente" : btnGuardar.SetBounds(150,y,190,38)
        btnGuardar.BackColor = Color.FromArgb(28, 112, 91)
        btnGuardar.ForeColor = Color.White
        btnGuardar.FlatStyle = FlatStyle.Flat
        btnGuardar.FlatAppearance.BorderSize = 0
        btnGuardar.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click, AddressOf Guardar

        Dim lblBuscar As New Label With {.Text="Búsqueda avanzada:",.Left=400,.Top=20,.AutoSize=True}
        cmbCampo.SetBounds(525,87,150,30)
        cmbCampo.DropDownStyle=ComboBoxStyle.DropDownList
        cmbCampo.Items.AddRange({"Todos","Identidad","Nombre","Apellidos","Teléfono"})
        cmbCampo.SelectedIndex=0

        txtBuscar.SetBounds(685,87,250,30)
        txtBuscar.PlaceholderText="Escriba para buscar..."
        Dim btnBuscar As New Button With {.Text="Buscar",.Left=945,.Top=87,.Width=90,.Height=30,.BackColor=Color.FromArgb(37,91,145),.ForeColor=Color.White,.FlatStyle=FlatStyle.Flat}
        Dim btnTodos As New Button With {.Text="Mostrar todos",.Left=1040,.Top=87,.Width=115,.Height=30,.BackColor=Color.White,.ForeColor=Color.FromArgb(37,55,78),.FlatStyle=FlatStyle.Flat}

        btnExpediente.Text="Abrir expediente"
        btnExpediente.BackColor = Color.FromArgb(37, 91, 145)
        btnExpediente.ForeColor = Color.White
        btnExpediente.FlatStyle = FlatStyle.Flat
        btnExpediente.FlatAppearance.BorderSize = 0
        btnExpediente.SetBounds(400,122,190,36)
        btnExpediente.Enabled=False

        AddHandler btnBuscar.Click, AddressOf Buscar
        AddHandler txtBuscar.TextChanged, AddressOf BuscarAutomaticamente
        AddHandler cmbCampo.SelectedIndexChanged, AddressOf BuscarAutomaticamente
        AddHandler btnTodos.Click, Sub() txtBuscar.Clear()
        AddHandler btnExpediente.Click, AddressOf AbrirExpediente
        AddHandler grid.CellDoubleClick, AddressOf AbrirExpediente

        grid.SetBounds(400,174,755,500)
        grid.ReadOnly=True
        grid.AllowUserToAddRows=False
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect=False
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.RowHeadersVisible = False
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 55, 83)
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        grid.ColumnHeadersHeight = 38
        grid.RowTemplate.Height = 32
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 232, 248)
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 45, 68)
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 253)

        Controls.AddRange({lblBuscar,cmbCampo,txtBuscar,btnBuscar,btnTodos,btnExpediente,grid})
        AddHandler grid.SelectionChanged, Sub() btnExpediente.Enabled=(grid.SelectedRows.Count>0)

        Cargar()
    End Sub

    Private Sub AddField(labelText As String, box As TextBox, y As Integer)
        Controls.Add(New Label With {.Text=labelText,.Left=20,.Top=y+7,.Width=120,.ForeColor=Color.FromArgb(55,70,88),.Font=New Font("Segoe UI",9,FontStyle.Bold)})
        box.SetBounds(150,y,220,30)
        box.Font = New Font("Segoe UI", 9)
        Controls.Add(box)
    End Sub

    Private Sub ConfigurarSoloDigitos(caja As TextBox)
        AddHandler caja.KeyPress, Sub(sender As Object, e As KeyPressEventArgs)
                                        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then e.Handled = True
                                    End Sub
        AddHandler caja.TextChanged, Sub(sender As Object, e As EventArgs)
                                          Dim limpio = New String(caja.Text.Where(Function(ch) Char.IsDigit(ch)).ToArray())
                                          If limpio <> caja.Text Then
                                              Dim posicion = caja.SelectionStart
                                              caja.Text = limpio
                                              caja.SelectionStart = Math.Min(posicion, caja.Text.Length)
                                          End If
                                      End Sub
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If txtIdentidad.Text.Trim()="" OrElse txtNombre.Text.Trim()="" OrElse txtApellidos.Text.Trim()="" Then
            MessageBox.Show("Identidad, nombre y apellidos son obligatorios.") : Return
        End If
        Try
            Using cn=Database.Connection()
                Using cmd=cn.CreateCommand()
                    cmd.CommandText="INSERT INTO Pacientes(Identidad,Nombre,Apellidos,FechaNacimiento,Sexo,Telefono,Direccion,Alergias,Antecedentes,FechaRegistro) VALUES($i,$n,$a,$f,$s,$t,$d,$al,$an,$r)"
                    cmd.Parameters.AddWithValue("$i",txtIdentidad.Text.Trim())
                    cmd.Parameters.AddWithValue("$n",txtNombre.Text.Trim())
                    cmd.Parameters.AddWithValue("$a",txtApellidos.Text.Trim())
                    cmd.Parameters.AddWithValue("$f",txtFecha.Value.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("$s",txtSexo.Text)
                    cmd.Parameters.AddWithValue("$t",txtTelefono.Text)
                    cmd.Parameters.AddWithValue("$d",txtDireccion.Text)
                    cmd.Parameters.AddWithValue("$al",txtAlergias.Text)
                    cmd.Parameters.AddWithValue("$an",txtAntecedentes.Text)
                    cmd.Parameters.AddWithValue("$r",DateTime.Now.ToString("s"))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Registro de paciente", "Pacientes", txtIdentidad.Text.Trim())
            MessageBox.Show("Paciente registrado.")
            Limpiar() : Cargar()
        Catch ex As Exception
            MessageBox.Show("No se pudo guardar: " & ex.Message)
        End Try
    End Sub

    Private Sub Limpiar()
        For Each c As Control In {txtIdentidad,txtNombre,txtApellidos,txtTelefono,txtDireccion,txtAlergias,txtAntecedentes}
            c.Text=""
        Next
        txtSexo.SelectedIndex=-1
    End Sub

    Private Sub Cargar()
        Buscar()
    End Sub

    Private Sub BuscarAutomaticamente(sender As Object,e As EventArgs)
        Buscar()
    End Sub

    Private Sub Buscar(sender As Object,e As EventArgs)
        Buscar()
    End Sub

    Private Sub Buscar()
        Dim dt As New DataTable()
        Dim filtro=txtBuscar.Text.Trim()
        Dim condicion As String

        Select Case cmbCampo.SelectedIndex
            Case 1 : condicion="Identidad LIKE $f"
            Case 2 : condicion="Nombre LIKE $f"
            Case 3 : condicion="Apellidos LIKE $f"
            Case 4 : condicion="Telefono LIKE $f"
            Case Else : condicion="(Identidad LIKE $f OR Nombre LIKE $f OR Apellidos LIKE $f OR Telefono LIKE $f)"
        End Select

        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText=$"SELECT Id,Identidad,Nombre,Apellidos,FechaNacimiento,Sexo,Telefono FROM Pacientes WHERE {condicion} ORDER BY Apellidos,Nombre"
                cmd.Parameters.AddWithValue("$f","%" & filtro & "%")
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub

    Private Sub AbrirExpediente(sender As Object,e As EventArgs)
        If grid.SelectedRows.Count=0 Then Return
        Dim id=Convert.ToInt32(grid.SelectedRows(0).Cells("Id").Value)
        Using f As New ExpedienteForm(id)
            f.ShowDialog()
        End Using
    End Sub
End Class