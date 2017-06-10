Public Class AddGate

    Public LocalGradientX As Double = 1.0
    Public LocalGradientY As Double = 1.0

    Public GradientX As Double
    Public GradientY As Double

    Public X As Double
    Public Y As Double

    Public Sub Clear()
        Me.GradientX = 0.0
        Me.GradientY = 0.0
    End Sub

    Public Function Forward(x As Double, y As Double) As Double
        'Me.X += x
        'Me.Y += y

        Return x + y
    End Function

    Public Sub Backward(direction As Double)
        Me.GradientX += direction * Me.LocalGradientX
        Me.GradientY += direction * Me.LocalGradientY
    End Sub

    Public Sub Update(learningRate As Double)
        'Me.X += Me.GradientX * learningRate
        'Me.Y += Me.GradientY * learningRate
    End Sub

End Class

Public Class AddBiasGate

    Public GradientBias As Double
    Public GradientInput As Double
    Public Bias As Double

    Public Sub Clear()
        Me.GradientBias = 0.0
        Me.GradientInput = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Return Me.Bias + x
    End Function

    Public Sub Backward(direction As Double)
        Me.GradientBias += direction
        Me.GradientInput += direction
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Bias += Me.GradientBias * learningRate
    End Sub

End Class

Public Class MultiplyGate

    Public LocalGradientX As Double
    Public LocalGradientY As Double

    Public GradientX As Double
    Public GradientY As Double

    Public X As Double
    Public Y As Double

    Public Sub Clear()
        Me.GradientX = 0.0
        Me.GradientY = 0.0
        Me.LocalGradientX = 0.0
        Me.LocalGradientY = 0.0
    End Sub

    Public Function Forward(x As Double, y As Double) As Double
        Me.X = x
        Me.Y = y

        Me.LocalGradientX += y
        Me.LocalGradientY += x

        Return x * y
    End Function

    Public Sub Backward(direction As Double)
        Me.GradientX += direction * Me.LocalGradientX
        Me.GradientY += direction * Me.LocalGradientY
    End Sub

    Public Sub Update(learningRate As Double)
        Me.X += Me.GradientX * learningRate
        Me.Y += Me.GradientY * learningRate
    End Sub

End Class

Public Class MultiplyWeightGate

    Public LocalGradientWeight As Double
    Public LocalGradientInput As Double
    Public GradientWeight As Double
    Public GradientInput As Double

    Public Input As Double
    Public Weight As Double

    Public Sub Clear()
        Me.LocalGradientWeight = 0.0
        Me.LocalGradientInput = 0.0
        Me.GradientWeight = 0.0
        Me.GradientInput = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Me.Input = x
        Me.LocalGradientWeight = x
        Me.LocalGradientInput = Me.Weight

        Return x * Me.Weight
    End Function

    Public Sub Backward(direction As Double)
        Me.GradientWeight += direction * Me.LocalGradientWeight
        Me.GradientInput += direction * Me.LocalGradientInput
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Weight += Me.GradientWeight * learningRate
    End Sub

End Class

Public Class Pow2WeightGate

    Public LocalGradientWeight As Double
    Public GradientWeight As Double

    Public Input As Double
    Public Weight As Double

    Public Sub Clear()
        Me.GradientWeight = 0.0
        Me.LocalGradientWeight = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Me.Input = x
        Me.LocalGradientWeight += 2 * Me.Weight * x

        Return x * x * Me.Weight
    End Function

    Public Sub Backward(direction As Double)
        Me.GradientWeight += direction * Me.LocalGradientWeight
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Weight += Me.GradientWeight * learningRate
    End Sub

End Class

Public Class TanhGate 'inputs über +-1 haben nen extrem kleinen gradient und lernen langsam

    Public LocalGradient As Double
    Public Gradient As Double
    Public Input As Double

    Public Sub Clear()
        Me.Gradient = 0.0
        Me.LocalGradient = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Dim t As Double

        t = Math.Tanh(x)

        Me.Input = x
        Me.LocalGradient += (1.0 + t) * (1.0 - t)

        Return t
    End Function

    Public Sub Backward(direction As Double)
        Me.Gradient += direction * Me.LocalGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Input += Me.Gradient * learningRate
    End Sub

End Class

Public Class SigmoidGate 'inputs über +-1 haben nen extrem kleinen gradient und lernen langsam

    Public LocalGradient As Double
    Public Gradient As Double
    Public Input As Double

    Private Function Sigmoid(x As Double) As Double
        Return 1.0 / (1.0 + Math.Exp(-x))
    End Function

    Public Sub Clear()
        Me.Gradient = 0.0
        Me.LocalGradient = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Dim s As Double

        s = Sigmoid(x)

        Me.Input = x
        Me.LocalGradient += s * (1.0 - s)

        Return s
    End Function

    Public Sub Backward(direction As Double)
        Me.Gradient += direction * Me.LocalGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Input += Me.Gradient * learningRate
    End Sub

End Class

Public Class ReluGate

    Public LocalGradient As Double
    Public Gradient As Double
    Public Input As Double

    Public Sub Clear()
        Me.Gradient = 0.0
        Me.LocalGradient = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Dim t As Double

        t = Math.Max(0.0, x)

        Me.Input = x

        If t > 0 Then
            Me.LocalGradient += x
        Else
            Me.LocalGradient += 0
        End If

        Return t
    End Function

    Public Sub Backward(direction As Double)
        Me.Gradient += direction * Me.LocalGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Input += Me.Gradient * learningRate
    End Sub

End Class
