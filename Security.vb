Imports System.Security.Cryptography

Public Module SecurityHelper
    Private Const Iterations As Integer = 100000
    Private Const SaltSize As Integer = 16
    Private Const HashSize As Integer = 32
    Private Const Prefix As String = "PBKDF2$"

    Public Function HashPassword(password As String) As String
        If password Is Nothing Then Throw New ArgumentNullException(NameOf(password))

        Dim salt(SaltSize - 1) As Byte
        RandomNumberGenerator.Fill(salt)

        Using pbkdf As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
            Dim hash = pbkdf.GetBytes(HashSize)
            Return Prefix & Iterations.ToString() & "$" &
                   Convert.ToBase64String(salt) & "$" &
                   Convert.ToBase64String(hash)
        End Using
    End Function

    Public Function VerifyPassword(password As String, stored As String) As Boolean
        If password Is Nothing OrElse String.IsNullOrWhiteSpace(stored) Then Return False

        Dim parts = stored.Split("$"c)
        If parts.Length <> 4 OrElse
           Not parts(0).Equals("PBKDF2", StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        Try
            Dim iterations As Integer
            If Not Integer.TryParse(parts(1), iterations) OrElse iterations <= 0 Then Return False

            Dim salt = Convert.FromBase64String(parts(2))
            Dim expected = Convert.FromBase64String(parts(3))

            If salt.Length = 0 OrElse expected.Length = 0 Then Return False

            Using pbkdf As New Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)
                Dim actual = pbkdf.GetBytes(expected.Length)
                Return CryptographicOperations.FixedTimeEquals(actual, expected)
            End Using
        Catch
            Return False
        End Try
    End Function

    Public Function IsHashed(stored As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(stored) AndAlso
               stored.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
    End Function
End Module
