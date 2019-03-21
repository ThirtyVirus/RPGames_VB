Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class copiedRegion
    Public Property size As Vector2
    Public Property tileMap As New List(Of Integer)
    Public Property objects As New List(Of terrainObject)
    Public Property items As New List(Of item)

    Public Sub New(s As Vector2, Optional t As List(Of Integer) = Nothing, Optional o As List(Of terrainObject) = Nothing, Optional i As List(Of item) = Nothing)
        If IsNothing(t) Then t = New List(Of Integer) : For counter As Integer = 0 To s.X * s.Y - 1 : t.Add(0) : Next
        If IsNothing(o) Then o = New List(Of terrainObject)
        If IsNothing(i) Then i = New List(Of item)
        size = s : tileMap = t : objects = o : items = i
    End Sub
End Class

Public Class worldEditor
    Public Property mode As String : Public Property id As Single : Public Property subType As Integer
    Public Property brush As Vector2 : Public Property grid As Vector4 'sizeX, sizeY, offsetX, offsetY 

    Public Property copiedPos As worldPosition : Public Property copiedPos2 As worldPosition
    'Public Property copyRegion As Vector4 : Public Property copiedRegion As copiedRegion

    Public Property subject As Object : Public Property mp As Vector2

    Public Shared cursor As Vector2
    Public Shared Sub getInput(ByRef p As player)
        If p.controlMethod = "keyboard/mouse" Then
            cursor = New Vector2(CInt((p.c.camera.location.X + Game.mouseState.X) / Game.tilesize - 1), CInt((p.c.camera.location.Y + Game.mouseState.Y) / Game.tilesize - 1))
            cursor = New Vector2(cursor.X - cursor.X Mod p.grid.X + p.grid.Z, cursor.Y - cursor.Y Mod p.grid.Y + p.grid.W)
        End If
        If p.controls.levelEditor.pressed(p) Then cursor = p.c.location

        'CAMERA MOVEMENT
        If p.controls.up.pressed(p) Then p.c.camera.location = New Vector2(p.c.camera.location.X, p.c.camera.location.Y - Game.tilesize)
        If p.controls.down.pressed(p) Then p.c.camera.location = New Vector2(p.c.camera.location.X, p.c.camera.location.Y + Game.tilesize)
        If p.controls.left.pressed(p) Then p.c.camera.location = New Vector2(p.c.camera.location.X - Game.tilesize, p.c.camera.location.Y)
        If p.controls.right.pressed(p) Then p.c.camera.location = New Vector2(p.c.camera.location.X + Game.tilesize, p.c.camera.location.Y)

        'GAMEPAD CURSOR MOVEMENT
        If p.controlMethod.Split(" ")(0) = "GamePad" Then
            If p.controls.cursorUp.pressed(p) Then cursor = New Vector2(cursor.X, cursor.Y - 1)
            If p.controls.cursorDown.pressed(p) Then cursor = New Vector2(cursor.X, cursor.Y + 1)
            If p.controls.cursorLeft.pressed(p) Then cursor = New Vector2(cursor.X - 1, cursor.Y)
            If p.controls.cursorRight.pressed(p) Then cursor = New Vector2(cursor.X + 1, cursor.Y)
        End If

        'KEYBOARD AND GAMEPAD CONTROLS
        If p.controls.action1.pressed(p) Then holdLeftClick(p, cursor)
        If p.controls.action1.justPressed(p) Then pressLeftClick(p, cursor)
        If p.controls.action2.pressed(p) Then holdRightClick(p, cursor)
        If p.controls.action2.justPressed(p) Then pressRightClick(p, cursor)
        If p.controls.action3.pressed(p) Then holdMiddleClick(p, cursor)
        If p.controls.action3.justPressed(p) Then pressMiddleClick(p, cursor)

        'MOUSE CONTROLS
        If p.controlMethod = "keyboard/mouse" Then
            If p.mouseState.LeftButton = ButtonState.Pressed Then holdLeftClick(p, cursor)
            If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then pressLeftClick(p, cursor)
            If p.mouseState.RightButton = ButtonState.Pressed Then holdRightClick(p, cursor)
            If p.mouseState.RightButton = ButtonState.Pressed And p.oldMouseState.RightButton <> ButtonState.Pressed Then pressRightClick(p, cursor)
            If p.mouseState.MiddleButton = ButtonState.Pressed Then holdMiddleClick(p, cursor)
            If p.mouseState.MiddleButton = ButtonState.Pressed And p.oldMouseState.MiddleButton <> ButtonState.Pressed Then pressMiddleClick(p, cursor)
        End If

        'LEVEL EDITOR PROCESSING
        p.c.visible = False
        If p.mode = "duplicate" Then
            If p.subType = 0 Then
                p.copiedPos = New worldPosition(p.c.area.name, New Vector2(cursor.X, cursor.Y), 0)
                p.copyRegion = New Vector4(cursor.X, cursor.Y, 0, 0)
            Else
                Dim startPos As Vector2 : Dim endPos As Vector2
                If p.copiedPos.location.Y < cursor.Y Then
                    startPos.Y = p.copiedPos.location.Y : endPos.Y = cursor.Y
                Else : startPos.Y = cursor.Y : endPos.Y = p.copiedPos.location.Y
                End If
                If p.copiedPos.location.X < cursor.X Then
                    startPos.X = p.copiedPos.location.X : endPos.X = cursor.X
                Else : startPos.X = cursor.X : endPos.X = p.copiedPos.location.X
                End If

                p.copyRegion = New Vector4(startPos.X, startPos.Y, Math.Abs(endPos.X - startPos.X), Math.Abs(endPos.Y - startPos.Y))
            End If
        End If
    End Sub
    Public Shared Sub holdLeftClick(ByRef p As player, mp As Vector2)
        Dim x As Integer = 0 : Dim y As Integer = 0 : Dim newLoc As New Vector2

        For counter As Integer = 0 To (p.brush.X * p.brush.Y) - 1
            newLoc = New Vector2(mp.X + x, mp.Y + y)
            If newLoc.X >= 0 And newLoc.Y >= 0 And newLoc.X < p.c.area.size.X And newLoc.Y < p.c.area.size.Y Then

                Select Case p.mode
                    Case "tile"
                        For counter2 As Integer = 0 To terrainManager.tileTypes.Count - 1
                            If terrainManager.tileTypes(counter2).id = p.id Then p.c.area.tileMap(p.c.area.size.X * newLoc.Y + newLoc.X) = p.id
                        Next
                    Case "fill"
                        For counter2 As Integer = 0 To terrainManager.tileTypes.Count - 1
                            If terrainManager.tileTypes(counter2).id = p.id Then fillTool(p, newLoc)
                        Next
                    Case "erase" : eraser(p, newLoc)

                    Case "item"
                        Dim placeItem As Boolean = True
                        For counter2 As Integer = 0 To p.c.area.items.Count - 1
                            If p.c.area.items(counter2).location = newLoc And p.c.area.items(counter2).type.id = p.id Then placeItem = False
                        Next : If placeItem Then p.c.area.place(New item(terrainManager.getItemType(p.id), 1, newLoc))

                        If p.brush <> New Vector2(1, 1) Then Exit Select

                    Case "object"
                        'WILL VERY DEPENDING ON AMOUNT OF ARGUMENTS
                        Dim objectType As terrainObjectType = terrainManager.getObjectType(p.id)

                        Select Case objectType.type
                            Case 0 : p.c.area.place(New terrainObject(objectType, newLoc))
                            Case 1 : p.c.area.place(New terrainObject(objectType, newLoc))
                            Case 2 : p.c.area.place(New terrainObject(objectType, newLoc, "open", objectType.name, New Inventory))
                            Case 3 : p.c.area.place(New terrainObject(objectType, newLoc, "open", New worldPosition))
                        End Select
                    Case "npc"
                        p.cmd = New console(New Vector2(Game.tilesize, p.c.camera.viewport.Height - 30), Game.defaultFont) : p.typing = True
                        p.cmd.text = "/createNPC " & p.c.area.name & " " & mp.X & " " & mp.Y & " " & p.subType + 1 & " skinName" & " name" : p.cmd.inputPos = p.cmd.text.Count
                End Select
            End If
            If x < p.brush.X Then x += 1
            If x >= p.brush.X Then x = 0 : y += 1
        Next
    End Sub
    Public Shared Sub pressLeftClick(ByRef p As player, mp As Vector2)
        Select Case p.mode
            Case "duplicate" : If p.subType = 0 Then placeRegion(p, mp)
            Case "select"
                For counter As Integer = 0 To p.c.area.objects.Count - 1
                    If mp.X >= p.c.area.objects(counter).location.X And mp.Y >= p.c.area.objects(counter).location.Y And mp.X < p.c.area.objects(counter).location.X + p.c.area.objects(counter).type.size.X And mp.Y < p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y Then
                        p.subject = p.c.area.objects(counter) : p.GUI.addChatMessage(p, "Subject Selected! (Subject type:  " & p.c.area.objects(counter).type.name & ")") : Exit For
                    End If
                Next
                For counter As Integer = 0 To p.c.area.npcs.Count - 1
                    If mp.X >= p.c.area.npcs(counter).c.location.X - 1 And mp.Y >= p.c.area.npcs(counter).c.location.Y - 2 And mp.X < p.c.area.npcs(counter).c.location.X + 2 And mp.Y < p.c.area.npcs(counter).c.location.Y + 2 Then
                        p.subject = p.c.area.npcs(counter).c : p.GUI.addChatMessage(p, "Subject Selected! (Subject name:  " & p.c.area.npcs(counter).c.name & ")") : Exit For
                    End If
                Next
                For counter As Integer = 0 To Game.players.Count - 1
                    If mp.X >= Game.players(counter).c.location.X - 1 And mp.Y >= Game.players(counter).c.location.Y - 2 And mp.X < Game.players(counter).c.location.X + 2 And mp.Y < Game.players(counter).c.location.Y + 2 Then
                        If Game.players(counter).c.area.name = p.c.area.name And Game.players(counter).c.name <> p.c.name Then p.subject = Game.players(counter).c : p.GUI.addChatMessage(p, "Subject Selected! (Subject name:  " & Game.players(counter).c.name & ")") : Exit For
                    End If
                Next

            Case "link"
                If p.subType = 0 Then
                    For counter As Integer = 0 To p.c.area.objects.Count - 1
                        If mp.X >= p.c.area.objects(counter).location.X And mp.Y >= p.c.area.objects(counter).location.Y And mp.X < p.c.area.objects(counter).location.X + p.c.area.objects(counter).type.size.X And mp.Y < p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y And p.c.area.objects(counter).type.type = 3 Then
                            p.copiedPos = New worldPosition(p.c.area.name, mp, 0) : p.GUI.addChatMessage(p, "Door Selected! Now select a door to link it to.") : p.subType = 1 : Exit Sub
                        End If
                    Next
                End If
                If p.subType = 1 Then
                    For counter As Integer = 0 To p.c.area.objects.Count - 1
                        If mp.X >= p.c.area.objects(counter).location.X And mp.Y >= p.c.area.objects(counter).location.Y And mp.X < p.c.area.objects(counter).location.X + p.c.area.objects(counter).type.size.X And mp.Y < p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y And p.c.area.objects(counter).type.type = 3 Then
                            p.copiedPos2 = New worldPosition(p.c.area.name, mp, 0)
                            p.c.area.objects(counter).destination = p.copiedPos.copy : exitEditor(p)
                            commandPrompt.simulateCommand(p, "/tp " & p.copiedPos.area & " " & p.copiedPos.location.X & " " & p.copiedPos.location.Y) : p.c.gamestate = "mapEdit"
                            p.c.location = p.copiedPos.location : p.c.camera.GetViewMatrix(p.c, Vector2.One)
                            For counter2 As Integer = 0 To p.c.area.objects.Count - 1
                                If p.copiedPos.location.X >= p.c.area.objects(counter2).location.X And p.copiedPos.location.Y >= p.c.area.objects(counter2).location.Y And p.copiedPos.location.X < p.c.area.objects(counter2).location.X + p.c.area.objects(counter2).type.size.X And p.copiedPos.location.Y < p.c.area.objects(counter2).location.Y + p.c.area.objects(counter2).type.size.Y And p.c.area.objects(counter2).type.type = 3 Then
                                    p.c.area.objects(counter2).destination = p.copiedPos2.copy
                                    p.GUI.addChatMessage(p, "Doors Linked!") : p.subType = 0
                                    Exit Sub
                                End If
                            Next
                            Exit Sub
                        End If
                    Next
                End If
        End Select
    End Sub
    Public Shared Sub holdRightClick(ByRef p As player, mp As Vector2)

    End Sub
    Public Shared Sub pressRightClick(ByRef p As player, mp As Vector2)
        If p.mode = "npc" And p.subType < 3 Then p.subType += 1 Else If p.mode = "npc" And p.subType = 3 Then p.subType = 0
        If p.mode = "duplicate" Then
            If p.subType = 0 Then : If p.copyRegion.X >= 0 And p.copyRegion.Y >= 0 Then p.subType = 1
            Else
                If p.copyRegion.X + p.copyRegion.Z <= p.c.area.size.X And p.copyRegion.Y + p.copyRegion.W <= p.c.area.size.Y Then
                    p.GUI.addChatMessage(p, "Region Copied! Left Click To Paste!") : copyRegion(p) : p.subType = 0
                End If
            End If
        End If
    End Sub
    Public Shared Sub holdMiddleClick(ByRef p As player, mp As Vector2)
        'CHANGES ID ACCORDING TO WHAT MOUSE IS HOVERING OVER
        Select Case p.mode
            Case "tile", "fill" : If mp.X > 0 And mp.X < p.c.area.size.X And mp.Y > 0 And mp.Y < p.c.area.size.Y Then p.id = p.c.area.tileMap(p.c.area.size.X * mp.Y + mp.X)
            Case "object"
                For counter As Integer = 0 To p.c.area.objects.Count - 1
                    If mp.X >= p.c.area.objects(counter).location.X And mp.Y >= p.c.area.objects(counter).location.Y And mp.X < p.c.area.objects(counter).location.X + p.c.area.objects(counter).type.size.X And mp.Y < p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y Then p.id = p.c.area.objects(counter).type.id
                Next
            Case "item"
                For counter As Integer = 0 To p.c.area.items.Count - 1
                    If p.c.area.items(counter).location = mp Then p.id = p.c.area.items(counter).type.id
                Next
        End Select
    End Sub
    Public Shared Sub pressMiddleClick(ByRef p As player, mp As Vector2)

    End Sub

    Public Shared Sub showGhostObject(ByRef p As player, sb As SpriteBatch)
        If p.typing = True Or p.paused Then Exit Sub
        If p.controlMethod = "keyboard/mouse" Then
            p.mp = New Vector2(CInt((p.c.camera.location.X + p.mouseState.X) / Game.tilesize - 1), CInt((p.c.camera.location.Y + p.mouseState.Y) / Game.tilesize - 1))
            p.mp = New Vector2(p.mp.X - p.mp.X Mod p.grid.X + p.grid.Z, p.mp.Y - p.mp.Y Mod p.grid.Y + p.grid.W)
        End If
        If p.controlMethod.Split(" ")(0) = "GamePad" Then p.mp = cursor

        Dim x As Integer = 0 : Dim y As Integer = 0 : Dim newLoc As New Vector2
        For counter As Integer = 0 To (p.brush.X * p.brush.Y) - 1
            newLoc = New Vector2(p.mp.X + x, p.mp.Y + y)
            Dim tile As New Rectangle(newLoc.X * Game.tilesize, newLoc.Y * Game.tilesize, Game.tilesize, Game.tilesize)
            Select Case p.mode
                Case "tile", "fill"
                    For counter2 As Integer = 0 To terrainManager.tileTypes.Count - 1
                        If terrainManager.tileTypes(counter2).id = p.id Then
                            sb.Draw(terrainManager.tileTypes(counter2).skin, tile, Color.White * 0.6F)
                            sb.Draw(Game.outline, tile, Color.White)
                        End If
                    Next
                Case "Object"
                    For counter2 As Integer = 0 To terrainManager.objectTypes.Count - 1
                        Dim ob As New Rectangle(newLoc.X * Game.tilesize, newLoc.Y * Game.tilesize, Game.tilesize * terrainManager.objectTypes(counter2).size.X, Game.tilesize * terrainManager.objectTypes(counter2).size.Y)
                        If terrainManager.objectTypes(counter2).id = p.id Then sb.Draw(terrainManager.objectTypes(counter2).skin, ob, Color.White * 0.6F) : Exit For
                    Next
                Case "item" : sb.Draw(terrainManager.itemTypes(p.id).skin, tile, Color.White * 0.6F) : sb.Draw(Game.outline, tile, Color.White)
                Case "npc" : sb.Draw(p.c.skin, New Rectangle((newLoc.X - 2) * Game.tilesize, (newLoc.Y - 2) * Game.tilesize, Game.tilesize * 3, Game.tilesize * 3), New Rectangle(49, p.subType * (Game.tilesize * 3) + p.subType, (Game.tilesize * 3), (Game.tilesize * 3)), Color.White * 0.6F)
                Case "Erase" : sb.Draw(Game.erasor, tile, Color.White * 0.6F)

                Case "duplicate"
                    If p.subType = 0 Then
                        Dim newerLoc As New Vector2 : Dim x2 As Integer = 0 : Dim y2 As Integer = 0
                        For counter2 As Integer = 0 To p.copiedRegion.size.X * p.copiedRegion.size.Y - 1
                            newerLoc = New Vector2(newLoc.X + x2, newLoc.Y + y2)
                            sb.Draw(terrainManager.tileTypes(p.copiedRegion.tileMap(counter2)).skin, New Rectangle(newerLoc.X * Game.tilesize, newerLoc.Y * Game.tilesize, Game.tilesize, Game.tilesize), New Rectangle(0, 0, Game.tilesize, Game.tilesize), Color.White * 0.6F, 0, New Vector2(0, 0), SpriteEffects.None, 0.2)
                            For counter3 As Integer = 0 To p.copiedRegion.objects.Count - 1
                                If p.copiedRegion.objects(counter3).location = New Vector2(x2, y2) Then
                                    Dim ob As New Rectangle(newerLoc.X * Game.tilesize, newerLoc.Y * Game.tilesize, Game.tilesize * p.copiedRegion.objects(counter3).type.size.X, Game.tilesize * p.copiedRegion.objects(counter3).type.size.Y)
                                    sb.Draw(p.copiedRegion.objects(counter3).type.skin, ob, New Rectangle(0, 0, Game.tilesize * p.copiedRegion.objects(counter3).type.size.X, Game.tilesize * p.copiedRegion.objects(counter3).type.size.Y), Color.White * 0.6F, 0, New Vector2(0, 0), SpriteEffects.None, 0)
                                End If
                            Next
                            For counter3 As Integer = 0 To p.copiedRegion.items.Count - 1
                                If p.copiedRegion.items(counter3).location = New Vector2(x2, y2) Then
                                    Dim it As New Rectangle(newerLoc.X * Game.tilesize, newerLoc.Y * Game.tilesize, Game.tilesize, Game.tilesize)
                                    sb.Draw(p.copiedRegion.items(counter3).type.skin, it, New Rectangle(0, 0, Game.tilesize, Game.tilesize), Color.White * 0.6F, 0, New Vector2(0, 0), SpriteEffects.None, 0.15)
                                End If
                            Next

                            If x2 < p.copiedRegion.size.X Then x2 += 1
                            If x2 >= p.copiedRegion.size.X Then x2 = 0 : y2 += 1
                        Next

                    End If
                    If p.subType = 1 Then
                        Dim r As New Rectangle(p.copyRegion.X * Game.tilesize, p.copyRegion.Y * Game.tilesize, p.copyRegion.Z * Game.tilesize, p.copyRegion.W * Game.tilesize)
                        sb.Draw(Game.blackBox, r, Color.White * 0.6F)
                    End If
                Case "Select", "link" : sb.Draw(Game.outline, tile, Color.White)

            End Select
            If x < p.brush.X Then x += 1
            If x >= p.brush.X Then x = 0 : y += 1
        Next
    End Sub

    Public Shared Sub fillTool(ByRef p As player, location As Vector2)
        If p.c.area.tileMap(p.c.area.size.X * location.Y + location.X) = p.id Then Exit Sub 'makes sure the area isnt already filled with your selected id
        Dim currentTiles As New List(Of Integer) 'tiles that are to be tested for directly connected similar tiles
        currentTiles.Add(p.c.area.size.X * location.Y + location.X) 'starts the cycle with the tile clicked on
        Dim fillID As Integer = p.c.area.tileMap(currentTiles(0)) 'the ID of the tile clicked on
        p.c.area.tileMap(currentTiles(0)) = p.id

        Dim counter As Integer = 0 : Do Until currentTiles.Count = 0
            If currentTiles.Count = 0 Then Exit Sub
            Dim addedOne As Boolean = False
            If currentTiles(counter) - 1 >= 0 Then 'LEFT
                If p.c.area.tileMap(currentTiles(counter) - 1) = fillID And testIfInList(currentTiles, currentTiles(counter) - 1) = False Then currentTiles.Add(currentTiles(counter) - 1) : addedOne = True
            End If
            If currentTiles(counter) + 1 < (p.c.area.size.X * p.c.area.size.Y) Then 'RIGHT
                If p.c.area.tileMap(currentTiles(counter) + 1) = fillID And testIfInList(currentTiles, currentTiles(counter) + 1) = False Then currentTiles.Add(currentTiles(counter) + 1) : addedOne = True
            End If
            If currentTiles(counter) - p.c.area.size.X >= 0 Then 'UP
                If p.c.area.tileMap(currentTiles(counter) - p.c.area.size.X) = fillID And testIfInList(currentTiles, currentTiles(counter) - p.c.area.size.X) = False Then currentTiles.Add(currentTiles(counter) - p.c.area.size.X) : addedOne = True
            End If
            If currentTiles(counter) + p.c.area.size.X < p.c.area.size.X * p.c.area.size.Y Then 'DOWN
                If p.c.area.tileMap(currentTiles(counter) + p.c.area.size.X) = fillID And testIfInList(currentTiles, currentTiles(counter) + p.c.area.size.X) = False Then currentTiles.Add(currentTiles(counter) + p.c.area.size.X) : addedOne = True
            End If
            p.c.area.tileMap(currentTiles(counter)) = p.id : currentTiles.RemoveAt(counter)

            If addedOne = True Or counter > currentTiles.Count - 2 Then counter = -1
            counter += 1 : Loop
    End Sub
    Public Shared Function testIfInList(testList As List(Of Integer), part As Integer) As Boolean
        For counter As Integer = 0 To testList.Count - 1
            If part = testList(counter) Then Return True
        Next : Return False
    End Function
    Public Shared Sub eraser(ByRef p As player, location As Vector2)
        'DELETES OBJECTS
        If p.c.area.objects.Count > 0 Then eraseObject(p, location)

        'DELETES ITEMS
        For counter As Integer = 0 To p.c.area.items.Count - 1
            If p.c.area.items(counter).location = location Then p.c.area.items.Remove(p.c.area.items(counter)) : Exit Sub
        Next
        'DELETES NPCS
        For counter As Integer = 0 To p.c.area.npcs.Count - 1
            If p.c.area.npcs(counter).c.location = location Then commandPrompt.simulateCommand(p, "/removeNPC " & p.c.area.npcs(counter).c.name) : Exit Sub
        Next
    End Sub
    Public Shared Sub eraseObject(ByRef p As player, location As Vector2)
        Dim finished As Boolean : Dim counter As Integer = 0
        Do While finished = False
            'TESTS IF OBJECT IS TOUCHING ERASER
            If location.X >= p.c.area.objects(counter).location.X And location.Y >= p.c.area.objects(counter).location.Y And location.X < p.c.area.objects(counter).location.X + p.c.area.objects(counter).type.size.X And location.Y < p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y Then
                'REMOVES HIT DETECTION
                Dim x As Integer = 0 : Dim y As Integer = 0
                For counter2 As Integer = 0 To p.c.area.objects(counter).type.collisionMap.Count - 1
                    p.c.area.removeCoordinate(New Vector2(p.c.area.objects(counter).location.X + x, p.c.area.objects(counter).location.Y + y))
                    If x < p.c.area.objects(counter).type.size.X Then x += 1
                    If x >= p.c.area.objects(counter).type.size.X Then x = 0 : y += 1
                Next
                'DELETES OBJECT
                p.c.area.objects.Remove(p.c.area.objects(counter)) : finished = True
            End If
            counter += 1 : If counter >= p.c.area.objects.Count Then Exit Do
        Loop
    End Sub

    Public Shared Sub copyRegion(ByRef p As player)
        Dim newLoc As New Vector2
        Dim x As Integer = 0 : Dim y As Integer = 0

        Dim t As New List(Of Integer) : Dim o As New List(Of terrainObject) : Dim i As New List(Of item)
        For counter As Integer = 0 To p.copyRegion.Z * p.copyRegion.W - 1
            newLoc = New Vector2(p.copyRegion.X + x, p.copyRegion.Y + y)
            'COPIES TILES
            t.Add(p.c.area.tileMap(p.c.area.size.X * newLoc.Y + newLoc.X))
            'COPIES OBJECTS
            For counter2 As Integer = 0 To p.c.area.objects.Count - 1
                If p.c.area.objects(counter2).location = newLoc Then
                    Select Case p.c.area.objects(counter2).type.type
                        Case 0 : o.Add(New terrainObject(p.c.area.objects(counter2).type, New Vector2(x, y)))
                        Case 1 : o.Add(New terrainObject(p.c.area.objects(counter2).type, New Vector2(x, y), p.c.area.objects(counter2).text))
                        Case 2 : o.Add(New terrainObject(p.c.area.objects(counter2).type, New Vector2(x, y), p.c.area.objects(counter2).condition, p.c.area.objects(counter2).name, p.c.area.objects(counter2).inventory.copy))
                        Case 3 : o.Add(New terrainObject(p.c.area.objects(counter2).type, New Vector2(x, y), p.c.area.objects(counter2).condition, p.c.area.objects(counter2).destination))
                    End Select
                End If
            Next
            'COPIES ITEMS
            For counter2 As Integer = 0 To p.c.area.items.Count - 1
                If p.c.area.items(counter2).location = newLoc Then i.Add(New item(p.c.area.items(counter2).type, p.c.area.items(counter2).quantity, New Vector2(x, y)))
            Next

            If x < p.copyRegion.Z Then x += 1
            If x >= p.copyRegion.Z Then x = 0 : y += 1
        Next

        p.copiedRegion = New copiedRegion(New Vector2(p.copyRegion.Z, p.copyRegion.W), t, o, i)
    End Sub
    Public Shared Sub placeRegion(ByRef p As player, location As Vector2)
        Dim newLoc As New Vector2

        'PLACES COPIED TILES
        Dim x As Integer = 0 : Dim y As Integer = 0
        For counter As Integer = 0 To p.copiedRegion.tileMap.Count - 1
            newLoc = New Vector2(location.X + x, location.Y + y)
            If newLoc.X >= 0 And newLoc.Y >= 0 And newLoc.X < p.c.area.size.X And newLoc.Y < p.c.area.size.Y Then
                p.c.area.tileMap(newLoc.Y * p.c.area.size.X + newLoc.X) = p.copiedRegion.tileMap(counter)
            End If
            If x < p.copiedRegion.size.X Then x += 1
            If x >= p.copiedRegion.size.X Then x = 0 : y += 1
        Next

        'PLACES COPIED OBEJCTS
        For counter As Integer = 0 To p.copiedRegion.objects.Count - 1
            newLoc = New Vector2(location.X + p.copiedRegion.objects(counter).location.X, location.Y + p.copiedRegion.objects(counter).location.Y)
            Select Case p.copiedRegion.objects(counter).type.type
                Case 0 : p.c.area.place(New terrainObject(p.copiedRegion.objects(counter).type, newLoc))
                Case 1 : p.c.area.place(New terrainObject(p.copiedRegion.objects(counter).type, newLoc, p.copiedRegion.objects(counter).text))
                Case 2 : p.c.area.place(New terrainObject(p.copiedRegion.objects(counter).type, newLoc, p.copiedRegion.objects(counter).condition, p.copiedRegion.objects(counter).name, p.copiedRegion.objects(counter).inventory.copy))
                Case 3 : p.c.area.place(New terrainObject(p.copiedRegion.objects(counter).type, newLoc, p.copiedRegion.objects(counter).condition, p.copiedRegion.objects(counter).destination))
            End Select
        Next

        'PLACES COPIED ITEMS
        For counter As Integer = 0 To p.copiedRegion.items.Count - 1
            newLoc = New Vector2(location.X + p.copiedRegion.items(counter).location.X, location.Y + p.copiedRegion.items(counter).location.Y)
            p.c.area.place(p.copiedRegion.items(counter).copy, newLoc)
        Next
    End Sub
    Public Shared Sub createArea(p As player, name As String, dimentions As Vector2, Optional saveName As String = "quickSave")
        Dim location As String = Game.directory & "saves\" & saveName & "\areas\" & name & "\"
        My.Computer.FileSystem.CreateDirectory(location)

        'SAVES BLANK TILEMAP
        Dim fs As FileStream = File.Create(location & "tileMap.txt") : fs.Close() : Dim write As New StreamWriter(location & "tileMap.txt")
        For counter As Integer = 0 To (dimentions.X * dimentions.Y) - 1 : write.Write("0, ") : Next : write.Close()
        'SAVES NEW SPECDATA
        Dim fs2 As FileStream = File.Create(location & "specData.txt") : fs2.Close() : write = New StreamWriter(location & "specData.txt")
        write.Write(dimentions.X & " " & dimentions.Y & " 100 none False none True") : write.Close()

        'SAVES NEW OBJECT/ITEM FILES
        Dim fs6 As FileStream = File.Create(location & "objects.txt") : fs6.Close()
        Dim fs3 As FileStream = File.Create(location & "items.txt") : fs3.Close()

        'TELPORTS PLAYERS TO NEW AREA
        If saveName = "quickSave" Then p.c.changeArea(saveName, name, New Vector2(1, 3), p.c.direction)
    End Sub

    Public Shared Sub exitEditor(p As player)
        'SAVES TILEMAP (debating on moving to changeArea code)
        Dim location As String = Game.directory & "saves\quickSave\areas\" & p.c.area.name & "\tileMap.txt" : Dim write As New StreamWriter(location)
        For counter As Integer = 0 To p.c.area.tileMap.Count - 1 : write.Write(p.c.area.tileMap(counter) & ", ") : Next : write.Write("0") : write.Close()
        p.GUI.removeMessage("Level Editor")
        p.c.gamestate = "inGame" : p.c.visible = True
    End Sub
End Class