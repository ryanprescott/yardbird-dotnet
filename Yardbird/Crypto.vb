Imports System.Security.Cryptography
Imports System.Text
Module Crypto
    Function SHA256(text As String)
        Dim sha As SHA256 = SHA256Managed.Create()
        Dim hashString = ""
        sha.ComputeHash(Encoding.UTF8.GetBytes(text))
        For Each Item In sha.Hash
            hashString = hashString & Format(Item, "X2")
        Next
        Return hashString
    End Function

    Function GetNewAESEncryptionKey()
        Dim byte_count As Byte() = New Byte(31) {}
        Dim random_number As New RNGCryptoServiceProvider()
        random_number.GetBytes(byte_count)
        Dim hashString = ""
        For Each Item In byte_count
            hashString = hashString & Format(Item, "X2")
        Next
        Return hashString
    End Function

    Function GetNewRandom128Bit()
        Dim byte_count As Byte() = New Byte(15) {}
        Dim random_number As New RNGCryptoServiceProvider()
        random_number.GetBytes(byte_count)
        Dim hashString = ""
        For Each Item In byte_count
            hashString = hashString & Format(Item, "X2")
        Next
        Return hashString
    End Function

    Function GetNewRandom16Bit()
        Dim byte_count As Byte() = New Byte(3) {}
        Dim random_number As New RNGCryptoServiceProvider()
        random_number.GetBytes(byte_count)
        Dim hashString = ""
        For Each Item In byte_count
            hashString = hashString & Format(Item, "X2")
        Next
        Return hashString
    End Function

    Function BerToXML(keyString As String) As String
        Dim finalParams As String = ""
        Dim expectedHeader() As Byte = {&H30, &H81, &H89, &H2, &H81, &H81, &H0}
        Dim gotHeader(6) As Byte
        Dim expectedSpacer() As Byte = {&H2, &H3}
        Dim gotSpacer(1) As Byte
        Dim bytes() As Byte = {}
        Dim modulus(255) As Byte
        Try
            bytes = Convert.FromBase64String(keyString)
            Array.ConstrainedCopy(bytes, 0, gotHeader, 0, expectedHeader.Length)
            If gotHeader.SequenceEqual(expectedHeader) Then
                Array.ConstrainedCopy(bytes, 256 + gotHeader.Length, gotSpacer, 0, expectedSpacer.Length)
                If gotSpacer.SequenceEqual(expectedSpacer) Then
                    Array.ConstrainedCopy(bytes, gotHeader.Length, modulus, 0, 256)
                    Dim exponent(bytes.Length - (gotHeader.Length + modulus.Length + gotSpacer.Length) - 1) As Byte
                    Array.ConstrainedCopy(bytes, gotHeader.Length + modulus.Length + gotSpacer.Length, exponent, 0, bytes.Length - (gotHeader.Length + modulus.Length + gotSpacer.Length))
                    finalParams = finalParams & "<RSAKeyValue>"
                    finalParams = finalParams & "   <Modulus>" & Convert.ToBase64String(modulus) & "</Modulus>"
                    finalParams = finalParams & "   <Exponent>" & Convert.ToBase64String(exponent) & "</Exponent>"
                    finalParams = finalParams & "</RSAKeyValue>"
                    Return finalParams
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Catch ex As Exception
            AddConLine("Error importing RSA Key: " & Hex(ex.HResult) & " " & ex.Message & "\n" & ex.StackTrace)
            Return Nothing
        End Try
    End Function
End Module
