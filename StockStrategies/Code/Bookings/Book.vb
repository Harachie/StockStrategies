Public Class Book

    Public Orders As New List(Of Order)
    Public Accounts As New Dictionary(Of String, Account)

    Public Sub AddAccount(name As String, type As Account.TypeE, openingBalance As Double)
        Me.Accounts.Add(name, New Account With {.Name = name, .Type = type, .OpeningBalance = openingBalance})
    End Sub

    Public Sub AddOrder(sourceAccount As String, targetAccount As String, amount As Double)
        Dim source As Account = Nothing
        Dim target As Account = Nothing
        Dim order As Order

        If Not Me.Accounts.TryGetValue(sourceAccount, source) Then
            Throw New InvalidOperationException("Cannot find source account.")
        End If

        order = New Order(sourceAccount, targetAccount, amount)
        Me.Orders.Add(order)

        For Each entry In order.TargetAccounts
            If Not Me.Accounts.TryGetValue(entry.TargetAccount, target) Then
                Throw New InvalidOperationException("Cannot find target account '" & entry.TargetAccount & "'.")
            End If

            target.Entries.Add(entry)
            source.Entries.Add(entry)
        Next
    End Sub

End Class
