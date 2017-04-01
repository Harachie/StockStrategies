Public Class DividendStrategy

    Public Property StartDate As Date = DateSerial(2000, 1, 1)
    Public Property StepSize As Integer = 130

    Public Function InvestMonthlyReinvestDividendsSelectMaximumDividend(stocks As IEnumerable(Of Stock), startCapitalPerStock As Double, moneyPerMonth As Double) As IEnumerable(Of StrategyResult)
        Dim results As New List(Of StrategyResult)
        Dim dataSets As IList(Of StockDataCollection) = New List(Of StockDataCollection)
        Dim dividendsIndex, currentMonth, investIndex As Integer
        Dim currentStockData As StockData
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount, currentDividendYield, max As Double
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
                max = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    dividends = dividendsData(stocksList(n))

                    If dividends IsNot Nothing Then
                        dividendsIndex = dividendIndieces(stocksList(n))
                        dividendsIndex -= 1

                        If dividendsIndex > 0 Then
                            currentStockData = dataSets(n)(i - 1)
                            dividends = stocksList(n).DividendsHistory(dividendsIndex)
                            currentDividendYield = dividends.Amount / currentStockData.Close

                            If currentDividendYield > max Then
                                max = currentDividendYield
                                investIndex = n
                            End If
                        End If
                    End If
                Next

                addedStockAmount = moneyPerMonth / dataSets(investIndex)(i).Open
                results(investIndex).Invested += moneyPerMonth
                results(investIndex).StockAmount += addedStockAmount
                results(investIndex).AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), results(investIndex).StockAmount)
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung investieren
                max = 0.0

                For n As Integer = 0 To dataSets.Count - 1
                    dividends = dividendsData(stocksList(n))

                    If dividends IsNot Nothing Then
                        dividendsIndex = dividendIndieces(stocksList(n))
                        dividendsIndex -= 1

                        If dividendsIndex > 0 Then
                            currentStockData = dataSets(n)(i - 1)
                            dividends = stocksList(n).DividendsHistory(dividendsIndex)
                            currentDividendYield = dividends.Amount / currentStockData.Close

                            If currentDividendYield > max Then
                                max = currentDividendYield
                                investIndex = n
                            End If
                        End If
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

        Return results
    End Function

End Class
