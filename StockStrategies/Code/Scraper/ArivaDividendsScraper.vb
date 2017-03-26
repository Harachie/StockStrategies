Public Class ArivaDividendsScraper

    Public Shared Function GetDividendsTable(html As String) As String
        Dim startIndex, endIndex As Integer

        startIndex = html.IndexOf("<div class=""histEventsBox new"" >")
        startIndex = html.IndexOf("<table", startIndex)
        endIndex = html.IndexOf("</table>", startIndex)

        Return html.Substring(startIndex, endIndex - startIndex + "</table>".Length)
    End Function

    Public Function LoadDividendsHistory(arrivaStockName As String) As DividendsHistory
        Dim loader As New Downloader
        Dim html As String = loader.DownloadArivaDividends(arrivaStockName)
        Dim dt As String = ArivaDividendsScraper.GetDividendsTable(html).Replace("&", "")
        Dim xml As XElement = XElement.Parse(dt)
        Dim counter As Integer
        Dim isDividende As Boolean
        Dim dividends As Double
        Dim datum As Date
        Dim history As New DividendsHistory

        history.Dividends = New List(Of DividendsHistory.Dividend)

        For Each rowX As XElement In xml.Elements.Skip(1)
            counter = 0
            isDividende = False

            For Each columnX As XElement In rowX.Elements
                If counter = 0 Then 'datum
                    Date.TryParseExact(columnX.Value, "dd.MM.yy", Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, datum)

                ElseIf counter = 1 Then
                    isDividende = (columnX.Value = "Dividende")

                ElseIf isDividende AndAlso counter = 3 Then
                    dividends = Double.Parse(columnX.Value.Replace(" EUR", "").Replace(",", "."), Globalization.CultureInfo.InvariantCulture)

                End If

                counter += 1
            Next

            If isDividende Then
                history.Dividends.Add(New DividendsHistory.Dividend With {.Amount = dividends, .DistributionDate = datum})
            End If
        Next

        history.Dividends.Reverse()

        Return history
    End Function

End Class
