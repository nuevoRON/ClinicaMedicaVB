Imports Microsoft.Data.Sqlite

Public Class MedicosForm
    Inherits Form
    Private grid As New DataGridView()
    Private txtNombre As New TextBox()
    Private txtEspecialidad As New TextBox()
    Private txtColegiado As New TextBox()
    Private btn As New Button()
    Private btnEditar As New Button()
    Private btnBaja As New Button()
    Private btnReactivar As New Button()
    Private medicoEditandoId As Integer = 0
    Private txtTelefono As New TextBox()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Medicos") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Medicos", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text="Gestión de médicos" : Width=1050 : Height=620 : MinimumSize=New Size(950,560) : StartPosition=FormStartPosition.CenterParent
        BackColor=Color.FromArgb(245,247,250) : Font=New Font("Segoe UI",9)
        Controls.Add(New Label With {.Text="Nombre",.Left=20,.Top=30})
        txtNombre.SetBounds(120,25,220,28) : Controls.Add(txtNombre)
        Controls.Add(New Label With {.Text="Especialidad",.Left=20,.Top=70})
        txtEspecialidad.SetBounds(120,65,220,28) : Controls.Add(txtEspecialidad)
        Controls.Add(New Label With {.Text="Colegiado",.Left=20,.Top=110})
        txtColegiado.SetBounds(120,105,220,28) : Controls.Add(txtColegiado)
        Controls.Add(New Label With {.Text="Teléfono",.Left=20,.Top=150})
        txtTelefono.SetBounds(120,145,220,28) : Controls.Add(txtTelefono)
        btn.Text="Guardar médico" : btn.SetBounds(120,185,220,35) : Controls.Add(btn)
        btn.BackColor=Color.FromArgb(35,99,160) : btn.ForeColor=Color.White : btn.FlatStyle=FlatStyle.Flat : btn.FlatAppearance.BorderSize=0
        AddHandler btn.Click, AddressOf Guardar
        grid.SetBounds(380,25,620,380) : grid.ReadOnly=True : grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect : grid.MultiSelect=False : Controls.Add(grid)
        btnEditar.Text="Actualizar seleccionado" : btnEditar.SetBounds(380,425,210,35) : Controls.Add(btnEditar)
        btnBaja.Text="Dar de baja" : btnBaja.SetBounds(600,425,120,35) : Controls.Add(btnBaja)
        btnReactivar.Text="Reactivar" : btnReactivar.SetBounds(730,425,120,35) : Controls.Add(btnReactivar)
        AddHandler btnEditar.Click, AddressOf CargarSeleccionado
        AddHandler btnBaja.Click, AddressOf DarDeBaja
        AddHandler btnReactivar.Click, AddressOf Reactivar
        AddHandler grid.CellDoubleClick, AddressOf CargarSeleccionado
        AddHandler grid.SelectionChanged, Sub(sender As Object,e As EventArgs) ActualizarBotones()
        Cargar()
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If txtNombre.Text.Trim()="" Then MessageBox.Show("Ingrese el nombre.") : Return
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                If medicoEditandoId=0 Then
                    cmd.CommandText="INSERT INTO Medicos(Nombre,Especialidad,Colegiado,Telefono) VALUES($n,$e,$c,$t)"
                Else
                    cmd.CommandText="UPDATE Medicos SET Nombre=$n,Especialidad=$e,Colegiado=$c,Telefono=$t WHERE Id=$id"
                    cmd.Parameters.AddWithValue("$id",medicoEditandoId)
                End If
                cmd.Parameters.AddWithValue("$n",txtNombre.Text.Trim())
                cmd.Parameters.AddWithValue("$e",txtEspecialidad.Text.Trim())
                cmd.Parameters.AddWithValue("$c",txtColegiado.Text.Trim())
                cmd.Parameters.AddWithValue("$t",txtTelefono.Text.Trim())
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, If(medicoEditandoId=0,"Registro de médico","Modificación de médico"), "Medicos", txtNombre.Text.Trim())
        MessageBox.Show(If(medicoEditandoId=0,"Médico registrado.","Datos del médico actualizados."))
        LimpiarEdicion() : Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Nombre,Especialidad,Colegiado,Telefono,CASE WHEN Activo=1 THEN 'Activo' ELSE 'Inactivo' END AS Estado FROM Medicos ORDER BY Activo DESC,Nombre"
                Using rd=cmd.ExecuteReader() : dt.Load(rd) : End Using
            End Using
        End Using
        grid.DataSource=dt
        If grid.Columns.Contains("Id") Then grid.Columns("Id").Visible=False
    End Sub

    Private Sub CargarSeleccionado(sender As Object,e As EventArgs)
        If grid.SelectedRows.Count=0 Then Return
        Dim id=Convert.ToInt32(grid.SelectedRows(0).Cells("Id").Value)
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Nombre,Especialidad,Colegiado,Telefono FROM Medicos WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id",id)
                Using rd=cmd.ExecuteReader()
                    If rd.Read() Then
                        medicoEditandoId=id
                        txtNombre.Text=If(rd("Nombre") Is DBNull.Value,"",rd("Nombre").ToString())
                        txtEspecialidad.Text=If(rd("Especialidad") Is DBNull.Value,"",rd("Especialidad").ToString())
                        txtColegiado.Text=If(rd("Colegiado") Is DBNull.Value,"",rd("Colegiado").ToString())
                        txtTelefono.Text=If(rd("Telefono") Is DBNull.Value,"",rd("Telefono").ToString())
                    End If
                End Using
            End Using
        End Using
        btn.Text="Guardar cambios"
    End Sub

    Private Sub DarDeBaja(sender As Object,e As EventArgs)
        If grid.SelectedRows.Count=0 Then Return
        Dim fila=grid.SelectedRows(0)
        If fila.Cells("Estado").Value.ToString()<>"Activo" Then Return
        Dim id=Convert.ToInt32(fila.Cells("Id").Value)
        Dim nombre=fila.Cells("Nombre").Value.ToString()
        If MessageBox.Show("¿Desea dar de baja a "&nombre&"? El registro se conservará para mantener el historial.","Confirmar baja",MessageBoxButtons.YesNo,MessageBoxIcon.Question)<>DialogResult.Yes Then Return
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="UPDATE Medicos SET Activo=0 WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id",id)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser,Session.CurrentRole,"Baja de médico","Medicos","Id="&id.ToString())
        Cargar()
    End Sub

    Private Sub Reactivar(sender As Object,e As EventArgs)
        If grid.SelectedRows.Count=0 Then Return
        Dim fila=grid.SelectedRows(0)
        If fila.Cells("Estado").Value.ToString()<>"Inactivo" Then Return
        Dim id=Convert.ToInt32(fila.Cells("Id").Value)
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="UPDATE Medicos SET Activo=1 WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id",id)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser,Session.CurrentRole,"Reactivación de médico","Medicos","Id="&id.ToString())
        Cargar()
    End Sub

    Private Sub LimpiarEdicion()
        medicoEditandoId=0
        txtNombre.Clear() : txtEspecialidad.Clear() : txtColegiado.Clear() : txtTelefono.Clear()
        btn.Text="Guardar médico"
    End Sub

    Private Sub ActualizarBotones()
        If grid.SelectedRows.Count=0 Then
            btnEditar.Enabled=False : btnBaja.Enabled=False : btnReactivar.Enabled=False : Return
        End If
        btnEditar.Enabled=True
        Dim activo=grid.SelectedRows(0).Cells("Estado").Value.ToString()="Activo"
        btnBaja.Enabled=activo : btnReactivar.Enabled=Not activo
    End Sub
End Class