Imports System.Text.RegularExpressions
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module Program

    Sub Doppelte()
        Dim book As New Book

        book.AddAccount("Kassenbestand", Account.TypeE.Active, 10000.0)
        book.AddAccount("Steuern", Account.TypeE.Active, 0.0)
        book.AddAccount("Wertpapiere", Account.TypeE.Active, 0.0)

        book.AddAccount("Kapital", Account.TypeE.Passive, 10000.0)
    End Sub

    Sub Snp()
        Dim sdc As StockDataCollection = StockDataCollection.ReadFromStooqFile(IO.Path.Combine(GetStooqDirectory(), "snp.txt"))
        Dim early, late As StockData
        Dim factor As Double
        Dim factors As New Dictionary(Of Integer, Double)
        Dim stepSize As Integer = 260
        Dim startDate As Date = DateSerial(1960, 1, 1)
        Dim data = sdc.FilterByMinimumStartDate(startDate)


        For i As Integer = stepSize To data.Count - 1
            early = data(i - stepSize)
            late = data(i)
            factor = late.Close / early.Open
            factors(i) = factor
        Next

        Dim highestFalls = From x In factors Order By x.Value
        Dim highestRise = From x In factors Order By x.Value Descending
    End Sub

    Public Sub ThreeToOne()
        Dim outputNeuron As Neuron
        Dim inputNeuron As InputNeuron
        Dim inputNeurons As New List(Of INeuron)
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim resultSet(100) As Double
        Dim labels As New List(Of Integer)
        Dim index As Integer
        Dim current As Double()
        Dim hits As Integer
        Dim result As Double

        'datas.Add({1.2, 0.7}) : labels.Add(1)
        'datas.Add({-0.3, -0.5}) : labels.Add(-1)
        'datas.Add({3.0, 0.1}) : labels.Add(1)
        'datas.Add({-0.1, -1.0}) : labels.Add(-1)
        'datas.Add({-1.0, 1.1}) : labels.Add(-1)
        'datas.Add({2.1, -3.0}) : labels.Add(1)

        datas.Add({0, 0}) : labels.Add(-1)
        datas.Add({0, 1}) : labels.Add(1)
        datas.Add({1, 0}) : labels.Add(1)
        datas.Add({1, 1}) : labels.Add(-1)

        For i As Integer = 1 To 5
            inputNeuron = New InputNeuron(2)

            For n As Integer = 1 To 2
                inputNeuron.Weights(n - 1) = rnd.NextDouble * 2 - 1.0
            Next

            inputNeurons.Add(inputNeuron)
        Next

        outputNeuron = New Neuron(inputNeurons.Count)

        For n As Integer = 1 To inputNeurons.Count - 1
            outputNeuron.Weights(n - 1) = rnd.NextDouble * 2 - 1.0
        Next


        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            outputNeuron.Clear()

            For n As Integer = 0 To inputNeurons.Count - 1
                inputNeurons(n).Clear()
            Next

            For n As Integer = 0 To inputNeurons.Count - 1
                resultSet(n) = inputNeurons(n).Forward(current)
            Next

            result = outputNeuron.Forward(resultSet)

            If labels(index) = 1 AndAlso result < 1 Then
                outputNeuron.Backward(1.0)

            ElseIf labels(index) = -1 AndAlso result > -1 Then
                outputNeuron.Backward(-1.0)

            Else
                outputNeuron.Backward(0.0)
            End If

            For n As Integer = 0 To inputNeurons.Count - 1
                inputNeurons(n).Backward(outputNeuron.InputGradients(n))
                inputNeurons(n).UpdateWeightsAdam(0.01)
            Next

            outputNeuron.Regularize()
            outputNeuron.UpdateWeightsAdam(0.01)

            hits = 0

            For n As Integer = 0 To datas.Count - 1
                current = datas(n)

                For ni As Integer = 0 To inputNeurons.Count - 1
                    resultSet(ni) = inputNeurons(ni).Forward(current)
                Next

                result = outputNeuron.Forward(resultSet)

                If labels(n) = 1 AndAlso result > 0 Then
                    hits += 1

                ElseIf labels(n) = -1 AndAlso result <= 0 Then
                    hits += 1
                End If
            Next

            If hits = datas.Count Then
                Dim stopHere = 1
            End If


            If i Mod 50 = 0 Then
                Console.WriteLine(hits)
            End If
        Next
    End Sub

    Public Sub ThreeToOneLayer()
        Dim inputLayer As New InputLayer(2, 3)
        Dim reluLayer As ReluLayer = inputLayer.CreateReluLayer()
        Dim hiddenLayer As NeuronLayer = reluLayer.CreateNeuronLayer(1)
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim resultSet(2) As Double
        Dim labels As New List(Of Integer)
        Dim index As Integer
        Dim current, inputResult, reluResult, hiddenResult As Double()
        Dim hits As Integer
        Dim result As Double

        datas.Add({1.2, 0.7}) : labels.Add(1)
        datas.Add({-0.3, -0.5}) : labels.Add(-1)
        datas.Add({3.0, 0.1}) : labels.Add(1)
        datas.Add({-0.1, -1.0}) : labels.Add(-1)
        datas.Add({-1.0, 1.1}) : labels.Add(-1)
        datas.Add({2.1, -3.0}) : labels.Add(1)


        inputLayer.Randomize(rnd)
        hiddenLayer.Randomize(rnd)


        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            inputLayer.Clear()
            hiddenLayer.Clear()


            inputResult = inputLayer.Forward(current)
            reluResult = reluLayer.Forward(inputResult)
            hiddenResult = hiddenLayer.Forward(reluResult)
            result = hiddenResult(0)

            If labels(index) = 1 AndAlso result < 1 Then
                hiddenLayer.Backward({1.0})

            ElseIf labels(index) = -1 AndAlso result > -1 Then
                hiddenLayer.Backward({-1.0})

            Else
                hiddenLayer.Backward({0.0})
            End If

            ' inputLayer.Regularize()
            ' hiddenLayer.Regularize()
            hiddenLayer.UpdateWeightsAdam(0.01)
            '  outputNeuron.Regularize()
            '    outputNeuron.UpdateWeightsAdam(0.01)
            hits = 0

            For n As Integer = 0 To datas.Count - 1
                current = datas(n)
                inputResult = inputLayer.Forward(current)
                reluResult = reluLayer.Forward(inputResult)
                hiddenResult = hiddenLayer.Forward(reluResult)
                result = hiddenResult(0)

                If labels(n) = 1 AndAlso result > 0 Then
                    hits += 1

                ElseIf labels(n) = -1 AndAlso result <= 0 Then
                    hits += 1
                End If
            Next

            If hits = 6 Then
                Dim stopHere = 1
            End If


            If i Mod 50 = 0 Then
                Console.WriteLine(hits)
            End If
        Next
    End Sub

    Public Sub NetworkTest()
        Dim nn As New NeuralNetwork(2)
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim resultSet(2) As Double
        Dim labels As New List(Of Integer)
        Dim index As Integer
        Dim current, nnResult As Double()
        Dim hits As Integer
        Dim result As Double

        datas.Add({1.2, 0.7}) : labels.Add(1)
        datas.Add({-0.3, -0.5}) : labels.Add(-1)
        datas.Add({3.0, 0.1}) : labels.Add(1)
        datas.Add({-0.1, -1.0}) : labels.Add(-1)
        datas.Add({-1.0, 1.1}) : labels.Add(-1)
        datas.Add({2.1, -3.0}) : labels.Add(1)


        'datas.Add({0, 0}) : labels.Add(-1)
        'datas.Add({0, 1}) : labels.Add(1)
        'datas.Add({1, 0}) : labels.Add(1)
        'datas.Add({1, 1}) : labels.Add(-1)


        nn.AddInputLayer(5)
        nn.AddTanhLayer()
        nn.AddNeuronLayer(1)
        nn.AddFinalLayer(1, ILayer.LayerTypeE.Tanh)
        nn.Randomize(rnd)
        Console.WriteLine()

        Dim err As Double

        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            nn.Clear()
            nnResult = nn.Forward(current)
            result = nnResult(0)
            err = labels(index) - result

            'If labels(index) = 1 AndAlso result < 1 Then
            '    nn.Backward({1.0})

            'ElseIf labels(index) = -1 AndAlso result > -1 Then
            '    nn.Backward({-1.0})

            'Else
            '    nn.Backward({0.0})
            'End If

            nn.Backward({err})
            'nn.Regularize()
            nn.UpdateWeightsAdam(0.01)

            hits = 0

            'If i Mod 50 = 0 Then
            '    Console.WriteLine(hits)
            'End If

            'If lastLayer.Neurons(0).Weights(0) > 5 OrElse lastLayer.Neurons(0).Weights(0) < -5 Then
            '    Dim stopHere = 1
            'End If

            For n As Integer = 0 To datas.Count - 1
                current = datas(n)
                nnResult = nn.Forward(current)
                result = nnResult(0)

                If labels(n) = 1 AndAlso result >= 0 Then
                    hits += 1

                ElseIf labels(n) = -1 AndAlso result <= 0 Then
                    hits += 1
                End If
            Next

            If hits = datas.Count Then
                Dim stopHere = 1
            End If

            If i Mod 5000 = 0 Then

                nn.Randomize(rnd)
            End If

        Next
    End Sub


    Public Sub NetworkTestSnp()
        Dim rnd As New Random(24)
        Dim datas As New List(Of Double())
        Dim labels As New List(Of Double)
        Dim datasC As New List(Of Double())
        Dim labelsC As New List(Of Double)
        Dim results As New List(Of Double)
        Dim resultsC As New List(Of Double)
        Dim index As Integer
        Dim current, nnResult As Double()
        Dim hits, hitsC As Integer
        Dim result As Double

        Dim sdc As StockDataCollection = StockDataCollection.ReadFromStooqFile(IO.Path.Combine(GetStooqDirectory(), "snp.txt"))
        Dim filtered = sdc.FilterByMinimumStartDate(DateSerial(1990, 1, 1))
        Dim afterSell As Double
        Dim compareBar, buyBar As StockData
        Dim data As New List(Of Double)
        Dim positiveL, negativeL As Integer
        Dim selectionIndices As New List(Of Integer)
        Dim sellAfter As Integer = 260

        selectionIndices.Add(260)
        selectionIndices.Add(195)
        selectionIndices.Add(130)
        selectionIndices.Add(65)
        'selectionIndices.Add(20)
        'selectionIndices.Add(15)
        'selectionIndices.Add(10)
        'selectionIndices.Add(5)
        'selectionIndices.Add(4)
        'selectionIndices.Add(3)
        'selectionIndices.Add(2)
        'selectionIndices.Add(1)

        Dim nn As New NeuralNetwork(selectionIndices.Count)
        Dim maxIndex As Integer = selectionIndices.Max
        Dim labelTarget As Double = 0.8
        Dim initialLearningRate As Double = 0.0001
        Dim learningRate As Double = initialLearningRate
        Dim addToCompare As Boolean

        For i As Integer = maxIndex To filtered.Count - (sellAfter + 2)
            data.Clear()
            compareBar = filtered(i)
            buyBar = filtered(i + 1)
            afterSell = filtered(i + sellAfter + 1).Close / buyBar.Close - 1.0

            For Each selectionIndex In selectionIndices
                data.Add(compareBar.Close / filtered(i - selectionIndex).Close - 1.0)
            Next

            If addToCompare Then
                datasC.Add(data.ToArray)
                resultsC.Add(afterSell)
            Else
                datas.Add(data.ToArray)
                results.Add(afterSell)
            End If

            If afterSell > 0.0 Then
                If addToCompare Then
                    labelsC.Add(labelTarget)
                Else
                    labels.Add(labelTarget)
                End If

                positiveL += 1
            ElseIf afterSell < 0 Then
                If addToCompare Then
                    labelsC.Add(-labelTarget)
                Else
                    labels.Add(-labelTarget)
                End If

                negativeL += 1
            End If

            If (positiveL > 700 AndAlso positiveL = negativeL) OrElse i = 5000 Then
                addToCompare = True
            End If
        Next

        Dim postiveHits As Double = labels.Where(Function(x) x > 0).Count / labels.Count
        Dim postiveHitsC As Double = labelsC.Where(Function(x) x > 0).Count / labelsC.Count

        nn.AddInputLayer(5)
        'nn.AddReluLayer()
        'nn.AddNeuronLayer(7)
        nn.AddReluLayer()
        nn.AddNeuronLayer(1)
        nn.AddFinalLayer(1, ILayer.LayerTypeE.Tanh)
        nn.Randomize(rnd)
        Console.WriteLine()
        Dim err, errC As Double

        For i As Integer = 1 To 400000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            nn.Clear()
            nnResult = nn.Forward(current)
            result = nnResult(0)
            err = labels(index) - result

            nn.Backward({err})
            nn.UpdateWeightsAdam(learningRate)

            hits = 0

            If i Mod 1000 = 0 Then
                err = 0
                errc = 0
                hits = 0
                hitsC = 0

                For n As Integer = 0 To datas.Count - 1
                    current = datas(n)
                    nnResult = nn.Forward(current)
                    result = nnResult(0)

                    If result > 0.0 AndAlso labels(n) > 0.0 Then
                        hits += 1
                    End If

                    If result < 0.0 AndAlso labels(n) < 0 Then
                        hits += 1
                    End If

                    err += 0.5 * (labels(n) - result) * (labels(n) - result)
                Next

                For n As Integer = 0 To datasC.Count - 1
                    current = datasC(n)
                    nnResult = nn.Forward(current)
                    result = nnResult(0)

                    If result > 0.0 AndAlso labelsC(n) > 0.0 Then
                        hitsC += 1
                    End If

                    If result < 0.0 AndAlso labelsC(n) < 0 Then
                        hitsC += 1
                    End If

                    errC += 0.5 * (labelsC(n) - result) * (labelsC(n) - result)
                Next

                If err < 500 AndAlso learningRate = initialLearningRate Then
                    learningRate /= 2.0
                End If

                If err < 450 AndAlso learningRate = initialLearningRate / 2.0 Then
                    learningRate /= 2.0
                End If

                If err < 400 AndAlso learningRate = initialLearningRate / 4.0 Then
                    learningRate /= 2.0
                End If

                ' Console.WriteLine("{0:n0} | {1:n0}", postiveHits, postiveHitsC)
                Console.WriteLine("hits {0:n0} / {1:n2}, err {2:n3}, hits {3:n0} / {4:n2}, err {5:n3}, lr {6:n5} | {7:n3}, {8:n3}", hits, hits / datas.Count, err, hitsC, hitsC / datasC.Count, errC, learningRate, postiveHits, postiveHitsC)
                ' Threading.Thread.Sleep(10)
            End If
        Next
    End Sub


    Public Sub TestTanh()
        Dim x As New TanhGate
        Dim result As Double

        x.Input = 10.0

        For i As Integer = 1 To 40
            x.Clear()
            result = x.Forward(x.Input)
            x.Backward(1)
            x.Update(0.01)
            'Console.WriteLine(result)
        Next
    End Sub

    Public Sub TestBias()
        Dim x As New AddBiasGate
        Dim result As Double

        x.Bias = 10.0

        For i As Integer = 1 To 40
            x.Clear()
            result = x.Forward(5)
            x.Backward(-1)
            x.Update(0.01)
            Console.WriteLine(result)
        Next
    End Sub

    Public Sub TestSigmoid()
        Dim x As New SigmoidGate
        Dim result As Double

        x.Input = 0.5

        For i As Integer = 1 To 40
            x.Clear()
            result = x.Forward(x.Input)
            x.Backward(-1)
            x.Update(0.01)
            '  Console.WriteLine(x.Input)
        Next
    End Sub

    Public Sub TestRelu()
        Dim x As New ReluGate
        Dim result As Double

        x.Input = 10.0

        For i As Integer = 1 To 40
            x.Clear()
            result = x.Forward(x.Input)
            x.Backward(-1)
            x.Update(0.01)
            ' Console.WriteLine(result)
        Next
    End Sub

    Public Sub TestXor()
        Dim one As Double() = {1, 1}
        Dim two As Double() = {1, 0}
        Dim three As Double() = {0, 1}
        Dim four As Double() = {0, 0}
        Dim x, y As Double

        Dim w1 As New MultiplyWeightGate With {.Weight = -0.5}
        Dim w2 As New MultiplyWeightGate With {.Weight = -0.7}
        Dim addw1w2 As New AddGate
        Dim bias1 As New AddBiasGate With {.Bias = -0.4}

        Dim w3 As New MultiplyWeightGate With {.Weight = 0.2}
        Dim w4 As New MultiplyWeightGate With {.Weight = -0.1}
        Dim addw3w4 As New AddGate
        Dim bias2 As New AddBiasGate With {.Bias = 0.2}

        Dim w5 As New MultiplyWeightGate With {.Weight = -0.8}
        Dim w6 As New MultiplyWeightGate With {.Weight = 0.2}
        Dim addw5w6 As New AddGate
        Dim bias3 As New AddBiasGate With {.Bias = 0.1}

        Dim addb1b2 As New AddGate
        Dim addb1b2b3 As New AddGate
        Dim wOut As New MultiplyWeightGate With {.Weight = 0.3}
        Dim biasOut As New AddBiasGate With {.Bias = -0.1}


        Dim result, b1Result, b2Result, b3Result, err As Double
        Dim learningRate As Double = 0.01
        Dim target As Double
        Dim rnd As New Random(23)


        For i As Integer = 1 To 400000
            Dim n As Integer = rnd.Next(4)

            Select Case n
                Case 0
                    x = 0
                    y = 0
                    target = 1

                Case 1
                    x = 1
                    y = 0
                    target = 1

                Case 2
                    x = 1
                    y = 0
                    target = 1

                Case 3
                    x = 1
                    y = 1
                    target = 1

            End Select

            w1.Clear()
            w2.Clear()
            addw1w2.Clear()
            bias1.Clear()

            w3.Clear()
            w4.Clear()
            addw3w4.Clear()
            bias2.Clear()

            w5.Clear()
            w6.Clear()
            addw5w6.Clear()
            bias3.Clear()

            biasOut.Clear()
            wOut.Clear()
            addb1b2.Clear()
            addb1b2b3.Clear()


            b1Result = bias1.Forward(addw1w2.Forward(w1.Forward(x), w2.Forward(y)))
            b2Result = bias2.Forward(addw3w4.Forward(w3.Forward(x), w4.Forward(y)))
            b3Result = bias3.Forward(addw5w6.Forward(w5.Forward(x), w6.Forward(y)))
            result = biasOut.Forward(wOut.Forward(addb1b2b3.Forward(addb1b2.Forward(b1Result, b2Result), b3Result)))

            err = target - result

            biasOut.Backward(err)
            wOut.Backward(biasOut.GradientX)
            addb1b2b3.Backward(wOut.GradientX)
            addb1b2.Backward(addb1b2b3.GradientX)

            bias3.Backward(addb1b2b3.GradientY)
            addw5w6.Backward(bias3.GradientX)
            w5.Backward(addw5w6.GradientX)
            w6.Backward(addw5w6.GradientY)

            bias1.Backward(addb1b2.GradientX)
            addw1w2.Backward(bias1.GradientX)
            w1.Backward(addw1w2.GradientX)
            w2.Backward(addw1w2.GradientY)

            bias2.Backward(addb1b2.GradientY)
            addw3w4.Backward(bias2.GradientX)
            w3.Backward(addw3w4.GradientX)
            w4.Backward(addw3w4.GradientY)


            biasOut.Update(learningRate)
            wOut.Update(learningRate)
            addb1b2b3.Update(learningRate)
            addb1b2.Update(learningRate)


            bias1.Update(learningRate)
            addw1w2.Update(learningRate)
            w1.Update(learningRate)
            w2.Update(learningRate)

            bias2.Update(learningRate)
            addw3w4.Update(learningRate)
            w3.Update(learningRate)
            w4.Update(learningRate)

            bias3.Update(learningRate)
            addw5w6.Update(learningRate)
            w5.Update(learningRate)
            w6.Update(learningRate)
            '      End If

            If i Mod 50 = 0 Then
                x = one(0)
                y = one(1)
                target = 0


                b1Result = bias1.Forward(addw1w2.Forward(w1.Forward(x), w2.Forward(y)))
                b2Result = bias2.Forward(addw3w4.Forward(w3.Forward(x), w4.Forward(y)))
                b3Result = bias3.Forward(addw5w6.Forward(w5.Forward(x), w6.Forward(y)))
                result = biasOut.Forward(wOut.Forward(addb1b2b3.Forward(addb1b2.Forward(b1Result, b2Result), b3Result)))
                Console.WriteLine("{0}, {1}", target, result)

                x = two(0)
                y = two(1)
                target = 1

                b1Result = bias1.Forward(addw1w2.Forward(w1.Forward(x), w2.Forward(y)))
                b2Result = bias2.Forward(addw3w4.Forward(w3.Forward(x), w4.Forward(y)))
                b3Result = bias3.Forward(addw5w6.Forward(w5.Forward(x), w6.Forward(y)))
                result = biasOut.Forward(wOut.Forward(addb1b2b3.Forward(addb1b2.Forward(b1Result, b2Result), b3Result)))
                Console.WriteLine("{0}, {1}", target, result)

                x = three(0)
                y = three(1)
                target = 1

                b1Result = bias1.Forward(addw1w2.Forward(w1.Forward(x), w2.Forward(y)))
                b2Result = bias2.Forward(addw3w4.Forward(w3.Forward(x), w4.Forward(y)))
                b3Result = bias3.Forward(addw5w6.Forward(w5.Forward(x), w6.Forward(y)))
                result = biasOut.Forward(wOut.Forward(addb1b2b3.Forward(addb1b2.Forward(b1Result, b2Result), b3Result)))
                Console.WriteLine("{0}, {1}", target, result)

                x = four(0)
                y = four(1)
                target = 0

                b1Result = bias1.Forward(addw1w2.Forward(w1.Forward(x), w2.Forward(y)))
                b2Result = bias2.Forward(addw3w4.Forward(w3.Forward(x), w4.Forward(y)))
                b3Result = bias3.Forward(addw5w6.Forward(w5.Forward(x), w6.Forward(y)))
                result = biasOut.Forward(wOut.Forward(addb1b2b3.Forward(addb1b2.Forward(b1Result, b2Result), b3Result)))
                Console.WriteLine("{0}, {1}", target, result)
                Console.WriteLine()
            End If

        Next
    End Sub

    Public Sub TestGates()
        Dim x As New Pow2WeightGate
        Dim y As New Pow2WeightGate
        Dim z As New AddGate
        Dim result, resultAfterUpdate As Double

        'relu bestimmt was gut war, diese dann verstärken als lernmethode
        'ich sage input war gut, dann wir dieser trampelpfad verstärkt
        'schlecht => pfad wird verringert
        'ist das nicht backprop mit relu? ja nur, dass ich nix berechnen muss, nur auf die relus erhöhen, wo es gut war um factor x
        TestXor()
        TestBias()
        TestRelu()
        TestSigmoid()
        TestTanh()
        x.Weight = 1
        y.Weight = -0.5

        For i As Integer = 1 To 40
            x.Clear()
            y.Clear()
            z.Clear()


            result = z.Forward(x.Forward(1), y.Forward(0.3))

            z.Backward(-1)
            x.Backward(z.GradientX)
            y.Backward(z.GradientY)

            z.Update(0.01)
            x.Update(0.01)
            y.Update(0.01)
            Console.WriteLine(result)
        Next
    End Sub

    Public Sub SimpleXor()
        Dim datas As New List(Of Double())
        Dim labels As New List(Of Double)
        Dim inputNeuron1, inputNeuron2 As TwoInputTanhNeuronGate
        Dim outputNeuron As TwoInputTanhNeuronGate
        Dim rnd As New Random(23)
        Dim index As Integer
        Dim input As Double()
        Dim result, in1, in2, err, lr As Double

        lr = 0.07

        inputNeuron1 = New TwoInputTanhNeuronGate
        inputNeuron2 = New TwoInputTanhNeuronGate
        outputNeuron = New TwoInputTanhNeuronGate

        inputNeuron1.Ax.Weight = -1.0
        inputNeuron1.By.Weight = -0.6

        inputNeuron2.Ax.Weight = -0.5
        inputNeuron2.By.Weight = 0.7

        outputNeuron.Ax.Weight = 0.3
        outputNeuron.By.Weight = 0.5

        datas.Add({0, 0}) : labels.Add(-0.7)
        datas.Add({1, 1}) : labels.Add(-0.7)
        datas.Add({1, 0}) : labels.Add(0.7)
        datas.Add({0, 1}) : labels.Add(0.7)

        For epoch As Integer = 1 To 5000000
            index = rnd.Next(datas.Count)
            input = datas(index)

            inputNeuron1.Clear()
            inputNeuron2.Clear()
            outputNeuron.Clear()

            in1 = inputNeuron1.Forward(input(0), input(1))
            in2 = inputNeuron2.Forward(input(0), input(1))
            result = outputNeuron.Forward(in1, in2)
            err = labels(index) - result

            outputNeuron.Backward(err)
            inputNeuron1.Backward(outputNeuron.GradientX)
            inputNeuron2.Backward(outputNeuron.GradientY)

            outputNeuron.Update(lr)
            inputNeuron1.Update(lr)
            inputNeuron2.Update(lr)

            If epoch Mod 100 = 0 Then
                Dim stopHere = 1
                err = 0.0

                For Each input In datas
                    index = datas.IndexOf(input)
                    in1 = inputNeuron1.Forward(input(0), input(1))
                    in2 = inputNeuron2.Forward(input(0), input(1))
                    result = outputNeuron.Forward(in1, in2)
                    err += 0.5 * (labels(index) - result) * (labels(index) - result)
                Next

                Console.WriteLine(err)
                Threading.Thread.Sleep(50)
            End If
        Next
    End Sub

    Public Sub SimpleXorCompareNet()
        Dim nn As New NeuralNetwork(2)
        Dim datas As New List(Of Double())
        Dim labels As New List(Of Double)
        Dim inputNeuron1, inputNeuron2 As TwoInputTanhNeuronGate
        Dim outputNeuron As TwoInputTanhNeuronGate
        Dim rnd As New Random(23)
        Dim index As Integer
        Dim input As Double()
        Dim result, resultNN, in1, in2, err, lr As Double

        lr = 0.075

        Dim il As InputLayer = nn.AddInputLayer(2)
        Dim th = nn.AddTanhLayer()
        Dim nl As NeuronLayer = CType(nn.AddNeuronLayer(1), NeuronLayer)
        Dim tf As TanhLayer = CType(nn.AddFinalLayer(1, ILayer.LayerTypeE.Tanh), TanhLayer)

        inputNeuron1 = New TwoInputTanhNeuronGate
        inputNeuron2 = New TwoInputTanhNeuronGate
        outputNeuron = New TwoInputTanhNeuronGate

        inputNeuron1.Ax.Weight = -1.0
        inputNeuron1.By.Weight = -0.6
        il.Neurons(0).Weights(0) = -1.0
        il.Neurons(0).Weights(1) = -0.6

        inputNeuron2.Ax.Weight = -0.5
        inputNeuron2.By.Weight = 0.7
        il.Neurons(1).Weights(0) = -0.5
        il.Neurons(1).Weights(1) = 0.7

        outputNeuron.Ax.Weight = 0.3
        outputNeuron.By.Weight = 0.5
        nl.Neurons(0).Weights(0) = 0.3
        nl.Neurons(0).Weights(1) = 0.5

        datas.Add({0, 0}) : labels.Add(-0.7)
        datas.Add({1, 1}) : labels.Add(-0.7)
        datas.Add({1, 0}) : labels.Add(0.7)
        datas.Add({0, 1}) : labels.Add(0.7)

        For epoch As Integer = 1 To 5000000
            index = rnd.Next(datas.Count)
            input = datas(index)

            inputNeuron1.Clear()
            inputNeuron2.Clear()
            outputNeuron.Clear()
            nn.Clear()

            in1 = inputNeuron1.Forward(input(0), input(1))
            in2 = inputNeuron2.Forward(input(0), input(1))
            result = outputNeuron.Forward(in1, in2)
            resultNN = nn.Forward(input)(0)

            If Math.Round(result, 5) <> Math.Round(resultNN, 5) Then
                Dim stopHere = 1
            End If

            err = labels(index) - result
            outputNeuron.Backward(err)
            inputNeuron1.Backward(outputNeuron.GradientX)
            inputNeuron2.Backward(outputNeuron.GradientY)

            err = labels(index) - resultNN
            nn.Backward({err})
            nn.UpdateWeights(lr)

            outputNeuron.Update(lr)
            inputNeuron1.Update(lr)
            inputNeuron2.Update(lr)


            'Assert.AreEqual(outputNeuron.Activation.GradientX.ToString, tf.Gradients(0).ToString)

            'Assert.AreEqual(outputNeuron.Ax.Weight.ToString, nl.Neurons(0).Weights(0).ToString)
            'Assert.AreEqual(outputNeuron.By.Weight.ToString, nl.Neurons(0).Weights(1).ToString)
            'Assert.AreEqual(outputNeuron.AxByC.Bias.ToString, nl.Neurons(0).Weights(2).ToString)

            'Assert.AreEqual(outputNeuron.Ax.GradientWeight.ToString, nl.Neurons(0).WeightGradients(0).ToString)
            'Assert.AreEqual(outputNeuron.By.GradientWeight.ToString, nl.Neurons(0).WeightGradients(1).ToString)
            'Assert.AreEqual(outputNeuron.AxByC.GradientBias.ToString, nl.Neurons(0).WeightGradients(2).ToString)

            'Assert.AreEqual(outputNeuron.Ax.GradientX.ToString, nl.Neurons(0).InputGradients(0).ToString)
            'Assert.AreEqual(outputNeuron.By.GradientX.ToString, nl.Neurons(0).InputGradients(1).ToString)



            'Assert.AreEqual(inputNeuron1.Ax.GradientWeight.ToString, il.Neurons(0).WeightGradients(0).ToString)
            'Assert.AreEqual(inputNeuron1.By.GradientWeight.ToString, il.Neurons(0).WeightGradients(1).ToString)
            'Assert.AreEqual(inputNeuron1.AxByC.GradientBias.ToString, il.Neurons(0).WeightGradients(2).ToString)

            'Assert.AreEqual(inputNeuron1.Ax.Weight.ToString, il.Neurons(0).Weights(0).ToString)
            'Assert.AreEqual(inputNeuron1.By.Weight.ToString, il.Neurons(0).Weights(1).ToString)
            'Assert.AreEqual(inputNeuron1.AxByC.Bias.ToString, il.Neurons(0).Weights(2).ToString)


            'Assert.AreEqual(inputNeuron2.Ax.GradientWeight.ToString, il.Neurons(1).WeightGradients(0).ToString)
            'Assert.AreEqual(inputNeuron2.By.GradientWeight.ToString, il.Neurons(1).WeightGradients(1).ToString)
            'Assert.AreEqual(inputNeuron2.AxByC.GradientBias.ToString, il.Neurons(1).WeightGradients(2).ToString)

            'Assert.AreEqual(inputNeuron2.Ax.Weight.ToString, il.Neurons(1).Weights(0).ToString)
            'Assert.AreEqual(inputNeuron2.By.Weight.ToString, il.Neurons(1).Weights(1).ToString)
            'Assert.AreEqual(inputNeuron2.AxByC.Bias.ToString, il.Neurons(1).Weights(2).ToString)

            If epoch Mod 100 = 0 Then
                Dim stopHere = 1
                err = 0.0

                For Each input In datas
                    index = datas.IndexOf(input)
                    in1 = inputNeuron1.Forward(input(0), input(1))
                    in2 = inputNeuron2.Forward(input(0), input(1))
                    result = outputNeuron.Forward(in1, in2)
                    err += 0.5 * (labels(index) - result) * (labels(index) - result)
                Next

                err = 0.0


                For Each input In datas
                    index = datas.IndexOf(input)
                    result = nn.Forward(input)(0)
                    err += 0.5 * (labels(index) - result) * (labels(index) - result)
                Next

                Console.WriteLine(err)
                Threading.Thread.Sleep(50)
            End If
        Next
    End Sub

    Public Sub NetworkTestXor()
        Dim nn As New NeuralNetwork(2)
        Dim rnd As New Random(23)
        Dim datas As New List(Of Double())
        Dim labels As New List(Of Double)
        Dim index As Integer
        Dim current, nnResult As Double()
        Dim result As Double
        Dim err As Double

        datas.Add({0, 0}) : labels.Add(-0.7)
        datas.Add({0, 1}) : labels.Add(0.7)
        datas.Add({1, 0}) : labels.Add(0.7)
        datas.Add({1, 1}) : labels.Add(-0.7)

        Dim il As InputLayer = nn.AddInputLayer(2)
        nn.AddTanhLayer()
        Dim nl As NeuronLayer = CType(nn.AddNeuronLayer(1), NeuronLayer)
        nn.AddFinalLayer(1, ILayer.LayerTypeE.Tanh)
        nn.Randomize(rnd)
        Console.WriteLine()

        il.Neurons(0).Weights(0) = -1.0
        il.Neurons(0).Weights(0) = -0.6

        il.Neurons(1).Weights(0) = -0.5
        il.Neurons(1).Weights(0) = 0.7

        nl.Neurons(0).Weights(0) = 0.3
        nl.Neurons(0).Weights(0) = 0.5

        For i As Integer = 1 To 4000000
            index = rnd.Next(datas.Count)
            current = datas(index)

            nn.Clear()
            nnResult = nn.Forward(current)
            result = nnResult(0)
            err = labels(index) - result


            nn.Backward({err})
            nn.UpdateWeights(0.07)

            If i Mod 100 = 0 Then
                err = 0

                For n As Integer = 0 To datas.Count - 1
                    current = datas(n)
                    nnResult = nn.Forward(current)
                    result = nnResult(0)
                    err += 0.5 * (labels(index) - result) * (labels(index) - result)
                Next

                Console.WriteLine(err)
            End If
        Next
    End Sub

    Sub Main()
        Dim invest As New InvestMonthlyStrategy
        Dim momentum As New MomentumStrategy
        Dim metac As StockMetaDataCollection
        Dim results As New List(Of StrategyResult)
        Dim loader As New Downloader
        Dim sd As String
        Dim s As Stock
        NetworkTestSnp()
        SimpleXorCompareNet()
        NetworkTestXor()
        SimpleXor()
        ' ThreeToOne()
        NetworkTestSnp()
        NetworkTest()
        TestGates()
        NetworkTestXor()
        ThreeToOneLayer()
        CreateAllDirectories()
        Snp()
        '     Dim snp = loader.DownloadStooqData("^spx")

        metac = StockMetaDataCollection.ReadFromFile("german.json")

        Dim allStocks As New List(Of Stock)

        For Each md As StockMetaData In metac
            s = Stock.ReadStooqFromMetaData(md)
            allStocks.Add(s)
            results.Add(invest.ReinvestDividends(s, 10000, 1200))
            '   sd = loader.DownloadStooqData(md)
            '  WriteAllText(IO.Path.Combine(GetStooqDirectory(), md.DataFileName), sd)
        Next

        Dim daimler = Stock.ReadFromMetaData("daimler.json")
        Dim basf = Stock.ReadFromMetaData("basf.json")
        Dim bmw As Stock = Stock.ReadFromMetaData("bmw.json")
        Dim vw As Stock = Stock.ReadFromMetaData("Volkswagen VZ.json")

        Dim resultDaimler = invest.ReinvestDividends(daimler, 10000, 1200)
        Dim resultBasf = invest.ReinvestDividends(basf, 10000, 1200)
        Dim resultBmw = invest.ReinvestDividends(bmw, 10000, 1200)
        Dim resultVw = invest.ReinvestDividends(vw, 10000, 1200)


        'Dim momrs = momentum.InvestMonthlySelectMaximumMomentum({daimler, basf, bmw}, 10000, 1200)
        'Dim momrsev = momentum.InvestMonthly({daimler, basf, bmw}, 10000, 1200)

        Dim kp As New List(Of Object)
        Dim investedEvenly, investedMax, investedMin As Double
        Dim divinvest As New DividendStrategy

        '  momentum.StartDate = DateSerial(2008, 1, 1)

        '  allStocks = {daimler, basf, bmw, vw}.ToList
        Dim addedStocks As New List(Of String)
        Dim dividendsResults As New List(Of IEnumerable(Of StrategyResult))

        Dim startDate As Date = DateSerial(2000, 1, 1)
        Dim investmentPerMonth As Double = 600
        Dim resultSets As New Dictionary(Of String, IEnumerable(Of StrategyResult))

        divinvest.StartDate = startDate
        momentum.StartDate = startDate

        For i As Integer = 0 To 780 Step 13
            momentum.StepSize = i
            divinvest.StepSize = i
            resultSets.Clear()


            Dim momrseva = momentum.InvestMonthlyReinvestDividends(allStocks, 10000, investmentPerMonth)
            Dim evenlyInvestment = momrseva.Select(Function(sr) sr.CurrentInvestmentValue).Sum

            resultSets("h") = momentum.InvestMonthlyReinvestDividendsSelectMaximumMomentum(allStocks, 10000, investmentPerMonth, 1)
            resultSets("l") = momentum.InvestMonthlyReinvestDividendsSelectMinimumMomentum(allStocks, 10000, investmentPerMonth, 1)

            For n As Integer = 1 To 3 Step 1
                '   resultSets("h" & n) = momentum.InvestMonthlyReinvestDividendsSelectMaximumMomentum(allStocks, 10000, investmentPerMonth, n)
                '   resultSets("l" & n) = momentum.InvestMonthlyReinvestDividendsSelectMinimumMomentum(allStocks, 10000, investmentPerMonth, n)
                resultSets("dh" & n) = divinvest.InvestMonthlyReinvestDividendsSelectMaximumDividend(allStocks, 10000, investmentPerMonth, n)
                resultSets("dl" & n) = divinvest.InvestMonthlyReinvestDividendsSelectMinimumDividend(allStocks, 10000, investmentPerMonth, n)
            Next


            Console.Write("e {0:n}", evenlyInvestment)

            For Each kv In resultSets
                Dim value = kv.Value.Select(Function(sr) sr.CurrentInvestmentValue).Sum

                PrettyPrint(kv.Key, evenlyInvestment, value)
            Next

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(", ")

            Console.WriteLine("s {0:n0}", i)
            '  Console.WriteLine(i)

            '  addedStocks.Add(String.Join(", ", maxDividendsResults.Where(Function(r) r.StockAmount > 0).Select(Function(r) r.Stock.MetaData.Name)))
        Next

        'antizyklisch investieren (ginmo)

        Dim ka = results.OrderByDescending(Function(sr) sr.GainedDividends).Select(Function(sr)
                                                                                       Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                   End Function).ToList


        Dim ka2 = results.OrderByDescending(Function(sr) sr.CurrentInvestmentValue).Select(Function(sr)
                                                                                               Return New With {.Name = sr.Stock.MetaData.Name, .Dividends = sr.GainedDividends, .Value = sr.CurrentInvestmentValue}
                                                                                           End Function).ToList
    End Sub

    Public Sub PrettyPrint(caption As String, evenly As Double, comparsionValue As Double)
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Write(", {0} ", caption)

        If comparsionValue > evenly Then
            Console.ForegroundColor = ConsoleColor.Green
            Console.Write("{0:n0}", comparsionValue)

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(" (")
            Console.ForegroundColor = ConsoleColor.Green
            Console.Write("{0:n2}", (comparsionValue / evenly).AsHumanReadablePercent)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(")")

        Else
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("{0:n0}", comparsionValue)

            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(" (")
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("{0:n2}", (comparsionValue / evenly).AsHumanReadablePercent)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.Write(")")
        End If
    End Sub

    Public Function Normalize(json As String) As String
        Return Regex.Replace(json, "(\d+),(\d+)", "$1.$2")
    End Function

    Public Function LoadNormalized(filePath As String) As String
        Dim fileNameWithoutExtension As String = IO.Path.GetFileNameWithoutExtension(filePath)
        Dim normalizedFileName As String = fileNameWithoutExtension & "_normalized" & IO.Path.GetExtension(filePath)
        Dim directoy As String = IO.Path.GetDirectoryName(filePath)
        Dim normalizedFilePath As String = IO.Path.Combine(directoy, normalizedFileName)
        Dim content As String

        If IO.File.Exists(normalizedFilePath) Then
            Return IO.File.ReadAllText(normalizedFilePath)
        End If

        content = IO.File.ReadAllText(filePath)
        content = Normalize(content)
        IO.File.WriteAllText(normalizedFilePath, content)

        Return content
    End Function

    Public Function LoadStocks(filePath As String) As List(Of StockFundamentals)
        Dim r As New List(Of StockFundamentals)
        Dim json As String = LoadNormalized(filePath)
        Dim semiRaw As Dictionary(Of String, StockFundamentals) = JsonConvert.DeserializeObject(Of Dictionary(Of String, StockFundamentals))(json)

        For Each kv In semiRaw
            kv.Value.Name = kv.Key
            r.Add(kv.Value)
        Next

        Return r
    End Function

    Public Function LoadDax() As List(Of StockFundamentals)
        Return LoadStocks(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "Data", "dax formated.json"))
    End Function

End Module
