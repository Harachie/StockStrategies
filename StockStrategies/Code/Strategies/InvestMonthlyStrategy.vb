Public Class InvestMonthlyStrategy

    Public Property StartDate As Date = DateSerial(2000, 1, 1)

    Public Function ReinvestDividends(stock As Stock, startCapital As Double, moneyPerMonth As Double) As StrategyResult
        Dim r As New StrategyResult
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount As Double
        Dim dividendsIndex, currentMonth As Integer
        Dim dividends As Dividend = Nothing
        Dim currentStockData As StockData = Nothing
        Dim data As IEnumerable(Of StockData)

        r.Stock = stock
        r.YearlyInvestment = moneyPerMonth * 12.0
        r.StartCaptial = startCapital
        r.Invested = r.StartCaptial

        If stock.HasDividends Then
            dividends = stock.DividendsHistory.Where(Function(d) d.DistributionDate >= Me.StartDate).FirstOrDefault

            If dividends IsNot Nothing Then
                dividendsIndex = stock.DividendsHistory.IndexOf(dividends)
            End If
        End If

        data = stock.Data.Where(Function(d) d.Date >= Me.StartDate)
        currentStockData = data.First
        currentMonth = currentStockData.Date.Month
        r.StockAmount = startCapital / currentStockData.Open
        r.AddLogEntry("{0:n2} start stocks bought for {1:n2}", r.StockAmount, r.StartCaptial)

        For Each currentStockData In data
            If moneyPerMonth > 0 AndAlso currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                addedStockAmount = moneyPerMonth / currentStockData.Open
                r.Invested += moneyPerMonth
                r.StockAmount += addedStockAmount
                r.AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), r.StockAmount)
            End If

            If dividendsMoney > 0 Then 'geld der dividenden kann ich nicht am tag der ausschüttung kaufen
                addedStockAmount = dividendsMoney / currentStockData.Open
                r.StockAmount += addedStockAmount
                r.AddLogEntry("Added {0:n2} dividends stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("dd MMM yyyy"), r.StockAmount)
                dividendsMoney = 0
            End If

            If dividends IsNot Nothing AndAlso currentStockData.Date >= dividends.DistributionDate Then
                dividendsIndex += 1
                dividendsBeforeTax += r.StockAmount * dividends.Amount

                If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                    dividendsAfterTax = TAX_FREE_AMOUNT + (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                    r.PaidTaxes += dividendsBeforeTax - dividendsAfterTax
                Else
                    dividendsAfterTax = dividendsBeforeTax
                End If

                r.AddLogEntry("Gained {0:n2} / {1:n2} dividends on {2}", dividendsAfterTax, dividendsBeforeTax, currentStockData.Date.ToString("dd MMM yyyy"))
                r.GainedDividends += dividendsAfterTax
                dividendsMoney = dividendsAfterTax

                If stock.DividendsHistory.Count > dividendsIndex Then
                    dividends = stock.DividendsHistory(dividendsIndex)
                Else
                    dividends = Nothing
                End If
            End If
        Next

        r.CurrentInvestmentValue = r.StockAmount * currentStockData.Close

        Return r
    End Function

End Class
