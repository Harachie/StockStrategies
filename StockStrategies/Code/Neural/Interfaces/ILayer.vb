Public Interface ILayer

    Function Forward(inputs As Double()) As Double()
    Sub Backward(adjustInDirection As Double())
    Sub UpdateWeights(learningRate As Double)
    Sub Clear()
    Sub Regularize()

End Interface
