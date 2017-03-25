Public Module Tools

    Public Function GetXetraDirectory() As String
        Return IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "xetra")
    End Function

End Module
