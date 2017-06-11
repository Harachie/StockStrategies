Public Class NeuronLayer
    Implements ILayer

    Public Property Inputs As Integer
    Public Property Outputs As Integer
    Public Property PreviousLayer As ILayer
    Public Property Neurons As New List(Of Neuron)

    Public Sub New(inputs As Integer, outputs As Integer, previousLayer As ILayer)
        Me.Inputs = inputs
        Me.Outputs = outputs
        Me.PreviousLayer = previousLayer

        For i As Integer = 1 To outputs
            Me.Neurons.Add(New Neuron(inputs))
        Next
    End Sub

    Public Function CreateNeuronLayer(outputs As Integer) As NeuronLayer Implements ILayer.CreateNeuronLayer
        Return New NeuronLayer(Me.Outputs, outputs, Me)
    End Function

    Public Function CreateReluLayer() As ReluLayer Implements ILayer.CreateReluLayer
        Return New ReluLayer(Me.Outputs, Me)
    End Function

    Public Function CreateTanhLayer() As TanhLayer Implements ILayer.CreateTahLayer 'das macht im relulayer wenig sinn... aber naja
        Return New TanhLayer(Me.Outputs, Me)
    End Function

    Public Sub Randomize(rnd As Random) Implements ILayer.Randomize
        For Each neuron In Me.Neurons
            For i As Integer = 0 To neuron.Weights.Length - 2 'bias startet bei 0
                neuron.Weights(i) = rnd.NextDouble * 2.0 - 1.0
                ' Console.WriteLine(neuron.Weights(i))
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
        Dim r(Me.Inputs - 1) As Double

        For i As Integer = 0 To Me.Outputs - 1
            Me.Neurons(i).Backward(adjustInDirection(i))

            For n As Integer = 0 To Me.Inputs - 1
                r(n) += Me.Neurons(i).InputGradients(n)
            Next
        Next

        Me.PreviousLayer.Backward(r)
    End Sub

    Public Sub UpdateWeights(learningRate As Double) Implements ILayer.UpdateWeights
        For i As Integer = 0 To Me.Outputs - 1
            Me.Neurons(i).UpdateWeights(learningRate)
        Next

        Me.PreviousLayer.UpdateWeights(learningRate)
    End Sub

    Public Sub UpdateWeightsAdam(learningRate As Double) Implements ILayer.UpdateWeightsAdam
        For i As Integer = 0 To Me.Outputs - 1
            Me.Neurons(i).UpdateWeightsAdam(learningRate)
        Next

        Me.PreviousLayer.UpdateWeightsAdam(learningRate)
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
