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
    Private pacienteNombreImpresion As String=""
    Private identidadImpresion As String=""
    Private nacimientoImpresion As String=""
    Private diagnosticoImpresion As String=""
    Private medicamentoImpresion As String=""
    Private dosisImpresion As String=""
    Private frecuenciaImpresion As String=""
    Private duracionImpresion As String=""
    Private indicacionesImpresion As String=""
    Private medicoImpresion As String=""
    Private especialidadImpresion As String=""
    Private colegiadoImpresion As String=""


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
        paciente.Items.Clear()
        paciente.SelectedIndex=-1
        paciente.Text=""
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
        AddHandler paciente.SelectedIndexChanged,AddressOf PacienteSeleccionado
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
        grid.ReadOnly=True : grid.AllowUserToAddRows=False : grid.AllowUserToDeleteRows=False : grid.MultiSelect=False : grid.EditMode=DataGridViewEditMode.EditProgrammatically
        grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill
        grid.AutoGenerateColumns=False
        grid.Columns.Clear()
        Dim colId As New DataGridViewTextBoxColumn With {.Name="Id",.DataPropertyName="Id",.HeaderText="Id",.Visible=False}
        Dim colFecha As New DataGridViewTextBoxColumn With {.Name="Fecha",.DataPropertyName="Fecha",.HeaderText="Fecha",.FillWeight=75}
        Dim colPaciente As New DataGridViewTextBoxColumn With {.Name="Paciente",.DataPropertyName="Paciente",.HeaderText="Paciente",.FillWeight=130}
        Dim colMedicamento As New DataGridViewTextBoxColumn With {.Name="Medicamento",.DataPropertyName="Medicamento",.HeaderText="Medicamento",.FillWeight=125}
        Dim colDosis As New DataGridViewTextBoxColumn With {.Name="Dosis",.DataPropertyName="Dosis",.HeaderText="Dosis",.FillWeight=85}
        Dim colFrecuencia As New DataGridViewTextBoxColumn With {.Name="Frecuencia",.DataPropertyName="Frecuencia",.HeaderText="Frecuencia",.FillWeight=90}
        Dim colDuracion As New DataGridViewTextBoxColumn With {.Name="Duracion",.DataPropertyName="Duracion",.HeaderText="Duración",.FillWeight=75}
        grid.Columns.AddRange({colId,colFecha,colPaciente,colMedicamento,colDosis,colFrecuencia,colDuracion})
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
            If grid.SelectedRows.Count>0 AndAlso grid.SelectedRows(0).Tag IsNot Nothing Then recetaSeleccionada=Convert.ToInt32(grid.SelectedRows(0).Tag)
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
        paciente.BeginUpdate()
        paciente.Items.Clear()
        paciente.SelectedIndex=-1
        paciente.Text=""
        Dim encontrados As Integer=0
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT DISTINCT P.Id,P.Identidad,P.Nombre||' '||P.Apellidos AS Nombre FROM Pacientes P INNER JOIN Consultas C ON C.PacienteId=P.Id WHERE date(C.FechaHora)=date('now','localtime') ORDER BY P.Apellidos,P.Nombre"
                Using rd=cmd.ExecuteReader()
                    While rd.Read()
                        paciente.Items.Add(New Item(CInt(rd("Id")),rd("Nombre").ToString() & " · " & rd("Identidad").ToString()))
                        encontrados+=1
                    End While
                End Using
            End Using
        End Using
        paciente.EndUpdate()
        paciente.SelectedIndex=-1
        paciente.Text=""
        paciente.Enabled=encontrados>0
        If encontrados=0 Then
            paciente.Items.Clear()
            paciente.Text=""
            paciente.Enabled=False
            MessageBox.Show("No hay pacientes con consulta registrada durante el día de hoy. Registre primero la atención clínica.", "Sin pacientes atendidos", MessageBoxButtons.OK, MessageBoxIcon.Information)
            paciente.Enabled=True
        End If
    End Sub

    Private Sub PacienteSeleccionado(sender As Object,e As EventArgs)
        If paciente.SelectedItem Is Nothing Then Return
        Dim p=DirectCast(paciente.SelectedItem,Item)
        CargarDatosAtencionPaciente(p.Id)
    End Sub

    Private Sub CargarDatosAtencionPaciente(pacienteId As Integer)
        ' La receta se vincula a una consulta de hoy para el paciente elegido.
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT C.Diagnostico,C.MedicoId,M.Nombre AS Medico,M.Especialidad,M.Colegiado FROM Consultas C LEFT JOIN Medicos M ON M.Id=C.MedicoId WHERE C.PacienteId=$p AND date(C.FechaHora)=date('now','localtime') ORDER BY C.FechaHora DESC,C.Id DESC LIMIT 1"
                cmd.Parameters.AddWithValue("$p",pacienteId)
                Using rd=cmd.ExecuteReader()
                    If rd.Read() Then
                        diagnosticoImpresion=If(rd("Diagnostico") Is DBNull.Value,"",rd("Diagnostico").ToString())
                        medicoImpresion=If(rd("Medico") Is DBNull.Value,"",rd("Medico").ToString())
                        especialidadImpresion=If(rd("Especialidad") Is DBNull.Value,"",rd("Especialidad").ToString())
                        colegiadoImpresion=If(rd("Colegiado") Is DBNull.Value,"",rd("Colegiado").ToString())
                    End If
                End Using
            End Using
        End Using
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
                cmd.CommandText="SELECT Id FROM Consultas WHERE PacienteId=$p AND date(FechaHora)=date('now','localtime') ORDER BY FechaHora DESC,Id DESC LIMIT 1"
                cmd.Parameters.AddWithValue("$p",p.Id)
                Dim resultado=cmd.ExecuteScalar()
                If resultado Is Nothing OrElse resultado Is DBNull.Value Then
                    MessageBox.Show("El paciente seleccionado no tiene una consulta registrada hoy. Solo se pueden emitir recetas para pacientes atendidos durante el día actual.", "Consulta requerida",MessageBoxButtons.OK,MessageBoxIcon.Warning)
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
        paciente.SelectedIndex=-1
        paciente.Text=""
    End Sub

    Private Sub CargarRecetas()
        Dim dt As New DataTable()
        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                cmd.CommandText="SELECT R.Id,substr(R.Fecha,1,10) AS Fecha,P.Nombre||' '||P.Apellidos AS Paciente,R.Medicamento,R.Dosis,R.Frecuencia,R.Duracion FROM Recetas R JOIN Consultas C ON C.Id=R.ConsultaId JOIN Pacientes P ON P.Id=C.PacienteId ORDER BY R.Id DESC"
                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using
        grid.DataSource=Nothing
        grid.Rows.Clear()
        For Each fila As DataRow In dt.Rows
            Dim indice As Integer=grid.Rows.Add()
            grid.Rows(indice).Cells("Fecha").Value=fila("Fecha").ToString()
            grid.Rows(indice).Cells("Paciente").Value=fila("Paciente").ToString()
            grid.Rows(indice).Cells("Medicamento").Value=fila("Medicamento").ToString()
            grid.Rows(indice).Cells("Dosis").Value=fila("Dosis").ToString()
            grid.Rows(indice).Cells("Frecuencia").Value=fila("Frecuencia").ToString()
            grid.Rows(indice).Cells("Duracion").Value=fila("Duracion").ToString()
            grid.Rows(indice).Tag=Convert.ToInt32(fila("Id"))
        Next
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
                    pacienteNombreImpresion=rd("Paciente").ToString()
                    identidadImpresion=rd("Identidad").ToString()
                    nacimientoImpresion=rd("FechaNacimiento").ToString()
                    diagnosticoImpresion=rd("Diagnostico").ToString()
                    medicamentoImpresion=rd("Medicamento").ToString()
                    dosisImpresion=rd("Dosis").ToString()
                    frecuenciaImpresion=rd("Frecuencia").ToString()
                    duracionImpresion=rd("Duracion").ToString()
                    indicacionesImpresion=rd("Indicaciones").ToString()
                    medicoImpresion=rd("Medico").ToString()
                    especialidadImpresion=rd("Especialidad").ToString()
                    colegiadoImpresion=rd("Colegiado").ToString()
                End Using
            End Using
        End Using

        Dim pd As New PrintDocument()
        pd.DefaultPageSettings.Margins=New Margins(42,42,35,35)
        pd.DefaultPageSettings.Landscape=False
        AddHandler pd.PrintPage,Sub(s,args)
            Dim g=args.Graphics
            Dim leftF=args.MarginBounds.Left
            Dim rightF=args.MarginBounds.Right
            Dim widthF=args.MarginBounds.Width
            Dim y As Single=args.MarginBounds.Top
            Using titleFont As New Font("Segoe UI",16,FontStyle.Bold),
                  subFont As New Font("Segoe UI",9,FontStyle.Bold),
                  labelFont As New Font("Segoe UI",8,FontStyle.Bold),
                  bodyFont As New Font("Segoe UI",9),
                  smallFont As New Font("Segoe UI",8),
                  linePen As New Pen(Color.FromArgb(75,105,140),1),
                  borderPen As New Pen(Color.FromArgb(175,190,205),1),
                  sf As New StringFormat()
                sf.Trimming=StringTrimming.Word
                g.DrawString("CLÍNICA MÉDICA",titleFont,Brushes.Navy,leftF,y)
                g.DrawString("RECETA MÉDICA",subFont,Brushes.Black,leftF,y+27)
                g.DrawString("Emitida: " & DateTime.Now.ToString("dd/MM/yyyy"),smallFont,Brushes.Black,rightF-160,y+8)
                y+=49
                g.DrawLine(linePen,leftF,y,rightF,y)
                y+=10
                g.DrawString("DATOS DEL PACIENTE",labelFont,Brushes.Navy,leftF,y)
                y+=17
                g.DrawString("Nombre: " & pacienteNombreImpresion,bodyFont,Brushes.Black,leftF,y)
                g.DrawString("Identidad: " & identidadImpresion,bodyFont,Brushes.Black,leftF+widthF*0.58F,y)
                y+=19
                g.DrawString("Fecha de nacimiento: " & nacimientoImpresion,bodyFont,Brushes.Black,leftF,y)
                y+=25
                g.DrawString("Diagnóstico: " & diagnosticoImpresion,bodyFont,Brushes.Black,New RectangleF(leftF,y,widthF,40),sf)
                y+=Math.Max(30, g.MeasureString("Diagnóstico: " & diagnosticoImpresion,bodyFont,CInt(widthF)).Height+8)
                g.DrawLine(borderPen,leftF,y,rightF,y)
                y+=10
                g.DrawString("Rp/",New Font("Segoe UI",13,FontStyle.Bold),Brushes.Navy,leftF,y)
                y+=25
                g.DrawString("MEDICAMENTO",labelFont,Brushes.Navy,leftF,y)
                y+=17
                g.DrawString(medicamentoImpresion,New Font("Segoe UI",11,FontStyle.Bold),Brushes.Black,New RectangleF(leftF,y,widthF,45),sf)
                y+=Math.Max(28,g.MeasureString(medicamentoImpresion,New Font("Segoe UI",11,FontStyle.Bold),CInt(widthF)).Height+6)
                g.DrawString("Dosis: " & dosisImpresion,bodyFont,Brushes.Black,leftF,y) : y+=18
                g.DrawString("Frecuencia: " & frecuenciaImpresion,bodyFont,Brushes.Black,leftF,y) : y+=18
                g.DrawString("Duración: " & duracionImpresion,bodyFont,Brushes.Black,leftF,y) : y+=22
                g.DrawString("Indicaciones:",labelFont,Brushes.Navy,leftF,y) : y+=16
                Dim altoIndicaciones As Single=Math.Max(24,g.MeasureString(indicacionesImpresion,bodyFont,CInt(widthF),sf).Height+4)
                g.DrawString(indicacionesImpresion,bodyFont,Brushes.Black,New RectangleF(leftF,y,widthF,altoIndicaciones),sf)
                y+=altoIndicaciones+10
                g.DrawLine(borderPen,leftF,y,rightF,y)
                y+=12
                g.DrawString("Médico: " & medicoImpresion,bodyFont,Brushes.Black,leftF,y) : y+=18
                g.DrawString("Especialidad: " & especialidadImpresion & "     Colegiado: " & colegiadoImpresion,smallFont,Brushes.Black,leftF,y)
                y+=42
                g.DrawLine(Pens.Black,leftF+widthF*0.55F,y,rightF,y)
                g.DrawString("Firma y sello del profesional",smallFont,Brushes.Black,leftF+widthF*0.55F,y+5)
            End Using
            args.HasMorePages=False
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