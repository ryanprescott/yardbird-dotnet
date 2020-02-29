Module IntrusionControl
    Sub LogImpersonationAttempt(sockID As Integer, endpoint As String, handle As String, realKey As String)
        If realKey = 0 Then
            AddConLine("ATTENTION: Peer " & sockID & " connected from " & endpoint & " tried to impersonate the SERVER user!")
            WriteServerData(sockID, "ERRS/201/Attempt detected to impersonate the SERVER user. You will now be disconnected.")
            GracefulClean(sockID)
        End If
    End Sub
End Module
