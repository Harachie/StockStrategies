Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Main()
        Dim json As String = IO.File.ReadAllText(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "dax formated.json"))
        Dim normalized As String = Normalize(json)
        Dim raw As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, String))) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, String))))(normalized)
        Dim semiRaw As Dictionary(Of String, Stock) = JsonConvert.DeserializeObject(Of Dictionary(Of String, Stock))(normalized)

    End Sub

    Public Function Normalize(json As String) As String
        Return Regex.Replace(json, "(\d+),(\d+)", "$1.$2")
    End Function

    Public Function LoadDax() As Object

    End Function

End Module
