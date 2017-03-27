Imports Newtonsoft.Json

Public Class DividendsHistory
    Inherits List(Of Dividend)

    Public Shared Function ReadFromFile(fileName As String) As DividendsHistory
        Dim r As DividendsHistory
        Dim json As String

        json = IO.File.ReadAllText(IO.Path.Combine(GetDividendsDirectory(), fileName))
        r = JsonConvert.DeserializeObject(Of DividendsHistory)(json)

        Return r
    End Function

    Public Sub Save(fileName As String)
        WriteAllText(IO.Path.Combine(GetDividendsDirectory(), fileName), JsonConvert.SerializeObject(Me))
    End Sub

End Class
