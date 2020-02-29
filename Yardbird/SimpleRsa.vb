
Imports System.Security.Cryptography
Public Class SimpleRSA
    Implements IDisposable
    Private disposedValue As Boolean
    Private rsa2048 As RSACryptoServiceProvider

    Sub New(Optional key As String = Nothing)
        If IsNothing(key) Then
            rsa2048 = New RSACryptoServiceProvider(2048)
        Else
            rsa2048 = New RSACryptoServiceProvider(2048)
            If IsNothing(key) Then
                Throw New SystemException("Invalid BER key.")
            Else
                rsa2048.FromXmlString(key)
            End If
        End If
    End Sub

    Public Function EncryptData(
    ByVal plaintext As String) As String

        ' convert the plaintext string to a byte array.
        Dim plaintextbytes() As Byte =
            System.Text.Encoding.UTF8.GetBytes(plaintext)

        Dim encryptedData = rsa2048.Encrypt(plaintextbytes, True)
        Return Convert.ToBase64String(encryptedData.ToArray)
    End Function
    Public Function ExportXML(Optional priv As Boolean = False) As String
        Return rsa2048.ToXmlString(priv)
    End Function
    Public Function ExportPublicKey(Optional DoNotConvert As Boolean = False) As String
        Dim params As RSAParameters = rsa2048.ExportParameters(False)
        Dim header() As Byte = {&H30, &H81, &H89, &H2, &H81, &H81, &H0}
        Dim modulus() As Byte = params.Modulus.ToArray
        Dim spacer() As Byte = {&H2, &H3}
        Dim exponent() As Byte = params.Exponent.ToArray
        Dim ber(header.Length + modulus.Length + spacer.Length + exponent.Length - 1) As Byte
        header.CopyTo(ber, 0)
        modulus.CopyTo(ber, header.Length)
        spacer.CopyTo(ber, header.Length + modulus.Length)
        exponent.CopyTo(ber, header.Length + modulus.Length + spacer.Length)
        Return Convert.ToBase64String(ber)
    End Function



    Public Function decryptdata(
    ByVal encryptedtext As String) As String
        Try
            ' convert the encrypted text string to a byte array.
            Dim encryptedbytes() As Byte
            encryptedbytes = Convert.FromBase64String(encryptedtext)

            Dim decryptedData = rsa2048.Decrypt(encryptedbytes, True)

            Return System.Text.Encoding.UTF8.GetString(decryptedData)
        Catch
            Return Nothing
        End Try
    End Function
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                rsa2048.Dispose()
            End If
        End If
        Me.disposedValue = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
End Class
