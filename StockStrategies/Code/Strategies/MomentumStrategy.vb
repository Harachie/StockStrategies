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

    Private Class MomentumIndex

        Public Property Momentum As Double
        Public Property Index As Integer

    End Class

End Class
