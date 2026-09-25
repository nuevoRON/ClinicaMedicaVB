Imports Microsoft.Data.Sqlite
Imports System.Drawing

Public Class UsuariosForm
    Inherits Form

    Private txtUsuario As New TextBox()
    Private txtClave As New TextBox()
    Private cmbRol As New ComboBox()
    Private chkActivo As New CheckBox()
    Private dgv As New DataGridView()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Usuarios") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Usuarios", "Usuarios")
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text = "Administración de usuarios"
        Width = 760 : Height = 520
        StartPosition = FormStartPosition.CenterParent

        Dim lblUsuario As New Label With {.Text="Usuario:", .Left=25, .Top=25, .AutoSize=True}
        txtUsuario.SetBounds(110,20,180,28)

        Dim lblClave As New Label With {.Text="Contraseña:", .Left=310, .Top=25, .AutoSize=True}
        txtClave.SetBounds(390,20,180,28)
        txtClave.PasswordChar="*"c

        Dim lblRol As New Label With {.Text="Rol:", .Left=25, .Top=70, .AutoSize=True}
        cmbRol.SetBounds(110,65,180,28)
        cmbRol.DropDownStyle=ComboBoxStyle.DropDownList
        cmbRol.Items.AddRange({"Administrador","Médico","Recepción"})
        cmbRol.SelectedIndex=1

        chkActivo.Text="Usuario activo"
        chkActivo.Checked=True
        chkActivo.SetBounds(310,65,150,28)

        Dim btnGuardar As New Button With {.Text="Guardar usuario",.Left=25,.Top=110,.Width=140,.Height=35}
        Dim btnDesactivar As New Button With {.Text="Desactivar seleccionado",.Left=180,.Top=110,.Width=170,.Height=35}
        Dim btnActivar As New Button With {.Text="Activar seleccionado",.Left=365,.Top=110,.Width=150,.Height=35}
        Dim btnLimpiar As New Button With {.Text="Limpiar",.Left=530,.Top=110,.Width=100,.Height=35}

        dgv.SetBounds(25,165,680,290)
        dgv.ReadOnly=True
        dgv.AllowUserToAddRows=False
        dgv.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        dgv.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill

        AddHandler btnGuardar.Click, AddressOf Guardar
        AddHandler btnDesactivar.Click, Sub() CambiarEstado(False)
        AddHandler btnActivar.Click, Sub() CambiarEstado(True)
        AddHandler btnLimpiar.Click, Sub() Limpiar()

        Controls.AddRange({lblUsuario,txtUsuario,lblClave,txtClave,lblRol,cmbRol,chkActivo,btnGuardar,btnDesactivar,btnActivar,btnLimpiar,dgv})
        CargarUsuarios()
    End Sub

    Private Sub CargarUsuarios()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id, Usuario, Rol, CASE WHEN Activo=1 THEN 'Sí' ELSE 'No' END AS Activo FROM Usuarios ORDER BY Usuario"
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        dgv.DataSource=dt
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        Dim usuario=txtUsuario.Text.Trim()
        Dim clave=txtClave.Text
        If usuario="" OrElse clave="" Then
            MessageBox.Show("Ingrese usuario y contraseña.","Validación")
            Return
        End If

        Try
            Using cn=Database.Connection()
                Using cmd=cn.CreateCommand()
                    cmd.CommandText="INSERT INTO Usuarios(Usuario,Clave,Rol,Activo) VALUES($u,$c,$r,$a)"
                    cmd.Parameters.AddWithValue("$u",usuario)
                    cmd.Parameters.AddWithValue("$c",SecurityHelper.HashPassword(clave))
                    cmd.Parameters.AddWithValue("$r",cmbRol.Text)
                    cmd.Parameters.AddWithValue("$a",If(chkActivo.Checked,1,0))
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Creación de usuario", "Usuarios", usuario & " | Rol: " & cmbRol.Text)
            MessageBox.Show("Usuario registrado correctamente.")
            CargarUsuarios()
            Limpiar()
        Catch ex As SqliteException
            MessageBox.Show("No se pudo registrar. Verifique que el nombre de usuario no esté repetido.","Error")
        End Try
    End Sub

    Private Sub CambiarEstado(activo As Boolean)
        If dgv.SelectedRows.Count=0 Then
            MessageBox.Show("Seleccione un usuario.")
            Return
        End If

        Dim id=Convert.ToInt32(dgv.SelectedRows(0).Cells("Id").Value)
        Dim nombre=dgv.SelectedRows(0).Cells("Usuario").Value.ToString()

        If nombre.Equals(Session.CurrentUser, StringComparison.OrdinalIgnoreCase) AndAlso Not activo Then
            MessageBox.Show("No puede desactivar el usuario con el que inició sesión.")
            Return
        End If

        If nombre.Equals("admin",StringComparison.OrdinalIgnoreCase) AndAlso Not activo Then
            MessageBox.Show("El usuario administrador principal no puede desactivarse.")
            Return
        End If

        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="UPDATE Usuarios SET Activo=$a WHERE Id=$id"
                cmd.Parameters.AddWithValue("$a",If(activo,1,0))
                cmd.Parameters.AddWithValue("$id",id)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, If(activo, "Activación de usuario", "Desactivación de usuario"), "Usuarios", nombre)
        CargarUsuarios()
    End Sub

    Private Sub Limpiar()
        txtUsuario.Clear()
        txtClave.Clear()
        cmbRol.SelectedIndex=1
        chkActivo.Checked=True
        txtUsuario.Focus()
    End Sub
End Class