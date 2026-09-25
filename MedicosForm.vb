Imports Microsoft.Data.Sqlite

Public Class MedicosForm
    Inherits Form
    Private grid As New DataGridView()
    Private txtNombre As New TextBox()
    Private txtEspecialidad As New TextBox()
    Private txtColegiado As New TextBox()
    Private btn As New Button()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Medicos") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Medicos", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text="Médicos" : Width=850 : Height=520 : StartPosition=FormStartPosition.CenterParent
        Controls.Add(New Label With {.Text="Nombre",.Left=20,.Top=30})
        txtNombre.SetBounds(120,25,220,28) : Controls.Add(txtNombre)
        Controls.Add(New Label With {.Text="Especialidad",.Left=20,.Top=70})
        txtEspecialidad.SetBounds(120,65,220,28) : Controls.Add(txtEspecialidad)
        Controls.Add(New Label With {.Text="Colegiado",.Left=20,.Top=110})
        txtColegiado.SetBounds(120,105,220,28) : Controls.Add(txtColegiado)
        btn.Text="Guardar" : btn.SetBounds(120,150,220,35) : Controls.Add(btn)
        AddHandler btn.Click, AddressOf Guardar
        grid.SetBounds(380,25,430,380) : grid.ReadOnly=True : grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : Controls.Add(grid)
        Cargar()
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If txtNombre.Text.Trim()="" Then MessageBox.Show("Ingrese el nombre.") : Return
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="INSERT INTO Medicos(Nombre,Especialidad,Colegiado) VALUES($n,$e,$c)"
                cmd.Parameters.AddWithValue("$n",txtNombre.Text.Trim())
                cmd.Parameters.AddWithValue("$e",txtEspecialidad.Text.Trim())
                cmd.Parameters.AddWithValue("$c",txtColegiado.Text.Trim())
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Registro de médico", "Medicos", txtNombre.Text.Trim())
        MessageBox.Show("Médico registrado.") : txtNombre.Clear() : txtEspecialidad.Clear() : txtColegiado.Clear() : Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Nombre,Especialidad,Colegiado FROM Medicos WHERE Activo=1 ORDER BY Nombre"
                Using rd=cmd.ExecuteReader() : dt.Load(rd) : End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub
End Class