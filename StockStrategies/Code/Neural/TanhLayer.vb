Public Class TanhLayer
    Implements ILayer

    Public Property Inputs As Integer
    Public Property Gradients As Double()
    Public Property PreviousLayer As ILayer

    Public Sub New(inputs As Integer, previousLayer As ILayer)
        Me.Inputs = inputs
        Me.PreviousLayer = previousLayer

        ReDim Me.Gradients(inputs - 1)
    End Sub

    Public Function CreateNeuronLayer(outputs As Integer) As NeuronLayer Implements ILayer.CreateNeuronLayer
        Return New NeuronLayer(Me.Inputs, outputs, Me)
    End Function

    Public Function CreateReluLayer() As ReluLayer Implements ILayer.CreateReluLayer 'das macht im relulayer wenig sinn... aber naja
        Return New ReluLayer(Me.Inputs, Me)
    End Function

    Public Function CreateTanhLayer() As TanhLayer Implements ILayer.CreateTahLayer
        Return New TanhLayer(Me.Inputs, Me)
    End Function

    Public Sub Randomize(rnd As Random) Implements ILayer.Randomize
    End Sub

    Public Function Forward(inputs As Double()) As Double() Implements ILayer.Forward
        Dim r(Me.Inputs - 1) As Double
        Dim t As Double

        For i As Integer = 0 To Me.Inputs - 1
            t = Math.Tanh(inputs(i))
            r(i) = t

            Me.Gradients(i) = (1.0 + t) * (1.0 - t)
        Next

        Return r
    End Function

    Public Sub Backward(adjustInDirection() As Double) Implements ILayer.Backward
        For i As Integer = 0 To Me.Inputs - 1
            Me.Gradients(i) = Me.Gradients(i) * adjustInDirection(i)
        Next

        Me.PreviousLayer.Backward(Me.Gradients)
    End Sub

    Public Sub UpdateWeights(learningRate As Double) Implements ILayer.UpdateWeights
        Me.PreviousLayer.UpdateWeights(learningRate)
    End Sub

    Public Sub UpdateWeightsAdam(learningRate As Double) Implements ILayer.UpdateWeightsAdam
        Me.PreviousLayer.UpdateWeightsAdam(learningRate)
    End Sub

    Public Sub Clear() Implements ILayer.Clear
    End Sub

    Public Sub Regularize() Implements ILayer.Regularize
    End Sub

End Class
