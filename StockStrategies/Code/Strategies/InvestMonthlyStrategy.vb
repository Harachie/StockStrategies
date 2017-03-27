Public Class InvestMonthlyStrategy

    Public Function ReinvestDividends(stock As Stock, startCapital As Double, moneyPerMonth As Double) As StrategyResult
        Dim r As New StrategyResult
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount As Double
        Dim dividendsIndex, currentMonth As Integer
        Dim dividends As Dividend = Nothing
        Dim currentStockData As StockData = Nothing
        Dim startDate As Date = DateSerial(2010, 1, 1)

        r.Stock = stock
        r.YearlyInvestment = moneyPerMonth * 12.0
        r.StartCaptial = startCapital
        r.Invested = r.StartCaptial

        If stock.HasDividends Then
            dividends = stock.DividendsHistory.Where(Function(d) d.DistributionDate >= startDate).FirstOrDefault
            dividendsIndex = stock.DividendsHistory.IndexOf(dividends)
        End If

        currentStockData = stock.Data.Where(Function(d) d.Date >= startDate).First
        currentMonth = currentStockData.Date.Month
        r.StockAmount = startCapital / currentStockData.Open
        r.AddLogEntry("{0:n2} start stocks bought for {1:n2}", r.StockAmount, r.StartCaptial)

        For Each currentStockData In stock.Data
            If currentStockData.Date < startDate Then
                Continue For
            End If

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

    Public Function ReinvestDividendsEvenly(stock As Stock, startCapital As Double, moneyPerMonth As Double) As StrategyResult
        Dim r As New StrategyResult
        Dim dividendsBeforeTax, dividendsAfterTax, dividendsMoney, addedStockAmount As Double
        Dim dividendsIndex, currentMonth, evenCounter As Integer
        Dim dividends As Dividend = Nothing
        Dim currentStockData As StockData = Nothing
        Dim monthlyInvestment As Double = moneyPerMonth

        r.YearlyInvestment = monthlyInvestment * 12.0
        r.StartCaptial = startCapital
        r.Invested = r.StartCaptial

        If stock.HasDividends Then
            dividends = stock.DividendsHistory(0)
        End If

        currentMonth = stock.Data(0).Date.Month
        r.StockAmount = startCapital / stock.Data(0).Open
        r.AddLogEntry("{0:n2} start stocks bought for {1:n2}", r.StockAmount, r.StartCaptial)

        For Each currentStockData In stock.Data
            If currentStockData.Date <= DateSerial(2000, 1, 1) Then
                Continue For
            End If

            If currentStockData.Date.Month <> currentMonth Then 'jeden monatswechsel kaufen
                currentMonth = currentStockData.Date.Month
                addedStockAmount = monthlyInvestment / currentStockData.Open
                r.Invested += monthlyInvestment
                r.StockAmount += addedStockAmount
                r.AddLogEntry("Added {0:n2} monthly stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("MMM yyyy"), r.StockAmount)

                If dividendsMoney > 0 Then
                    addedStockAmount = dividendsMoney / currentStockData.Open
                    evenCounter -= 1
                    r.StockAmount += addedStockAmount
                    r.AddLogEntry("Added {0:n2} dividends stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("dd MMM yyyy"), r.StockAmount)

                    If evenCounter = 0 Then
                        dividendsMoney = 0
                    End If
                End If
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
                dividendsMoney = (dividendsMoney * evenCounter + dividendsAfterTax) / 12.0
                evenCounter = 12

                If stock.DividendsHistory.Count > dividendsIndex Then
                    dividends = stock.DividendsHistory(dividendsIndex)
                Else
                    dividends = Nothing
                End If
            End If
        Next

        If dividendsMoney > 0 Then
            addedStockAmount = dividendsMoney / currentStockData.Open
            r.StockAmount += addedStockAmount
            r.AddLogEntry("Added {0:n2} dividends stocks ({1}), new amount {2:n2}", addedStockAmount, currentStockData.Date.ToString("dd MMM yyyy"), r.StockAmount)
        End If

        r.CurrentInvestmentValue = r.StockAmount * currentStockData.Close

        Return r
    End Function

End Class
