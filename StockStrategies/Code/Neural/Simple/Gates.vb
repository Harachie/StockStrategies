Public Class AddGate

    Public GradientX As Double
    Public GradientY As Double

    Public X As Double
    Public Y As Double

    Public Sub Clear()
        Me.GradientX = 0.0
        Me.GradientY = 0.0
    End Sub

    Public Function Forward(x As Double, y As Double) As Double
        Me.X += x
        Me.Y += y

        Return x + y
    End Function

    Public Sub Backward(topGradient As Double)
        Me.GradientX += topGradient
        Me.GradientY += topGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.X += Me.GradientX * learningRate
        Me.Y += Me.GradientY * learningRate
    End Sub

End Class

Public Class AddBiasGate

    Public GradientBias As Double
    Public GradientX As Double
    Public Bias As Double

    Public Sub Clear()
        Me.GradientBias = 0.0
        Me.GradientX = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Return Me.Bias + x
    End Function

    Public Sub Backward(topGradient As Double)
        Me.GradientBias += topGradient
        Me.GradientX += topGradient
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

    Public Sub Backward(topGradient As Double)
        Me.GradientX += topGradient * Me.LocalGradientX
        Me.GradientY += topGradient * Me.LocalGradientY
    End Sub

    Public Sub Update(learningRate As Double)
        Me.X += Me.GradientX * learningRate
        Me.Y += Me.GradientY * learningRate
    End Sub

End Class

Public Class MultiplyWeightGate

    Public LocalGradientWeight As Double
    Public LocalGradientX As Double
    Public GradientWeight As Double
    Public GradientX As Double

    Public Input As Double
    Public Weight As Double

    Public Sub Clear()
        Me.LocalGradientWeight = 0.0
        Me.LocalGradientX = 0.0
        Me.GradientWeight = 0.0
        Me.GradientX = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Me.Input = x
        Me.LocalGradientWeight = x
        Me.LocalGradientX = Me.Weight

        Return x * Me.Weight
    End Function

    Public Sub Backward(topGradient As Double)
        Me.GradientX += topGradient * Me.LocalGradientX
        Me.GradientWeight += topGradient * Me.LocalGradientWeight
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Weight += Me.GradientWeight * learningRate
    End Sub

End Class

Public Class Pow2WeightGate

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
        Me.LocalGradientInput = Me.Weight
        Me.LocalGradientWeight += 2 * Me.Weight * x

        Return x * x * Me.Weight
    End Function

    Public Sub Backward(topGradient As Double)
        Me.GradientInput += topGradient * Me.LocalGradientInput
        Me.GradientWeight += topGradient * Me.LocalGradientWeight
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Weight += Me.GradientWeight * learningRate
    End Sub

End Class

Public Class TanhGate 'inputs über +-1 haben nen extrem kleinen gradient und lernen langsam

    Public LocalGradient As Double
    Public GradientX As Double
    Public Input As Double

    Public Sub Clear()
        Me.GradientX = 0.0
        Me.LocalGradient = 0.0
    End Sub

    Public Function Forward(x As Double) As Double
        Dim t As Double

        t = Math.Tanh(x)

        Me.Input = x
        Me.LocalGradient += (1.0 + t) * (1.0 - t)

        Return t
    End Function

    Public Sub Backward(topGradient As Double)
        Me.GradientX += topGradient * Me.LocalGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Input += Me.GradientX * learningRate
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

    Public Sub Backward(topGradient As Double)
        Me.Gradient += topGradient * Me.LocalGradient
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

    Public Sub Backward(topGradient As Double)
        Me.Gradient += topGradient * Me.LocalGradient
    End Sub

    Public Sub Update(learningRate As Double)
        Me.Input += Me.Gradient * learningRate
    End Sub

End Class

Public Class TwoInputTanhNeuronGate

    Public GradientX As Double
    Public GradientY As Double

    Public Ax As New MultiplyWeightGate
    Public By As New MultiplyWeightGate
    Public AxBy As New AddGate
    Public AxByC As New AddBiasGate
    Public Activation As New TanhGate

    Public Sub Clear()
        Me.GradientX = 0.0
        Me.GradientY = 0.0

        Me.Ax.Clear()
        Me.By.Clear()
        Me.AxBy.Clear()
        Me.AxByC.Clear()
        Me.Activation.Clear()
    End Sub

    Public Function Forward(x As Double, y As Double) As Double
        Return Me.Activation.Forward(Me.AxByC.Forward(Me.AxBy.Forward(Me.Ax.Forward(x), Me.By.Forward(y))))
    End Function

    Public Sub Backward(topGradient As Double)
        Me.Activation.Backward(topGradient)
        Me.AxByC.Backward(Me.Activation.GradientX)
        Me.AxBy.Backward(Me.AxByC.GradientX)
        Me.Ax.Backward(Me.AxBy.GradientX)
        Me.By.Backward(Me.AxBy.GradientY)

        Me.GradientX = Me.Ax.GradientX
        Me.GradientY = Me.By.GradientX
    End Sub

    Public Sub Update(learningRate As Double)
        ' Me.Activation.Update(learningRate) 'does nothing, has no parameter
        Me.AxByC.Update(learningRate) 'updates c (bias)
        '  Me.AxBy.Update(learningRate) 'does nothing, has no parameter
        Me.Ax.Update(learningRate) 'updates a
        Me.By.Update(learningRate) 'updates b
    End Sub

End Class