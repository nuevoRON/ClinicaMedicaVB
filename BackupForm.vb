Imports System.IO
Imports System.Drawing

Public Class BackupForm
    Inherits Form

    Private lblEstado As New Label()
    Private dgv As New DataGridView()
    Private nudIntervalo As New NumericUpDown()
    Private nudMaximas As New NumericUpDown()
    Private txtCarpeta As New TextBox()

    Public Sub New()
        If Not PermissionHelper.PuedeAcceder(Session.CurrentRole, "Backup") Then
            Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", "Backup", Me.Text)
            MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            AddHandler Me.Load, Sub() Me.Close()
            Return
        End If
        Text = "Copias de seguridad y restauración"
        Width = 920 : Height = 650
        StartPosition = FormStartPosition.CenterParent

        Dim titulo As New Label With {.Text = "GESTIÓN DE COPIAS DE SEGURIDAD", .Font = New Font("Segoe UI", 16, FontStyle.Bold), .Left = 25, .Top = 20, .AutoSize = True}

        Dim btnCrear As New Button With {.Text = "Crear copia de seguridad", .Left = 25, .Top = 70, .Width = 210, .Height = 40}
        Dim btnRestaurar As New Button With {.Text = "Restaurar copia", .Left = 250, .Top = 70, .Width = 180, .Height = 40}
        Dim btnActualizar As New Button With {.Text = "Actualizar lista", .Left = 445, .Top = 70, .Width = 150, .Height = 40}
        Dim btnConfiguracion As New Button With {.Text = "Guardar configuración", .Left = 610, .Top = 70, .Width = 200, .Height = 40}

        Dim grp As New GroupBox With {.Text = "Configuración de copias automáticas", .Left = 25, .Top = 125, .Width = 785, .Height = 115}
        Dim lblIntervalo As New Label With {.Text = "Frecuencia (días):", .Left = 15, .Top = 28, .AutoSize = True}
        nudIntervalo.SetBounds(125, 24, 70, 25)
        nudIntervalo.Minimum = 1 : nudIntervalo.Maximum = 30

        Dim lblMaximas As New Label With {.Text = "Copias automáticas a conservar:", .Left = 220, .Top = 28, .AutoSize = True}
        nudMaximas.SetBounds(405, 24, 70, 25)
        nudMaximas.Minimum = 1 : nudMaximas.Maximum = 100

        Dim lblRuta As New Label With {.Text = "Carpeta:", .Left = 15, .Top = 68, .AutoSize = True}
        txtCarpeta.SetBounds(75, 64, 570, 25)
        Dim btnRuta As New Button With {.Text = "Examinar...", .Left = 655, .Top = 62, .Width = 105, .Height = 28}

        grp.Controls.AddRange({lblIntervalo, nudIntervalo, lblMaximas, nudMaximas, lblRuta, txtCarpeta, btnRuta})

        lblEstado.SetBounds(25, 250, 785, 45)
        lblEstado.BorderStyle = BorderStyle.FixedSingle

        dgv.SetBounds(25, 310, 785, 250)
        dgv.ReadOnly = True
        dgv.AllowUserToAddRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        AddHandler btnCrear.Click, AddressOf CrearCopia
        AddHandler btnRestaurar.Click, AddressOf RestaurarCopia
        AddHandler btnActualizar.Click, AddressOf CargarCopias
        AddHandler btnConfiguracion.Click, AddressOf GuardarConfiguracion
        AddHandler btnRuta.Click, AddressOf SeleccionarCarpeta

        Controls.AddRange({titulo, btnCrear, btnRestaurar, btnActualizar, btnConfiguracion, grp, lblEstado, dgv})

        CargarConfiguracion()
        CargarCopias()
    End Sub

    Private Sub CargarConfiguracion()
        Dim cfg = BackupSettings.Cargar()
        nudIntervalo.Value = Math.Max(nudIntervalo.Minimum, Math.Min(nudIntervalo.Maximum, cfg.IntervaloDias))
        nudMaximas.Value = Math.Max(nudMaximas.Minimum, Math.Min(nudMaximas.Maximum, cfg.CopiasMaximas))
        txtCarpeta.Text = cfg.ObtenerCarpetaDestino()
        lblEstado.Text = "Configure la frecuencia, conservación y ubicación de las copias automáticas."
    End Sub

    Private Sub GuardarConfiguracion(sender As Object, e As EventArgs)
        Try
            Dim cfg = New BackupSettings With {
                .IntervaloDias = CInt(nudIntervalo.Value),
                .CopiasMaximas = CInt(nudMaximas.Value),
                .CarpetaDestino = txtCarpeta.Text.Trim()
            }

            If String.IsNullOrWhiteSpace(cfg.CarpetaDestino) Then
                cfg.CarpetaDestino = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups")
                txtCarpeta.Text = cfg.CarpetaDestino
            End If

            Directory.CreateDirectory(cfg.CarpetaDestino)
            cfg.Guardar()
            Database.LimpiarCopiasAntiguas()

            lblEstado.Text = "Configuración guardada correctamente."
            CargarCopias()
            MessageBox.Show("La configuración de copias de seguridad fue guardada.", "Configuración")
        Catch ex As Exception
            MessageBox.Show("No se pudo guardar la configuración: " & ex.Message, "Error")
        End Try
    End Sub

    Private Sub SeleccionarCarpeta(sender As Object, e As EventArgs)
        Using dlg As New FolderBrowserDialog()
            dlg.Description = "Seleccione la carpeta donde se almacenarán las copias automáticas."
            dlg.SelectedPath = txtCarpeta.Text

            If dlg.ShowDialog() = DialogResult.OK Then
                txtCarpeta.Text = dlg.SelectedPath
            End If
        End Using
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

                Database.BackupDatabase(dlg.FileName)
                lblEstado.Text = $"Copia creada correctamente: {Path.GetFileName(dlg.FileName)}"
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