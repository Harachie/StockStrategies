Public Class StrategyResult

    Public Property Stock As Stock
    Public Property StockFundamentals As StockFundamentals
    Public Property StartCaptial As Double
    Public Property YearlyInvestment As Double
    Public Property StockAmount As Double
    Public Property Invested As Double
    Public Property GainedDividends As Double
    Public Property PaidTaxes As Double
    Public Property CurrentInvestmentValue As Double
    Public Property Log As New List(Of String)

    Public Sub AddLogEntry(message As String)
        Me.Log.Add(message)
    End Sub

    Public Sub AddLogEntry(format As String, ParamArray args() As Object)
        Me.Log.Add(String.Format(format, args))
    End Sub

End Class
