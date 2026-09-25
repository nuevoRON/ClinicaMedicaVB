Public Module Session
    Public Property CurrentUser As String = "SISTEMA"
    Public Property CurrentRole As String = "SISTEMA"

    Public Sub Start(usuario As String, rol As String)
        CurrentUser = If(String.IsNullOrWhiteSpace(usuario), "SISTEMA", usuario.Trim())
        CurrentRole = If(String.IsNullOrWhiteSpace(rol), "SISTEMA", rol.Trim())
    End Sub

    Public Sub EndSession()
        CurrentUser = "SISTEMA"
        CurrentRole = "SISTEMA"
    End Sub
End Module
