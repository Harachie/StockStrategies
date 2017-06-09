Imports StockStrategies

Public Class ReluLayer
    Implements ILayer

    Public Property Inputs As Integer
    Public Property Gradients As Double()
    Public Property PreviousLayer As ILayer

    Public Sub New(inputs As Integer, previousLayer As ILayer)
        Me.Inputs = inputs
        Me.PreviousLayer = previousLayer

        ReDim Me.Gradients(inputs - 1)
    End Sub

    Public Function CreateNeuronLayer(outputs As Integer) As NeuronLayer
        Return New NeuronLayer(Me.Inputs, outputs, Me)
    End Function

    Public Function Forward(inputs As Double()) As Double() Implements ILayer.Forward
        Dim r(Me.Inputs - 1) As Double

        For i As Integer = 0 To Me.Inputs - 1
            r(i) = Math.Max(0.0, inputs(0))

            If r(i) = 0.0 Then
                Me.Gradients(i) = 0
            Else
                Me.Gradients(i) = 1
            End If
        Next

        Return r
    End Function

    Public Sub Backward(adjustInDirection() As Double) Implements ILayer.Backward
        Me.PreviousLayer.Backward(Me.Gradients)
    End Sub

    Public Sub UpdateWeights(learningRate As Double) Implements ILayer.UpdateWeights
        Me.PreviousLayer.UpdateWeights(learningRate)
    End Sub

    Public Sub Clear() Implements ILayer.Clear
    End Sub

    Public Sub Regularize() Implements ILayer.Regularize
    End Sub

End Class
