Imports StockStrategies

Public Interface ILayer

    Enum LayerTypeE
        FullyConnected
        Relu
        Tanh
    End Enum

    Sub Randomize(rnd As Random)
    Function Forward(inputs As Double()) As Double()
    Sub Backward(adjustInDirection As Double())
    Sub UpdateWeights(learningRate As Double)
    Sub UpdateWeightsAdam(learningRate As Double)
    Sub Clear()
    Sub Regularize()
    Function CreateNeuronLayer(outputs As Integer) As NeuronLayer
    Function CreateReluLayer() As ReluLayer
    Function CreateTahLayer() As TanhLayer

End Interface
