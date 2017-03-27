Public Class StockDataCollection
    Inherits List(Of StockData)

    Public Shared Function ReadFromFile(filePath As String) As StockDataCollection
        Dim r As New StockDataCollection
        Dim line As String

        Using reader As New IO.StreamReader(IO.File.OpenRead(filePath))
            reader.ReadLine()

            While Not reader.EndOfStream
                line = reader.ReadLine
                r.Add(StockData.FromLine(line))
            End While
        End Using

        Return r
    End Function

    Public Shared Function ReadFromStooqFile(filePath As String) As StockDataCollection
        Dim r As New StockDataCollection
        Dim line As String

        Using reader As New IO.StreamReader(IO.File.OpenRead(filePath))
            reader.ReadLine()

            While Not reader.EndOfStream
                line = reader.ReadLine
                r.Add(StockData.FromStooqLine(line))
            End While
        End Using

        Return r
    End Function

End Class
