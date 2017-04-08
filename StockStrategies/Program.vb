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

        metac = StockMetaDataCollection.ReadFromFile("germanhandpicked.json")

        Dim allStocks As New List(Of Stock)

        For Each md As StockMetaData In metac
            s = Stock.ReadStooqFromMetaData(md)
            allStocks.Add(s)
            results.Add(invest.ReinvestDividends(s, 10000, 1200))
            '   sd = loader.DownloadStooqData(md)
            '  WriteAllText(IO.Path.Combine(GetStooqDirectory(), md.DataFileName), sd)
        Next

        Dim daimler = Stock.ReadFromMetaData("daimler.json")
        Dim basf = Stock.ReadFromMetaData("basf.json")
        Dim bmw As Stock = Stock.ReadFromMetaData("bmw.json")
        Dim vw As Stock = Stock.ReadFromMetaData("Volkswagen VZ.json")

        Dim resultDaimler = invest.ReinvestDividends(daimler, 10000, 1200)
        Dim resultBasf = invest.ReinvestDividends(basf, 10000, 1200)
        Dim resultBmw = invest.ReinvestDividends(bmw, 10000, 1200)
        Dim resultVw = invest.ReinvestDividends(vw, 10000, 1200)


        'Dim momrs = momentum.InvestMonthlySelectMaximumMomentum({daimler, basf, bmw}, 10000, 1200)
        'Dim momrsev = momentum.InvestMonthly({daimler, basf, bmw}, 10000, 1200)

        Dim kp As New List(Of Object)
        Dim investedEvenly, investedMax, investedMin As Double
        Dim divinvest As New DividendStrategy

        '  momentum.StartDate = DateSerial(2008, 1, 1)

        '  allStocks = {daimler, basf, bmw, vw}.ToList
        Dim addedStocks As New List(Of String)
        Dim dividendsResults As New List(Of IEnumerable(Of StrategyResult))

        Dim startDate As Date = DateSerial(2000, 1, 1)
        Dim investmentPerMonth As Double = 600
        Dim resultSets As New Dictionary(Of String, IEnumerable(Of StrategyResult))

        divinvest.StartDate = startDate
        momentum.StartDate = startDate

        For i As Integer = 0 To 520 Step 13
            momentum.StepSize = i
            divinvest.StepSize = i
            resultSets.Clear()


            Dim momrseva = momentum.InvestMonthlyReinvestDividends(allStocks, 10000, investmentPerMonth)
            Dim evenlyInvestment = momrseva.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            resultSets("h") = momentum.InvestMonthlyReinvestDividendsSelectMaximumMomentum(allStocks, 10000, investmentPerMonth, 1)
            resultSets("l") = momentum.InvestMonthlyReinvestDividendsSelectMinimumMomentum(allStocks, 10000, investmentPerMonth, 1)

            For n As Integer = 1 To 7 Step 1
                resultSets("d" & n) = divinvest.InvestMonthlyReinvestDividendsSelectMaximumDividend(allStocks, 10000, investmentPerMonth, n)
            Next


            Console.Write("e {0:n}", evenlyInvestment)

            For Each kv In resultSets
                Dim value = kv.Value.Select(Function(sr) sr.CurrentInvestmentValue).Sum

                PrettyPrint(kv.Key, evenlyInvestment, value)
            Next

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(", ")

            Console.WriteLine("s {0:n0}", i)
            '  Console.WriteLine(i)

            '  addedStocks.Add(String.Join(", ", maxDividendsResults.Where(Function(r) r.StockAmount > 0).Select(Function(r) r.Stock.MetaData.Name)))
        Next

        Dim ka = results.OrderByDescending(Function(sr) sr.GainedDividends).Select(Function(sr)
                                                                                       Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                   End Function).ToList


        Dim ka2 = results.OrderByDescending(Function(sr) sr.CurrentInvestmentValue).Select(Function(sr)
                                                                                               Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                           End Function).ToList
    End Sub

    Public Sub PrettyPrint(caption As String, evenly As Double, comparsionValue As Double)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(", {0} ", caption)

        If comparsionValue > evenly Then
            Console.ForegroundColor = ConsoleColor.Green
            Console.Write("{0:n0}", comparsionValue)

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(" (")
            Console.ForegroundColor = ConsoleColor.Green
            Console.Write("{0:n2}", (comparsionValue / evenly).AsHumanReadablePercent)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(")")

        Else
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("{0:n0}", comparsionValue)

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(" (")
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("{0:n2}", (comparsionValue / evenly).AsHumanReadablePercent)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(")")
        End If
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
