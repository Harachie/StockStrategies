Imports System.Runtime.CompilerServices

Public Module Tools

    Public Const TAX_FREE_MULTIPLIER As Double = 0.73625
    Public Const TAX_FREE_AMOUNT As Double = 801.0

    Public Sub WriteAllText(filePath As String, content As String)
        IO.File.WriteAllText(filePath, content, New Text.UTF8Encoding(False))
    End Sub

    Public Function GetXetraDirectory() As String
        Return IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "xetra")
    End Function

    Public Function GetDividendsDirectory() As String
        Return IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "dividends")
    End Function

    Public Function GetCacheDirectory() As String
        Return IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "cache")
    End Function

    <Extension> Public Function Sha1(this As String) As String
        Dim hash As New System.Security.Cryptography.SHA1Managed
        Dim data As Byte() = Text.Encoding.UTF8.GetBytes(this)

        Return BitConverter.ToString(hash.ComputeHash(data)).Replace("-", String.Empty)
    End Function

End Module
