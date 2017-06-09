Public Class Account

    Public Enum TypeE
        Active
        Passive
    End Enum

    Public Enum ActiveAccountNameE
        Finanzanlagen
        Kassenbestand

        'Umlauf
        GuthabenBeiKreditinstituten
        Wertpapiere
    End Enum

    Public Enum PassiveAccountNameE
        GezeichnetesKapital
    End Enum

    Public Name As String
    Public Type As TypeE
    Public OpeningBalance As Double
    Public Entries As New List(Of Entry)

End Class
