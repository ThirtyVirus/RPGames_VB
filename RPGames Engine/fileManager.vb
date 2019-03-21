Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Xml
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class FileManager
    'SAVING AND LOADING
    Public Shared Function save(saveName As String, Optional returnTitle As Boolean = False) As Integer
        'SHOWS PLAYERS THAT GAME IS SAVING
        For counter As Integer = 0 To Game.players.Count - 1
            Game.players(counter).GUI.messages.Add(New message(New Vector2(Game.players(counter).c.camera.viewport.Width / 2 - (Game.defaultFont4.MeasureString("Saving").X) / 2, Game.tilesize), "Saving", Game.defaultFont4, Color.Yellow, 80))
        Next

        Dim location As String = Game.directory & "saves\" & saveName

        'COPIES QUICKSAVE TO SAVE
        If saveName <> "quickSave" Then
            If My.Computer.FileSystem.FileExists(location) = True Then Directory.Delete(location, True)
            My.Computer.FileSystem.CopyDirectory(Game.directory & "saves\quickSave", location, True)
        End If

        'SAVES PLAYERS
        For counter As Integer = 0 To Game.players.Count - 1 : savePlayer(saveName, Game.players(counter)) : Next

        'SAVES WORLD DATA
        writeTextFile(location & "\worldData.txt", Game.time.ToString("hh\.mm"))

        'SAVES LOADED AREAS
        Dim savedAreas As New List(Of String)
        For counter As Integer = 0 To Game.players.Count - 1
            Dim alreadySaved As Boolean = False
            For Each a As String In savedAreas
                If a = Game.players(counter).c.area.name Then alreadySaved = True
            Next
            If alreadySaved = False Then saveArea(Game.players(counter).c.area, saveName) : savedAreas.Add(Game.players(counter).c.area.name)
        Next

        'SHOWS PLAYERS THAT SAVING IS COMPLETE
        For counter As Integer = 0 To Game.players.Count - 1 : Game.players(counter).GUI.removeMessage("Saving") : Next

        'RETURNS TO TITLE SCREEN (IF REQUESTED)
        If Game.returnTitle = True Then Game.returnTitle = False : Game.loadTitle()
        Return 0
    End Function
    Public Shared Sub load(saveName As String, player1 As String, Optional player2 As String = "", Optional player3 As String = "", Optional player4 As String = "")
        Game.saveName = saveName

        'MAKES QUICKSAVE
        If saveName <> "quickSave" Then
            If My.Computer.FileSystem.FileExists(Game.directory & "saves\quickSave\worldData.txt") = True Then Directory.Delete(Game.directory & "saves\quickSave", True)
            My.Computer.FileSystem.CopyDirectory(Game.directory & "saves\" & saveName, Game.directory & "saves\quickSave", True)
        End If

        'LOADS WORLD SETTINGS
        Dim text As String = My.Computer.FileSystem.ReadAllText(Game.directory & "saves\" & saveName & "\worldData.txt")
        Dim n = text.Split(" ") : Dim h = n(0).Split(".") : Game.time = TimeSpan.FromHours(h(0)) + TimeSpan.FromMinutes(h(1)) : Game.timeTick = 0.25

        'LOADS PLAYERS
        Game.players.Clear()
        If player1 <> "" Then Game.players.Add(loadPlayer(saveName, player1, True))
        If player2 <> "" Then Game.players.Add(loadPlayer(saveName, player2, True))
        If player3 <> "" Then Game.players.Add(loadPlayer(saveName, player3, True))
        If player4 <> "" Then Game.players.Add(loadPlayer(saveName, player4, True))
        gameDebug.allocatePlayerScreenSpace(Game.players, Game.resolution)
    End Sub
    Public Shared Sub createNewSave(saveName As String, startName As String, startSize As Vector2)
        Dim location As String = Game.directory & "saves\" & saveName

        My.Computer.FileSystem.CreateDirectory(location)
        My.Computer.FileSystem.CreateDirectory(location & "/areas")
        My.Computer.FileSystem.CreateDirectory(location & "/NPCs")
        My.Computer.FileSystem.CreateDirectory(location & "/players")
        writeTextFile(location & "/worldData.txt", "12.00")

        worldEditor.createArea(New player(), startName, startSize, saveName)
    End Sub

    Public Shared Sub saveArea(a As area, saveName As String) 'SAVES OCCUPIED AREAS
        Dim location As String = Game.directory & "saves\" & saveName
        My.Computer.FileSystem.CreateDirectory(location & "\areas\" & a.name)

        'SAVES AREA DATA
        writeTextFile(location & "\areas\" & a.name & "\specData.txt", a.size.X & " " & a.size.Y & " " & a.brightness & " " & a.music & " " & a.outside & " " & a.greetMessage & " " & a.saveAble)

        'SAVES OBJECTS
        Dim objects As String = ""
        For counter As Integer = 0 To a.objects.Count - 1
            Select Case a.objects(counter).type.type
                Case 0 : objects = objects & a.objects(counter).type.id & "-" & a.objects(counter).location.X & "-" & a.objects(counter).location.Y & "*"
                Case 1 : objects = objects & a.objects(counter).type.id & "-" & a.objects(counter).location.X & "-" & a.objects(counter).location.Y & "-" & a.objects(counter).text & "*"
                Case 2 : objects = objects & a.objects(counter).type.id & "-" & a.objects(counter).location.X & "-" & a.objects(counter).location.Y & "-" & a.objects(counter).condition & "-" & a.objects(counter).name & "-" & gameDebug.translateInventory(a.objects(counter).inventory) & "*"
                Case 3 : objects = objects & a.objects(counter).type.id & "-" & a.objects(counter).location.X & "-" & a.objects(counter).location.Y & "-" & a.objects(counter).condition & "-" & a.objects(counter).destination.area & "-" & a.objects(counter).destination.location.X & "-" & a.objects(counter).destination.location.Y & "-" & a.objects(counter).destination.direction & "*"
            End Select
        Next
        writeTextFile(location & "\areas\" & a.name & "\objects.txt", objects)

        'SAVES ITEMS
        Dim items As String = "" : For Each item As item In a.items
            items = items & item.type.id & "-" & item.location.X & "-" & item.location.Y & "-" & item.quantity & " " & vbNewLine
        Next
        writeTextFile(location & "\areas\" & a.name & "\items.txt", items)

        'SAVES NPCS (want to update format)
        My.Computer.FileSystem.CreateDirectory(location & "\NPCs")
        For counter As Integer = 0 To a.npcs.Count - 1
            Dim npc As String = ""
            npc = npc & a.npcs(counter).c.area.name & "-" & a.npcs(counter).c.location.X & "-" & a.npcs(counter).c.location.Y & "-" & a.npcs(counter).c.direction & "-"
            npc = npc & a.npcs(counter).c.health & "-" & a.npcs(counter).c.maxHealth & "-" & a.npcs(counter).c.texture & "-" & a.npcs(counter).c.name & "-" & a.npcs(counter).attribute & "-" & a.npcs(counter).c.gamestate & "-" & a.npcs(counter).c.inventory.money & "-"
            npc = npc & gameDebug.translateInventory(a.npcs(counter).c.inventory)

            writeTextFile(location & "\NPCs\" & a.npcs(counter).c.name & ".txt", npc)
        Next
    End Sub
    Public Shared Function loadArea(saveName As String, areaName As String) As area
        'SYNCS AREAS BETWEEN PLAYERS IN MULTIPLAYER
        For Each pl As player In Game.players
            If pl.c.area.name = areaName And pl.c.area.size <> Nothing Then Return pl.c.area
        Next

        Dim location As String = Game.directory & "saves\" & saveName : Dim text As String = ""
        Dim a As area = New area : a.name = areaName : a.forcedBrightness = -1

        'LOADS MAP DATA
        Dim prop As String = My.Computer.FileSystem.ReadAllText(Game.directory & "saves\" & saveName & "\areas\" & a.name & "\specData.txt")
        a.size = New Vector2(prop.Split(" ")(0), prop.Split(" ")(1))
        a.brightness = prop.Split(" ")(2) : a.music = prop.Split(" ")(3) : a.outside = prop.Split(" ")(4)
        a.greetMessage = prop.Split(" ")(5) : a.saveAble = prop.Split(" ")(6)

        'LOADS TILEMAP AND HIT DETECTION
        a.tileMap = New List(Of Integer)
        Dim com = My.Computer.FileSystem.ReadAllText(Game.directory & "saves\" & saveName & "\areas\" & areaName & "\tileMap.txt").Split(",")
        For counter As Integer = 0 To com.Count - 2 : a.tileMap.Add(com(counter)) : Next
        ReDim a.hitDetect(a.tileMap.Count)

        'LOADS OBJECTS
        text = My.Computer.FileSystem.ReadAllText(location & "\areas\" & a.name & "\objects.txt")
        Dim objectSplit = text.Split("*")
        For counter As Integer = 0 To objectSplit.Count - 2
            Dim objectInfo = objectSplit(counter).Split("-")
            'WILL VERY DEPENDING ON AMOUNT OF ARGUMENTS
            Dim objectType As terrainObjectType = terrainManager.getObjectType(objectInfo(0))
            Select Case objectType.type
                Case 0 : a.place(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2))))
                Case 1 : a.place(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3)))
                Case 2 : a.place(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3), objectInfo(4), gameDebug.translateInventory(objectInfo(5))))
                Case 3 : a.place(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3), New worldPosition(objectInfo(4), New Vector2(objectInfo(5), objectInfo(6)), objectInfo(7))))
            End Select
        Next

        'LOADS ITEMS
        text = My.Computer.FileSystem.ReadAllText(location & "\areas\" & a.name & "\items.txt")
        For counter As Integer = 0 To text.Split(" ").Count - 2
            Dim currentItem As String = text.Split(" ")(counter)
            Dim itemInfo = currentItem.Split("-")
            a.place(New item(terrainManager.getItemType(itemInfo(0)), itemInfo(3), New Vector2(itemInfo(1), itemInfo(2))))
        Next

        'LOADS NPCS (want to update storage format)
        location = Game.directory & "saves\" & saveName & "\NPCs\"
        Dim files As String() = Directory.GetFiles(location)
        For counter As Integer = 0 To files.Count - 1

            text = My.Computer.FileSystem.ReadAllText(files(counter)) : Dim split = text.Split("-")
            If split(0) = a.name Then
                'GATHERS NPC DATA
                Dim inventory As Inventory = gameDebug.translateInventory(split(11)) : inventory.money = CInt(split(10))

                'CREATES NPC
                Dim newNPC As npc = New npc(New character(createTexture(Game.directory & "textures/characters/" & split(6) & ".png"), split(6), split(7), inventory, split(4), split(5), split(9)), split(8))

                'PLACES NPC INTO WOLRD
                newNPC.c.hostNPC = newNPC : newNPC.c.area = a : newNPC.c.location = New Vector2(split(1), split(2)) : newNPC.c.direction = split(3) : a.npcs.Add(newNPC)
            End If
        Next

        Return a
    End Function

    Public Shared Sub savePlayer(saveName As String, p As player)
        'Username area x y direction health maxhealth <inventory>
        Dim player As String = p.c.name & " " & p.c.area.name & " " & p.c.location.X & " " & p.c.location.Y & " " & p.c.direction & " " & p.c.health & " " & p.c.maxHealth & " " & p.c.inventory.money & " " & gameDebug.translateInventory(p.c.inventory)
        writeTextFile(Game.directory & "saves\" & saveName & "\players\" & p.c.name & ".txt", player)

        'SAVES PLAYER
        'Dim settings As New XmlWriterSettings : settings.Indent = True : settings.IndentChars = vbTab
        'Dim writer As XmlWriter = XmlWriter.Create(Game.directory & "saves\" & saveName & "\players\" & p.c.name & ".xml", settings)
        'writer.WriteStartDocument() : writer.WriteStartElement(p.c.name)
        'System.Console.WriteLine(DateTime.Now.ToString)
        'writer.WriteAttributeString("area", p.c.area.name)
        'writer.WriteAttributeString("location", p.c.location.X & ", " & p.c.location.Y)
        'writer.WriteAttributeString("direction", p.c.direction)
        'writer.WriteAttributeString("health", p.c.health)
        'writer.WriteAttributeString("maxHealth", p.maxHealth)
        'writer.WriteAttributeString("money", p.c.inventory.money)
        'writer.WriteAttributeString("inventory", gameDebug.translateInventory(p.c.inventory))

        'writer.WriteEndElement() : writer.WriteEndDocument()
        'writer.Flush() : writer.Close()

    End Sub
    Public Shared Function loadPlayer(saveName As String, playerName As String, Optional logon As Boolean = False) As player
        Dim location As String = Game.directory & "saves\" & saveName & "\players\" & playerName & ".txt"

        'ADDS PLAYER TO SAVE IF NOT THERE PREVIOUSLY
        If My.Computer.FileSystem.FileExists(location) = False Then writeTextFile(location, playerName & " " & saveName & "_Start 1 1 2 100 100 0")

        'GETS ATTRIBUTES OF PLAYER (want to update storage format)
        Dim text As String = My.Computer.FileSystem.ReadAllText(location) : Dim attribute = text.Split(" ")
        Dim inventory As New Inventory : inventory.money = attribute(7)
        For counter As Integer = 8 To attribute.Count - 2
            Dim split2 = attribute(counter).Split(".")
            inventory.add(New item(terrainManager.getItemType(split2(0)), split2(1), New Vector2))
            counter += 1
        Next

        'CREATES PLAYER
        Dim p As player = New player(New character(Nothing, Nothing, attribute(0), inventory, attribute(5), attribute(6), "inGame"))

        'PLACES PLAYER IN WOLRD
        If logon Then
            'GATHERS PLAYER SETTINGS
            loadPlayerSettings(p) : p.c.hostPlayer = p

            'PLACES INTO WORLD
            p.c.changeArea("quickSave", attribute(1), New Vector2(attribute(2), attribute(3)), attribute(4))

            'NOTIFIES ALL PLAYERS OF JOINING GAME
            For counter As Integer = 0 To Game.players.Count - 1 : Game.players(counter).GUI.addChatMessage(Game.players(counter), p.c.name & " joined the game.") : Next
        End If
        Return p
    End Function

    'FILE AND TEXTURE CREATION
    Public Shared Function createTexture(path As String) As Texture2D
        Return Texture2D.FromStream(Game.graphics.GraphicsDevice, New StreamReader(path).BaseStream)
    End Function
    Public Shared Sub writeTextFile(directory As String, text As String)
        'creates the text file in the directory if one isn't there already
        If My.Computer.FileSystem.FileExists(directory) = False Then Dim fs As FileStream = File.Create(directory) : fs.Close()
        Dim write As New StreamWriter(directory)
        write.Write(text) : write.Close()
    End Sub

    'SELECTION MANAGEMENT
    Public Shared Sub saveSelection(selection As copiedRegion, selectionName As String)
        Dim location As String = Game.directory & "selections\" & selectionName : My.Computer.FileSystem.CreateDirectory(location)
        'SAVES SELECTION ATTRIBUTES AND TILEMAP
        writeTextFile(location & "\specData.txt", selection.size.X & " " & selection.size.Y)
        Dim write As String = "" : For Each tile As Integer In selection.tileMap : write = write & tile & ", " : Next
        writeTextFile(location & " \ tileMap.txt", write)

        'SAVES OBJECTS
        Dim objects As String = ""
        For counter As Integer = 0 To selection.objects.Count - 1
            Select Case selection.objects(counter).type.type
                Case 0 : objects = objects & selection.objects(counter).type.id & "-" & selection.objects(counter).location.X & "-" & selection.objects(counter).location.Y & "*"
                Case 1 : objects = objects & selection.objects(counter).type.id & "-" & selection.objects(counter).location.X & "-" & selection.objects(counter).location.Y & "-" & selection.objects(counter).text & "*"
                Case 2 : objects = objects & selection.objects(counter).type.id & "-" & selection.objects(counter).location.X & "-" & selection.objects(counter).location.Y & "-" & selection.objects(counter).condition & "-" & selection.objects(counter).name & "-" & gameDebug.translateInventory(selection.objects(counter).inventory) & "*"
                Case 3 : objects = objects & selection.objects(counter).type.id & "-" & selection.objects(counter).location.X & "-" & selection.objects(counter).location.Y & "-" & selection.objects(counter).condition & "-" & selection.objects(counter).destination.area & "-" & selection.objects(counter).destination.location.X & "-" & selection.objects(counter).destination.location.Y & "-" & selection.objects(counter).destination.direction & "*"
            End Select
        Next : writeTextFile(location & "\objects.txt", objects)

        'SAVES ITEMS
        Dim items As String = "" : For Each item As item In selection.items
            items = items & item.type.id & "-" & item.location.X & "-" & item.location.Y & "-" & item.quantity & " " & vbNewLine
        Next : writeTextFile(location & "\items.txt", items)
    End Sub
    Public Shared Function loadSelection(selectionName As String)
        Dim location As String = Game.directory & "selections\" & selectionName : Dim text As String = "" : Dim s As New Vector2
        Dim t As New List(Of Integer) : Dim o As New List(Of terrainObject) : Dim i As New List(Of item)

        'LOADS ATTRIBUTES AND TILES
        text = My.Computer.FileSystem.ReadAllText(location & "\specData.txt") : s = New Vector2(text.Split(" ")(0), text.Split(" ")(1))
        text = My.Computer.FileSystem.ReadAllText(location & "\tilemap.txt") : Dim com = text.Split(", ")
        For counter As Integer = 0 To com.Count - 2 : t.Add(com(counter)) : Next

        'LOADS OBJECTS
        text = My.Computer.FileSystem.ReadAllText(location & " \ objects.txt")
        Dim objectSplit = text.Split("*")
        For counter As Integer = 0 To objectSplit.Count - 2
            Dim objectInfo = objectSplit(counter).Split("-")
            'WILL VERY DEPENDING ON AMOUNT OF ARGUMENTS
            Dim objectType As terrainObjectType = terrainManager.getObjectType(objectInfo(0))
            Select Case objectType.type
                Case 0 : o.Add(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2))))
                Case 1 : o.Add(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3)))
                Case 2 : o.Add(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3), objectInfo(4), gameDebug.translateInventory(objectInfo(5))))
                Case 3 : o.Add(New terrainObject(objectType, New Vector2(objectInfo(1), objectInfo(2)), objectInfo(3), New worldPosition(objectInfo(4), New Vector2(objectInfo(5), objectInfo(6)), objectInfo(7))))
            End Select
        Next

        'LOADS ITEMS
        text = My.Computer.FileSystem.ReadAllText(location & "\items.txt")
        For counter As Integer = 0 To text.Split(" ").Count - 2
            Dim currentItem As String = text.Split(" ")(counter) : Dim itemInfo = currentItem.Split("-")
            For counter2 As Integer = 0 To terrainManager.itemTypes.Count - 1 : If Convert.ToSingle(itemInfo(0)) = terrainManager.itemTypes(counter2).id Then i.Add(New item(terrainManager.itemTypes(counter2), itemInfo(3), New Vector2(itemInfo(1), itemInfo(2))))
            Next : Next

        Return New copiedRegion(s, t, o, i)
    End Function

    'LOADING SETTINGS
    Public Shared Sub loadPlayerSettings(ByRef p As player)
        'CONTROLS
        p.controls = New controlScheme

        'MOVEMENT
        p.controls.up = New actionControl(Keys.W)
        p.controls.down = New actionControl(Keys.S)
        p.controls.left = New actionControl(Keys.A)
        p.controls.right = New actionControl(Keys.D)

        'LEVEL EDIT
        p.controls.cursorUp = New actionControl(Keys.Up)
        p.controls.cursorDown = New actionControl(Keys.Down)
        p.controls.cursorLeft = New actionControl(Keys.Left)
        p.controls.cursorRight = New actionControl(Keys.Right)
        p.controls.action1 = New actionControl(Keys.NumPad1, Buttons.A)
        p.controls.action2 = New actionControl(Keys.NumPad2, Buttons.B)
        p.controls.action3 = New actionControl(Keys.NumPad3, Buttons.X)

        'WORLD
        p.controls.interact = New actionControl(Keys.F, Buttons.A)
        p.controls.itemTransfer = New actionControl(Keys.Q, Buttons.A)
        p.controls.useItem = New actionControl(Keys.Enter, Buttons.RightStick)
        p.controls.back = New actionControl(Keys.E, Buttons.B)
        p.controls.sprint = New actionControl(Keys.LeftShift, Buttons.LeftStick)
        p.controls.attack = New actionControl(Keys.R)

        'MENUS
        p.controls.pressButton = New actionControl(Keys.Enter, Buttons.A)
        p.controls.downSlider = New actionControl(Keys.Left, Buttons.LeftTrigger)
        p.controls.upSlider = New actionControl(Keys.Right, Buttons.RightTrigger)
        p.controls.exitTextBox = New actionControl(Keys.Enter, Buttons.B)
        p.controls.prevMenu = New actionControl(Keys.Back, Buttons.B)

        'MISC.
        p.controls.console = New actionControl(Keys.T)
        p.controls.consolePlus = New actionControl(Keys.OemQuestion)
        p.controls.levelEditor = New actionControl(Keys.X, Buttons.Back)
        p.controls.pause = New actionControl(Keys.Escape, Buttons.Start)
        p.controls.screenShot = New actionControl(Keys.F2)
        p.controls.quickSave = New actionControl(Keys.F4, Buttons.DPadDown)
        p.controls.fullScreen = New actionControl(Keys.F11)

        'SETS CONTROL METHOD TO FIRST AVAILABLE
        p.plrIndex = Game.players.Count

        p.GUI = New GUI(p)
    End Sub

    Public Shared Sub saveGameData(directory As String)
        'SAVES GAME DATA
        Dim settings As New XmlWriterSettings : settings.Indent = True : settings.IndentChars = vbTab
        Dim gameData As XmlWriter = XmlWriter.Create(directory & "gameData.xml", settings)
        gameData.WriteStartDocument() : gameData.WriteStartElement("GameData")

        gameData.WriteStartElement("tiles")
        For counter As Integer = 0 To terrainManager.tileTypes.Count - 1
            gameData.WriteStartElement(terrainManager.tileTypes(counter).name)
            gameData.WriteAttributeString("id", terrainManager.tileTypes(counter).id)
            gameData.WriteAttributeString("minSpeed", terrainManager.tileTypes(counter).minSpeed)
            gameData.WriteAttributeString("maxSpeed", terrainManager.tileTypes(counter).maxSpeed)
            gameData.WriteAttributeString("stepSound", terrainManager.tileTypes(counter).stepSound)
            gameData.WriteAttributeString("soundQuantity", terrainManager.tileTypes(counter).stepSoundAmount) 'MAY REMOVE THIS ATTRIBUTE

            gameData.WriteEndElement()
        Next : gameData.WriteEndElement()

        gameData.WriteStartElement("objects")
        For counter As Integer = 0 To terrainManager.objectTypes.Count - 1
            gameData.WriteStartElement(terrainManager.objectTypes(counter).name)
            gameData.WriteAttributeString("id", terrainManager.objectTypes(counter).id)
            gameData.WriteAttributeString("width", terrainManager.objectTypes(counter).size.X)
            gameData.WriteAttributeString("height", terrainManager.objectTypes(counter).size.Y)
            gameData.WriteAttributeString("type", terrainManager.objectTypes(counter).type)
            gameData.WriteAttributeString("castShadow", terrainManager.objectTypes(counter).castShadow)
            gameData.WriteAttributeString("collisionMap", gameDebug.translateCollisionMap(terrainManager.objectTypes(counter).collisionMap))

            Select Case terrainManager.objectTypes(counter).type
                Case 2, 3
                    gameData.WriteAttributeString("openSound", terrainManager.objectTypes(counter).openSound)
                    gameData.WriteAttributeString("closeSound", terrainManager.objectTypes(counter).closeSound)
            End Select : gameData.WriteEndElement()

        Next

        gameData.WriteEndElement()
        gameData.WriteStartElement("items")
        For counter As Integer = 0 To terrainManager.itemTypes.Count - 1
            gameData.WriteStartElement(terrainManager.itemTypes(counter).name)
            gameData.WriteAttributeString("id", terrainManager.itemTypes(counter).id)
            gameData.WriteAttributeString("Weight", terrainManager.itemTypes(counter).weight)
            gameData.WriteAttributeString("Value", terrainManager.itemTypes(counter).value)
            gameData.WriteAttributeString("MaxStack", terrainManager.itemTypes(counter).maximumStack)
            gameData.WriteAttributeString("Description", terrainManager.itemTypes(counter).description)

            gameData.WriteEndElement()
        Next : gameData.WriteEndElement()

        gameData.WriteEndDocument()
        gameData.Flush() : gameData.Close()
    End Sub
End Class