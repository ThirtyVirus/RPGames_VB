Imports System.Collections.Generic

Public Class convoNode
    Public Property parent As Object
    Public Property text As String
    Public Property answers As New List(Of String)
    Public Property responses As New List(Of convoNode)

    Sub New()
        text = ""
        answers = New List(Of String)
        responses = New List(Of convoNode)
    End Sub
End Class

Public Class conversation
    Public Property firstTime As Boolean 'Wont be used yet
    Public Property struct As convoNode

    Sub New()
        firstTime = True
    End Sub

End Class

Public Class dialogue
    Public Shared counter As Integer = 1

    Public Shared Function generateNode(parentNode As convoNode, text As String, ByRef counter As Integer) As convoNode

        Dim currentNode As New convoNode
        currentNode.parent = parentNode
        While counter < text.Length

            If text(counter) = "*" Then
                counter += 1
                currentNode.responses.Add(generateNode(currentNode, text, counter))

            ElseIf text(counter) = "<" Then
                counter += 1
                While text(counter) <> "*" And text(counter) <> "<" And text(counter) <> ">" And text(counter) <> "^"
                    currentNode.text = currentNode.text & text(counter)
                    counter += 1
                End While
                counter -= 1

            ElseIf text(counter) = ">" Then
                Dim newAnswer As String = ""
                counter += 1
                While text(counter) <> "*" And text(counter) <> "<" And text(counter) <> ">" And text(counter) <> "^"
                    newAnswer = newAnswer & text(counter)
                    counter += 1
                End While
                currentNode.answers.Add(newAnswer)
                counter -= 1

            ElseIf text(counter) = "^" Then
                counter += 0
                Return currentNode
            End If

            counter += 1
        End While

    End Function
    Public Shared Function generateConversation(text As String)
        Dim convo As conversation = New conversation

        counter = 1
        convo.struct = generateNode(Nothing, text, counter)
        Return convo
    End Function

    Public Shared Sub playConversation(p As player, talk As conversation)

    End Sub

End Class
