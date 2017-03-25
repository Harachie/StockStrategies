Public Class Stock

    Public Property Name As String
    Public Property Data As New List(Of StockData)
    Public Property Dividends As DividendsHistory

    Public Shared Function ReadFromFile(filePath As String) As Stock
        Dim r As New Stock
        Dim line As String

        Using reader As New IO.StreamReader(IO.File.OpenRead(filePath))
            reader.ReadLine()

            While Not reader.EndOfStream
                line = reader.ReadLine
                r.Data.Add(StockData.FromLine(line))
            End While
        End Using

        Return r
    End Function

End Class
