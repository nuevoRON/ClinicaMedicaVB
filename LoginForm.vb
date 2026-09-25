Imports Microsoft.Data.Sqlite

Public Class LoginForm
    Inherits Form
    Private txtUsuario As New TextBox()
    Private txtClave As New TextBox()
    Private btnIngresar As New Button()

    Public Sub New()
        Text = "Clínica Médica - Inicio de sesión"
        Width = 420 : Height = 270
        StartPosition = FormStartPosition.CenterScreen
        Dim lbl1 As New Label With {.Text="Usuario:", .Left=45, .Top=55, .Width=90}
        Dim lbl2 As New Label With {.Text="Contraseña:", .Left=45, .Top=105, .Width=90}
        txtUsuario.SetBounds(145,50,190,28)
        txtClave.SetBounds(145,100,190,28)
        txtClave.PasswordChar="*"c
        btnIngresar.Text="Ingresar"
        btnIngresar.SetBounds(145,155,190,35)
        AddHandler btnIngresar.Click, AddressOf Ingresar
        Controls.AddRange({lbl1,lbl2,txtUsuario,txtClave,btnIngresar})
        AcceptButton = btnIngresar
    End Sub

    Private Sub Ingresar(sender As Object, e As EventArgs)
        Using cn = Database.Connection()
            Using cmd = cn.CreateCommand()
                cmd.CommandText = "SELECT Clave, Rol FROM Usuarios WHERE Usuario=$u AND Activo=1"
                cmd.Parameters.AddWithValue("$u", txtUsuario.Text.Trim())
                Dim rol As String = ""
                Dim claveAlmacenada As String = ""
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        claveAlmacenada = rd.GetString(0)
                        rol = rd.GetString(1)
                    End If
                End Using
                If Not String.IsNullOrWhiteSpace(rol) AndAlso SecurityHelper.VerifyPassword(txtClave.Text, claveAlmacenada) Then
                    Session.Start(txtUsuario.Text.Trim(), rol)
                    Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Inicio de sesión", "Seguridad", "Acceso autorizado")
                    Hide()
                    Using f As New MainForm(Session.CurrentUser, Session.CurrentRole)
                        f.ShowDialog()
                    End Using
                    Show()
                    Session.EndSession
                    txtClave.Clear()
                Else
                    Database.RegistrarAccion(txtUsuario.Text.Trim(), "", "Intento de inicio de sesión", "Seguridad", "Acceso rechazado")
                    MessageBox.Show("Usuario o contraseña incorrectos.")
                End If
            End Using
        End Using
    End Sub
End Class