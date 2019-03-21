Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class npc
    Public Property c As New character

    Public Property attribute As String 'ex- villager/grunt/boss/gunner/etCharacterHandler...
    Public Property target As character

    'For Merchants ONLY
    Public Property priceMultiplier As Single

    Public Sub New(ch As character, a As String)
        c = ch : c.host = "npc"

        c.animationprogress = 1 : attribute = a
    End Sub

    Public Sub update()
        Select Case c.gamestate
            Case "still" : c.animOffset = 0 : c.animationprogress = 1
            Case "idle" : actIdle()
            Case "hostile" : actHostile()
                'Case "scared" : actScared()
        End Select

        c.manageMovement()

        If c.health <= 0 Then c.gamestate = "dead"
    End Sub

    Public Sub communicate(p As player)
        'MAKES NPC LOOK TO PLAYER WHEN SPOKEN TO
        If p.c.direction = 1 Then c.direction = 2
        If p.c.direction = 2 Then c.direction = 1
        If p.c.direction = 3 Then c.direction = 4
        If p.c.direction = 4 Then c.direction = 3

        'HANDLES GAMESTATE
        p.interact = c : c.prevGamestate = c.gamestate : c.gamestate = "speaking"

        'LOADS IN DIALOGUE
        Dim dialogue As String : Dim location As String = Game.directory & "saves/" & Game.saveName & "/NPCs/" & c.name & "_dialogue.txt"
        If My.Computer.FileSystem.FileExists(location) Then dialogue = My.Computer.FileSystem.ReadAllText(location) Else dialogue = "..."

        'STATEMENT IS CHOSEN FROM ALL DIALOGUE
        Dim statement As String = dialogue
        p.recieveMessage(statement)

        'INITIATES TRADING (should make this happen after dialogue option)
        If attribute = "merchant" Then p.side = 1 : p.c.gamestate = "trading" : Exit Sub
    End Sub

    'AI for action types
    Public Sub actIdle()
        Dim randomInt As Integer = CInt(Math.Ceiling(Rnd() * 500))

        'CHANGING DIRECTION
        If randomInt = 1 Or (c.directions.Contains(1) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(1) = False Then c.directions.Add(1)
        If randomInt = 2 Or (c.directions.Contains(2) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(2) = False Then c.directions.Add(2)
        If randomInt = 3 Or (c.directions.Contains(3) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(3) = False Then c.directions.Add(3)
        If randomInt = 4 Or (c.directions.Contains(4) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(4) = False Then c.directions.Add(4)

    End Sub
    Public Sub actHostile()
        Dim distance As Vector2 = New Vector2(Math.Abs(c.location.X - target.location.X), Math.Abs(c.location.Y - target.location.Y))

        If distance.X > distance.Y Or c.location.X = target.location.X Then
            If c.location.Y < target.location.Y Or (c.directions.Contains(2) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(2) = False Then c.directions.Add(2)
            If c.location.Y > target.location.Y Or (c.directions.Contains(1) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(1) = False Then c.directions.Add(1)
        End If
        If distance.Y > distance.X Or c.location.Y = target.location.Y Then
            If c.location.X < target.location.X Or (c.directions.Contains(4) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(4) = False Then c.directions.Add(4)
            If c.location.X > target.location.X Or (c.directions.Contains(3) And c.stepProgress > 0) Then c.stepping = True : If c.directions.Contains(3) = False Then c.directions.Add(3)
        End If

    End Sub
    'Public Sub actScared()
    '    If stepping Then takeStep() : Exit Sub
    '    speed = 6 : animOffset = 3

    '    Dim distance As Vector2 = New Vector2(Math.Abs(location.X - target.location.X), Math.Abs(location.Y - target.location.Y))

    '    If distance.X > distance.Y Or location.X = p.c.location.X Then
    '        If location.Y < p.c.location.Y And detectHit() <> 1 Then direction = 1
    '        If location.Y > p.c.location.Y And detectHit() <> 2 Then direction = 2
    '    End If
    '    If distance.Y > distance.X Or location.Y = p.c.location.Y Then
    '        If location.X < p.c.location.X And detectHit() <> 3 Then direction = 3
    '        If location.X > p.c.location.X And detectHit() <> 4 Then direction = 4
    '    End If
    '    If detectHit() <> direction Then stepping = True : takeStep()
    'End Sub
End Class