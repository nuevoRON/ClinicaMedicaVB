Imports Microsoft.Data.Sqlite
Imports System.Data
Imports System.Drawing
Imports System.Drawing.Printing

Public Class RecetasForm
    Inherits Form

    Private paciente As New ComboBox()
    Private medicamento As New TextBox()
    Private dosis As New TextBox()
    Private frecuencia As New TextBox()
    Private duracion As New TextBox()
    Private indicaciones As New TextBox()
    Private grid As New DataGridView()
    Private btnGuardar As New Button()
    Private btnImprimir As New Button()
    Private recetaSeleccionada As Integer = 0
    Private textoImpresion As String = ""

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Recetas") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Recetas", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub(sender As Object,e As EventArgs) Me.Close()
            Return
        End If

        Text="Emisión de recetas médicas"
        Width=1180 : Height=760 : MinimumSize=New Size(1050,680)
        StartPosition=FormStartPosition.CenterParent
        BackColor=Color.FromArgb(244,247,251)
        Font=New Font("Segoe UI",9)

        Dim header As New Panel With {.BackColor=Color.FromArgb(24,40,70),.Dock=DockStyle.Top,.Height=78}
        header.Controls.Add(New Label With {.Text="RECETAS MÉDICAS",.ForeColor=Color.White,.Font=New Font("Segoe UI",18,FontStyle.Bold),.AutoSize=True,.Location=New Point(22,10)})
        header.Controls.Add(New Label With {.Text="Elaboración y emisión de indicaciones farmacológicas",.ForeColor=Color.FromArgb(205,216,230),.AutoSize=True,.Location=New Point(25,47)})
        Controls.Add(header)

        Dim panel As New Panel With {.BackColor=Color.White,.Location=New Point(18,98),.Size=New Size(390,590),.BorderStyle=BorderStyle.FixedSingle}
        Controls.Add(panel)
        panel.Controls.Add(New Label With {.Text="Nueva prescripción",.Left=20,.Top=18,.AutoSize=True,.Font=New Font("Segoe UI Semibold",13),.ForeColor=Color.FromArgb(35,99,160)})
        AddField(panel,"Paciente",paciente,60,True)
        AddField(panel,"Medicamento",medicamento,112)
        AddField(panel,"Dosis",dosis,164)
        AddField(panel,"Frecuencia",frecuencia,216)
        AddField(panel,"Duración",duracion,268)
        panel.Controls.Add(New Label With {.Text="Indicaciones adicionales",.Left=20,.Top=323,.AutoSize=True,.Font=New Font("Segoe UI Semibold",9),.ForeColor=Color.FromArgb(45,55,72)})
        indicaciones.Multiline=True
        indicaciones.ScrollBars=ScrollBars.Vertical
        indicaciones.SetBounds(20,348,345,92)
        indicaciones.BorderStyle=BorderStyle.FixedSingle
        panel.Controls.Add(indicaciones)

        btnGuardar.Text="Emitir receta"
        btnGuardar.SetBounds(20,460,345,42)
        EstiloBoton(btnGuardar,Color.FromArgb(28,112,91),Color.White)
        panel.Controls.Add(btnGuardar)
        AddHandler btnGuardar.Click,AddressOf Guardar

        btnImprimir.Text="Vista previa / Imprimir seleccionada"
        btnImprimir.SetBounds(20,512,345,42)
        EstiloBoton(btnImprimir,Color.FromArgb(35,99,160),Color.White)
        panel.Controls.Add(btnImprimir)
        AddHandler btnImprimir.Click,AddressOf Imprimir

        Dim lista As New Panel With {.BackColor=Color.White,.Location=New Point(428,98),.Size=New Size(Width-466,590),.Anchor=AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,.BorderStyle=BorderStyle.FixedSingle}
        Controls.Add(lista)
        lista.Controls.Add(New Label With {.Text="Historial de recetas emitidas",.Left=18,.Top=18,.AutoSize=True,.Font=New Font("Segoe UI Semibold",13),.ForeColor=Color.FromArgb(45,55,72)})
        grid.SetBounds(18,55,lista.Width-36,lista.Height-75)
        grid.Anchor=AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grid.ReadOnly=True : grid.AllowUserToAddRows=False : grid.MultiSelect=False
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor=Color.White : grid.BorderStyle=BorderStyle.None : grid.RowHeadersVisible=False
        grid.EnableHeadersVisualStyles=False
        grid.ColumnHeadersDefaultCellStyle.BackColor=Color.FromArgb(31,55,83)
        grid.ColumnHeadersDefaultCellStyle.ForeColor=Color.White
        grid.ColumnHeadersDefaultCellStyle.Font=New Font("Segoe UI",9,FontStyle.Bold)
        grid.ColumnHeadersHeight=38 : grid.RowTemplate.Height=32
        grid.AlternatingRowsDefaultCellStyle.BackColor=Color.FromArgb(247,250,253)
        lista.Controls.Add(grid)
        AddHandler grid.SelectionChanged,Sub(sender As Object,e As EventArgs)
            recetaSeleccionada=0
            If grid.SelectedRows.Count>0 Then recetaSeleccionada=Convert.ToInt32(grid.SelectedRows(0).Cells("Id").Value)
        End Sub
        AddHandler grid.CellDoubleClick,AddressOf Imprimir

        CargarPacientes()
        CargarRecetas()
    End Sub

    Private Sub AddField(panel As Control,caption As String,c As Control,y As Integer,Optional isCombo As Boolean=False)
        panel.Controls.Add(New Label With {.Text=caption,.Left=20,.Top=y+6,.Width=115,.ForeColor=Color.FromArgb(45,55,72),.Font=New Font("Segoe UI Semibold",9)})
        c.SetBounds(140,y,225,30)
        If TypeOf c Is ComboBox Then DirectCast(c,ComboBox).DropDownStyle=ComboBoxStyle.DropDownList
        If TypeOf c Is TextBox Then DirectCast(c,TextBox).BorderStyle=BorderStyle.FixedSingle
        panel.Controls.Add(c)
    End Sub

    Private Sub EstiloBoton(b As Button,fondo As Color,texto As Color)
        b.BackColor=fondo : b.ForeColor=texto : b.FlatStyle=FlatStyle.Flat
        b.FlatAppearance.BorderSize=0 : b.Font=New Font("Segoe UI Semibold",9)
    End Sub

    Private Sub CargarPacientes()
        paciente.Items.Clear()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id,Identidad,Nombre||' '||Apellidos AS Nombre FROM Pacientes ORDER BY Apellidos,Nombre"
                Using rd=cmd.ExecuteReader()
                    While rd.Read()
                        paciente.Items.Add(New Item(CInt(rd("Id")),rd("Nombre").ToString() & " · " & rd("Identidad").ToString()))
                    End While
                End Using
            End Using
        End Using
        If paciente.Items.Count>0 Then paciente.SelectedIndex=0
    End Sub

    Private Sub Guardar(sender As Object,e As EventArgs)
        If paciente.SelectedItem Is Nothing OrElse String.IsNullOrWhiteSpace(medicamento.Text) OrElse String.IsNullOrWhiteSpace(dosis.Text) OrElse String.IsNullOrWhiteSpace(frecuencia.Text) OrElse String.IsNullOrWhiteSpace(duracion.Text) OrElse String.IsNullOrWhiteSpace(indicaciones.Text) Then
            MessageBox.Show("Seleccione un paciente y complete medicamento, dosis, frecuencia, duración e indicaciones.", "Datos requeridos", MessageBoxButtons.OK,MessageBoxIcon.Warning)
            Return
        End If

        Dim p=DirectCast(paciente.SelectedItem,Item)
        Dim consultaId As Integer
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT Id FROM Consultas WHERE PacienteId=$p ORDER BY FechaHora DESC,Id DESC LIMIT 1"
                cmd.Parameters.AddWithValue("$p",p.Id)
                Dim resultado=cmd.ExecuteScalar()
                If resultado Is Nothing OrElse resultado Is DBNull.Value Then
                    MessageBox.Show("Este paciente no tiene una consulta clínica registrada. Registre primero la consulta para vincular correctamente la receta.", "Consulta requerida",MessageBoxButtons.OK,MessageBoxIcon.Warning)
                    Return
                End If
                consultaId=Convert.ToInt32(resultado)
            End Using
            Using cmd=cn.CreateCommand()
                cmd.CommandText="INSERT INTO Recetas(ConsultaId,Fecha,Medicamento,Dosis,Frecuencia,Duracion,Indicaciones) VALUES($c,$f,$m,$d,$fr,$du,$i)"
                cmd.Parameters.AddWithValue("$c",consultaId)
                cmd.Parameters.AddWithValue("$f",DateTime.Now.ToString("s"))
                cmd.Parameters.AddWithValue("$m",medicamento.Text.Trim())
                cmd.Parameters.AddWithValue("$d",dosis.Text.Trim())
                cmd.Parameters.AddWithValue("$fr",frecuencia.Text.Trim())
                cmd.Parameters.AddWithValue("$du",duracion.Text.Trim())
                cmd.Parameters.AddWithValue("$i",indicaciones.Text.Trim())
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Database.RegistrarAccion(Session.CurrentUser,Session.CurrentRole,"Emisión de receta","Recetas","PacienteId=" & p.Id.ToString())
        MessageBox.Show("Receta registrada. Selecciónela en el historial para revisar o imprimir.", "Receta médica",MessageBoxButtons.OK,MessageBoxIcon.Information)
        medicamento.Clear() : dosis.Clear() : frecuencia.Clear() : duracion.Clear() : indicaciones.Clear()
        CargarRecetas()
    End Sub

    Private Sub CargarRecetas()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT R.Id,R.Fecha,P.Nombre||' '||P.Apellidos AS Paciente,R.Medicamento,R.Dosis,R.Frecuencia,R.Duracion FROM Recetas R JOIN Consultas C ON C.Id=R.ConsultaId JOIN Pacientes P ON P.Id=C.PacienteId ORDER BY R.Id DESC"
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource=dt
        If grid.Columns.Contains("Id") Then grid.Columns("Id").Visible=False
    End Sub

    Private Sub Imprimir(sender As Object,e As EventArgs)
        If recetaSeleccionada=0 Then
            MessageBox.Show("Seleccione una receta del historial.")
            Return
        End If

        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT P.Nombre||' '||P.Apellidos AS Paciente,P.Identidad,P.FechaNacimiento,R.Fecha,R.Medicamento,R.Dosis,R.Frecuencia,R.Duracion,R.Indicaciones,M.Nombre AS Medico,M.Especialidad,M.Colegiado,C.Diagnostico FROM Recetas R JOIN Consultas C ON C.Id=R.ConsultaId JOIN Pacientes P ON P.Id=C.PacienteId LEFT JOIN Medicos M ON M.Id=C.MedicoId WHERE R.Id=$id"
                cmd.Parameters.AddWithValue("$id",recetaSeleccionada)
                Using rd=cmd.ExecuteReader()
                    If Not rd.Read() Then Return
                    textoImpresion="CLÍNICA MÉDICA" & Environment.NewLine &
                        "RECETA MÉDICA" & Environment.NewLine &
                        "Fecha de emisión: " & rd("Fecha").ToString() & Environment.NewLine & Environment.NewLine &
                        "PACIENTE" & Environment.NewLine &
                        "Nombre: " & rd("Paciente").ToString() & Environment.NewLine &
                        "Identidad: " & rd("Identidad").ToString() & Environment.NewLine &
                        "Fecha de nacimiento: " & rd("FechaNacimiento").ToString() & Environment.NewLine & Environment.NewLine &
                        "Diagnóstico: " & rd("Diagnostico").ToString() & Environment.NewLine & Environment.NewLine &
                        "Rp/" & Environment.NewLine &
                        "Medicamento: " & rd("Medicamento").ToString() & Environment.NewLine &
                        "Dosis: " & rd("Dosis").ToString() & Environment.NewLine &
                        "Frecuencia: " & rd("Frecuencia").ToString() & Environment.NewLine &
                        "Duración: " & rd("Duracion").ToString() & Environment.NewLine &
                        "Indicaciones: " & rd("Indicaciones").ToString() & Environment.NewLine & Environment.NewLine & Environment.NewLine &
                        "Médico: " & rd("Medico").ToString() & Environment.NewLine &
                        "Especialidad: " & rd("Especialidad").ToString() & Environment.NewLine &
                        "Colegiado: " & rd("Colegiado").ToString() & Environment.NewLine & Environment.NewLine &
                        "Firma y sello: __________________________________"
                End Using
            End Using
        End Using

        Dim pd As New PrintDocument()
        pd.DefaultPageSettings.Margins=New Margins(65,65,55,55)
        AddHandler pd.PrintPage,Sub(s,args)
            Dim ancho=args.MarginBounds.Width
            Using titleFont As New Font("Segoe UI",15,FontStyle.Bold), sectionFont As New Font("Segoe UI",10,FontStyle.Bold), bodyFont As New Font("Segoe UI",10)
                Dim y=args.MarginBounds.Top
                args.Graphics.DrawString("CLÍNICA MÉDICA",titleFont,Brushes.Navy,args.MarginBounds.Left,y)
                y+=34
                args.Graphics.DrawLine(Pens.SteelBlue,args.MarginBounds.Left,y,args.MarginBounds.Right,y)
                y+=20
                args.Graphics.DrawString(textoImpresion,bodyFont,Brushes.Black,New RectangleF(args.MarginBounds.Left,y,ancho,args.MarginBounds.Height-y+args.MarginBounds.Top))
            End Using
        End Sub
        Using dlg As New PrintPreviewDialog()
            dlg.Document=pd
            dlg.Width=950 : dlg.Height=750
            dlg.ShowDialog(Me)
        End Using
    End Sub

    Private Class Item
        Public ReadOnly Id As Integer
        Private ReadOnly Texto As String
        Public Sub New(id As Integer,texto As String)
            Me.Id=id : Me.Texto=texto
        End Sub
        Public Overrides Function ToString() As String
            Return Texto
        End Function
    End Class
End Class