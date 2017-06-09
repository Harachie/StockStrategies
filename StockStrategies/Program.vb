Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Doppelte()
        Dim book As New Book

        book.AddAccount("Kassenbestand", Account.TypeE.Active, 10000.0)
        book.AddAccount("Steuern", Account.TypeE.Active, 0.0)
        book.AddAccount("Wertpapiere", Account.TypeE.Active, 0.0)

        book.AddAccount("Kapital", Account.TypeE.Passive, 10000.0)
    End Sub

    Sub Snp()
        Dim sdc As StockDataCollection = StockDataCollection.ReadFromStooqFile(IO.Path.Combine(GetStooqDirectory(), "snp.txt"))
        Dim early, late As StockData
        Dim factor As Double
        Dim factors As New Dictionary(Of Integer, Double)
        Dim stepSize As Integer = 260
        Dim startDate As Date = DateSerial(1960, 1, 1)
        Dim data = sdc.FilterByMinimumStartDate(startDate)


        For i As Integer = stepSize To data.Count - 1
            early = data(i - stepSize)
            late = data(i)
            factor = late.Close / early.Open
            factors(i) = factor
        Next

        Dim highestFalls = From x In factors Order By x.Value
        Dim highestRise = From x In factors Order By x.Value Descending
    End Sub

    Public Sub ThreeToOne()
        Dim outputNeuron As Neuron
        Dim inputNeuron As InputNeuron
        Dim inputNeurons As New List(Of INeuron)
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim resultSet(2) As Double
        Dim labels As New List(Of Integer)
        Dim index As Integer
        Dim current As Double()
        Dim hits As Integer
        Dim result As Double

        datas.Add({1.2, 0.7}) : labels.Add(1)
        datas.Add({-0.3, -0.5}) : labels.Add(-1)
        datas.Add({3.0, 0.1}) : labels.Add(1)
        datas.Add({-0.1, -1.0}) : labels.Add(-1)
        datas.Add({-1.0, 1.1}) : labels.Add(-1)
        datas.Add({2.1, -3.0}) : labels.Add(1)

        For i As Integer = 1 To 3
            inputNeuron = New InputNeuron(2)

            For n As Integer = 1 To 2
                inputNeuron.Weights(n - 1) = rnd.NextDouble * 2 - 1.0
            Next

            inputNeurons.Add(inputNeuron)
        Next

        outputNeuron = New Neuron(3)

        For n As Integer = 1 To 3
            outputNeuron.Weights(n - 1) = rnd.NextDouble * 2 - 1.0
        Next


        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            outputNeuron.Clear()
            resultSet(0) = inputNeurons(0).Forward(current)
            resultSet(1) = inputNeurons(1).Forward(current)
            resultSet(2) = inputNeurons(2).Forward(current)

            result = outputNeuron.Forward(resultSet)

            If labels(index) = 1 AndAlso result < 1 Then
                outputNeuron.Backward(1.0)

            ElseIf labels(index) = -1 AndAlso result > -1 Then
                outputNeuron.Backward(-1.0)

            Else
                outputNeuron.Backward(0.0)
            End If

            '  outputNeuron.Regularize()
            outputNeuron.UpdateWeightsAdam(0.01)
            hits = 0

            For n As Integer = 0 To datas.Count - 1
                current = datas(n)
                resultSet(0) = inputNeurons(0).Forward(current)
                resultSet(1) = inputNeurons(1).Forward(current)
                resultSet(2) = inputNeurons(2).Forward(current)

                result = outputNeuron.Forward(resultSet)

                If labels(n) = 1 AndAlso result > 0 Then
                    hits += 1

                ElseIf labels(n) = -1 AndAlso result <= 0 Then
                    hits += 1
                End If
            Next

            If hits = 6 Then
                Dim stopHere = 1
            End If


            If i Mod 50 = 0 Then
                Console.WriteLine(hits)
            End If
        Next
    End Sub

    Public Sub ThreeToOneLayer()
        Dim inputLayer As New InputLayer(2, 9)
        Dim reluLayer As ReluLayer = inputLayer.CreateReluLayer()
        Dim hiddenLayer As NeuronLayer = reluLayer.CreateNeuronLayer(1)
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim resultSet(2) As Double
        Dim labels As New List(Of Integer)
        Dim index As Integer
        Dim current, inputResult, reluResult, hiddenResult As Double()
        Dim hits As Integer
        Dim result As Double

        datas.Add({1.2, 0.7}) : labels.Add(1)
        datas.Add({-0.3, -0.5}) : labels.Add(-1)
        datas.Add({3.0, 0.1}) : labels.Add(1)
        datas.Add({-0.1, -1.0}) : labels.Add(-1)
        datas.Add({-1.0, 1.1}) : labels.Add(-1)
        datas.Add({2.1, -3.0}) : labels.Add(1)


        inputLayer.Randomize(rnd)
        hiddenLayer.Randomize(rnd)


        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            inputLayer.Clear()
            hiddenLayer.Clear()


            inputResult = inputLayer.Forward(current)
            reluResult = reluLayer.Forward(current)
            hiddenResult = hiddenLayer.Forward(reluResult)
            result = hiddenResult(0)

            If labels(index) = 1 AndAlso result < 1 Then
                hiddenLayer.Backward({1.0})

            ElseIf labels(index) = -1 AndAlso result > -1 Then
                hiddenLayer.Backward({-1.0})

            Else
                hiddenLayer.Backward({0.0})
            End If

            hiddenLayer.UpdateWeights(0.01)
            '  outputNeuron.Regularize()
            '    outputNeuron.UpdateWeightsAdam(0.01)
            hits = 0

            For n As Integer = 0 To datas.Count - 1
                current = datas(n)
                inputResult = inputLayer.Forward(current)
                reluResult = reluLayer.Forward(current)
                hiddenResult = hiddenLayer.Forward(reluResult)
                result = hiddenResult(0)

                If labels(n) = 1 AndAlso result > 0 Then
                    hits += 1

                ElseIf labels(n) = -1 AndAlso result <= 0 Then
                    hits += 1
                End If
            Next

            If hits = 6 Then
                Dim stopHere = 1
            End If


            If i Mod 50 = 0 Then
                Console.WriteLine(hits)
            End If
        Next
    End Sub

    Sub Main()
        Dim invest As New InvestMonthlyStrategy
        Dim momentum As New MomentumStrategy
        Dim metac As StockMetaDataCollection
        Dim results As New List(Of StrategyResult)
        Dim loader As New Downloader
        Dim sd As String
        Dim s As Stock

        ThreeToOneLayer()
        CreateAllDirectories()
        Snp()
        '     Dim snp = loader.DownloadStooqData("^spx")

        metac = StockMetaDataCollection.ReadFromFile("german.json")

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

        For i As Integer = 0 To 780 Step 13
            momentum.StepSize = i
            divinvest.StepSize = i
            resultSets.Clear()


            Dim momrseva = momentum.InvestMonthlyReinvestDividends(allStocks, 10000, investmentPerMonth)
            Dim evenlyInvestment = momrseva.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            resultSets("h") = momentum.InvestMonthlyReinvestDividendsSelectMaximumMomentum(allStocks, 10000, investmentPerMonth, 1)
            resultSets("l") = momentum.InvestMonthlyReinvestDividendsSelectMinimumMomentum(allStocks, 10000, investmentPerMonth, 1)

            For n As Integer = 1 To 3 Step 1
                '   resultSets("h" & n) = momentum.InvestMonthlyReinvestDividendsSelectMaximumMomentum(allStocks, 10000, investmentPerMonth, n)
                '   resultSets("l" & n) = momentum.InvestMonthlyReinvestDividendsSelectMinimumMomentum(allStocks, 10000, investmentPerMonth, n)
                resultSets("dh" & n) = divinvest.InvestMonthlyReinvestDividendsSelectMaximumDividend(allStocks, 10000, investmentPerMonth, n)
                resultSets("dl" & n) = divinvest.InvestMonthlyReinvestDividendsSelectMinimumDividend(allStocks, 10000, investmentPerMonth, n)
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

        'antizyklisch investieren (ginmo)

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
