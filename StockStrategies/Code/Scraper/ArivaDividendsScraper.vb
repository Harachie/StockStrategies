Public Class ArivaDividendsScraper

    Public Shared Function GetDividendsTable(html As String) As String
        Dim startIndex, endIndex As Integer

        startIndex = html.IndexOf("<div class=""histEventsBox new"" >")
        startIndex = html.IndexOf("<table", startIndex)
        endIndex = html.IndexOf("</table>", startIndex)

        Return html.Substring(startIndex, endIndex - startIndex + "</table>".Length)
    End Function

End Class
