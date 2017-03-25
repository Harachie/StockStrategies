Public Module Tools

    Public Const TAX_FREE_MULTIPLIER As Double = 0.73625
    Public Const TAX_FREE_AMOUNT As Double = 801.0

    Public Function GetXetraDirectory() As String
        Return IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "xetra")
    End Function

End Module
