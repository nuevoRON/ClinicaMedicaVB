Imports System.IO
Imports System.Drawing

Public Class BackupForm
    Inherits Form

    Private lblEstado As New Label()
    Private dgv As New DataGridView()

    Public Sub New()
        Text = "Copias de seguridad y restauración"
        Width = 850 : Height = 560
        StartPosition = FormStartPosition.CenterParent

        Dim titulo As New Label With {.Text = "GESTIÓN DE COPIAS DE SEGURIDAD", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .Left = 25, .Top = 20, .AutoSize = True}
        Dim btnCrear As New Button With {.Text = "Crear copia de seguridad", .Left = 25, .Top = 70, .Width = 210, .Height = 40}
        Dim btnRestaurar As New Button With {.Text = "Restaurar copia", .Left = 250, .Top = 70, .Width = 180, .Height = 40}
        Dim btnActualizar As New Button With {.Text = "Actualizar lista", .Left = 445, .Top = 70, .Width = 150, .Height = 40}
        lblEstado.SetBounds(25, 120, 760, 45)
        lblEstado.BorderStyle = BorderStyle.FixedSingle
        lblEstado.Text = "Las copias automáticas se guardan en la carpeta Backups."

        dgv.SetBounds(25, 180, 760, 290)
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        AddHandler btnCrear.Click, AddressOf CrearCopia
        AddHandler btnRestaurar.Click, AddressOf RestaurarCopia
        AddHandler btnActualizar.Click, AddressOf CargarCopias

        Controls.AddRange({titulo, btnCrear, btnRestaurar, btnActualizar, lblEstado, dgv})
        CargarCopias()
    End Sub

    Private Sub CrearCopia(sender As Object, e As EventArgs)
        Try
            If Not File.Exists(Database.DbPath) Then
                MessageBox.Show("No existe la base de datos para copiar.", "Copia")
                Return
            End If

            Using dlg As New SaveFileDialog()
                dlg.Title = "Guardar copia de seguridad"
                dlg.Filter = "Base de datos SQLite (*.db)|*.db"
                dlg.InitialDirectory = Database.CarpetaCopias()
                dlg.FileName = $"clinica_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"

                If dlg.ShowDialog() <> DialogResult.OK Then Return

                Dim destino = dlg.FileName
                Database.BackupDatabase(destino)

                lblEstado.Text = $"Copia creada correctamente: {Path.GetFileName(destino)}"
                CargarCopias()
                MessageBox.Show("La copia de seguridad se creó correctamente.", "Copia completada")
            End Using
        Catch ex As Exception
            MessageBox.Show("No se pudo crear la copia: " & ex.Message, "Error")
        End Try
    End Sub

    Private Sub RestaurarCopia(sender As Object, e As EventArgs)
        If dgv.SelectedRows.Count = 0 Then
            MessageBox.Show("Seleccione una copia de seguridad.")
            Return
        End If

        Dim ruta = dgv.SelectedRows(0).Cells("Ruta").Value.ToString()
        If Not File.Exists(ruta) Then
            MessageBox.Show("La copia seleccionada ya no existe.")
            CargarCopias()
            Return
        End If

        If MessageBox.Show("La restauración reemplazará la base de datos actual. Se creará una copia preventiva antes de continuar." & Environment.NewLine & Environment.NewLine & "¿Desea continuar?", "Confirmar restauración", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

        Try
            Dim preventiva = Path.Combine(Database.CarpetaCopias(), $"antes_restaurar_{DateTime.Now:yyyyMMdd_HHmmss}.db")
            Database.BackupDatabase(preventiva)

            GC.Collect()
            GC.WaitForPendingFinalizers()

            File.Copy(ruta, Database.DbPath, True)

            lblEstado.Text = $"Base de datos restaurada desde: {Path.GetFileName(ruta)}"
            MessageBox.Show("Restauración completada. Cierre y vuelva a abrir el sistema para trabajar con la información restaurada.", "Restauración completada", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CargarCopias()
        Catch ex As Exception
            MessageBox.Show("No se pudo restaurar la copia: " & ex.Message & Environment.NewLine & "Si el archivo está siendo utilizado, cierre el sistema y vuelva a intentarlo.", "Error")
        End Try
    End Sub

    Private Sub CargarCopias()
        Dim dt As New DataTable()
        dt.Columns.Add("Archivo")
        dt.Columns.Add("Fecha")
        dt.Columns.Add("Tamaño")
        dt.Columns.Add("Ruta")

        For Each archivo In Directory.GetFiles(Database.CarpetaCopias(), "*.db")
            Dim info As New FileInfo(archivo)
            dt.Rows.Add(info.Name, info.LastWriteTime.ToString("dd/MM/yyyy HH:mm:ss"), $"{info.Length / 1024.0:N1} KB", archivo)
        Next

        dgv.DataSource = dt
        If dgv.Columns.Contains("Ruta") Then dgv.Columns("Ruta").Visible = False
    End Sub
End Class