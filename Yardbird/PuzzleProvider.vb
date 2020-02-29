Public Class PuzzleProvider
    Implements IDisposable

    Private Declare Auto Function BitBlt Lib "GDI32.DLL" _
    (ByVal hdcDest As IntPtr, ByVal nXDest As Integer,
    ByVal nYDest As Integer, ByVal nWidth As Integer,
    ByVal nHeight As Integer, ByVal hdcSrc As IntPtr,
    ByVal nXSrc As Integer, ByVal nYSrc As Integer,
    ByVal dwRop As Int32) As Boolean

    Private disposedValue As Boolean
    Private Rand As New Random(Now.Millisecond)
    Private number1 As Integer
    Private number2 As Integer
    Private puzzleString As String = ""
    Private puzzleAnswer As Integer = 0
    Private puzzleImage As Bitmap
    Private puzzleImage64 As String

    ReadOnly Property Solution As Integer
        Get
            Return puzzleAnswer
        End Get
    End Property

    ReadOnly Property Puzzle As String
        Get
            Return puzzleString
        End Get
    End Property

    ReadOnly Property Image As Bitmap
        Get
            Return puzzleImage
        End Get
    End Property

    ReadOnly Property ImageString As String
        Get
            Return puzzleImage64
        End Get
    End Property

    Function RasterNum()
        Using Collection As New System.Drawing.Text.PrivateFontCollection
            Dim Bytes() As Byte = My.Resources.RasterNum3
            Dim Ptr As IntPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(Bytes.Length)
            System.Runtime.InteropServices.Marshal.Copy(Bytes, 0, Ptr, Bytes.Length)
            Collection.AddMemoryFont(Ptr, Bytes.Length)
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(Ptr)
            Return New Drawing.Font(Collection.Families(0), 12, FontStyle.Regular)
        End Using
    End Function
    Public Sub New(Optional minValue As Integer = 0, Optional maxValue As Integer = 20, Optional allowSame As Boolean = True)
        number1 = (Rand.Next(19) + 1)
        number2 = (Rand.Next(19) + 1)
        If number1 = number2 And allowSame = False Then
            Dim iterator = 0
            While number1 = number2
                If iterator = 100 Then
                    Throw New SystemException("While loop iteration max count exceeded")
                End If
                number1 = (Rand.Next(19) + 1)
                number2 = (Rand.Next(19) + 1)
                iterator += 1
            End While
        End If
        Dim chance = Rand.Next(1)
        If chance = 1 Then
            If number1 > number2 Then
                puzzleAnswer = number1 - number2
                puzzleString = number1 & "-" & number2 & "="
            Else
                puzzleAnswer = number1 + number2
                puzzleString = number1 & "+" & number2 & "="
            End If
        Else
            If number2 > number1 Then
                puzzleAnswer = number2 - number1
                puzzleString = number2 & "-" & number1 & "="
            Else
                puzzleAnswer = number1 + number2
                puzzleString = number1 & "+" & number2 & "="
            End If
        End If
        Dim image = GenerateImage(puzzleString)
        puzzleImage = image.Key
        puzzleImage64 = image.Value
    End Sub

    Private Protected Function GenerateImage(str As String) As KeyValuePair(Of Bitmap, String)
        Dim width = (str.Length * 10) + 3
        Dim im As New Bitmap(width, 11)
        im.SetResolution(96, 96)
        Dim gfx As Graphics = Graphics.FromImage(im)
        gfx.SmoothingMode = Drawing2D.SmoothingMode.None
        gfx.TextRenderingHint = Drawing.Text.TextRenderingHint.SingleBitPerPixel
        Dim fnt = RasterNum()
        Dim bsh As New SolidBrush(Color.Black)
        gfx.DrawString(str, fnt, bsh, 0, 0)
        Dim tmpBmp As Bitmap = im.Clone(New Rectangle(0, 0, im.Width, im.Height), Imaging.PixelFormat.Format1bppIndexed)
        im.Dispose()
        Dim ms As New IO.MemoryStream
        tmpBmp.Save(ms, Imaging.ImageFormat.Bmp)
        Dim tmp64 As String = Convert.ToBase64String(ms.ToArray)
        ms.Dispose()
        Dim tmpDict As New KeyValuePair(Of Bitmap, String)(tmpBmp.Clone, tmp64)
        tmpBmp.Dispose()
        Return tmpDict
    End Function
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                puzzleImage.Dispose()
                Rand = Nothing
                number1 = Nothing
                number2 = Nothing
                puzzleString = Nothing
                puzzleAnswer = Nothing
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
