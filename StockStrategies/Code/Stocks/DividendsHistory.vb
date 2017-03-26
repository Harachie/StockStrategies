Imports Newtonsoft.Json

Public Class DividendsHistory

    <JsonProperty("Dividenden")> Public Property Dividends As List(Of Dividend)

    Public Shared Function ReadFromFile(filePath As String) As DividendsHistory
        Dim r As DividendsHistory
        Dim json As String = IO.File.ReadAllText(filePath)

        r = JsonConvert.DeserializeObject(Of DividendsHistory)(json)

        Return r
    End Function

    Public Sub Save(fileName As String)
        WriteAllText(IO.Path.Combine(GetDividendsDirectory(), fileName), JsonConvert.SerializeObject(Me))
    End Sub

    Public Class Dividend

        <JsonProperty("Datum")> Public Property DistributionDate As Date
        <JsonProperty("Betrag")> Public Property Amount As Double

    End Class

End Class
