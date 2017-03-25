Public Class Stock

    Public Property Name As String
    Public Property Data As StockDataCollection
    Public Property DividendsHistory As DividendsHistory

    Public ReadOnly Property HasDividends As Boolean
        Get
            Return Me.DividendsHistory IsNot Nothing AndAlso Me.DividendsHistory.Dividends IsNot Nothing AndAlso Me.DividendsHistory.Dividends.Count > 0
        End Get
    End Property

    Public Shared Function ReadFromFile(dataFilePath As String, dividendsFilePath As String) As Stock
        Dim r As New Stock

        r.Data = StockDataCollection.ReadFromFile(dataFilePath)
        r.DividendsHistory = DividendsHistory.ReadFromFile(dividendsFilePath)

        Return r
    End Function

End Class
