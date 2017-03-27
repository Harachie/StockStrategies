Public Class StockData

    Public Property [Date] As Date
    Public Property Open As Double
    Public Property High As Double
    Public Property Low As Double
    Public Property Close As Double
    Public Property Volume As Long

    Public Shared Function FromLine(line As String) As StockData
        Dim r As New StockData
        Dim splits As String()

        splits = line.Split(","c)
        r.Date = Date.ParseExact(splits(0), "yyyyMMdd", Globalization.CultureInfo.InvariantCulture)
        r.Open = Double.Parse(splits(1), Globalization.CultureInfo.InvariantCulture)
        r.High = Double.Parse(splits(2), Globalization.CultureInfo.InvariantCulture)
        r.Low = Double.Parse(splits(3), Globalization.CultureInfo.InvariantCulture)
        r.Close = Double.Parse(splits(4), Globalization.CultureInfo.InvariantCulture)
        r.Volume = Long.Parse(splits(5))

        Return r
    End Function

    Public Shared Function FromStooqLine(line As String) As StockData
        Dim r As New StockData
        Dim splits As String()

        splits = line.Split(","c)
        r.Date = Date.ParseExact(splits(0), "yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture)
        r.Open = Double.Parse(splits(1), Globalization.CultureInfo.InvariantCulture)
        r.High = Double.Parse(splits(2), Globalization.CultureInfo.InvariantCulture)
        r.Low = Double.Parse(splits(3), Globalization.CultureInfo.InvariantCulture)
        r.Close = Double.Parse(splits(4), Globalization.CultureInfo.InvariantCulture)

        If splits.Length > 5 Then
            r.Volume = Long.Parse(splits(5))
        End If

        Return r
    End Function

End Class
