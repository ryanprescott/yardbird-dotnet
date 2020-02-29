Imports System.IO
Public Class Ini
    Dim iniText As String = ""
    Public iniPath As String = ""
    Dim sections As Dictionary(Of String, List(Of List(Of Object)))
    Sub New(pathToIni As String)
        LoadIni(pathToIni)
    End Sub

    Private Sub LoadIni(pathtoini)
        sections = New Dictionary(Of String, List(Of List(Of Object)))
        iniPath = pathtoini
        iniText = vbCrLf & File.ReadAllText(iniPath)
        Dim firstSplit = Split(iniText, vbCrLf & "[")
        For Each Item As String In firstSplit
            Dim secondSplit = Split(Item, "]" & vbCrLf)
            Dim okay = True
            If secondSplit(0).Contains(vbCrLf) Or
               secondSplit(0).Contains("[") Or
               secondSplit(0).Contains("]") Or
               secondSplit(0).Contains("=") Or
               secondSplit(0) = "" Then
                okay = False
            End If
            If okay = True Then
                Dim sectionName = secondSplit(0)
                sections.Add(sectionName, New List(Of List(Of Object)))
                If secondSplit.Count > 1 Then
                    Dim thirdSplit = Split(secondSplit(1), vbCrLf)
                    Dim iterator = 0
                    For Each KVP As String In thirdSplit
                        Dim fourthSplit = Split(KVP, "=")
                        If fourthSplit.Count > 1 Then
                            Dim k As String = fourthSplit(0)
                            Dim v As String = fourthSplit(1)
                            sections(sectionName).Add(New List(Of Object) From {True, k, v})
                        Else
                            If KVP <> "" Then
                                sections(sectionName).Add(New List(Of Object) From {False, KVP})
                            End If
                        End If
                    Next
                End If
            End If
        Next
        If sections.Count = 0 Then
            Throw New Exception("File is empty or not a valid INI.")
        End If
    End Sub

    Function GetKey(section As String, key As String, Optional defaultValue As Object = Nothing) As Object
        If sections.ContainsKey(section) Then
            For Each LOO As List(Of Object) In sections(section)
                If LOO(0) = True Then
                    If LOO(1).ToString.ToLower = key.ToLower Then
                        Dim value As Object = Nothing
                        If Not Integer.TryParse(LOO(2).ToString, value) Then
                            If Not Boolean.TryParse(LOO(2).ToString, value) Then
                                value = LOO(2)
                            End If
                        End If
                        Return value
                    End If
                End If
            Next
            Return defaultValue
        Else
            Return defaultValue
        End If
    End Function
    Function GetSection(section As String) As Dictionary(Of String, Object)
        If sections.ContainsKey(section) Then
            Dim tmpDict = New Dictionary(Of String, Object)
            For Each LOO As List(Of Object) In sections(section)
                tmpDict.Add(LOO(1), LOO(2))
            Next
            Return tmpDict
        Else
            Return Nothing
        End If
    End Function
    Function SetKey(section As String, key As String, value As Object) As Boolean
        If sections.ContainsKey(section) Then
            Dim tmpDict As New Dictionary(Of String, Object)
            For Each LOO As List(Of Object) In sections(section)
                If LOO(0) = True Then
                    If LOO(1).ToString.ToLower = key.ToLower Then
                        LOO(2) = value
                        Return True
                    End If
                End If
            Next
            Return False
        Else
            Return False
        End If
    End Function
    Function AddKey(section As String, key As String, Optional value As Object = "") As Boolean
        For Each LOO As List(Of Object) In sections(section)
            If LOO(1).ToString.ToLower = key.ToLower Then
                Return False
            End If
        Next
        If IsNothing(value) Then
            Try
                sections(section).Add(New List(Of Object) From {False, key})
                Return True
            Catch
                Return False
            End Try
        Else
            Try
                sections(section).Add(New List(Of Object) From {True, key, value})
                Return True
            Catch
                Return False
            End Try
        End If
    End Function

    Function WriteOut()
        Try
            Dim bldr As String = ""
            For Each Item As KeyValuePair(Of String, List(Of List(Of Object))) In sections
                bldr = bldr & "[" & Item.Key & "]" & vbCrLf
                For Each LOO As List(Of Object) In Item.Value
                    If LOO(0) = True Then
                        bldr = bldr & LOO(1) & "=" & LOO(2) & vbCrLf
                    Else
                        bldr = bldr & LOO(1) & vbCrLf
                    End If
                Next
                bldr = bldr & vbCrLf
            Next
            File.WriteAllText(iniPath, bldr)
            Return True
        Catch
            Return False
        End Try
    End Function

    Function Reload()
        Try
            LoadIni(iniPath)
            Return True
        Catch
            Return False
        End Try
    End Function
End Class
