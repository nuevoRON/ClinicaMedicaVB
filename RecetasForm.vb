Imports Microsoft.Data.Sqlite
Imports System.Drawing
Imports System.Drawing.Printing

Public Class RecetasForm
    Inherits Form

    Private paciente As New ComboBox()
    Private consulta As New ComboBox()
    Private medicamento As New TextBox()
    Private dosis As New TextBox()
    Private frecuencia As New TextBox()
    Private duracion As New TextBox()
    Private indicaciones As New TextBox()
    Private grid As New DataGridView()
    Private btnGuardar As New Button()
    Private btnImprimir As New Button()
    Private recetaSeleccionada As Integer = 0

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Recetas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Recetas", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text="Recetas médicas"
        Width=1100 : Height=700
        StartPosition=FormStartPosition.CenterParent

        AddField("Paciente",paciente,20)
        AddField("Consulta",consulta,60)
        AddField("Medicamento",medicamento,100)
        AddField("Dosis",dosis,140)
        AddField("Frecuencia",frecuencia,180)
        AddField("Duración",duracion,220)
        AddField("Indicaciones",indicaciones,260)

        btnGuardar.Text="Agregar medicamento"
        btnGuardar.SetBounds(150,310,180,35)
        Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click, AddressOf Guardar

        btnImprimir.Text="Imprimir receta"
        btnImprimir.SetBounds(340,310,180,35)
        Controls.Add(btnImprimir)
        AddHandler btnImprimir.Click, AddressOf Imprimir

        grid.SetBounds(20,365,1030,240)
        grid.ReadOnly=True
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        Controls.Add(grid)
        AddHandler grid.CellClick, AddressOf SeleccionarReceta

        AddHandler paciente.SelectedIndexChanged, AddressOf CargarConsultas
        CargarPacientes()
        CargarRecetas()
    End Sub

    Private Sub AddField(t As String,c As Control,y As Integer)
        Controls.Add(New Label With {.Text=t,.Left=20,.Top=y+5,.Width=120})
        c.SetBounds(150,y,370,28)
        Controls.Add(c)
    End Sub

    Private Sub CargarPacientes()
        paciente.Items.Clear()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Identidad,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos,Nombre"
                Using rd=cmd.ExecuteReader()
                    While rd.Read()
                        paciente.Items.Add(New Item(CInt(rd("Id")),rd("Nombre").ToString() & " - " & rd("Identidad").ToString()))
                    End While
                End Using
            End Using
        End Using
        If paciente.Items.Count>0 Then paciente.SelectedIndex=0
    End Sub

    Private Sub CargarConsultas(sender As Object,e As EventArgs)
        consulta.Items.Clear()
        If paciente.SelectedItem Is Nothing Then Return
        Dim p=DirectCast(paciente.SelectedItem,Item)
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,FechaHora,Diagnostico FROM Consultas WHERE PacienteId=$p ORDER BY FechaHora DESC"
                cmd.Parameters.AddWithValue("$p",p.Id)
                Using rd=cmd.ExecuteReader()
                    While rd.Read()
                        consulta.Items.Add(New Item(CInt(rd("Id")),rd("FechaHora").ToString() & " - " & rd("Diagnostico").ToString()))
                    End While
                End Using
            End Using
        End Using
        If consulta.Items.Count>0 Then consulta.SelectedIndex=0
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If consulta.SelectedItem Is Nothing Then
            MessageBox.Show("Seleccione una consulta.")
            Return
        End If
        If medicamento.Text.Trim()="" Then
            MessageBox.Show("Ingrese el medicamento.")
            Return
        End If
        Dim c=DirectCast(consulta.SelectedItem,Item)
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="INSERT INTO Recetas(ConsultaId,Fecha,Medicamento,Dosis,Frecuencia,Duracion,Indicaciones) VALUES($c,$f,$m,$d,$fr,$du,$i)"
                cmd.Parameters.AddWithValue("$c",c.Id)
                cmd.Parameters.AddWithValue("$f",DateTime.Now.ToString("s"))
                cmd.Parameters.AddWithValue("$m",medicamento.Text.Trim())
                cmd.Parameters.AddWithValue("$d",dosis.Text.Trim())
                cmd.Parameters.AddWithValue("$fr",frecuencia.Text.Trim())
                cmd.Parameters.AddWithValue("$du",duracion.Text.Trim())
                cmd.Parameters.AddWithValue("$i",indicaciones.Text.Trim())
                cmd.ExecuteNonQuery()
            End Using
        End Using
        MessageBox.Show("Medicamento agregado a la receta.")
        medicamento.Clear() : dosis.Clear() : frecuencia.Clear() : duracion.Clear() : indicaciones.Clear()
        CargarRecetas()
    End Sub

    Private Sub CargarRecetas()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT R.Id,R.Fecha,P.Nombre||' '||P.Apellidos AS Paciente,C.FechaHora AS Consulta,R.Medicamento,R.Dosis,R.Frecuencia,R.Duracion,R.Indicaciones FROM Recetas R JOIN Consultas C ON C.Id=R.ConsultaId JOIN Pacientes P ON P.Id=C.PacienteId ORDER BY R.Id DESC"
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource=dt
    End Sub

    Private Sub SeleccionarReceta(sender As Object,e As DataGridViewCellEventArgs)
        If e.RowIndex>=0 Then
            recetaSeleccionada=Convert.ToInt32(grid.Rows(e.RowIndex).Cells("Id").Value)
        End If
    End Sub

    Private Sub Imprimir(sender As Object,e As EventArgs)
        If recetaSeleccionada=0 Then
            MessageBox.Show("Seleccione una receta en la lista.")
            Return
        End If

        Dim texto=""
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT P.Nombre||' '||P.Apellidos AS Paciente,P.Identidad,R.Fecha,R.Medicamento,R.Dosis,R.Frecuencia,R.Duracion,R.Indicaciones,M.Nombre AS Medico,M.Especialidad,C.Diagnostico FROM Recetas R JOIN Consultas C ON C.Id=R.ConsultaId JOIN Pacientes P ON P.Id=C.PacienteId LEFT JOIN Medicos M ON M.Id=C.MedicoId WHERE R.Id=$id"
                cmd.Parameters.AddWithValue("$id",recetaSeleccionada)
                Using rd=cmd.ExecuteReader()
                    If rd.Read() Then
                        texto="RECETA MÉDICA" & Environment.NewLine & Environment.NewLine &
                            "Paciente: " & rd("Paciente").ToString() & Environment.NewLine &
                            "Identidad: " & rd("Identidad").ToString() & Environment.NewLine &
                            "Fecha: " & rd("Fecha").ToString() & Environment.NewLine &
                            "Diagnóstico: " & rd("Diagnostico").ToString() & Environment.NewLine & Environment.NewLine &
                            "Medicamento: " & rd("Medicamento").ToString() & Environment.NewLine &
                            "Dosis: " & rd("Dosis").ToString() & Environment.NewLine &
                            "Frecuencia: " & rd("Frecuencia").ToString() & Environment.NewLine &
                            "Duración: " & rd("Duracion").ToString() & Environment.NewLine &
                            "Indicaciones: " & rd("Indicaciones").ToString() & Environment.NewLine & Environment.NewLine &
                            "Médico: " & rd("Medico").ToString() & Environment.NewLine &
                            "Especialidad: " & rd("Especialidad").ToString()
                    End If
                End Using
            End Using
        End Using

        Dim pd As New PrintDocument()
        AddHandler pd.PrintPage, Sub(s,args)
            Using f As New Font("Segoe UI",11)
                args.Graphics.DrawString(texto,f,Brushes.Black,60,60)
            End Using
        End Sub
        Dim dlg As New PrintPreviewDialog()
        dlg.Document=pd
        dlg.Width=900 : dlg.Height=700
        dlg.ShowDialog()
    End Sub

    Private Class Item
        Public ReadOnly Id As Integer
        Private ReadOnly Texto As String
        Public Sub New(id As Integer,texto As String)
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Recetas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Recetas", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
            Me.Id=id : Me.Texto=texto
        End Sub
        Public Overrides Function ToString() As String
            Return Texto
        End Function
    End Class
End Class