Imports Microsoft.Data.Sqlite
Imports System.Drawing

Public Class ExpedienteForm
    Inherits Form

    Private paciente As New ComboBox()
    Private lblDatos As New Label()
    Private historial As New DataGridView()
    Private citas As New DataGridView()
    Private btnActualizar As New Button()

    Public Sub New(Optional pacienteIdInicial As Integer = 0)
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Expediente") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Expediente", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text = "Expediente clínico"
        Width = 1100
        Height = 700
        StartPosition = FormStartPosition.CenterParent

        Controls.Add(New Label With {.Text="Seleccione paciente:",.Left=20,.Top=22,.Width=130})
        paciente.SetBounds(155,18,430,30)
        Controls.Add(paciente)

        btnActualizar.Text="Actualizar expediente"
        btnActualizar.SetBounds(600,18,180,30)
        Controls.Add(btnActualizar)
        AddHandler btnActualizar.Click, AddressOf CargarExpediente
        AddHandler paciente.SelectedIndexChanged, AddressOf CargarExpediente

        lblDatos.SetBounds(20,65,1030,80)
        lblDatos.BorderStyle=BorderStyle.FixedSingle
        lblDatos.AutoSize=False
        Controls.Add(lblDatos)

        Dim tabs As New TabControl With {.Left=20,.Top=160,.Width=1030,.Height=450}
        Dim tabHist As New TabPage("Historial de consultas")
        Dim tabCitas As New TabPage("Citas")
        historial.Dock=DockStyle.Fill
        historial.ReadOnly=True
        historial.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        tabHist.Controls.Add(historial)
        citas.Dock=DockStyle.Fill
        citas.ReadOnly=True
        citas.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        tabCitas.Controls.Add(citas)
        tabs.TabPages.Add(tabHist)
        tabs.TabPages.Add(tabCitas)
        Controls.Add(tabs)

        CargarPacientes(pacienteIdInicial)
    End Sub

    Private Sub CargarPacientes(Optional pacienteIdInicial As Integer = 0)
        paciente.Items.Clear()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Identidad,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos,Nombre"
                Using rd=cmd.ExecuteReader()
                    While rd.Read()
                        paciente.Items.Add(New PacienteItem(
                            CInt(rd("Id")),
                            rd("Identidad").ToString(),
                            rd("Nombre").ToString()))
                    End While
                End Using
            End Using
        End Using
        If paciente.Items.Count>0 Then
            If pacienteIdInicial>0 Then
                For i As Integer=0 To paciente.Items.Count-1
                    If DirectCast(paciente.Items(i),PacienteItem).Id=pacienteIdInicial Then paciente.SelectedIndex=i : Exit For
                Next
            End If
            If paciente.SelectedIndex<0 Then paciente.SelectedIndex=0
        End If
    End Sub

    Private Sub CargarExpediente(sender As Object, e As EventArgs)
        If paciente.SelectedItem Is Nothing Then Return
        Dim p=DirectCast(paciente.SelectedItem,PacienteItem)

        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Identidad,Nombre,Apellidos,FechaNacimiento,Sexo,Telefono,Direccion,Alergias,Antecedentes FROM Pacientes WHERE Id=$id"
                cmd.Parameters.AddWithValue("$id",p.Id)
                Using rd=cmd.ExecuteReader()
                    If rd.Read() Then
                        lblDatos.Text=$"Identidad: {rd("Identidad")}    Nombre: {rd("Nombre")} {rd("Apellidos")}" &
                            Environment.NewLine &
                            $"Nacimiento: {rd("FechaNacimiento")}    Sexo: {rd("Sexo")}    Teléfono: {rd("Telefono")}" &
                            Environment.NewLine &
                            $"Dirección: {rd("Direccion")}" &
                            Environment.NewLine &
                            $"Alergias: {rd("Alergias")}    Antecedentes: {rd("Antecedentes")}"
                    End If
                End Using
            End Using
        End Using

        CargarHistorial(p.Id)
        CargarCitas(p.Id)
    End Sub

    Private Sub CargarHistorial(id As Integer)
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT C.FechaHora AS Fecha,M.Nombre AS Medico,C.Motivo,C.Presion,C.Temperatura,C.FrecuenciaCardiaca AS Frecuencia,C.Saturacion,C.Peso,C.Diagnostico,C.Tratamiento,C.Observaciones FROM Consultas C LEFT JOIN Medicos M ON M.Id=C.MedicoId WHERE C.PacienteId=$id ORDER BY C.FechaHora DESC"
                cmd.Parameters.AddWithValue("$id",id)
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        historial.DataSource=dt
    End Sub

    Private Sub CargarCitas(id As Integer)
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT C.FechaHora AS Fecha,M.Nombre AS Medico,C.Motivo,C.Estado,C.Observaciones FROM Citas C LEFT JOIN Medicos M ON M.Id=C.MedicoId WHERE C.PacienteId=$id ORDER BY C.FechaHora DESC"
                cmd.Parameters.AddWithValue("$id",id)
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        citas.DataSource=dt
    End Sub

    Private Class PacienteItem
        Public ReadOnly Id As Integer
        Private ReadOnly Identidad As String
        Private ReadOnly NombreCompleto As String
        Public Sub New(id As Integer,identidad As String,nombre As String)
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Expediente") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Expediente", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
            Me.Id=id : Me.Identidad=identidad : Me.NombreCompleto=nombre
        End Sub
        Public Overrides Function ToString() As String
            Return $"{NombreCompleto} - {Identidad}"
        End Function
    End Class
End Class