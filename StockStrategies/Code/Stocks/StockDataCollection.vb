Public Class StockDataCollection
    Inherits List(Of StockData)

    Public Function FilterByMinimumStartDate(startDate As Date) As StockDataCollection
        Dim r As New StockDataCollection

        r.AddRange(Me.Where(Function(sd) sd.Date >= startDate))

        Return r
    End Function

    Public Function Filter(predicate As Func(Of StockData, Boolean)) As StockDataCollection
        Dim r As New StockDataCollection

        r.AddRange(Me.Where(predicate))

        Return r
    End Function

    Public Shared Function StripNonExistentDates(ParamArray collections As StockDataCollection()) As IList(Of StockDataCollection)
        Dim r As New List(Of StockDataCollection)
        Dim dateSets As New List(Of HashSet(Of Date))
        Dim hs As HashSet(Of Date) = Nothing

        For Each collection In collections
            hs = New HashSet(Of Date)
            dateSets.Add(hs)

            For Each sd As StockData In collection
                hs.Add(sd.Date)
            Next
        Next

        For i As Integer = 0 To dateSets.Count - 2
            hs.IntersectWith(dateSets(i))
        Next

        For Each collection In collections
            r.Add(collection.Filter(Function(sd) hs.Contains(sd.Date)))
        Next

        Return r
    End Function

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
