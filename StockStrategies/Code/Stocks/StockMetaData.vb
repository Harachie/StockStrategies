Imports Newtonsoft.Json

Public Class StockMetaData

    Public Property Name As String
    Public Property ArivaName As String
    Public Property DividendsFileName As String
    Public Property DataFileName As String

    Public Shared Function ReadFromFile(filePath As String) As StockMetaData
        Return FromJson(IO.File.ReadAllText(filePath))
    End Function

    Public Shared Function FromJson(json As String) As StockMetaData
        Return JsonConvert.DeserializeObject(Of StockMetaData)(json)
    End Function

    Public Function ToJson() As String
        Return JsonConvert.SerializeObject(Me)
    End Function

    Public Sub Save()
        WriteAllText(IO.Path.Combine(GetMetaDirectory(), Me.Name & ".json"), JsonConvert.SerializeObject(Me))
    End Sub

End Class
