Public Module PermissionHelper
    Public Function PuedeAcceder(rol As String, modulo As String) As Boolean
        If String.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase) Then Return True
        If String.Equals(rol, "Médico", StringComparison.OrdinalIgnoreCase) Then
            Return {"Pacientes","Citas","Consultas","Expediente","Recetas","Reportes"}.Contains(modulo)
        End If
        If String.Equals(rol, "Recepción", StringComparison.OrdinalIgnoreCase) Then
            Return {"Pacientes","Citas"}.Contains(modulo)
        End If
        Return False
    End Function

    Public Function VerificarAcceso(rol As String, modulo As String, formulario As Form) As Boolean
        If PuedeAcceder(rol, modulo) Then Return True
        Database.RegistrarAccion(Session.CurrentUser, Session.CurrentRole, "Acceso denegado", modulo, formulario.Text)
        MessageBox.Show("No tiene permisos para acceder a este módulo.", "Acceso restringido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        formulario.BeginInvoke(New Action(Sub() formulario.Close()))
        Return False
    End Function
End Module
