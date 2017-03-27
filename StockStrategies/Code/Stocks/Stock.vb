Imports StockStrategies

Public Class Stock

    Public Property MetaData As StockMetaData
    Public Property Data As StockDataCollection
    Public Property DividendsHistory As DividendsHistory

    Public ReadOnly Property HasDividends As Boolean
        Get
            Return Me.DividendsHistory IsNot Nothing AndAlso Me.DividendsHistory.Count > 0
        End Get
    End Property

    Public Shared Function ReadFromFile(dataFilePath As String, dividendsFilePath As String) As Stock
        Dim r As New Stock

        r.Data = StockDataCollection.ReadFromFile(dataFilePath)
        r.DividendsHistory = DividendsHistory.ReadFromFile(dividendsFilePath)

        Return r
    End Function

    Public Shared Function ReadFromMetaData(metaDataFileName As String) As Stock
        Dim metaData As StockMetaData
        Dim filePath As String = IO.Path.Combine(GetMetaDirectory(), metaDataFileName)

        metaData = StockMetaData.ReadFromFile(filePath)

        Return ReadFromMetaData(metaData)
    End Function

    Public Shared Function ReadFromMetaData(metaData As StockMetaData) As Stock
        Dim r As New Stock
        Dim scraper As ArivaDividendsScraper

        r.MetaData = metaData
        r.Data = StockDataCollection.ReadFromFile(IO.Path.Combine(GetXetraDirectory(), metaData.DataFileName))

        If String.IsNullOrWhiteSpace(metaData.DividendsFileName) Then
            scraper = New ArivaDividendsScraper
            r.DividendsHistory = scraper.LoadDividendsHistory(metaData.ArivaName)
            r.DividendsHistory.Save(metaData.ArivaName & ".json")
            metaData.DividendsFileName = metaData.ArivaName & ".json"
            metaData.Save()

        Else
            r.DividendsHistory = DividendsHistory.ReadFromFile(IO.Path.Combine(GetDividendsDirectory(), metaData.DividendsFileName))
        End If


        Return r
    End Function

End Class
