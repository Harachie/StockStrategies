Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Main()
        Dim invest As New InvestMonthlyStrategy
        Dim momentum As New MomentumStrategy
        Dim metac As StockMetaDataCollection
        Dim results As New List(Of StrategyResult)
        Dim loader As New Downloader
        Dim sd As String
        Dim s As Stock

        CreateAllDirectories()

        metac = StockMetaDataCollection.ReadFromFile("german.json")

        Dim allStocks As New List(Of Stock)

        For Each md As StockMetaData In metac
            s = Stock.ReadStooqFromMetaData(md)
            allStocks.Add(s)
            results.Add(invest.ReinvestDividends(s, 10000, 100))
            '   sd = loader.DownloadStooqData(md)
            '  WriteAllText(IO.Path.Combine(GetStooqDirectory(), md.DataFileName), sd)
        Next

        Dim daimler = Stock.ReadFromMetaData("daimler.json")
        Dim basf = Stock.ReadFromMetaData("basf.json")
        Dim bmw As Stock = Stock.ReadFromMetaData("bmw.json")

        'Dim resultDaimler = invest.ReinvestDividends(daimler, 10000, 600)
        'Dim resultBasf = invest.ReinvestDividends(basf, 10000, 600)
        'Dim resultBmw = invest.ReinvestDividends(bmw, 10000, 600)

        'Dim momrs = momentum.InvestMonthlySelectMaximumMomentum({daimler, basf, bmw}, 10000, 1200)
        'Dim momrsev = momentum.InvestMonthly({daimler, basf, bmw}, 10000, 1200)

        Dim kp As New List(Of Object)
        Dim investedEvenly, investedMax, investedMin As Double

        For i As Integer = 5 To 260 Step 5
            momentum.StepSize = i

            Dim momrsa = momentum.InvestMonthlySelectMaximumMomentum(allStocks, 10000, 1200)
            Dim overall = momrsa.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            Dim momrsevai = momentum.InvestMonthlySelectMinimumMomentum(allStocks, 10000, 1200)
            Dim overallei = momrsevai.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            Dim momrseva = momentum.InvestMonthly(allStocks, 10000, 1200)
            Dim overalle = momrseva.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            Dim max3Momentum = momentum.InvestMonthlySelectMaximumMomentum(allStocks, 10000, 1200, 2)
            Dim max3Overall = max3Momentum.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            Dim min3Momentum = momentum.InvestMonthlySelectMinimumMomentum(allStocks, 10000, 1200, 2)
            Dim min3Overall = min3Momentum.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            investedEvenly = momrseva.Select(Function(sr) sr.Invested).Sum
            investedMax = momrsa.Select(Function(sr) sr.Invested).Sum
            investedMin = momrsevai.Select(Function(sr) sr.Invested).Sum

            kp.Add(New With {.Even = overalle, .MH = Math.Round(overall, 0), .MH3 = Math.Round(max3Overall, 0), .ML = Math.Round(overallei, 0), .ML3 = Math.Round(min3Overall, 0), .S = i})
            Console.WriteLine("E {0:n0}, mh {1:n0}, mh3 {2:n0}, ml {3:n0}, m3l {4:n0}, s {5:n0}", overalle, overall, max3Overall, overallei, min3Overall, i)
            '  Console.WriteLine(i)
        Next

        'Dim ka = results.OrderByDescending(Function(sr) sr.GainedDividends).Select(Function(sr)
        '                                                                               Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
        '                                                                           End Function).ToList


        'Dim ka2 = results.OrderByDescending(Function(sr) sr.CurrentInvestmentValue).Select(Function(sr)
        '                                                                                       Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
        '                                                                                   End Function).ToList
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
