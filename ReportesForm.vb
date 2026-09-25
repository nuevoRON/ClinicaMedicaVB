Imports Microsoft.Data.Sqlite
Imports System.Drawing
Imports System.Drawing.Printing

Public Class ReportesForm
    Inherits Form

    Private cmbReporte As New ComboBox()
    Private dtDesde As New DateTimePicker()
    Private dtHasta As New DateTimePicker()
    Private txtFiltro As New TextBox()
    Private dgv As New DataGridView()
    Private btnGenerar As New Button()
    Private btnVista As New Button()
    Private btnImprimir As New Button()

    Private printDoc As New PrintDocument()
    Private printPreview As New PrintPreviewDialog()
    Private printRow As Integer = 0

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Reportes") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Reportes", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text = "Reportes profesionales"
        Width = 1050 : Height = 650
        StartPosition = FormStartPosition.CenterParent

        Dim lblReporte As New Label With {.Text="Reporte:",.Left=20,.Top=20,.AutoSize=True}
        cmbReporte.SetBounds(85,15,260,28)
        cmbReporte.DropDownStyle=ComboBoxStyle.DropDownList
        cmbReporte.Items.AddRange({
            "Pacientes registrados",
            "Citas por período",
            "Consultas por período",
            "Recetas por período",
            "Resumen general"
        })
        cmbReporte.SelectedIndex=0

        Dim lblDesde As New Label With {.Text="Desde:",.Left=365,.Top=20,.AutoSize=True}
        dtDesde.SetBounds(415,15,150,28)
        dtDesde.Value=DateTime.Today.AddMonths(-1)

        Dim lblHasta As New Label With {.Text="Hasta:",.Left=585,.Top=20,.AutoSize=True}
        dtHasta.SetBounds(630,15,150,28)
        dtHasta.Value=DateTime.Today

        Dim lblFiltro As New Label With {.Text="Filtro:",.Left=20,.Top=62,.AutoSize=True}
        txtFiltro.SetBounds(85,57,260,28)
        txtFiltro.PlaceholderText="Nombre, identidad o texto"

        btnGenerar.Text="Generar reporte"
        btnGenerar.SetBounds(365,55,140,35)
        btnVista.Text="Vista previa"
        btnVista.SetBounds(515,55,120,35)
        btnImprimir.Text="Imprimir"
        btnImprimir.SetBounds(645,55,110,35)

        dgv.SetBounds(20,110,990,470)
        dgv.ReadOnly=True
        dgv.AllowUserToAddRows=False
        dgv.SelectionMode=DataGridViewSelectionMode.FullRowSelect
        dgv.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill

        AddHandler btnGenerar.Click, AddressOf Generar
        AddHandler btnVista.Click, AddressOf VistaPrevia
        AddHandler btnImprimir.Click, AddressOf Imprimir

        printDoc.DefaultPageSettings.Landscape=True
        AddHandler printDoc.PrintPage, AddressOf ImprimirPagina

        Controls.AddRange({lblReporte,cmbReporte,lblDesde,dtDesde,lblHasta,dtHasta,lblFiltro,txtFiltro,btnGenerar,btnVista,btnImprimir,dgv})
    End Sub

    Private Sub Generar(sender As Object,e As EventArgs)
        If dtDesde.Value.Date > dtHasta.Value.Date Then
            MessageBox.Show("La fecha inicial no puede ser mayor que la fecha final.","Validación")
            Return
        End If

        Dim dt As New DataTable()

        Using cn=Database.Connection()
            Using cmd=cn.CreateCommand()
                Select Case cmbReporte.SelectedIndex
                    Case 0
                        cmd.CommandText="SELECT Identidad AS 'Identidad', Nombre || ' ' || Apellidos AS 'Paciente', FechaNacimiento AS 'Fecha nacimiento', Sexo AS 'Sexo', Telefono AS 'Teléfono', FechaRegistro AS 'Registro' FROM Pacientes WHERE (Identidad LIKE $f OR Nombre LIKE $f OR Apellidos LIKE $f OR Telefono LIKE $f) ORDER BY Apellidos, Nombre"
                        cmd.Parameters.AddWithValue("$f","%" & txtFiltro.Text.Trim() & "%")

                    Case 1
                        cmd.CommandText="SELECT c.FechaHora AS 'Fecha y hora', p.Identidad AS 'Identidad', p.Nombre || ' ' || p.Apellidos AS 'Paciente', COALESCE(m.Nombre,'Sin médico') AS 'Médico', c.Motivo AS 'Motivo', c.Estado AS 'Estado' FROM Citas c INNER JOIN Pacientes p ON p.Id=c.PacienteId LEFT JOIN Medicos m ON m.Id=c.MedicoId WHERE date(c.FechaHora) BETWEEN date($d) AND date($h) AND (p.Nombre || ' ' || p.Apellidos LIKE $f OR p.Identidad LIKE $f) ORDER BY c.FechaHora"
                        cmd.Parameters.AddWithValue("$d",dtDesde.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$h",dtHasta.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$f","%" & txtFiltro.Text.Trim() & "%")

                    Case 2
                        cmd.CommandText="SELECT q.FechaHora AS 'Fecha y hora', p.Identidad AS 'Identidad', p.Nombre || ' ' || p.Apellidos AS 'Paciente', COALESCE(m.Nombre,'Sin médico') AS 'Médico', q.Diagnostico AS 'Diagnóstico', q.Tratamiento AS 'Tratamiento' FROM Consultas q INNER JOIN Pacientes p ON p.Id=q.PacienteId LEFT JOIN Medicos m ON m.Id=q.MedicoId WHERE date(q.FechaHora) BETWEEN date($d) AND date($h) AND (p.Nombre || ' ' || p.Apellidos LIKE $f OR p.Identidad LIKE $f) ORDER BY q.FechaHora"
                        cmd.Parameters.AddWithValue("$d",dtDesde.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$h",dtHasta.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$f","%" & txtFiltro.Text.Trim() & "%")

                    Case 3
                        cmd.CommandText="SELECT r.Fecha AS 'Fecha', p.Identidad AS 'Identidad', p.Nombre || ' ' || p.Apellidos AS 'Paciente', r.Medicamento AS 'Medicamento', r.Dosis AS 'Dosis', r.Frecuencia AS 'Frecuencia', r.Duracion AS 'Duración' FROM Recetas r INNER JOIN Consultas q ON q.Id=r.ConsultaId INNER JOIN Pacientes p ON p.Id=q.PacienteId WHERE date(r.Fecha) BETWEEN date($d) AND date($h) AND (p.Nombre || ' ' || p.Apellidos LIKE $f OR p.Identidad LIKE $f) ORDER BY r.Fecha"
                        cmd.Parameters.AddWithValue("$d",dtDesde.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$h",dtHasta.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$f","%" & txtFiltro.Text.Trim() & "%")

                    Case 4
                        cmd.CommandText="SELECT 'Pacientes registrados' AS 'Indicador', COUNT(*) AS 'Total' FROM Pacientes UNION ALL SELECT 'Médicos activos', COUNT(*) FROM Medicos WHERE Activo=1 UNION ALL SELECT 'Citas', COUNT(*) FROM Citas WHERE date(FechaHora) BETWEEN date($d) AND date($h) UNION ALL SELECT 'Consultas', COUNT(*) FROM Consultas WHERE date(FechaHora) BETWEEN date($d) AND date($h) UNION ALL SELECT 'Recetas', COUNT(*) FROM Recetas WHERE date(Fecha) BETWEEN date($d) AND date($h)"
                        cmd.Parameters.AddWithValue("$d",dtDesde.Value.ToString("yyyy-MM-dd"))
                        cmd.Parameters.AddWithValue("$h",dtHasta.Value.ToString("yyyy-MM-dd"))
                End Select

                Using rd=cmd.ExecuteReader()
                    dt.Load(rd)
                End Using
            End Using
        End Using

        dgv.DataSource=dt
    End Sub

    Private Sub VistaPrevia(sender As Object,e As EventArgs)
        If dgv.Rows.Count=0 Then
            MessageBox.Show("Primero genere un reporte.","Reporte")
            Return
        End If
        printRow=0
        printPreview.Document=printDoc
        printPreview.WindowState=FormWindowState.Maximized
        printPreview.ShowDialog()
    End Sub

    Private Sub Imprimir(sender As Object,e As EventArgs)
        If dgv.Rows.Count=0 Then
            MessageBox.Show("Primero genere un reporte.","Reporte")
            Return
        End If
        printRow=0
        Using dlg As New PrintDialog()
            dlg.Document=printDoc
            If Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Consulta de reporte", "Reportes", "Reporte generado")
        dlg.ShowDialog()=DialogResult.OK Then
                printDoc.Print()
            End If
        End Using
    End Sub

    Private Sub ImprimirPagina(sender As Object,e As PrintPageEventArgs)
        Dim g=e.Graphics
        Dim left=e.MarginBounds.Left
        Dim y=e.MarginBounds.Top
        Dim fontTitulo As New Font("Segoe UI",14,FontStyle.Bold)
        Dim fontNormal As New Font("Segoe UI",8)
        Dim fontHeader As New Font("Segoe UI",8,FontStyle.Bold)

        g.DrawString("CLÍNICA MÉDICA",fontTitulo,Brushes.Black,left,y)
        y += 28
        g.DrawString(cmbReporte.Text,fontHeader,Brushes.Black,left,y)
        y += 18
        g.DrawString($"Período: {dtDesde.Value:dd/MM/yyyy} - {dtHasta.Value:dd/MM/yyyy}    Generado: {DateTime.Now:dd/MM/yyyy HH:mm}",fontNormal,Brushes.Black,left,y)
        y += 25

        Dim widths As Integer = Math.Max(1,dgv.Columns.Count)
        Dim colWidth As Integer = Math.Max(80,e.MarginBounds.Width  widths)
        Dim x=left

        For Each col As DataGridViewColumn In dgv.Columns
            g.DrawString(col.HeaderText,fontHeader,Brushes.Black,New RectangleF(x,y,colWidth-5,35))
            x += colWidth
        Next
        y += 32

        While printRow < dgv.Rows.Count
            If y + 28 > e.MarginBounds.Bottom Then
                e.HasMorePages=True
                Return
            End If

            x=left
            For Each cell As DataGridViewCell In dgv.Rows(printRow).Cells
                Dim valor=If(cell.Value Is Nothing,"",cell.Value.ToString())
                g.DrawString(valor,fontNormal,Brushes.Black,New RectangleF(x,y,colWidth-5,28))
                x += colWidth
            Next
            y += 28
            printRow += 1
        End While

        g.DrawString("Documento generado por el Sistema de Gestión de Clínica Médica.",fontNormal,Brushes.Black,left,e.MarginBounds.Bottom+10)
        e.HasMorePages=False
    End Sub
End Class