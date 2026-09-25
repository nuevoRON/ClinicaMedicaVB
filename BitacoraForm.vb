Imports Microsoft.Data.Sqlite
Imports System.Drawing

Public Class BitacoraForm
    Inherits Form
    Private dgv As New DataGridView()
    Private txtUsuario As New TextBox()
    Private txtModulo As New TextBox()
    Private txtAccion As New TextBox()
    Private dtDesde As New DateTimePicker()
    Private dtHasta As New DateTimePicker()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Bitacora") Then
            MessageBox.Show("Solo un Administrador puede consultar la bitácora.", "Acceso restringido")
            BeginInvoke(New Action(Sub() Close()))
            Return
        End If
        Text = "Bitácora de auditoría"
        Width = 1100 : Height = 700
        StartPosition = FormStartPosition.CenterParent

        Dim labels = {"Usuario","Módulo","Acción"}
        Dim boxes = {txtUsuario,txtModulo,txtAccion}
        For i=0 To 2
            Controls.Add(New Label With {.Text=labels(i),.Left=20+i*230,.Top=20,.AutoSize=True})
            boxes(i).SetBounds(20+i*230,45,200,28)
            Controls.Add(boxes(i))
        Next
        Controls.Add(New Label With {.Text="Desde",.Left=710,.Top=20,.AutoSize=True})
        dtDesde.SetBounds(710,45,150,28) : dtDesde.Value=DateTime.Today.AddDays(-30) : Controls.Add(dtDesde)
        Controls.Add(New Label With {.Text="Hasta",.Left=875,.Top=20,.AutoSize=True})
        dtHasta.SetBounds(875,45,150,28) : dtHasta.Value=DateTime.Today : Controls.Add(dtHasta)
        Dim btnBuscar As New Button With {.Text="Filtrar",.Left=20,.Top=85,.Width=100,.Height=32}
        Dim btnTodos As New Button With {.Text="Mostrar todo",.Left=130,.Top=85,.Width=110,.Height=32}
        AddHandler btnBuscar.Click, AddressOf Cargar
        AddHandler btnTodos.Click, Sub() txtUsuario.Clear() : txtModulo.Clear() : txtAccion.Clear() : Cargar()
        Controls.Add(btnBuscar) : Controls.Add(btnTodos)
        dgv.SetBounds(20,130,1040,490)
        dgv.ReadOnly=True : dgv.AllowUserToAddRows=False : dgv.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        Controls.Add(dgv)
        Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,FechaHora,Usuario,Rol,Accion,Modulo,Detalle FROM RegistroAcciones WHERE FechaHora >= $d AND FechaHora < $h AND Usuario LIKE $u AND Modulo LIKE $m AND Accion LIKE $a ORDER BY Id DESC"
                cmd.Parameters.AddWithValue("$d",dtDesde.Value.Date.ToString("s"))
                cmd.Parameters.AddWithValue("$h",dtHasta.Value.Date.AddDays(1).ToString("s"))
                cmd.Parameters.AddWithValue("$u","%" & txtUsuario.Text.Trim() & "%")
                cmd.Parameters.AddWithValue("$m","%" & txtModulo.Text.Trim() & "%")
                cmd.Parameters.AddWithValue("$a","%" & txtAccion.Text.Trim() & "%")
                Using rd=cmd.ExecuteReader() : dt.Load(rd) : End Using
            End Using
        End Using
        dgv.DataSource=dt
    End Sub
End Class
