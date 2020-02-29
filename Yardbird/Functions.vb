Imports System.IO
Imports System.Reflection
Imports System.Drawing.Text
Imports System.Runtime.InteropServices
Module Functions
    Dim tip As ToolTip

    Function TrimWhite(data As String)
        Dim tmpData = data.TrimEnd(Chr(32), Chr(10), Chr(13), Chr(9), Chr(0))
        tmpData = tmpData.TrimStart(Chr(32), Chr(10), Chr(13), Chr(9), Chr(0))
        Return tmpData
    End Function

    Function RealPathtoWebPath(path)
        Try
            IO.Path.GetFullPath(path)
            Return "file:///" & Replace(path, "\", "/")
        Catch ex As Exception
            ErrHndl(ex.HResult, ex.Message, ex.StackTrace, "")
            Return ""
        End Try
    End Function


    Sub ErrHndl(hresult As Integer, innerex As String, stacktrace As String, source As String)
        Try
            AddConLine("ERR: " & Hex(hresult) & " - " & innerex)
        Catch
            MsgBox("A serious error has occurred. Please report this to the developer." & vbCrLf & vbCrLf & innerex & vbCrLf & vbCrLf & stacktrace)
        End Try
    End Sub
End Module

