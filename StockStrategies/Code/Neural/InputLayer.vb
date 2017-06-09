Imports StockStrategies

Public Class InputLayer
    Implements ILayer

    Public Property Inputs As Integer
    Public Property Outputs As Integer
    Public Property Neurons As New List(Of InputNeuron)

    Public Sub New(inputs As Integer, outputs As Integer)
        Me.Inputs = inputs
        Me.Outputs = outputs

        For i As Integer = 1 To outputs
            Me.Neurons.Add(New InputNeuron(inputs))
        Next
    End Sub

    Public Function CreateNeuronLayer(outputs As Integer) As NeuronLayer
        Return New NeuronLayer(Me.Outputs, outputs, Me)
    End Function

    Public Function CreateReluLayer() As ReluLayer
        Return New ReluLayer(Me.Outputs, Me)
    End Function

    Public Sub Randomize(rnd As Random)
        For Each neuron In Me.Neurons
            For i As Integer = 0 To neuron.Weights.Length - 1
                neuron.Weights(i) = rnd.NextDouble * 2.0 - 1.0
            Next
        Next
    End Sub

    Public Function Forward(inputs As Double()) As Double() Implements ILayer.Forward
        Dim r(Me.Outputs - 1) As Double

        For i As Integer = 0 To Me.Outputs - 1
            r(i) = Me.Neurons(i).Forward(inputs)
        Next

        Return r
    End Function

    Public Sub Backward(adjustInDirection As Double()) Implements ILayer.Backward
        For i As Integer = 0 To Me.Outputs - 1
            Me.Neurons(i).Backward(adjustInDirection(i))
        Next
    End Sub

    Public Sub UpdateWeights(learningRate As Double) Implements ILayer.UpdateWeights
        For i As Integer = 0 To Me.Outputs - 1
            Me.Neurons(i).UpdateWeights(learningRate)
        Next
    End Sub

    Public Sub Clear() Implements ILayer.Clear
        For Each neuron In Me.Neurons
            neuron.Clear()
        Next
    End Sub

    Public Sub Regularize() Implements ILayer.Regularize
        For Each neuron In Me.Neurons
            neuron.Regularize()
        Next
    End Sub

End Class
