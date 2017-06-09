Public Class InputNeuron
    Implements INeuron

    Public Property Inputs As Integer
    Public Property Weights As Double()
    Public Property Momentum As Double()
    Public Property Velocity As Double()
    Public Property LocalWeightGradients As Double()
    Public Property WeightGradients As Double()

    Public Sub New(inputs As Integer)
        Me.Inputs = inputs

        ReDim Me.Weights(inputs)
        ReDim Me.Momentum(inputs)
        ReDim Me.Velocity(inputs)
        ReDim Me.LocalWeightGradients(inputs)
        ReDim Me.WeightGradients(inputs)

        Me.Weights(inputs) = 0.01
    End Sub

    Public Sub Clear() Implements INeuron.Clear
        For i As Integer = 0 To Me.Inputs
            Me.LocalWeightGradients(i) = 0.0
            Me.WeightGradients(i) = 0.0
        Next

        Me.LocalWeightGradients(Me.Inputs) = 1.0 'das add gate vom bias hat immer 1 als gradient
    End Sub

    Public Function Forward(inputs As Double()) As Double Implements INeuron.Forward 'das gilt nur für einen input forward pass, ansonsten muss ich die gradienten von x, y etc auch berechnen
        Dim r As Double = Me.Weights(Me.Inputs) 'a*x + b*y + ... + z | z = bias und das letze Gewicht

        For i As Integer = 0 To Me.Inputs - 1
            Me.LocalWeightGradients(i) += inputs(i)
            r += Me.Weights(i) * inputs(i)
        Next

        Return r
    End Function

    Public Sub Backward(adjustInDirection As Double) Implements INeuron.Backward 'das gilt nur für einen input backward pass
        For i As Integer = 0 To Me.Inputs
            Me.WeightGradients(i) += Me.LocalWeightGradients(i) * adjustInDirection
        Next
    End Sub

    Public Sub Regularize() Implements INeuron.Regularize
        For i As Integer = 0 To Me.Inputs - 1 'das bias nicht regulieren
            Me.WeightGradients(i) -= Me.Weights(i)
        Next
    End Sub

    Public Sub UpdateWeights(stepSize As Double) Implements INeuron.UpdateWeights
        For i As Integer = 0 To Me.Inputs
            Me.Weights(i) += Me.WeightGradients(i) * stepSize
        Next
    End Sub

    Public Sub UpdateWeightsRms(stepSize As Double)
        For i As Integer = 0 To Me.Inputs
            Me.Velocity(i) = DecayRate * Me.Velocity(i) + (1.0 - DecayRate) * Me.WeightGradients(i) * Me.WeightGradients(i)
            Me.Weights(i) += stepSize * Me.WeightGradients(i) / (Math.Sqrt(Me.Velocity(i)) + ZeroDivisor)
        Next
    End Sub

    Public Sub UpdateWeightsAdam(stepSize As Double) Implements INeuron.UpdateWeightsAdam
        For i As Integer = 0 To Me.Inputs
            Me.Momentum(i) = MomentumBeta * Me.Momentum(i) + (1.0 - MomentumBeta) * Me.WeightGradients(i)
            Me.Velocity(i) = VelocityBeta * Me.Velocity(i) + (1.0 - VelocityBeta) * Me.WeightGradients(i) * Me.WeightGradients(i)
            Me.Weights(i) += stepSize * Me.Momentum(i) / (Math.Sqrt(Me.Velocity(i)) + ZeroDivisor)
        Next
    End Sub

End Class
