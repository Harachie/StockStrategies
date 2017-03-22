Imports Newtonsoft.Json

Public Class Stock

    Public Property Name As String

    <JsonProperty("GuV/Bilanz Operatives Ergebnis (EBIT)")>
    Public Property Ebits As Dictionary(Of String, Double)

    <JsonProperty("Aktie Dividende je Aktie")>
    Public Property Dividends As Dictionary(Of String, Double)

    <JsonProperty("Aktie Buchwert je Aktie")>
    Public Property Bookvalues As Dictionary(Of String, Double)

    <JsonProperty("Aktie Cashflow je Aktie")>
    Public Property Cashflows As Dictionary(Of String, Double)

    <JsonProperty("Bewertung Dividendenrendite in %")>
    Public Property DividendYields As Dictionary(Of String, Double)

    <JsonProperty("Bewertung KGV (Kurs/Gewinn)")>
    Public Property Kgvs As Dictionary(Of String, Double)

    <JsonProperty("Bewertung KUV (Kurs/Umsatz)")>
    Public Property Kuvs As Dictionary(Of String, Double)

    <JsonProperty("Bewertung KBV (Kurs/Buchwert)")>
    Public Property Kbvs As Dictionary(Of String, Double)

    <JsonProperty("Bewertung KCV (Kurs/Cashflow)")>
    Public Property Kcvs As Dictionary(Of String, Double)

    Public ReadOnly Property StockValues As Dictionary(Of String, Double)
        Get
            Dim r As New Dictionary(Of String, Double)
            Dim v As Double

            For Each kv In Me.Dividends
                If Me.DividendYields.TryGetValue(kv.Key, v) Then
                    r(kv.Key) = kv.Value * 100.0 / v
                End If
            Next

            Return r
        End Get
    End Property

End Class
