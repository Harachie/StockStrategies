Public Class NeuralNetwork

    Public Property Inputs As Integer
    Public Property Outputs As Integer
    Public Property Layers As New List(Of ILayer)

    Public Sub New(inputs As Integer)
        Me.Inputs = inputs
        Me.Outputs = Outputs
    End Sub

    Public Function AddInputLayer(outputs As Integer) As InputLayer
        Dim r As InputLayer

        r = New InputLayer(Me.Inputs, outputs)
        Me.Layers.Add(r)

        Return r
    End Function

    Public Function AddNeuronLayer(outputs As Integer) As ILayer
        Dim r As ILayer

        If Me.Layers.Count = 0 Then
            r = New InputLayer(Me.Inputs, outputs)
        Else
            r = Me.Layers(Me.Layers.Count - 1).CreateNeuronLayer(outputs)
        End If

        Me.Layers.Add(r)

        Return r
    End Function

    Public Function AddReluLayer() As ReluLayer
        Dim r As ReluLayer

        r = Me.Layers(Me.Layers.Count - 1).CreateReluLayer()
        Me.Layers.Add(r)

        Return r
    End Function

    Public Function AddTanhLayer() As TanhLayer
        Dim r As TanhLayer

        r = Me.Layers(Me.Layers.Count - 1).CreateTahLayer()
        Me.Layers.Add(r)

        Return r
    End Function

    Public Function AddFinalLayer(outputs As Integer, type As ILayer.LayerTypeE) As ILayer
        Me.Outputs = outputs

        Select Case type
            Case ILayer.LayerTypeE.FullyConnected
                Return Me.AddNeuronLayer(outputs)

            Case ILayer.LayerTypeE.Relu
                Return Me.AddReluLayer()

            Case ILayer.LayerTypeE.Tanh
                Return Me.AddTanhLayer()

            Case Else
                Throw New InvalidOperationException("Invalid layer type.")

        End Select
    End Function

    Public Sub Randomize(rnd As Random)
        For Each layer In Me.Layers
            layer.Randomize(rnd)
        Next
    End Sub

    Public Function Forward(inputs As Double()) As Double()
        Dim r As Double()

        r = Me.Layers(0).Forward(inputs)

        For i As Integer = 1 To Me.Layers.Count - 1
            r = Me.Layers(i).Forward(r)
        Next

        Return r
    End Function

    Public Sub Backward(adjustInDirection As Double())
        Me.Layers(Me.Layers.Count - 1).Backward(adjustInDirection)
    End Sub

    Public Sub Regularize()
        For Each layer In Me.Layers
            layer.Regularize()
        Next
    End Sub

    Public Sub UpdateWeights(learningRate As Double)
        Me.Layers(Me.Layers.Count - 1).UpdateWeights(learningRate)
    End Sub

    Public Sub UpdateWeightsAdam(learningRate As Double)
        Me.Layers(Me.Layers.Count - 1).UpdateWeightsAdam(learningRate)
    End Sub

    Public Sub Clear()
        For Each layer In Me.Layers
            layer.Clear()
        Next
    End Sub

End Class
