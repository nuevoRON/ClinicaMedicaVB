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
        Text="Pacientes" : Width=1150 : Height=700 : StartPosition=FormStartPosition.CenterParent

        Dim y=20
        AddField("Identidad",txtIdentidad,y) : y+=38
        AddField("Nombre",txtNombre,y) : y+=38
        AddField("Apellidos",txtApellidos,y) : y+=38
        txtFecha.SetBounds(150,y,220,28)
        Controls.Add(New Label With {.Text="Fecha nacimiento",.Left=20,.Top=y+5,.Width=120})
        Controls.Add(txtFecha) : y+=38
        txtSexo.Items.AddRange({"Femenino","Masculino","Otro"})
        txtSexo.SetBounds(150,y,220,28)
        Controls.Add(New Label With {.Text="Sexo",.Left=20,.Top=y+5,.Width=120}) : Controls.Add(txtSexo) : y+=38
        AddField("Teléfono",txtTelefono,y) : y+=38
        AddField("Dirección",txtDireccion,y) : y+=38
        AddField("Alergias",txtAlergias,y) : y+=38
        AddField("Antecedentes",txtAntecedentes,y) : y+=38

        btnGuardar.Text="Guardar paciente" : btnGuardar.SetBounds(150,y,180,35)
        Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click, AddressOf Guardar

        Dim lblBuscar As New Label With {.Text="Búsqueda avanzada:",.Left=400,.Top=20,.AutoSize=True}
        cmbCampo.SetBounds(525,15,150,28)
        cmbCampo.DropDownStyle=ComboBoxStyle.DropDownList
        cmbCampo.Items.AddRange({"Todos","Identidad","Nombre","Apellidos","Teléfono"})
        cmbCampo.SelectedIndex=0

        txtBuscar.SetBounds(685,15,250,28)
        txtBuscar.PlaceholderText="Escriba para buscar..."
        Dim btnBuscar As New Button With {.Text="Buscar",.Left=945,.Top=15,.Width=80,.Height=28}
        Dim btnTodos As New Button With {.Text="Mostrar todos",.Left=945,.Top=50,.Width=100,.Height=28}

        btnExpediente.Text="Abrir expediente"
        btnExpediente.SetBounds(400,55,180,35)
        btnExpediente.Enabled=False

        AddHandler btnBuscar.Click, AddressOf Buscar
        AddHandler txtBuscar.TextChanged, AddressOf BuscarAutomaticamente
        AddHandler cmbCampo.SelectedIndexChanged, AddressOf BuscarAutomaticamente
        AddHandler btnTodos.Click, Sub() txtBuscar.Clear()
        AddHandler btnExpediente.Click, AddressOf AbrirExpediente
        AddHandler grid.CellDoubleClick, AddressOf AbrirExpediente

        grid.SetBounds(400,105,700,500)
        grid.ReadOnly=True
        grid.AllowUserToAddRows=False
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        grid.MultiSelect=False
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill

        Controls.AddRange({lblBuscar,cmbCampo,txtBuscar,btnBuscar,btnTodos,btnExpediente,grid})
        AddHandler grid.SelectionChanged, Sub() btnExpediente.Enabled=(grid.SelectedRows.Count>0)

        Cargar()
    End Sub

    Private Sub AddField(labelText As String, box As TextBox, y As Integer)
        Controls.Add(New Label With {.Text=labelText,.Left=20,.Top=y+5,.Width=120})
        box.SetBounds(150,y,220,28) : Controls.Add(box)
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