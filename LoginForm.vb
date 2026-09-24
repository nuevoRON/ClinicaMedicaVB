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
                cmd.CommandText = "SELECT Rol FROM Usuarios WHERE Usuario=$u AND Clave=$c AND Activo=1"
                cmd.Parameters.AddWithValue("$u", txtUsuario.Text.Trim())
                cmd.Parameters.AddWithValue("$c", txtClave.Text)
                Dim rol = cmd.ExecuteScalar()
                If rol IsNot Nothing Then
                    Hide()
                    Using f As New MainForm(txtUsuario.Text.Trim(), rol.ToString())
                        f.ShowDialog()
                    End Using
                    Show()
                    txtClave.Clear()
                Else
                    MessageBox.Show("Usuario o contraseña incorrectos.")
                End If
            End Using
        End Using
    End Sub
End Class