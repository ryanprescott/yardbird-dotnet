Imports System.Net
Public Class frmHostSearch
    Private Sub frmHostSearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        GetAllSubnets()
    End Sub
    Public Function IPsAndSubnets() As Dictionary(Of IPAddress, IPAddress)
        Dim nic = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
        Dim colNicConfigs = nic.ExecQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True")
        Dim ipList As New Dictionary(Of IPAddress, IPAddress)
        For Each nic In colNicConfigs
            For i = 0 To DirectCast(nic.IPAddress, Array).GetUpperBound(0)
                ipList.Add(Net.IPAddress.Parse(nic.IPAddress(i).ToString), Net.IPAddress.Parse(nic.IPSubnet(i).ToString))
            Next
        Next
            Return ipList
    End Function

    Function GetAllSubnets()
        For Each Item As KeyValuePair(Of IPAddress, IPAddress) In IPsAndSubnets()
            Dim ip = Item.Key.GetAddressBytes
            Dim subnet = Item.Value.GetAddressBytes
            Try
                Dim startIPBytes(ip.Length - 1) As Byte
                Dim endIPBytes(ip.Length - 1) As Byte

                For i As Integer = 0 To ip.Length - 1
                    startIPBytes(i) = CByte(ip(i) And subnet(i))
                    endIPBytes(i) = CByte(ip(i) Or (Not subnet(i)))
                Next i
            Catch
            End Try
        Next
        Return False
    End Function
End Class