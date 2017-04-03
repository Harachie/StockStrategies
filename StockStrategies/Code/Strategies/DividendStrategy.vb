Public Class DividendStrategy

    Public Property StartDate As Date = DateSerial(2000, 1, 1)
    Public Property StepSize As Integer = 130

    Public Function InvestMonthlyReinvestDividendsSelectMaximumDividend(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double, stocksToPick As Integer) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim dividendsIndex, currentMonth As Integer
        Dim currentStockData As StockData
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount, currentDividendYield, moneyToInvest As Double
        Dim dividends As Dividend = Nothing
        Dim stocksList As New List(Of Stock)
        Dim dividendIndieces As New Dictionary(Of Stock, Integer)
        Dim dividendsData As New Dictionary(Of Stock, Dividend)
        Dim valueIndices As New List(Of DoubleIndex)
        Dim usedDividends As New HashSet(Of Dividend)
        Dim tempInvest As Double

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
        moneyToInvest = moneyPerMonth / stocksToPick

        For i As Integer = Me.StepSize To dataSets(0).Count - 1
            currentStockData = dataSets(0)(i)
            tempInvest = moneyPerMonth + 1

            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                valueIndices.Clear()

                For n As Integer = 0 To dataSets.Count - 1
                    dividends = dividendsData(stocksList(n))

                    If dividends IsNot Nothing Then
                        dividendsIndex = dividendIndieces(stocksList(n))
                        dividendsIndex -= 1

                        If dividendsIndex > 0 Then
                            currentStockData = dataSets(n)(i - 1)
                            dividends = stocksList(n).DividendsHistory(dividendsIndex)
                            currentDividendYield = dividends.Amount / currentStockData.Close
                            valueIndices.Add(New DoubleIndex With {.Value = currentDividendYield, .Index = n})
                        End If
                    End If
                Next

                If valueIndices.Count < stocksToPick Then
                    moneyToInvest = moneyPerMonth / valueIndices.Count
                Else
                    moneyToInvest = moneyPerMonth / stocksToPick
                End If

                For Each di In valueIndices.OrderByDescending(Function(vi) vi.Value).Take(stocksToPick)
                    addedStockAmount = moneyToInvest / dataSets(di.Index)(i).Open
                    tempInvest -= moneyToInvest
                    results(di.Index).Invested += moneyToInvest
                    results(di.Index).StockAmount += addedStockAmount
                    results(di.Index).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(di.Index).StockAmount)
                Next

                If tempInvest < 0 Then
                    Dim stophere = 1
                End If
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                valueIndices.Clear()
                tempInvest = dividendsMoney + 1

                For n As Integer = 0 To dataSets.Count - 1
                    dividends = dividendsData(stocksList(n))

                    If dividends IsNot Nothing Then
                        dividendsIndex = dividendIndieces(stocksList(n))
                        dividendsIndex -= 1

                        If dividendsIndex > 0 Then
                            currentStockData = dataSets(n)(i - 1)
                            dividends = stocksList(n).DividendsHistory(dividendsIndex)
                            currentDividendYield = dividends.Amount / currentStockData.Close
                            valueIndices.Add(New DoubleIndex With {.Value = currentDividendYield, .Index = n})
                        End If
                    End If
                Next

                If valueIndices.Count < stocksToPick Then
                    moneyToInvest = dividendsMoney / valueIndices.Count
                Else
                    moneyToInvest = dividendsMoney / stocksToPick
                End If

                For Each di In valueIndices.OrderByDescending(Function(vi) vi.Value).Take(stocksToPick)
                    addedStockAmount = moneyToInvest / dataSets(di.Index)(i).Open
                    tempInvest -= moneyToInvest
                    results(di.Index).StockAmount += addedStockAmount
                    results(di.Index).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(di.Index).StockAmount)
                Next


                If tempInvest < 0 Then
                    Dim stophere = 1
                End If

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

                    If Not usedDividends.Add(dividends) Then
                        Dim stopHere = 1
                    End If

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


        Dim overall = results.Select(Function(sr) sr.Invested).Sum

        Return results
    End Function

End Class
