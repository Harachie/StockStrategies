Public Class MomentumStrategy

    Public Property StartDate As Date = DateSerial(2000, 1, 1)
    Public Property StepSize As Integer = 130

    Public Function InvestMonthlySelectMaximumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim investIndex As Integer
        Dim currentStockData As StockData
        Dim currentMonth As Integer
        Dim momentum, maxMomentum, addedStockAmount As Double

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum > maxMomentum Then
                        maxMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = moneyPerMonth / dataSets(investIndex)(i).Open
                results(investIndex).Invested += moneyPerMonth
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
            End If
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlyReinvestDividendsSelectMaximumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim dividendsIndex, currentMonth, investIndex As Integer
        Dim currentStockData As StockData
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount, momentum, maxMomentum As Double
        Dim dividends As Dividend = Nothing
        Dim stocksList As New List(Of Stock)
        Dim dividendIndieces As New Dictionary(Of Stock, Integer)
        Dim dividendsData As New Dictionary(Of Stock, Dividend)

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
            stocksList.Add(s)

            If s.HasDividends Then
                dividends = s.DividendsHistory.Where(Function(d) d.DistributionDate >= Me.StartDate).FirstOrDefault

                If dividends IsNot Nothing Then
                    dividendsData.Add(s, dividends)
                    dividendIndieces.Add(s, s.DividendsHistory.IndexOf(dividends))
                End If
            End If
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum > maxMomentum Then
                        maxMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = moneyPerMonth / dataSets(investIndex)(i).Open
                results(investIndex).Invested += moneyPerMonth
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum > maxMomentum Then
                        maxMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = dividendsMoney / dataSets(investIndex)(i).Open
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} dividend stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                dividendsMoney = 0
            End If

            For n As Integer = 0 To dataSets.Count - 1
                currentStockData = dataSets(n)(i)
                dividends = dividendsData(stocksList(n))

                If dividends IsNot Nothing AndAlso currentStockData.Date >= dividends.DistributionDate Then
                    dividendsIndex = dividendIndieces(stocksList(n))
                    dividendsIndex += 1
                    dividendIndieces(stocksList(n)) = dividendsIndex
                    dividendsBeforeTax = results(n).StockAmount * dividends.Amount

                    If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                        dividendsAfterTax = TAX_FREE_AMOUNT + (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                        results(n).PaidTaxes += dividendsBeforeTax - dividendsAfterTax
                    Else
                        dividendsAfterTax = dividendsBeforeTax
                    End If

                    If dividendsAfterTax > 0 Then
                        results(n).AddLogEntry("Gained {0:n2} / {1:n2} dividends on {2} from {3}", dividendsAfterTax, dividendsBeforeTax, currentStockData.Date.ToString("dd MMM yyyy"), stocksList(n).MetaData.Name)
                        results(n).GainedDividends += dividendsAfterTax
                        dividendsMoney += dividendsAfterTax
                    End If

                    If stocksList(n).DividendsHistory.Count > dividendsIndex Then
                        dividendsData(stocksList(n)) = stocksList(n).DividendsHistory(dividendsIndex)
                    Else
                        dividendsData(stocksList(n)) = Nothing
                    End If
                End If
            Next


        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlySelectMaximumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double, stocksToSelect As Integer) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim investIndex As Integer
        Dim currentStockData As StockData
        Dim currentMonth As Integer
        Dim momentum, maxMomentum, addedStockAmount, moneyToInvest As Double
        Dim momentums As New List(Of MomentumIndex)

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month
        moneyToInvest = moneyPerMonth / stocksToSelect

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                momentums.Clear()
                currentMonth = currentStockData.Date.Month
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close
                    momentums.Add(New MomentumIndex With {.Index = n, .Momentum = momentum})
                Next

                For Each topIndex In momentums.OrderByDescending(Function(mi) mi.Momentum).Take(stocksToSelect)
                    investIndex = topIndex.Index
                    addedStockAmount = moneyToInvest / dataSets(investIndex)(i).Open
                    results(investIndex).Invested += moneyToInvest
                    results(investIndex).StockAmount += addedStockAmount
                    results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                Next
            End If
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlyReinvestDividendsSelectMaximumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double, stocksToSelect As Integer) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim stocksList As New List(Of Stock)
        Dim currentStockData As StockData
        Dim currentMonth, dividendsIndex, investIndex As Integer
        Dim momentum, maxMomentum, addedStockAmount, moneyToInvest, dividendsBeforeTax, dividendsAfterTax, dividendsMoney As Double
        Dim momentums As New List(Of MomentumIndex)
        Dim dividendIndieces As New Dictionary(Of Stock, Integer)
        Dim dividendsData As New Dictionary(Of Stock, Dividend)
        Dim dividends As Dividend

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
            stocksList.Add(s)

            If s.HasDividends Then
                dividends = s.DividendsHistory.Where(Function(d) d.DistributionDate >= Me.StartDate).FirstOrDefault

                If dividends IsNot Nothing Then
                    dividendsData.Add(s, dividends)
                    dividendIndieces.Add(s, s.DividendsHistory.IndexOf(dividends))
                End If
            End If
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month
        moneyToInvest = moneyPerMonth / stocksToSelect

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                momentums.Clear()
                currentMonth = currentStockData.Date.Month
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close
                    momentums.Add(New MomentumIndex With {.Index = n, .Momentum = momentum})
                Next

                For Each topIndex In momentums.OrderByDescending(Function(mi) mi.Momentum).Take(stocksToSelect)
                    investIndex = topIndex.Index
                    addedStockAmount = moneyToInvest / dataSets(investIndex)(i).Open
                    results(investIndex).Invested += moneyToInvest
                    results(investIndex).StockAmount += addedStockAmount
                    results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                Next
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                momentums.Clear()
                maxMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close
                    momentums.Add(New MomentumIndex With {.Index = n, .Momentum = momentum})
                Next

                For Each topIndex In momentums.OrderByDescending(Function(mi) mi.Momentum).Take(stocksToSelect)
                    investIndex = topIndex.Index
                    addedStockAmount = (dividendsMoney / stocksToSelect) / dataSets(investIndex)(i).Open
                    results(investIndex).StockAmount += addedStockAmount
                    results(investIndex).AddLogEntry("Added {0:n2} dividends stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                Next

                dividendsMoney = 0
            End If

            For n As Integer = 0 To dataSets.Count - 1
                currentStockData = dataSets(n)(i)
                dividends = dividendsData(stocksList(n))

                If dividends IsNot Nothing AndAlso currentStockData.Date >= dividends.DistributionDate Then
                    dividendsIndex = dividendIndieces(stocksList(n))
                    dividendsIndex += 1
                    dividendIndieces(stocksList(n)) = dividendsIndex
                    dividendsBeforeTax = results(n).StockAmount * dividends.Amount

                    If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                        dividendsAfterTax = TAX_FREE_AMOUNT + (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                        results(n).PaidTaxes += dividendsBeforeTax - dividendsAfterTax
                    Else
                        dividendsAfterTax = dividendsBeforeTax
                    End If

                    If dividendsAfterTax > 0 Then
                        results(n).AddLogEntry("Gained {0:n2} / {1:n2} dividends on {2} from {3}", dividendsAfterTax, dividendsBeforeTax, currentStockData.Date.ToString("dd MMM yyyy"), stocksList(n).MetaData.Name)
                        results(n).GainedDividends += dividendsAfterTax
                        dividendsMoney += dividendsAfterTax
                    End If

                    If stocksList(n).DividendsHistory.Count > dividendsIndex Then
                        dividendsData(stocksList(n)) = stocksList(n).DividendsHistory(dividendsIndex)
                    Else
                        dividendsData(stocksList(n)) = Nothing
                    End If
                End If
            Next
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlySelectMinimumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim investIndex As Integer
        Dim currentStockData As StockData
        Dim currentMonth As Integer
        Dim momentum, minMomentum, addedStockAmount As Double

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                minMomentum = 100.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum < minMomentum Then
                        minMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = moneyPerMonth / dataSets(investIndex)(i).Open
                results(investIndex).Invested += moneyPerMonth
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
            End If
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlyReinvestDividendsSelectMinimumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim dividendsIndex, currentMonth, investIndex As Integer
        Dim currentStockData As StockData
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount, momentum, minMomentum As Double
        Dim dividends As Dividend = Nothing
        Dim stocksList As New List(Of Stock)
        Dim dividendIndieces As New Dictionary(Of Stock, Integer)
        Dim dividendsData As New Dictionary(Of Stock, Dividend)

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
            stocksList.Add(s)

            If s.HasDividends Then
                dividends = s.DividendsHistory.Where(Function(d) d.DistributionDate >= Me.StartDate).FirstOrDefault

                If dividends IsNot Nothing Then
                    dividendsData.Add(s, dividends)
                    dividendIndieces.Add(s, s.DividendsHistory.IndexOf(dividends))
                End If
            End If
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                minMomentum = 100.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum < minMomentum Then
                        minMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = moneyPerMonth / dataSets(investIndex)(i).Open
                results(investIndex).Invested += moneyPerMonth
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                minMomentum = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close

                    If momentum < minMomentum Then
                        minMomentum = momentum
                        investIndex = n
                    End If
                Next

                addedStockAmount = dividendsMoney / dataSets(investIndex)(i).Open
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} dividend stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                dividendsMoney = 0
            End If

            For n As Integer = 0 To dataSets.Count - 1
                currentStockData = dataSets(n)(i)
                dividends = dividendsData(stocksList(n))

                If dividends IsNot Nothing AndAlso currentStockData.Date >= dividends.DistributionDate Then
                    dividendsIndex = dividendIndieces(stocksList(n))
                    dividendsIndex += 1
                    dividendIndieces(stocksList(n)) = dividendsIndex
                    dividendsBeforeTax = results(n).StockAmount * dividends.Amount

                    If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                        dividendsAfterTax = TAX_FREE_AMOUNT + (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                        results(n).PaidTaxes += dividendsBeforeTax - dividendsAfterTax
                    Else
                        dividendsAfterTax = dividendsBeforeTax
                    End If

                    If dividendsAfterTax > 0 Then
                        results(n).AddLogEntry("Gained {0:n2} / {1:n2} dividends on {2} from {3}", dividendsAfterTax, dividendsBeforeTax, currentStockData.Date.ToString("dd MMM yyyy"), stocksList(n).MetaData.Name)
                        results(n).GainedDividends += dividendsAfterTax
                        dividendsMoney += dividendsAfterTax
                    End If

                    If stocksList(n).DividendsHistory.Count > dividendsIndex Then
                        dividendsData(stocksList(n)) = stocksList(n).DividendsHistory(dividendsIndex)
                    Else
                        dividendsData(stocksList(n)) = Nothing
                    End If
                End If
            Next


        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlySelectMinimumMomentum(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double, stocksToSelect As Integer) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim investIndex As Integer
        Dim currentStockData As StockData
        Dim currentMonth As Integer
        Dim momentum, minMomentum, addedStockAmount, moneyToInvest As Double
        Dim momentums As New List(Of MomentumIndex)

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month
        moneyToInvest = moneyPerMonth / stocksToSelect

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                momentums.Clear()
                currentMonth = currentStockData.Date.Month
                minMomentum = 10.0

                For n As Integer = 0 To dataSets.Count - 1
                    momentum = dataSets(n)(i - 1).Close / dataSets(n)(i - Me.StepSize).Close
                    momentums.Add(New MomentumIndex With {.Index = n, .Momentum = momentum})
                Next

                For Each topIndex In momentums.OrderBy(Function(mi) mi.Momentum).Take(stocksToSelect)
                    investIndex = topIndex.Index
                    addedStockAmount = moneyToInvest / dataSets(investIndex)(i).Open
                    results(investIndex).Invested += moneyToInvest
                    results(investIndex).StockAmount += addedStockAmount
                    results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
                Next
            End If
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthly(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim currentStockData As StockData
        Dim currentMonth As Integer
        Dim addedStockAmount As Double

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month

                For n As Integer = 0 To dataSets.Count - 1
                    addedStockAmount = (moneyPerMonth / dataSets.Count) / dataSets(n)(i).Open
                    results(n).Invested += (moneyPerMonth / dataSets.Count)
                    results(n).StockAmount += addedStockAmount
                    results(n).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(n).StockAmount)

                Next

            End If
        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Public Function InvestMonthlyReinvestDividends(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim dividendsIndex, currentMonth, investIndex As Integer
        Dim currentStockData As StockData
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount As Double
        Dim dividends As Dividend = Nothing
        Dim stocksList As New List(Of Stock)
        Dim dividendIndieces As New Dictionary(Of Stock, Integer)
        Dim dividendsData As New Dictionary(Of Stock, Dividend)

        For Each s As Stock In stocks
            results.Add(New StrategyResult With {.Stock = s})
            dataSets.Add(s.Data.FilterByMinimumStartDate(Me.StartDate))
            stocksList.Add(s)

            If s.HasDividends Then
                dividends = s.DividendsHistory.Where(Function(d) d.DistributionDate >= Me.StartDate).FirstOrDefault

                If dividends IsNot Nothing Then
                    dividendsData.Add(s, dividends)
                    dividendIndieces.Add(s, s.DividendsHistory.IndexOf(dividends))
                End If
            End If
        Next

        dataSets = StockDataCollection.StripNonExistentDates(dataSets.ToArray())
        currentMonth = dataSets(0)(Me.StepSize).Date.Month

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month

                For n As Integer = 0 To dataSets.Count - 1
                    addedStockAmount = (moneyPerMonth / dataSets.Count) / dataSets(n)(i).Open
                    results(n).Invested += (moneyPerMonth / dataSets.Count)
                    results(n).StockAmount += addedStockAmount
                    results(n).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(n).StockAmount)
                Next
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                For n As Integer = 0 To dataSets.Count - 1
                    addedStockAmount = (dividendsMoney / dataSets.Count) / dataSets(n)(i).Open
                    results(n).Invested += (dividendsMoney / dataSets.Count)
                    results(n).StockAmount += addedStockAmount
                    results(n).AddLogEntry("Added {0:n2} dividend stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(n).StockAmount)
                Next

                dividendsMoney = 0
            End If

            For n As Integer = 0 To dataSets.Count - 1
                currentStockData = dataSets(n)(i)
                dividends = dividendsData(stocksList(n))

                If dividends IsNot Nothing AndAlso currentStockData.Date >= dividends.DistributionDate Then
                    dividendsIndex = dividendIndieces(stocksList(n))
                    dividendsIndex += 1
                    dividendIndieces(stocksList(n)) = dividendsIndex
                    dividendsBeforeTax = results(n).StockAmount * dividends.Amount

                    If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                        dividendsAfterTax = TAX_FREE_AMOUNT + (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                        results(n).PaidTaxes += dividendsBeforeTax - dividendsAfterTax
                    Else
                        dividendsAfterTax = dividendsBeforeTax
                    End If

                    If dividendsAfterTax > 0 Then
                        results(n).AddLogEntry("Gained {0:n2} / {1:n2} dividends on {2} from {3}", dividendsAfterTax, dividendsBeforeTax, currentStockData.Date.ToString("dd MMM yyyy"), stocksList(n).MetaData.Name)
                        results(n).GainedDividends += dividendsAfterTax
                        dividendsMoney += dividendsAfterTax
                    End If

                    If stocksList(n).DividendsHistory.Count > dividendsIndex Then
                        dividendsData(stocksList(n)) = stocksList(n).DividendsHistory(dividendsIndex)
                    Else
                        dividendsData(stocksList(n)) = Nothing
                    End If
                End If
            Next


        Next

        For n As Integer = 0 To results.Count - 1
            results(n).CurrentInvestmentValue = results(n).StockAmount * dataSets(n).Last.Close
        Next

        Dim overall = results.Select(Function(sr) sr.CurrentInvestmentValue).Sum

        Return results
    End Function

    Private Class MomentumIndex

        Public Property Momentum As Double
        Public Property Index As Integer

    End Class

End Class
