Imports Newtonsoft.Json

Public Class StockMetaDataCollection
    Inherits List(Of StockMetaData)

    Public Shared Function ReadFromFile(fileName As String) As StockMetaDataCollection
        Dim r As StockMetaDataCollection
        Dim filePath As String

        filePath = IO.Path.Combine(GetCollectionsDirectory(), fileName)
        r = JsonConvert.DeserializeObject(Of StockMetaDataCollection)(IO.File.ReadAllText(filePath))

        Return r
    End Function

End Class
