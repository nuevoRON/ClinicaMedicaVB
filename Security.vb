Imports System.Security.Cryptography

Public Module Session
    Public Property CurrentUser As String = ""
    Public Property CurrentRole As String = ""

    Public Sub Start(usuario As String, rol As String)
        CurrentUser = usuario
        CurrentRole = rol
    End Sub

    Public Sub EndSession()
        CurrentUser = ""
        CurrentRole = ""
    End Sub
End Module

Public Module SecurityHelper
    Private Const Iterations As Integer = 100000
    Private Const SaltSize As Integer = 16
    Private Const HashSize As Integer = 32

    Public Function HashPassword(password As String) As String
        Dim salt(SaltSize - 1) As Byte
        RandomNumberGenerator.Fill(salt)
        Using pbkdf As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
            Dim hash = pbkdf.GetBytes(HashSize)
            Return "PBKDF2$" & Iterations.ToString() & "$" & Convert.ToBase64String(salt) & "$" & Convert.ToBase64String(hash)
        End Using
    End Function

    Public Function VerifyPassword(password As String, stored As String) As Boolean
        If String.IsNullOrWhiteSpace(stored) Then Return False
        Dim parts = stored.Split("$"c)
        If parts.Length <> 4 OrElse Not parts(0).Equals("PBKDF2", StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If
        Try
            Dim iterations = Integer.Parse(parts(1))
            Dim salt = Convert.FromBase64String(parts(2))
            Dim expected = Convert.FromBase64String(parts(3))
            Using pbkdf As New Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)
                Dim actual = pbkdf.GetBytes(expected.Length)
                Return CryptographicOperations.FixedTimeEquals(actual, expected)
            End Using
        Catch
            Return False
        End Try
    End Function

    Public Function IsHashed(stored As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(stored) AndAlso stored.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase)
    End Function
End Module