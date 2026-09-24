Imports Microsoft.Data.Sqlite

Public Class CitasForm
    Inherits Form
    Private paciente As New ComboBox()
    Private medico As New ComboBox()
    Private fecha As New DateTimePicker()
    Private motivo As New TextBox()
    Private estado As New ComboBox()
    Private grid As New DataGridView()
    Private btn As New Button()

    Public Sub New()
        Text="Citas" : Width=1000 : Height=600 : StartPosition=FormStartPosition.CenterParent
        Controls.Add(New Label With {.Text="Paciente",.Left=20,.Top=25})
        paciente.SetBounds(100,20,260,28) : Controls.Add(paciente)
        Controls.Add(New Label With {.Text="Médico",.Left=20,.Top=65})
        medico.SetBounds(100,60,260,28) : Controls.Add(medico)
        Controls.Add(New Label With {.Text="Fecha y hora",.Left=20,.Top=105})
        fecha.SetBounds(100,100,260,28) : fecha.Format=DateTimePickerFormat.Custom : fecha.CustomFormat="dd/MM/yyyy HH:mm" : fecha.ShowUpDown=True : Controls.Add(fecha)
        Controls.Add(New Label With {.Text="Motivo",.Left=20,.Top=145})
        motivo.SetBounds(100,140,260,28) : Controls.Add(motivo)
        estado.Items.AddRange({"Pendiente","Confirmada","Atendida","Cancelada"}) : estado.SelectedIndex=0
        Controls.Add(New Label With {.Text="Estado",.Left=20,.Top=185}) : estado.SetBounds(100,180,260,28) : Controls.Add(estado)
        btn.Text="Guardar cita" : btn.SetBounds(100,225,260,35) : Controls.Add(btn) : AddHandler btn.Click, AddressOf Guardar
        grid.SetBounds(400,20,550,450) : grid.ReadOnly=True : grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : Controls.Add(grid)
        CargarCombos() : Cargar()
    End Sub

    Private Sub CargarCombos()
        paciente.Items.Clear() : medico.Items.Clear()
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
        If paciente.SelectedItem Is Nothing Then MessageBox.Show("Seleccione un paciente.") : Return
        Dim p=DirectCast(paciente.SelectedItem,ComboItem)
        Dim mid As Object=DBNull.Value
        If medico.SelectedItem IsNot Nothing Then mid=DirectCast(medico.SelectedItem,ComboItem).Id
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="INSERT INTO Citas(PacienteId,MedicoId,FechaHora,Motivo,Estado) VALUES($p,$m,$f,$mo,$e)"
                cmd.Parameters.AddWithValue("$p",p.Id)
                cmd.Parameters.AddWithValue("$m",mid)
                cmd.Parameters.AddWithValue("$f",fecha.Value.ToString("s"))
                cmd.Parameters.AddWithValue("$mo",motivo.Text)
                cmd.Parameters.AddWithValue("$e",estado.Text)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        MessageBox.Show("Cita registrada.") : Cargar()
    End Sub

    Private Sub Cargar()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT C.Id,P.Nombre||' '||P.Apellidos AS Paciente,M.Nombre AS Medico,C.FechaHora,C.Motivo,C.Estado FROM Citas C JOIN Pacientes P ON P.Id=C.PacienteId LEFT JOIN Medicos M ON M.Id=C.MedicoId ORDER BY C.FechaHora DESC"
                Using rd=cmd.ExecuteReader() : dt.Load(rd) : End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub

    Private Class ComboItem
        Public Property Id As Integer
        Public Property Nombre As String
        Public Sub New(i As Integer,n As String) : Id=i : Nombre=n : End Sub
        Public Overrides Function ToString() As String : Return Nombre : End Function
    End Class
End Class