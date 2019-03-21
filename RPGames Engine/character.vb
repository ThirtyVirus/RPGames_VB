Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class character
    'GRAPHICS
    Public Property camera As New Camera(New Viewport())
    Public Property skin As Texture2D : Public Property texture As String
    Public Property visible As Boolean : Public Property effects As New List(Of gameEffect)

    'STANDARD PROPERTIES
    Public Property name As String : Public Property inventory As New Inventory
    Public Property health As Integer : Public Property maxHealth As Integer
    Public Property gamestate As String : Public Property prevGamestate As String

    'WORLD PLACEMENT
    Public Property area As area : Public Property previousAreas As New List(Of String)
    Public Property location As Vector2 : Public Property previousLocations As New List(Of Vector2)
    Public Property direction As Integer : Public Property directions As New List(Of Integer)
    Public Property tileStandingOn As tileType : Public Property tileJustStoodOn As tileType

    'MOVEMENT
    Public Property stepping As Boolean : Public Property stepProgress As Integer
    Public Property speed As Single : Public Property speedChange As Boolean : Public Property forcedSpeed As Single
    Public Property animationprogress As Integer : Public Property animOffset As Integer : Public Property r As Boolean
    Public Property path As List(Of Vector2) : Public Property pathProgress As Integer
    Public Property noClip As Boolean : Public Property justUsedDoor As Boolean

    'PATHFINDING
    Public Shared startNode As Node : Public Shared endNode As Node
    Public Shared nodes As List(Of Node)

    'HOST LINKING
    Public Property host As String
    Public Property hostPlayer As player
    Public Property hostNPC As npc

    Public Sub New()

    End Sub
    Public Sub New(s As Texture2D, t As String, n As String, i As Inventory, h As Integer, mh As Integer, g As String)
        skin = s : texture = t

        name = n : inventory = i
        health = h : maxHealth = mh
        gamestate = g

        visible = True : forcedSpeed = 0
        tileStandingOn = terrainManager.tileTypes(0) : tileJustStoodOn = terrainManager.tileTypes(0)
        speed = tileStandingOn.minSpeed
    End Sub

    'MOVEMENT
    Public Sub manageMovement()
        If noClip Or gamestate = "mapEdit" Then tileStandingOn = terrainManager.tileTypes(0) Else tileStandingOn = terrainManager.tileTypes(area.tileMap(area.size.X * Int(location.Y) + Int(location.X)))
        If gamestate = "followPath" Then followPath()

        'CHANGES SPEED BETWEEN MINIMUM AND MAXIMUM
        If speedChange = True Then
            If speed = tileStandingOn.minSpeed Then speed = tileStandingOn.maxSpeed Else speed = tileStandingOn.minSpeed
            speedChange = False
        End If
        If speed = tileJustStoodOn.minSpeed And speed <> tileStandingOn.minSpeed Then speed = tileStandingOn.minSpeed
        If speed = tileJustStoodOn.maxSpeed And speed <> tileStandingOn.maxSpeed Then speed = tileStandingOn.maxSpeed

        'FORCES SPEED IF COMMAND IS USED
        If forcedSpeed <> 0 Then speed = forcedSpeed

        'MAKES THE STEP
        If stepping Then
            takeStep()
        Else
            speed = tileStandingOn.minSpeed
        End If

        'SPEED-BASED ANIMATION CHANGES (Should also have some based on tile being stepped on)
        Select Case speed
            Case 1, 1.4 : animOffset = 6
            Case 2.5 : animOffset = 3

            Case Else : animOffset = 0
        End Select
    End Sub
    Public Sub takeStep()
        'STARTS SHOWING NEXT SPRITE
        If speed = tileStandingOn.maxSpeed And stepProgress = 4 Then stepProgress = 0
        If stepProgress = 6 Then stepProgress = 0

        'REMOVES NEGATED DIRECTIONS
        If directions.Count > 1 Then
            If directions.Contains(1) And directions.Contains(2) Then directions.Remove(1) : directions.Remove(2)
            If directions.Contains(3) And directions.Contains(4) Then directions.Remove(3) : directions.Remove(4)
        End If

        'SETS VISUAL DIRECTION
        If directions.Count > 0 Then direction = directions(0)

        Dim finalSpeed As Single = speed / 10
        Dim secondFinalSpeed As Single = speed / 20

        'REMOVES BLOCKED DIRECTIONS
        'Primary Direction
        If directions.Count > 0 Then
            If directions(0) = 1 Then
                If detectHit(New Vector2(location.X, location.Y - finalSpeed)) Then directions.Remove(1)
            ElseIf directions(0) = 2 Then
                If detectHit(New Vector2(location.X, location.Y + finalSpeed)) Then directions.Remove(2)
            ElseIf directions(0) = 3 Then
                If detectHit(New Vector2(location.X - finalSpeed, location.Y)) Then directions.Remove(3)
            ElseIf directions(0) = 4 Then
                If detectHit(New Vector2(location.X + finalSpeed, location.Y)) Then directions.Remove(4)
            End If
        End If

        'Secondary Direction
        If directions.Count > 1 Then
            If directions(1) = 1 Then
                If detectHit(New Vector2(location.X, location.Y - secondFinalSpeed)) Then directions.Remove(1)
            ElseIf directions(1) = 2 Then
                If detectHit(New Vector2(location.X, location.Y + secondFinalSpeed)) Then directions.Remove(2)
            ElseIf directions(1) = 3 Then
                If detectHit(New Vector2(location.X - secondFinalSpeed, location.Y)) Then directions.Remove(3)
            ElseIf directions(1) = 4 Then
                If detectHit(New Vector2(location.X + secondFinalSpeed, location.Y)) Then directions.Remove(4)
            End If
        End If

        'STOPS WALK CYCLE IF NO DIRECTIONS LEFT
        If directions.Count = 0 Then animationprogress = 1 : speed = tileStandingOn.minSpeed

        'SLOWS DOWN IF GOING DIAGONALLY
        If directions.Count > 1 Then finalSpeed /= 1.2

        'MOVES CHARACTER
        'Primary Direction
        If directions.Count > 0 Then
            If directions(0) = 1 Then location = New Vector2(location.X, location.Y - finalSpeed)
            If directions(0) = 2 Then location = New Vector2(location.X, location.Y + finalSpeed)
            If directions(0) = 3 Then location = New Vector2(location.X - finalSpeed, location.Y)
            If directions(0) = 4 Then location = New Vector2(location.X + finalSpeed, location.Y)
        End If

        If directions.Count > 1 Then
            If directions(1) = 1 Then location = New Vector2(location.X, location.Y - secondFinalSpeed)
            If directions(1) = 2 Then location = New Vector2(location.X, location.Y + secondFinalSpeed)
            If directions(1) = 3 Then location = New Vector2(location.X - secondFinalSpeed, location.Y)
            If directions(1) = 4 Then location = New Vector2(location.X + secondFinalSpeed, location.Y)
        End If

        'CHANGES SPRITE
        If stepProgress = 0 And directions.Count > 0 Then
            If animationprogress <> 1 Then
                If animationprogress < 1 Then
                    r = True
                    If host = "player" Then gameDebug.playSound(tileStandingOn.stepSound,, Game.soundVolume / 5,, tileStandingOn.stepSoundAmount)
                Else
                    r = False
                    If host = "player" Then gameDebug.playSound(tileStandingOn.stepSound,, Game.soundVolume / 5,, tileStandingOn.stepSoundAmount)
                End If
            End If

            If r = True Then animationprogress += 1 Else animationprogress -= 1
        End If

        stepProgress += 1 : stepping = False
    End Sub
    Public Function detectHit(target As Vector2) As Boolean
        If noClip = True Then Return False

        'PLAYER DETECTION
        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).c.area.name = area.name And Game.players(counter).c.name <> name And Game.players(counter).c.gamestate <> "mapEdit" Then
                If target.X > Game.players(counter).c.location.X - 2 And target.X < Game.players(counter).c.location.X + 2 And target.Y > Game.players(counter).c.location.Y - 1 And target.Y < Game.players(counter).c.location.Y + 1 Then Return True
            End If
        Next

        'NPC DETECTION
        For counter As Integer = 0 To area.npcs.Count - 1
            If area.npcs(counter).c.name <> name Then
                If target.X > area.npcs(counter).c.location.X - 2 And target.X < area.npcs(counter).c.location.X + 2 And target.Y > area.npcs(counter).c.location.Y - 1 And target.Y < area.npcs(counter).c.location.Y + 1 Then Return True
            End If
        Next

        'OLD OBJECT DETECTION
        Dim testPoint As Vector2 = New Vector2(Math.Truncate(target.X) + 1, Math.Truncate(target.Y - 0.5) + 1) 'the  - 0.5 is for visual purposes, so that the 2D character looks like he has depth 
        If area.size.X * testPoint.Y + testPoint.X - 1 >= 0 And area.size.X * testPoint.Y + testPoint.X + 1 < area.hitDetect.Count Then
            If (host = "npc" And area.hitDetect(area.size.X * testPoint.Y + testPoint.X - 1).Y <> 1) Or (host = "player" And area.hitDetect(area.size.X * testPoint.Y + testPoint.X - 1).Y <> 2) Then
                Select Case area.hitDetect(area.size.X * testPoint.Y + testPoint.X - 1).X
                    Case 1 : Return True
                    Case 2 : If justUsedDoor = False Then Return detectDoor(New Vector2(testPoint.X - 1, testPoint.Y))
                End Select
            End If
            If (host = "npc" And area.hitDetect(area.size.X * testPoint.Y + testPoint.X).Y <> 1) Or (host = "player" And area.hitDetect(area.size.X * testPoint.Y + testPoint.X).Y <> 2) Then
                Select Case area.hitDetect(area.size.X * testPoint.Y + testPoint.X).X
                    Case 1 : Return True
                    Case 2 : If justUsedDoor = False Then Return detectDoor(New Vector2(testPoint.X, testPoint.Y))
                End Select
            End If
        End If

        'NEW OBJECT DETECTION
        'Dim playerBounds As Rectangle = New Rectangle(target.X - 1, target.Y - 0.25, 3, 0.5)

        'For Each obj As terrainObject In area.objects
        '    Dim bounds As Rectangle = New Rectangle(obj.location.X, obj.location.Y, obj.type.size.X, obj.type.size.Y)
        '    If bounds.Intersects(playerBounds) Then
        '        Return True
        '        'gameDebug.playSound("sfx/interact/pop", "sound", Game.soundVolume)
        '    End If
        'Next

        'ITEM DETECTION
        Dim count As Integer
        Do While (count <> area.items.Count)
            For count = 0 To area.items.Count - 1
                If target.X > area.items(count).location.X - 1 And target.X < area.items(count).location.X + 1 And target.Y > area.items(count).location.Y - 1 And target.Y < area.items(count).location.Y + 1 Then
                    inventory.add(area.items(count)) : gameDebug.playSound("sfx/interact/pop", "sound", Game.soundVolume)
                    area.removeItem(area.items(count).type.name, area.items(count).location) : Exit For
                End If
            Next
        Loop

        'MAP BOUNDARIES DETECTION
        If target.X < 1 Or target.X > area.size.X - 2 Or target.Y < 0 Or target.Y > area.size.Y - 1 Then Return True

        Return False
    End Function
    Public Function detectDoor(l As Vector2) As Boolean
        For counter As Integer = 0 To area.objects.Count - 1

            'TESTS IF OBJECT IS A DOOR
            If area.objects(counter).type.type = 3 Then

                Dim x As Integer = 0 : Dim y As Integer = 0
                For counter2 As Integer = 0 To area.objects(counter).type.collisionMap.Count - 1
                    Dim newLoc As Vector2 = New Vector2(area.objects(counter).location.X + x, area.objects(counter).location.Y + y)

                    If area.objects(counter).type.collisionMap(counter2).X = 2 And newLoc = l Then
                        If area.objects(counter).condition = "locked" Or area.objects(counter).destination.area = "" Or area.objects(counter).destination.location = New Vector2 Then
                            If host = "player" Then hostPlayer.GUI.addMessage(New message(New Vector2(camera.viewport.Width / 2 - Game.defaultFont4.MeasureString("Locked").X / 2, camera.viewport.Height / 2), "Locked", Game.defaultFont4, Color.White, False, False, 10, 10))
                            Return True
                        End If
                        If area.objects(counter).condition = "open" Then
                            justUsedDoor = True
                            gameDebug.playSound(area.objects(counter).type.openSound, "sound", Game.soundVolume)
                            Dim dir As Integer = area.objects(counter).destination.direction : If dir = 0 Then dir = direction
                            changeArea("quicksave", area.objects(counter).destination.area, area.objects(counter).destination.location, dir) : Return True
                        End If
                    End If

                    If x < area.objects(counter).type.size.X Then x += 1
                    If x >= area.objects(counter).type.size.X Then x = 0 : y += 1
                Next

            End If
        Next
        Return True
    End Function
    Public Sub followPath()
        If pathProgress >= path.Count Then pathProgress = 0 : gamestate = prevGamestate : Exit Sub

        If path(pathProgress).Y < location.Y Then stepping = True : If directions.Contains(1) = False Then directions.Add(1)
        If path(pathProgress).Y > location.Y Then stepping = True : If directions.Contains(2) = False Then directions.Add(2)
        If path(pathProgress).X < location.X Then stepping = True : If directions.Contains(3) = False Then directions.Add(3)
        If path(pathProgress).X > location.X Then stepping = True : If directions.Contains(4) = False Then directions.Add(4)

        'STOPS JITTERING WHEN GOING IN A STRAIT LINE
        If Math.Abs(path(pathProgress).Y - location.Y) < speed / 10 Then directions.Remove(1) : directions.Remove(2)
        If Math.Abs(path(pathProgress).X - location.X) < speed / 10 Then directions.Remove(3) : directions.Remove(4)

        If Math.Abs(path(pathProgress).X - location.X) < speed And Math.Abs(path(pathProgress).Y - location.Y) < speed Then pathProgress += 1
    End Sub

    Public Sub changeArea(saveName As String, areaName As String, loc As Vector2, dir As Integer)
        If host = "player" Then
            If gamestate = "mapEdit" Then worldEditor.exitEditor(hostPlayer)

            'THIS NEEDS WORK (this causes the lag between areas)
            If saveName = "quickSave" Then FileManager.save("quickSave")
        End If

        'TAKES PREVIOUS AREA INFORMATION
        Dim oldMusic As String : Dim oldGreet As String
        If direction <> 0 Then
            oldMusic = area.music : oldGreet = area.greetMessage
            previousAreas.Add(area.name) : previousLocations.Add(location)
        End If

        'SETS CHARACTER'S NEW AREA, LOCATION, AND DIRECTION
        area = FileManager.loadArea(saveName, areaName)
        location = loc : direction = dir

        If host = "player" Then
            'STARTS AREA AMBIANCE / MUSIC
            If name <> "title" And Game.autoMusicUpdate = True Then updateMusic(oldMusic, area.music, 2)

            'DRAWS AREA GREET MESSAGE
            If oldGreet <> area.greetMessage And camera.viewport.Width <> 0 Then
                If oldGreet <> "none" And oldGreet <> "" Then hostPlayer.GUI.removeMessage(oldGreet.Replace("_", " "))
                Dim greet As String = area.greetMessage.Replace("_", " ")
                If area.greetMessage <> "none" Then hostPlayer.GUI.addMessage(New message(New Vector2(camera.viewport.Width / 2 - (Game.defaultFont4.MeasureString(greet).X) / 2, camera.viewport.Height / 10 - (Game.defaultFont4.MeasureString(greet).Y) / 2), greet, Game.defaultFont4, Color.White, False, False, 100, 3))
            End If
            gamestate = "inGame"
            If name <> "title" Then hostPlayer.GUI.areaName.text = "Area:   " & Replace(area.name, "_", " ")
        End If
    End Sub
    Public Sub updateMusic(oldMusic As String, newMusic As String, fadeRate As Integer, Optional restart As Boolean = False)
        'UPDATES MAIN MUSIC BEING PLAYED
        'players(0) to be replaced with global auto effects
        '100 FADERATE MEANS NO FADE
        If newMusic <> oldMusic Then
            If oldMusic <> "none" Then commandPrompt.simulateCommand(hostPlayer, "/addEffect fadeMusicOut " & fadeRate & " all")
            Dim alreadyPlayed As Boolean = False
            For counter As Integer = 0 To Game.music.Count - 1
                If Game.music(counter).name = newMusic Then
                    alreadyPlayed = True
                    If restart = False Then Game.music(counter).sound.Resume() Else Game.music(counter).sound.Play()
                End If
            Next
            If alreadyPlayed = False Then gameDebug.playSound(newMusic, "music", 0, True)
            commandPrompt.executeCommandLater(hostPlayer, "/addEffect fadeMusicIn " & fadeRate & " " & newMusic, 100)
        End If
    End Sub

    'PATHFINDING
    Public Function findPath(startLoc As Vector2, endLoc As Vector2) As List(Of Vector2)
        nodes = gameDebug.translateCollisionMap(area, endLoc)

        'DEFINES START AND END NODES
        startNode = nodes(startLoc.Y * area.size.X + startLoc.X) : startNode.State = NodeState.Open
        endNode = nodes(endLoc.Y * area.size.X + endLoc.X)

        'GENERATES PATH
        Dim path As New List(Of Vector2)
        If search(area, startNode) Then
            Dim node As Node = endNode
            While IsNothing(node.ParentNode) = False
                path.Add(node.Location)
                node = node.ParentNode
            End While
            path.Reverse()
        End If

        Return path
    End Function
    Private Shared Function search(a As area, currentNode As Node) As Boolean
        currentNode.State = NodeState.Closed
        Dim nextNodes As List(Of Node) = GetAdjacentWalkableNodes(a, currentNode)
        nextNodes.Sort(Function(node1, node2) node1.F.CompareTo(node2.F)) '???

        For Each nextNode As Node In nextNodes
            If nextNode.Location = endNode.Location Then
                Return True
            Else
                If search(a, nextNode) Then Return True
            End If
        Next
        Return False
    End Function
    Private Shared Function GetAdjacentWalkableNodes(a As area, oldNode As Node) As List(Of Node)
        Dim accessableNodes As New List(Of Node)

        'GENRATES LIST OF SURROUNDING NODES
        Dim nextLocations As List(Of Vector2) = New List(Of Vector2) From {
        New Vector2(oldNode.Location.X - 1, oldNode.Location.Y),
        New Vector2(oldNode.Location.X, oldNode.Location.Y + 1),
        New Vector2(oldNode.Location.X + 1, oldNode.Location.Y),
        New Vector2(oldNode.Location.X, oldNode.Location.Y - 1),
        New Vector2(oldNode.Location.X - 1, oldNode.Location.Y - 1),
        New Vector2(oldNode.Location.X - 1, oldNode.Location.Y + 1),
        New Vector2(oldNode.Location.X + 1, oldNode.Location.Y - 1),
        New Vector2(oldNode.Location.X + 1, oldNode.Location.Y + 1)}

        For Each location As Vector2 In nextLocations
            'TESTS FOR AREA BOUNDS
            If location.X < 0 Or location.X >= a.size.X Or location.Y < 0 Or location.Y >= a.size.Y Then Continue For

            'DEFINES CURRENT NODE
            Dim currentNode As Node = nodes(location.Y * a.size.X + location.X)

            'SKIPS NODE IF INACCESSABLE
            If currentNode.IsWalkable = False Then Continue For
            'SKIPS ALREADY-CLOSED NODES
            If currentNode.State = NodeState.Closed Then Continue For

            ' Already-open nodes are only added to the list if their G-value is lower going via this route.
            If currentNode.State = NodeState.Open Then
                Dim traversalCost As Single = Node.GetTraversalCost(currentNode.Location, currentNode.ParentNode.Location)
                Dim gTemp As Single = oldNode.G + traversalCost
                If gTemp < currentNode.G Then
                    currentNode.ParentNode = oldNode
                    accessableNodes.Add(currentNode)
                End If
            Else
                ' If it's untested, set the parent and flag it as 'Open' for consideration
                currentNode.ParentNode = oldNode
                currentNode.State = NodeState.Open
                accessableNodes.Add(currentNode)
            End If

        Next

        Return accessableNodes
    End Function

    'COMBAT
    Public Sub swingSword()
        'DEFINES WHERE THE STRIKE IS
        Dim testpoint As Vector2 = New Vector2(Math.Truncate(location.X) + 1, Math.Truncate(location.Y - 0.5) + 1)
        Select Case direction
            Case 1 : testpoint.Y -= 1
            Case 2 : testpoint.Y += 1
            Case 3 : testpoint.X -= 2
            Case 4 : testpoint.X += 1
        End Select

        'DEBUG
        'area.tileMap(testpoint.Y * area.size.X + testpoint.X) = id

        'SEARCHES FOR TARGETS
        For counter As Integer = 0 To area.npcs.Count - 1
            If testpoint.X > area.npcs(counter).c.location.X - 3 And testpoint.X < area.npcs(counter).c.location.X + 3 And testpoint.Y > area.npcs(counter).c.location.Y - 2 And testpoint.Y < area.npcs(counter).c.location.Y + 2 Then area.npcs(counter).c.health -= 100
        Next

        area.objects.RemoveAll(Function(x) x.type.type = 4 And testpoint.X > x.location.X - 3 And testpoint.X < x.location.X + 3 And testpoint.Y > x.location.Y - 2 And testpoint.Y < x.location.Y + 2)
    End Sub
End Class