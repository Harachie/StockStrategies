Public Class BuyAndHoldStrategies

    Public Const TAX_FREE_MULTIPLIER As Double = 0.73625
    Public Const TAX_FREE_AMOUNT As Double = 801.0

    Public Shared Function InvestConstantly(fundamentals As StockFundamentals, startCapital As Double, moneyPerMonth As Double) As StrategyResult
        Dim r As New StrategyResult
        Dim dividendsBeforeTax, dividendsAfterTax, addedStockAmount As Double

        r.StockFundamentals = fundamentals
        r.YearlyInvestment = moneyPerMonth * 12.0
        r.StartCaptial = startCapital
        r.Invested = r.StartCaptial

        If fundamentals.Dividends.Count > 0 Then
            r.StockAmount = startCapital / fundamentals.StockValues(fundamentals.Dividends.Keys(0))
        End If

        For Each kv In fundamentals.Dividends
            dividendsBeforeTax = r.StockAmount * kv.Value

            If dividendsBeforeTax > TAX_FREE_AMOUNT Then
                dividendsAfterTax = (dividendsBeforeTax - TAX_FREE_AMOUNT) * TAX_FREE_MULTIPLIER
                r.PaidTaxes += dividendsBeforeTax - dividendsAfterTax
            Else
                dividendsAfterTax = dividendsBeforeTax
            End If

            r.GainedDividends += dividendsAfterTax

            addedStockAmount = dividendsAfterTax / fundamentals.StockValues(kv.Key)
            r.StockAmount += addedStockAmount

            r.Invested += r.YearlyInvestment
            addedStockAmount = r.YearlyInvestment / fundamentals.StockValues(kv.Key)
            r.StockAmount += addedStockAmount
        Next


        Return r
    End Function

End Class
