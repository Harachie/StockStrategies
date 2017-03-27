Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Main()
        Dim invest As New InvestMonthlyStrategy
        Dim metac As StockMetaDataCollection
        Dim results As New List(Of StrategyResult)

        CreateAllDirectories()

        metac = StockMetaDataCollection.ReadFromFile("german.json")

        For Each md As StockMetaData In metac
            results.Add(invest.ReinvestDividends(Stock.ReadFromMetaData(md), 10000, 600))
        Next

        'Dim daimler = Stock.ReadFromFile(IO.Path.Combine(GetXetraDirectory(), "dai.de.txt"), IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "Data", "dividends", "daimler.json"))
        'Dim basf = Stock.ReadFromFile(IO.Path.Combine(GetXetraDirectory(), "bas.de.txt"), IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "Data", "dividends", "basf.json"))
        'Dim bmw As Stock = Stock.ReadFromMetaData("bmw.json")

        'Dim resultDaimler = invest.ReinvestDividends(daimler, 10000, 600)
        'Dim resultBasf = invest.ReinvestDividends(basf, 10000, 600)
        'Dim resultBmw = invest.ReinvestDividends(bmw, 10000, 600)

        Dim ka = results.OrderByDescending(Function(sr) sr.GainedDividends).Select(Function(sr)
                                                                                       Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                   End Function).ToList


        Dim ka2 = results.OrderByDescending(Function(sr) sr.CurrentInvestmentValue).Select(Function(sr)
                                                                                               Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                           End Function).ToList
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
