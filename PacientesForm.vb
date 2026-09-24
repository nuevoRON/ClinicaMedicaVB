Imports Microsoft.Data.Sqlite

Public Class PacientesForm
    Inherits Form
    Private grid As New DataGridView()
    Private txtId As New TextBox()
    Private txtIdentidad As New TextBox()
    Private txtNombre As New TextBox()
    Private txtApellidos As New TextBox()
    Private txtFecha As New DateTimePicker()
    Private txtSexo As New ComboBox()
    Private txtTelefono As New TextBox()
    Private txtDireccion As New TextBox()
    Private txtAlergias As New TextBox()
    Private txtAntecedentes As New TextBox()
    Private btnGuardar As New Button()

    Public Sub New()
        Text="Pacientes" : Width=1000 : Height=650 : StartPosition=FormStartPosition.CenterParent
        txtId.Visible=False
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
        btnGuardar.Text="Guardar paciente" : btnGuardar.SetBounds(150,y,220,35)
        AddHandler btnGuardar.Click, AddressOf Guardar
        Controls.Add(btnGuardar)
        grid.SetBounds(400,20,560,520)
        grid.ReadOnly=True : grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        Controls.Add(grid)
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
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Identidad,Nombre,Apellidos,FechaNacimiento,Sexo,Telefono FROM Pacientes ORDER BY Apellidos,Nombre"
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub
End Class