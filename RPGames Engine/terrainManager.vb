Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics

Public Class area
    Public Property name As String : Public Property size As Vector2
    Public Property tileMap As New List(Of Integer)
    Public Property hitDetect As Vector2()

    Public Property objects As New List(Of terrainObject) : Public Property items As New List(Of item)
    Public Property npcs As New List(Of npc)

    Public Property brightness As Integer : Public Property forcedBrightness As Integer
    Public Property outside As Boolean : Public Property saveAble As Boolean
    Public Property greetMessage As String
    Public Property music As String

    Public Sub place(obj As terrainObject, Optional loc As Vector2 = Nothing)
        If loc <> New Vector2 Then obj.location = loc
        If obj.location.X >= 0 And obj.location.Y >= 0 And obj.location.X < size.X And obj.location.Y < size.Y Then

            'TESTS FOR ALREADY EXISTING OBJECTS
            For counter As Integer = 0 To objects.Count - 1 : If objects(counter).location = obj.location And objects(counter).type.id = obj.type.id Then Exit Sub
            Next

            'SETS HIT DETECTION
            Dim x As Integer = 0 : Dim y As Integer = 0
            For counter As Integer = 0 To obj.type.collisionMap.Count - 1
                If obj.type.collisionMap(counter).X <> 0 And (size.X * (obj.location.Y + y) + (obj.location.X + x)) >= 0 And (size.X * (obj.location.Y + y) + (obj.location.X + x)) < hitDetect.Count Then
                    If hitDetect(size.X * (obj.location.Y + y) + (obj.location.X + x)).X <> 5.0F And hitDetect(size.X * (obj.location.Y + y) + (obj.location.X + x)).X <> 7.0F Then takeCoordinate(New Vector2(obj.location.X + x, obj.location.Y + y), New Vector2(obj.type.collisionMap(counter).X, obj.type.collisionMap(counter).Y))
                End If
                If x < obj.type.size.X Then x += 1
                If x >= obj.type.size.X Then x = 0 : y += 1
            Next

            objects.Add(obj)
        End If
    End Sub
    Public Sub place(it As item, Optional loc As Vector2 = Nothing)
        If loc <> New Vector2 Then it.location = loc
        If it.location.X >= 0 And it.location.Y >= 0 And it.location.X < size.X And it.location.Y < size.Y Then
            'FUSES ITEM WITH ITEMS IN SAME LOCATION
            For Each i As item In items : If i.type.id = it.type.id And i.location = it.location Then i.quantity += it.quantity : Exit Sub
            Next
            items.Add(it)
        End If
    End Sub

    Public Sub takeCoordinate(location As Vector2, mode As Vector2)
        If location.X >= 0 And location.Y >= 0 And location.X < size.X And location.Y < size.Y Then
            hitDetect(size.X * location.Y + location.X) = mode
        End If
    End Sub
    Public Sub removeCoordinate(location As Vector2)
        If location.X >= 0 And location.Y >= 0 And location.X < size.X And location.Y < size.Y Then
            hitDetect(size.X * location.Y + location.X) = New Vector2(0, 0)
        End If
    End Sub
    Public Sub removeItem(name As String, location As Vector2)
        For Each i As item In items : If i.location = location And i.type.name = name Then items.Remove(i) : Exit Sub
        Next
    End Sub
End Class
Public Class worldPosition
    Public Property area As String
    Public Property location As Vector2
    Public Property direction As Integer

    Public Sub New()

    End Sub
    Public Sub New(a As String, l As Vector2, d As Integer)
        area = a : location = l
        If d <> 0 Then direction = d
    End Sub

    Public Function copy() As worldPosition
        Return New worldPosition(area, location, direction)
    End Function
End Class
Public Class tileType
    Public Property id As Single : Public Property name As String : Public Property skin As Texture2D
    Public Property minSpeed As Single : Public Property maxSpeed As Single
    Public Property stepSound As String : Public Property stepSoundAmount As Integer

    Public Sub New(num As Single, n As String, sMin As Single, sMax As Single, ss As String, ssa As Integer)
        id = num : name = n : skin = FileManager.createTexture(Game.directory & "textures/environment/tiles/" & n & ".png")
        minSpeed = sMin : maxSpeed = sMax : stepSound = ss : stepSoundAmount = ssa
    End Sub
End Class

Public Class terrainObjectType
    Public Property id As Single : Public Property name As String : Public Property skin As Texture2D

    Public Property size As Vector2 : Public Property collisionMap As List(Of Vector3) 'X IS EFFECT, Y IS SUBJECT, Z IS DIRECTION OF INTERACTION (0 means none, 5 means all sides)
    Public Property type As Integer 'DETERMINES HOW OBJECT REACTS WHEN INTERRACTED WITH
    Public Property castShadow As Integer

    'ATTRIBUTE - SPECIFIC VARIABLES
    Public Property openSound As String : Public Property closeSound As String

    Public Sub New(num As Single, n As String, s As Vector2, cm As List(Of Vector3), a As Integer, sh As Integer)
        id = num : name = n : skin = FileManager.createTexture(Game.directory & "textures/environment/terrainobjects/" & n & ".png")
        size = s : collisionMap = cm : type = a : castShadow = sh
    End Sub
    Public Sub New(num As Single, n As String, s As Vector2, cm As List(Of Vector3), a As Integer, sh As Integer, os As String, cs As String)
        id = num : name = n : skin = FileManager.createTexture(Game.directory & "textures/environment/terrainobjects/" & n & ".png")
        size = s : collisionMap = cm : type = a : castShadow = sh
        openSound = os : closeSound = cs
    End Sub

End Class
Public Class terrainObject
    Public Property type As terrainObjectType : Public Property location As Vector2
    Public Property visible As Boolean

    'ATTRIBUTE - SPECIFIC VARIABLES
    Public Property text As String
    Public Property name As String : Public Property inventory As Inventory
    Public Property condition As String : Public Property destination As worldPosition

    Public Sub New(t As terrainObjectType, l As Vector2)
        'DECORATIONS
        type = t : location = l
        If t.id <> 0 And t.id <> 20 And t.id <> 20.1F And t.id <> 0.1F Then visible = True
    End Sub
    Public Sub New(t As terrainObjectType, l As Vector2, te As String)
        'SIGNS
        type = t : location = l : text = te
        If t.id <> 0 And t.id <> 20 And t.id <> 20.1F And t.id <> 0.1F Then visible = True
    End Sub
    Public Sub New(t As terrainObjectType, l As Vector2, con As String, n As String, i As Inventory)
        'CONTAINERS
        type = t : location = l : condition = con : name = n : inventory = i
        If t.id <> 0 And t.id <> 20 And t.id <> 20.1F And t.id <> 0.1F Then visible = True
    End Sub
    Public Sub New(t As terrainObjectType, l As Vector2, con As String, d As worldPosition)
        'DOORS
        type = t : location = l : condition = con : destination = d
        If t.id <> 0 And t.id <> 20 And t.id <> 20.1F And t.id <> 0.1F Then visible = True
    End Sub

    Public Sub interact(ByRef p As player, Optional specialCondition As Boolean = False)
        If specialCondition Or condition <> "locked" Then
            Select Case type.type
                Case 1 : If text = "" Then text = "..."
                    p.interact = Me : p.recieveMessage(text)
                Case 2
                    'UNLOCKS CONTAINER
                    If specialCondition Then
                        condition = "open" : p.c.gamestate = "inGame"
                        gameDebug.playSound("sfx/interact/unlockDoor")
                        Exit Sub
                    End If
                    gameDebug.playSound(type.openSound, "sound", Game.soundVolume)
                    If inventory.items.Count > 0 Then inventory.selectedItem = inventory.items(0)
                    p.interact = Me : p.c.prevGamestate = p.c.gamestate : p.c.gamestate = "looting"
                    If p.interact.inventory.items.Count = 0 Then p.side = 1 Else p.side = 2
                Case 3
                    'UNLOCKS DOOR
                    If specialCondition Then
                        condition = "open" : p.c.gamestate = "inGame" : p.GUI.removeMessage("Door is Locked", True, 8)
                        gameDebug.playSound("sfx/interact/unlockDoor") : p.c.stepping = False
                    End If
                Case 4
                    'INTERACTABLE OBJECTS (SUCH AS CLEARABLE BUSHES)
                    p.c.area.objects.Remove(Me)
            End Select
        Else
            If type.type = 2 Then p.GUI.addMessage(New message(New Vector2(p.c.camera.viewport.Width / 2 - (Game.defaultFont4.MeasureString("Locked").X) / 2, p.c.camera.viewport.Height / 2.5 - (Game.defaultFont4.MeasureString("Locked").Y) / 2), "Locked", Game.defaultFont4, Color.White, False, False, 20, 10))
        End If
    End Sub
End Class

Public Class itemType
    Public Property id As Single : Public Property name As String : Public Property skin As Texture2D

    Public Property weight As String : Public Property value As String
    Public Property maximumStack As Integer : Public Property description As String

    Public Sub New(num As Single, n As String, w As Integer, v As String, ms As Integer, d As String)
        id = num : name = n : skin = FileManager.createTexture(Game.directory & "textures/items/" & n & ".png")
        weight = w : value = v : maximumStack = ms : description = d
    End Sub
End Class
Public Class item
    Public Property type As itemType : Public Property quantity As Integer
    Public Property location As Vector2 'REPRESENTS COORDINATE IN WORLD AND IN INVENTORY

    Public Sub New()

    End Sub
    Public Sub New(t As itemType, q As Integer, l As Vector2)
        type = t : quantity = q : location = l
    End Sub
    Public Function copy() As item
        Return New item(terrainManager.getItemType(type.id), quantity, location)
    End Function

    Public Sub draw(graphics As GraphicsDevice, sb As SpriteBatch, vp As Viewport, Optional drawBounds As Rectangle = Nothing)
        If IsNothing(type) Then Exit Sub
        'Viewport is entire window
        'DrawBounds is region where item info is drawn

        If drawBounds = Nothing Then
            drawBounds = New Rectangle(0, 0, vp.Width, vp.Height)
        End If

        'DISPLAYS ITEM IN AN INVENTORY SETTING

        '(change how it is displayed based on what type of item it is IE weapon, health item, misc...)
        Dim prevViewport As Viewport = New Viewport(graphics.Viewport.X, graphics.Viewport.Y, graphics.Viewport.Width, graphics.Viewport.Height) : graphics.Viewport = vp
        sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, Nothing, Nothing)

        'DRAWS BACKGROUND IMAGE
        sb.Draw(Game.invPane, New Rectangle(0, 0, vp.Width, vp.Height), New Rectangle(0, 0, vp.Width, vp.Height), Color.White * 0.8, Nothing, Nothing, SpriteEffects.None, 0.0001)

        'DRAWS DRAWBOUNDS (debug)
        'sb.Draw(Game.invPane, New Rectangle(drawBounds.X, drawBounds.Y, drawBounds.Width, drawBounds.Height), New Rectangle(0, 0, drawBounds.Width, drawBounds.Height), Color.White * 0.8, Nothing, Nothing, SpriteEffects.None, 0.0001) 'DRAWS BACKGROUND OF INVENTORY

        Dim imgWidth As Integer = drawBounds.Width / 3 - 20

        If drawBounds.Height < imgWidth + 20 Then imgWidth = drawBounds.Height - 20

        'DRAWS IMAGE (need to center in the left thirds of the item display screen)
        sb.Draw(type.skin, New Rectangle(drawBounds.X + 10, drawBounds.Y + 10, imgWidth, imgWidth), New Rectangle(0, 0, Game.tilesize, Game.tilesize), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.00001)

        'DRAWS DESCRIPTION
        gameDebug.wrapText(New Rectangle(drawBounds.X + imgWidth + 20, 0, drawBounds.Width / 3 * 2, drawBounds.Height), Game.defaultFont2, type.description, sb, Color.Black, 0)

        'DRAWS VALUE
        sb.DrawString(Game.defaultFont2, "Value:   " & type.value, New Vector2(vp.Width - Game.defaultFont2.MeasureString("Value:   " & type.value).X - 5 - drawBounds.X, vp.Height - Game.defaultFont2.MeasureString("I").Y - 5 - drawBounds.Y), Color.Black, 0, New Vector2(), 1, SpriteEffects.None, 0)

        sb.End() : graphics.Viewport = prevViewport
    End Sub
    Public Sub use(p As player)
        If type.id = 1 Then p.c.inventory.add(copy(), 1)
        If type.id = 4 Then If terrainManager.interactObject(p, True) Then p.c.inventory.remove(Me)
        If type.id = 5 Then
            If p.c.health = p.c.maxHealth Then p.recieveMessage("*You already have max HP!") : Exit Sub
            p.c.health += p.c.maxHealth / 2 : p.recieveMessage("*You ate the Pie, health restored.") : If p.c.health > p.c.maxHealth Then p.c.health = p.c.maxHealth : p.c.inventory.remove(Me)
        End If
    End Sub
End Class

Public Class terrainManager
    Public Shared tileTypes As New List(Of tileType) : Public Shared objectTypes As New List(Of terrainObjectType)
    Public Shared itemTypes As New List(Of itemType)

    Public Shared Function interactObject(ByRef p As player, Optional specialCondition As Boolean = False) As Boolean
        Dim testpoint As Vector2 = New Vector2(Math.Truncate(p.c.location.X) + 1, Math.Truncate(p.c.location.Y) + 1)

        Dim x As Integer : Dim y As Integer
        For counter As Integer = 0 To p.c.area.objects.Count - 1
            x = 0 : y = 0 : If p.c.area.objects(counter).type.type = 0 Then Continue For
            For counter2 As Integer = 0 To p.c.area.objects(counter).type.collisionMap.Count - 1

                'TESTS IF INTERACTABLE FROM THIS TILE
                If p.c.area.objects(counter).type.collisionMap(counter2).Z > 0 Then
                    If p.c.direction = 1 And (p.c.area.objects(counter).type.collisionMap(counter2).Z = 5 Or p.c.area.objects(counter).type.collisionMap(counter2).Z = 2) Then
                        If p.c.area.objects(counter).location.X + x = testpoint.X - 1 And p.c.area.objects(counter).location.Y + y = testpoint.Y - 1 And p.c.direction = 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                        If p.c.area.objects(counter).location.X + x = testpoint.X And p.c.area.objects(counter).location.Y + y = testpoint.Y - 1 And p.c.direction = 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                        If p.c.area.objects(counter).location.X + x = testpoint.X + 1 And p.c.area.objects(counter).location.Y + y = testpoint.Y - 1 And p.c.direction = 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                    End If
                    If p.c.direction = 2 And (p.c.area.objects(counter).type.collisionMap(counter2).Z = 5 Or p.c.area.objects(counter).type.collisionMap(counter2).Z = 1) Then
                        If p.c.area.objects(counter).location.X + x = testpoint.X - 1 And p.c.area.objects(counter).location.Y + y = testpoint.Y + 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                        If p.c.area.objects(counter).location.X + x = testpoint.X And p.c.area.objects(counter).location.Y + y = testpoint.Y + 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                        If p.c.area.objects(counter).location.X + x = testpoint.X + 1 And p.c.area.objects(counter).location.Y + y = testpoint.Y + 1 Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                    End If
                    If p.c.direction = 3 And (p.c.area.objects(counter).type.collisionMap(counter2).Z = 5 Or p.c.area.objects(counter).type.collisionMap(counter2).Z = 4) Then
                        If p.c.area.objects(counter).location.X + x = testpoint.X - 2 And p.c.area.objects(counter).location.Y + y = testpoint.Y Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                    End If
                    If p.c.direction = 4 And (p.c.area.objects(counter).type.collisionMap(counter2).Z = 5 Or p.c.area.objects(counter).type.collisionMap(counter2).Z = 3) Then
                        If p.c.area.objects(counter).location.X + x = testpoint.X + 2 And p.c.area.objects(counter).location.Y + y = testpoint.Y Then p.c.area.objects(counter).interact(p, specialCondition) : Return True
                    End If
                End If
                If x < p.c.area.objects(counter).type.size.X Then x += 1
                If x >= p.c.area.objects(counter).type.size.X Then x = 0 : y += 1
            Next
        Next
        Return False
    End Function
    Public Shared Sub interactNPC(c As character)
        Dim testpoint As Vector2 = New Vector2(Math.Truncate(c.location.X) + 1, Math.Truncate(c.location.Y - 0.5) + 1)
        Select Case c.direction
            Case 1 : testpoint.Y -= 1
            Case 2 : testpoint.Y += 1
            Case 3 : testpoint.X -= 2
            Case 4 : testpoint.X += 1
        End Select

        For counter As Integer = 0 To c.area.npcs.Count - 1
            If c.area.npcs(counter).c.gamestate = "idle" Or c.area.npcs(counter).c.gamestate = "still" Then
                If testpoint.X > c.area.npcs(counter).c.location.X - 3 And testpoint.X < c.area.npcs(counter).c.location.X + 3 And testpoint.Y > c.area.npcs(counter).c.location.Y - 2 And testpoint.Y < c.area.npcs(counter).c.location.Y + 2 Then c.area.npcs(counter).c.hostNPC.communicate(c.hostPlayer)
            End If
        Next

    End Sub

    Public Shared Function getObjectType(id As Single) As terrainObjectType
        For counter As Integer = 0 To objectTypes.Count - 1
            If id = objectTypes(counter).id Then Return objectTypes(counter)
        Next
        Return Nothing
    End Function
    Public Shared Function getItemType(id As Single) As itemType
        For counter As Integer = 0 To itemTypes.Count - 1
            If id = itemTypes(counter).id Then Return itemTypes(counter)
        Next
        Return Nothing
    End Function
End Class