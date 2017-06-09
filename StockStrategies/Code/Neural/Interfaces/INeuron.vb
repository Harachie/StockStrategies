Public Interface INeuron

    Function Forward(inputs As Double()) As Double
    Sub Backward(adjustInDirection As Double)
    Sub Regularize()
    Sub UpdateWeights(stepSize As Double)
    Sub UpdateWeightsAdam(stepSize As Double)
    Sub Clear()

End Interface
