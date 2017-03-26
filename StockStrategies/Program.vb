Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Main()
        Dim dax As List(Of StockFundamentals) = LoadDax()
        Dim invest As New InvestMonthlyStrategy
        Dim loader As New Downloader

        Dim daimler = Stock.ReadFromFile(IO.Path.Combine(GetXetraDirectory(), "dai.de.txt"), IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "Data", "dividends", "daimler.json"))
        Dim basf = Stock.ReadFromFile(IO.Path.Combine(GetXetraDirectory(), "bas.de.txt"), IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "Data", "dividends", "basf.json"))

        Dim resultDaimler = invest.ReinvestDividends(daimler, 10000, 600)
        Dim resultBasf = invest.ReinvestDividends(basf, 10000, 600)

        Dim html As String = loader.DownloadArivaDividends("bmw")
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

        history.Save(IO.Path.Combine(GetDividendsDirectory(), "bmw.json"))
    End Sub

    Public Function Normalize(json As String) As String
        Return Regex.Replace(json, "(\d+),(\d+)", "$1.$2")
    End Function

    Public Function LoadNormalized(filePath As String) As String
        Dim fileNameWithoutExtension As String = IO.Path.GetFileNameWithoutExtension(filePath)
        Dim normalizedFileName As String = fileNameWithoutExtension & "_normalized" & IO.Path.GetExtension(filePath)
        Dim directoy As String = IO.Path.GetDirectoryName(filePath)
        Dim normalizedFilePath As String = IO.Path.Combine(directoy, normalizedFileName)
        Dim content As String

        If IO.File.Exists(normalizedFilePath) Then
            Return IO.File.ReadAllText(normalizedFilePath)
        End If

        content = IO.File.ReadAllText(filePath)
        content = Normalize(content)
        IO.File.WriteAllText(normalizedFilePath, content)

        Return content
    End Function

    Public Function LoadStocks(filePath As String) As List(Of StockFundamentals)
        Dim r As New List(Of StockFundamentals)
        Dim json As String = LoadNormalized(filePath)
        Dim semiRaw As Dictionary(Of String, StockFundamentals) = JsonConvert.DeserializeObject(Of Dictionary(Of String, StockFundamentals))(json)

        For Each kv In semiRaw
            kv.Value.Name = kv.Key
            r.Add(kv.Value)
        Next

        Return r
    End Function

    Public Function LoadDax() As List(Of StockFundamentals)
        Return LoadStocks(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "dax formated.json"))
    End Function

End Module
