Public Class Order

    Public [Date] As Date
    Public SourceAccount As String
    Public TargetAccounts As New List(Of Entry)
    Public Comment As String

    Public Sub New(sourceAccount As String, targetAccount As String, amount As Double)
        Me.Date = Date.UtcNow
        Me.SourceAccount = sourceAccount
        Me.TargetAccounts.Add(New Entry With {.ParentOrder = Me, .TargetAccount = targetAccount, .Amount = amount})
        Me.Comment = String.Empty
    End Sub

    Public Sub New(sourceAccount As String, targetAccount1 As String, amount1 As Double, targetAccount2 As String, amount2 As Double)
        Me.Date = Date.UtcNow
        Me.SourceAccount = sourceAccount
        Me.TargetAccounts.Add(New Entry With {.ParentOrder = Me, .TargetAccount = targetAccount1, .Amount = amount1})
        Me.TargetAccounts.Add(New Entry With {.ParentOrder = Me, .TargetAccount = targetAccount2, .Amount = amount2})
        Me.Comment = String.Empty
    End Sub


End Class
