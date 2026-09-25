Imports Microsoft.Data.Sqlite

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
    Private btn As New Button()
    Private grid As New DataGridView()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Consultas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Consultas", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text="Consultas médicas" : Width=1100 : Height=700 : StartPosition=FormStartPosition.CenterParent
        Dim y=20
        AddField("Paciente",paciente,y,True) : y+=38
        AddField("Médico",medico,y,True) : y+=38
        AddField("Motivo",motivo,y) : y+=38
        AddField("Presión",presion,y) : y+=38
        AddField("Temperatura",temperatura,y) : y+=38
        AddField("Frec. cardiaca",frecuencia,y) : y+=38
        AddField("Saturación",saturacion,y) : y+=38
        AddField("Peso",peso,y) : y+=38
        AddField("Diagnóstico",diagnostico,y) : y+=38
        AddField("Tratamiento",tratamiento,y) : y+=38
        AddField("Observaciones",observaciones,y) : y+=38
        btn.Text="Guardar consulta" : btn.SetBounds(150,y,220,35) : Controls.Add(btn) : AddHandler btn.Click, AddressOf Guardar
        grid.SetBounds(520,20,540,560) : grid.ReadOnly=True : grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : Controls.Add(grid)
        CargarCombos() : Cargar()
    End Sub

    Private Sub AddField(t As String,c As Control,y As Integer, Optional combo As Boolean=False)
        Controls.Add(New Label With {.Text=t,.Left=20,.Top=y+5,.Width=120})
        c.SetBounds(150,y,330,28) : Controls.Add(c)
    End Sub

    Private Sub CargarCombos()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos"
                Using rd=cmd.ExecuteReader()
                    While rd.Read() : paciente.Items.Add(New ComboItem(CInt(rd("Id")),rd("Nombre").ToString())) : End While
                End Using
            End Using
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Nombre FROM Medicos WHERE Activo=1 ORDER BY Nombre"
                Using rd=cmd.ExecuteReader()
                    While rd.Read() : medico.Items.Add(New ComboItem(CInt(rd("Id")),rd("Nombre").ToString())) : End While
                End Using
            End Using
        End Using
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If paciente.SelectedItem Is Nothing Then MessageBox.Show("Seleccione paciente.") : Return
        Dim p=DirectCast(paciente.SelectedItem,ComboItem)
        Dim mid As Object=DBNull.Value
        If medico.SelectedItem IsNot Nothing Then mid=DirectCast(medico.SelectedItem,ComboItem).Id
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="INSERT INTO Consultas(PacienteId,MedicoId,FechaHora,Motivo,Presion,Temperatura,FrecuenciaCardiaca,Saturacion,Peso,Diagnostico,Tratamiento,Observaciones) VALUES($p,$m,$f,$mo,$pr,$te,$fc,$sa,$pe,$di,$tr,$ob)"
                cmd.Parameters.AddWithValue("$p",p.Id) : cmd.Parameters.AddWithValue("$m",mid)
                cmd.Parameters.AddWithValue("$f",DateTime.Now.ToString("s")) : cmd.Parameters.AddWithValue("$mo",motivo.Text)
                cmd.Parameters.AddWithValue("$pr",presion.Text) : cmd.Parameters.AddWithValue("$te",temperatura.Text)
                cmd.Parameters.AddWithValue("$fc",frecuencia.Text) : cmd.Parameters.AddWithValue("$sa",saturacion.Text)
                cmd.Parameters.AddWithValue("$pe",peso.Text) : cmd.Parameters.AddWithValue("$di",diagnostico.Text)
                cmd.Parameters.AddWithValue("$tr",tratamiento.Text) : cmd.Parameters.AddWithValue("$ob",observaciones.Text)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Registro de consulta", "Consultas", "PacienteId=" & p.Id.ToString())
        MessageBox.Show("Consulta registrada.") : Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT C.Id,P.Nombre||' '||P.Apellidos AS Paciente,C.FechaHora,C.Diagnostico,C.Tratamiento FROM Consultas C JOIN Pacientes P ON P.Id=C.PacienteId ORDER BY C.FechaHora DESC"
                Using rd=cmd.ExecuteReader() : dt.Load(rd) : End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub

    Private Class ComboItem
        Public Property Id As Integer
        Public Property Nombre As String
        Public Sub New(i As Integer,n As String)
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Consultas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Consultas", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If : Id=i : Nombre=n : End Sub
        Public Overrides Function ToString() As String : Return Nombre : End Function
    End Class
End Class