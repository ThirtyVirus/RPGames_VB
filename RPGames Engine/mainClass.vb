Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Xml
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

'DO NOT SAVE GAME WHEN IN NEGATIVE COORDINATES --- LOOPING MUSIC MUST BE IN UNCOMPRESSED WAV FORMAT --- AVOID CREATING NEW RENDERTARGETS AND SPRITEBATCHES --- DO NOT SET TILE MINIMUM SPEED EQUAL TO MAXIMUM SPEED

'SHOULD STORE NPC DIALOGUE SIMILAR TO HOW IT IS ASKED --- REPONSES SHOULD BE STORED SEPERATED WITH A SYMBOL AND ORDER OF THEM COORESPONDS TO THE PLAYER'S ANSWER TO THE PREVIOUS QUESTION
' --- EXAMPLE: Chocolate^Vanilla ->    Not my favorite$Great choice! Would you like strawberry more? yes^no --- IN THIS EXAMPLE $ IS USED TO SEPERATE RESPONSES, ALSO ANOTHER QUESTION IS ASKED

'TO-DO-LIST
'shift music code to be more global/game based and less player/character based
'make music processing continue even if window de-selected

'add color and font options to console/textboxes
'settings for controls
'organize object interaction code (number representing type of interaction rather than specialcondition boolean)
'deny players access to naming themselves certain things (such as subject) to prevent code conflicts

'want ability to zoom in farther
'add ice tile that slides you

'make pathfinding use shortest path
'fix right direction for new movement system
'add sound effect for "take all" (enter key)
'change text sound to something less harsh
'make terrain object visability stay when saved (invisible items stay invisible after relog)
'find way to save areas faster when exiting (changearea code) 

'npc dialogue system
'combat system
'make area 'place' function for players and npcs
'npc "disposable" property that makes them not be stored in game files and spawn in and can be killed
'need function that copies a character
'make npc and player step sounds get fainter with distance (rather than only players making noise)
'animated doors (3 by 3 tiles)
'"edit save" screen (lets you rename, access editor if allowed, restart progress)

'have "barrier only" option in level editor
'make gui for level editor (for selecting id and mode)
'make the level editor into a class, move all properties to there
'combine tp and move commands
'alphabetize commands in help menu
'command syntax that selects closest subject (like @p for closest player)
'make giveitem command assume giving to self when typing /giveitem itemId/name id
'make "setgamestate" command work for more than just players
'make rename command work on more than just subjects
'update the /getPos command
'setdirection command needs a warning if typed other than a number
'make /possess command work on players
'make give command give player items if not in level editor and remove giveitem command
'make commands as a class that can be put into a list and sorted, also make it so commands do not display to chat if simulated
'/undo and /redo commands for level editor, no limit on how far back it goes

'fix npc health / death / storage mechanics
'make help entry for /possess command

'add selections to console (holding mouse click to select regions of text)

'Get rid of "shared" and impliment 'get' and 'set's to variables
'fix menu system and use objects to remotely change values

'BUGS

'gamepads are detected when not plugged in
'music sometimes doesnt play at start of game
'game draws two objects at the same z if on the same x coordinate
'object interact detection not being accurate
'fix keys being used up on already unlocked objects

'make use of "enabled" property of elements in menus (such as if no controls are available other than the one you are using, the button to switch it is disabled; or if making an area and certain properties not put in)

'total lines of code (as of 5/11/16): 4,757
Public Class Game
    Inherits Microsoft.Xna.Framework.Game
    Public Shared win As GameWindow : Public Shared graphics As GraphicsDeviceManager : Public Shared spriteBatch As SpriteBatch
    Public Shared resolution As Vector2 : Public Shared windowedResolution As Vector2 : Public Shared tilesize As Integer : Public Shared frames As New FrameCounter
    Public Shared defaultFont As SpriteFont : Public Shared defaultFont2 As SpriteFont : Public Shared defaultFont3 As SpriteFont : Public Shared defaultFont4 As SpriteFont
    Public Shared saveName As String : Public Shared availableControls As List(Of String)

    Public Shared directory As String = "E:\Programming\RPGames Engine\RPGames Engine VB (Final)\files\" 'GAME FILE DIRECTORY (DEBUG)
    'Public Shared directory As String = "files\" 'GAME FILE DIRECTORY (RELEASE)

    Public Shared time As TimeSpan : Public Shared timeTick As Double
    Public Shared brightness As Single : Public Shared outSideBrightness As Single : Public Shared combinedBrightness As Single
    Public Shared sounds As New List(Of gamesound) : Public Shared soundVolume As Integer
    Public Shared music As New List(Of gameSound) : Public Shared musicVolume As Integer
    Public Shared keyboardstate As KeyboardState : Public Shared mouseState As MouseState

    Public Shared players As New List(Of player)

    'MISC VARIABLES
    Public Shared tileX As Integer : Public Shared tileY As Integer : Public Shared z As Double
    Public Shared returnTitle As Boolean : Public Shared doShadow As Boolean
    Public Shared whiteBox As Texture2D : Public Shared blackBox As Texture2D
    Public Shared textPane As Texture2D : Public Shared invPane As Texture2D
    Public Shared drawbounds As Rectangle : Public Shared autoMusicUpdate As Boolean

    'LEVEL EDITOR SPRITES
    Public Shared erasor As Texture2D
    Public Shared outline As Texture2D

    Public Sub New()
        availableControls = New List(Of String) : graphics = New GraphicsDeviceManager(Me)

        'LOADS GAME SETTINGS
        Dim settings As XmlReader = XmlReader.Create(directory & "settings.xml")
        While settings.Read()
            If settings.NodeType = XmlNodeType.Element And settings.Name = "settings" Then
                resolution = New Vector2(settings.GetAttribute(0).Split(",")(0), settings.GetAttribute(0).Split(",")(1))
                graphics.PreferredBackBufferWidth = resolution.X
                graphics.PreferredBackBufferHeight = resolution.Y
                graphics.ApplyChanges()
                windowedResolution = resolution
                tilesize = settings.GetAttribute(1)
                soundVolume = settings.GetAttribute(2)
                musicVolume = settings.GetAttribute(3)
                brightness = settings.GetAttribute(4)
                doShadow = settings.GetAttribute(5)
                graphics.IsFullScreen = settings.GetAttribute(6)
            End If
        End While : settings.Close()

        'Funny trick that makes game pace as fast as framerate
        'graphics.SynchronizeWithVerticalRetrace = False
        'IsFixedTimeStep = False
    End Sub
    Protected Overrides Sub Initialize()
        win = Window : gameDebug.content = Content
        Window.AllowUserResizing = True : Window.Title = "RPGames Engine - By ThirtyVirus"
        IsMouseVisible = True : Content.RootDirectory = "Content"

        defaultFont = Content.Load(Of SpriteFont)("Fonts/consolas_16") : defaultFont2 = Content.Load(Of SpriteFont)("Fonts/consolas_16Bold")
        defaultFont3 = Content.Load(Of SpriteFont)("Fonts/consolas_28") : defaultFont4 = Content.Load(Of SpriteFont)("Fonts/consolas_28Bold")

        whiteBox = gameDebug.createBox(New Vector2(1, 1), Color.White)
        blackBox = gameDebug.createBox(New Vector2(1, 1), Color.Black)
        textPane = FileManager.createTexture(directory & "textures/misc/textPane.png")
        invPane = FileManager.createTexture(directory & "textures/misc/invPane.png")

        erasor = FileManager.createTexture(Game.directory & "\textures\misc\eraser.png")
        outline = FileManager.createTexture(Game.directory & "\textures\misc\outline.png")

        'INITIALIZES GAME DATA
        Dim gameData As XmlReader = XmlReader.Create(directory & "gameData.xml")

        Dim parent As String = ""
        While gameData.Read()
            If gameData.NodeType = XmlNodeType.Element And gameData.Name = "tiles" Then parent = "tiles"
            If gameData.NodeType = XmlNodeType.Element And gameData.Name = "objects" Then parent = "objects"
            If gameData.NodeType = XmlNodeType.Element And gameData.Name = "items" Then parent = "items"

            If gameData.AttributeCount > 0 Then
                If parent = "tiles" Then terrainManager.tileTypes.Add(New tileType(gameData.GetAttribute(0), gameData.Name, gameData.GetAttribute(1), gameData.GetAttribute(2), gameData.GetAttribute(3), gameData.GetAttribute(4)))
                If parent = "objects" Then
                    Select Case gameData.GetAttribute(3)
                        Case 0, 1 : terrainManager.objectTypes.Add(New terrainObjectType(Single.Parse(gameData.GetAttribute(0)), gameData.Name, New Vector2(gameData.GetAttribute(1), gameData.GetAttribute(2)), gameDebug.translateCollisionMap(gameData.GetAttribute(5)), gameData.GetAttribute(3), gameData.GetAttribute(4)))
                        Case 2, 3 : terrainManager.objectTypes.Add(New terrainObjectType(Single.Parse(gameData.GetAttribute(0)), gameData.Name, New Vector2(gameData.GetAttribute(1), gameData.GetAttribute(2)), gameDebug.translateCollisionMap(gameData.GetAttribute(5)), gameData.GetAttribute(3), gameData.GetAttribute(4), gameData.GetAttribute(6), gameData.GetAttribute(7)))
                    End Select
                End If
                If parent = "items" Then terrainManager.itemTypes.Add(New itemType(gameData.GetAttribute(0), gameData.Name, gameData.GetAttribute(1), gameData.GetAttribute(2), gameData.GetAttribute(3), gameData.GetAttribute(4)))
            End If
        End While
        gameData.Close()

        autoMusicUpdate = True
        spriteBatch = New SpriteBatch(GraphicsDevice) : MyBase.Initialize()
        graphics.GraphicsDevice.SetRenderTarget(Nothing)
    End Sub
    Protected Overrides Sub LoadContent()
        loadTitle()
        'FileManager.load("markiplier", "ThirtyVirus") : players(0).controlMethod = "keyboard/mouse" : players(0).debugmode = True 'FOR DEBUG PERPOSES

        'FOR FASTER LEVEL EDITING
        commandPrompt.simulateCommand(players(0), "/bindKey D1 /use tile")
        commandPrompt.simulateCommand(players(0), "/bindKey D2 /use fill")
        commandPrompt.simulateCommand(players(0), "/bindKey D3 /use object")
        commandPrompt.simulateCommand(players(0), "/bindKey D4 /use item")
        'commandPrompt.simulateCommand(players(0), "/bindKey D5 /use npc")
        commandPrompt.simulateCommand(players(0), "/bindKey D6 /use select")
        commandPrompt.simulateCommand(players(0), "/bindKey D7 /use link")
        commandPrompt.simulateCommand(players(0), "/bindKey D8 /use duplicate")
        commandPrompt.simulateCommand(players(0), "/bindKey D9 /use erase")

        'Dim testConvo As conversation = dialogue.generateConversation(My.Computer.FileSystem.ReadAllText(directory & "dialogue_test.txt"))
        'Dim counter As Integer = 0

        MyBase.LoadContent()
    End Sub
    Protected Overrides Sub UnloadContent()
        'ACTIONS TO BE DONE UPON PROGRAM CLOSE

        'SAVES GAME SETTINGS
        Dim settings As New XmlWriterSettings : settings.Indent = True : settings.IndentChars = vbTab
        Dim writer As XmlWriter = XmlWriter.Create(directory & "settings.xml", settings)
        writer.WriteStartDocument() : writer.WriteStartElement("settings")

        writer.WriteAttributeString("Resolution", resolution.X & "," & resolution.Y)
        writer.WriteAttributeString("TileSize", tilesize)
        writer.WriteAttributeString("SoundVolume", soundVolume)
        writer.WriteAttributeString("MusicVolume", musicVolume)
        writer.WriteAttributeString("Brightness", brightness)
        writer.WriteAttributeString("Shadows", doShadow)
        writer.WriteAttributeString("Fullscreen", graphics.IsFullScreen)

        writer.WriteEndElement() : writer.WriteEndDocument()
        writer.Flush() : writer.Close()

        MyBase.UnloadContent()
    End Sub

    Public Shared Sub loadTitle()
        'EMPTIES PLAYER LIST
        For counter As Integer = 0 To players.Count - 1 : players(counter).leaving = True : Next

        'CREATES EMPTY "CAMERA" PLAYER
        Dim start As New player(New character(Nothing, Nothing, "title", Nothing, Nothing, Nothing, Nothing))
        start.c.visible = False : start.GUI = New GUI(start) : time = TimeSpan.FromHours(12) : timeTick = 0
        start.c.changeArea("mainSave", "Pallet_Town", start.c.location, start.c.direction) : players.Add(start)
        availableControls = getAvailableControls() : FileManager.loadPlayerSettings(start)

        'PLAYS TITLE SCREEN MUSIC
        gameDebug.stopSounds("music") : gameDebug.playSound("music/title", "music", musicVolume, True)

        'MISC PROCESSES
        gameDebug.allocatePlayerScreenSpace(players, resolution)
        start.previousMenus = New List(Of String) : start.pauseMenu = New menu(start, "title") : start.paused = True
        start.c.camera.followPath(start.c, New List(Of Vector2) From {New Vector2(25 * tilesize, (start.c.area.size.Y / 2 - 40) * tilesize), New Vector2((start.c.area.size.X - 100) * tilesize, (start.c.area.size.Y / 2) * tilesize)}, 0.5, True, New Vector2(50 * tilesize, (start.c.area.size.Y / 2 - 20) * tilesize))
    End Sub

    Protected Overrides Sub Draw(gameTime As GameTime)
        If returnTitle = True Then Exit Sub
        graphics.GraphicsDevice.Clear(Color.Black)

        For counter As Integer = 0 To players.Count - 1
            graphics.GraphicsDevice.Viewport = players(counter).c.camera.viewport

            'SCREENSHOTS
            If players(counter).tookScreenShot Then gameDebug.takeScreenShot(players(counter), graphics.GraphicsDevice) : players(counter).tookScreenShot = False

            'DRAWS WORLD
            drawArea(players(counter), spriteBatch) : spriteBatch.End() 'DRAWS WORLD, PLAYERS, OBJECTS, ETC... 

            'DRAWS GUI ELEMENTS 
            drawGUI(players(counter), spriteBatch)
        Next

        'UPDATES GAME TIME AND FPS COUNTER
        If players.Count > 1 Or (players.Count = 1 And players(0).paused = False) Then frames.Increment()

        MyBase.Draw(gameTime)
    End Sub
    Public Shared Sub drawArea(p As player, sb As SpriteBatch)
        sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, Nothing, Nothing, Nothing, p.c.camera.GetViewMatrix(p.c, Vector2.One))

        'DEFINES DRAWING AREA
        drawbounds = New Rectangle(p.c.camera.location.X / tilesize - 2, p.c.camera.location.Y / tilesize - 2, (p.c.camera.location.X + p.c.camera.viewport.Width) / tilesize + 2, (p.c.camera.location.Y + p.c.camera.viewport.Height) / tilesize + 2)
        If drawbounds.X < 0 Then drawbounds.X = 0
        If drawbounds.Y < 0 Then drawbounds.Y = 0
        If drawbounds.Width > p.c.area.size.X Then drawbounds.Width = p.c.area.size.X
        If drawbounds.Height > p.c.area.size.Y Then drawbounds.Height = p.c.area.size.Y

        If p.c.camera.Rotation <> 0 Then
            Dim x1 As Integer = drawbounds.X - (drawbounds.Width - drawbounds.X) / 16 : If x1 < 0 Then x1 = 0
            Dim y1 As Integer = drawbounds.Y - (drawbounds.Height - drawbounds.Y) / 2.5 : If y1 < 0 Then y1 = 0
            Dim x2 As Integer = drawbounds.Width + (drawbounds.Width - drawbounds.X) / 16 : If x2 > p.c.area.size.X Then x2 = p.c.area.size.X
            Dim y2 As Integer = drawbounds.Height + (drawbounds.Height - drawbounds.Y) / 2.5 : If y2 > p.c.area.size.Y Then y2 = p.c.area.size.Y
            drawbounds = New Rectangle(x1, y1, x2, y2)
        End If

        'DRAWS TILES
        tileX = drawbounds.X : tileY = drawbounds.Y : z = 1
        Do Until tileY >= drawbounds.Height
            sb.Draw(terrainManager.tileTypes(p.c.area.tileMap(tileY * p.c.area.size.X + tileX)).skin, New Rectangle(tileX * tilesize, tileY * tilesize, tilesize, tilesize), New Rectangle(0, 0, 16, 16), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, z)
            tileX += 1 : If tileX >= drawbounds.Width Then tileX = drawbounds.X : tileY += 1
        Loop

        'DRAWS OBJECTS
        For counter As Integer = 0 To p.c.area.objects.Count - 1
            If p.c.area.objects(counter).location.X >= drawbounds.X - p.c.area.objects(counter).type.size.X And p.c.area.objects(counter).location.X <= drawbounds.Width And p.c.area.objects(counter).location.Y >= drawbounds.Y - p.c.area.objects(counter).type.size.Y And p.c.area.objects(counter).location.Y <= drawbounds.Height And (p.c.area.objects(counter).visible Or p.c.gamestate = "mapEdit") Then
                If p.c.area.objects(counter).type.id = 0.0F Or p.c.area.objects(counter).type.id = 0.1F Then z = 0.01 Else z = 1 - ((p.c.area.objects(counter).location.Y + p.c.area.objects(counter).type.size.Y - 1) / p.c.area.size.Y / 10)
                sb.Draw(p.c.area.objects(counter).type.skin, New Rectangle(p.c.area.objects(counter).location.X * tilesize, p.c.area.objects(counter).location.Y * tilesize, tilesize * p.c.area.objects(counter).type.size.X, tilesize * p.c.area.objects(counter).type.size.Y), New Rectangle(0, 0, 16 * p.c.area.objects(counter).type.size.X, 16 * p.c.area.objects(counter).type.size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, z)
                If doShadow = True And p.c.area.objects(counter).type.castShadow <> 0 Then drawShadow(p.c.area.objects(counter), p, sb)
            End If
        Next

        'DRAWS ITEMS
        For counter As Integer = 0 To p.c.area.items.Count - 1
            If p.c.area.items(counter).location.X >= drawbounds.X And p.c.area.items(counter).location.X <= drawbounds.Width And p.c.area.items(counter).location.Y >= drawbounds.Y And p.c.area.items(counter).location.Y <= drawbounds.Height Then
                z = 1 - ((p.c.area.items(counter).location.Y - 1) / p.c.area.size.Y / 10)
                sb.Draw(p.c.area.items(counter).type.skin, New Rectangle(p.c.area.items(counter).location.X * tilesize, p.c.area.items(counter).location.Y * tilesize, tilesize, tilesize), New Rectangle(0, 0, 16, 16), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, z)
            End If
        Next

        'DRAWS PLAYERS
        For counter As Integer = 0 To players.Count - 1
            If players(counter).c.location.X >= drawbounds.X And players(counter).c.location.X <= drawbounds.Width And players(counter).c.location.Y >= drawbounds.Y And players(counter).c.location.Y <= drawbounds.Height Then
                If players(counter).c.area.name = p.c.area.name And players(counter).c.visible And players(counter).c.speed <> 0 Then
                    Dim x As Integer = (players(counter).c.location.X - 1) * tilesize : Dim y As Integer = (players(counter).c.location.Y - 2) * tilesize
                    Dim a As Integer = players(counter).c.animationprogress
                    Dim d As New Rectangle(a * 48 + a + (players(counter).c.animOffset * 48 + players(counter).c.animOffset), (players(counter).c.direction - 1) * 48 + (players(counter).c.direction - 1), 48, 48)

                    z = 1 - (players(counter).c.location.Y / p.c.area.size.Y / 10)
                    sb.Draw(players(counter).c.skin, New Rectangle(x, y, 3 * tilesize, 3 * tilesize), d, Color.White, 0, New Vector2(0, 0), SpriteEffects.None, z)
                End If
            End If
        Next

        'DRAWS NPCS
        For counter As Integer = 0 To p.c.area.npcs.Count - 1
            If p.c.area.npcs(counter).c.location.X >= drawbounds.X - 2 And p.c.area.npcs(counter).c.location.X <= drawbounds.Width And p.c.area.npcs(counter).c.location.Y >= drawbounds.Y And p.c.area.npcs(counter).c.location.Y <= drawbounds.Height And p.c.area.npcs(counter).c.visible Then
                Dim x As Integer = (p.c.area.npcs(counter).c.location.X - 1) * tilesize : Dim y As Integer = (p.c.area.npcs(counter).c.location.Y - 2) * tilesize
                Dim d As New Rectangle(p.c.area.npcs(counter).c.animationprogress * 48 + p.c.area.npcs(counter).c.animationprogress + p.c.area.npcs(counter).c.animOffset * 48 + p.c.area.npcs(counter).c.animOffset, (p.c.area.npcs(counter).c.direction - 1) * 48 + (p.c.area.npcs(counter).c.direction - 1), 48, 48)

                z = 1 - (p.c.area.npcs(counter).c.location.Y / p.c.area.size.Y / 10)
                sb.Draw(p.c.area.npcs(counter).c.skin, New Rectangle(x, y, tilesize * 3, tilesize * 3), d, Color.White, 0, New Vector2(0, 0), SpriteEffects.None, z)
            End If
        Next

        If p.c.gamestate = "mapEdit" Then worldEditor.showGhostObject(p, sb)
    End Sub
    Public Shared Sub drawShadow(a As Object, p As player, sb As SpriteBatch)
        Dim location As Vector2 : Dim size As Vector2
        Select Case a.type.castShadow
            Case 1 '0.25 OFFSET, FULL X, HALF Y
                location = New Vector2((a.location.X + 0.25) * tilesize, (a.location.Y + a.type.size.Y / 2 + 0.25) * tilesize)
                size = New Vector2(a.type.size.X * tilesize, a.type.size.Y / 2 * tilesize)
            Case 2 '0.25 OFFSET, FULL X, FULL Y
                location = New Vector2((a.location.X + 0.25) * tilesize, (a.location.Y + 0.25) * tilesize)
                size = New Vector2(a.type.size.X * tilesize, a.type.size.Y * tilesize)
        End Select

        Dim l As Vector2 = New Vector2(location.X / tilesize, location.Y / tilesize) : Dim s As Vector2 = New Vector2(size.X / tilesize, size.Y / tilesize)
        z = 1 - ((a.location.Y + a.type.size.Y - 1) / p.c.area.size.Y / 10) + 0.005
        sb.Draw(blackBox, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, 1, 1), Color.White * 0.2, 0, New Vector2(0, 0), SpriteEffects.None, z)
    End Sub
    Public Shared Sub drawGUI(p As player, sb As SpriteBatch)
        sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, Nothing, Nothing)

        'DARKENS CANVAS
        If p.c.area.outside = True Then p.c.area.brightness = 100 - outSideBrightness * 100
        combinedBrightness = 1 - p.c.area.brightness / 100 + (100 - brightness) / 100
        If p.c.area.forcedBrightness <> -1 Then combinedBrightness = 1 - p.c.area.forcedBrightness / 100 + (100 - brightness) / 100
        sb.Draw(blackBox, New Rectangle(0, 0, p.c.camera.viewport.Width, p.c.camera.viewport.Height), New Rectangle(0, 0, 1, 1), Color.White * combinedBrightness, 0, New Vector2(0, 0), SpriteEffects.None, 0.002)

        'DRAWS PAUSE SCREENS
        If p.paused Then
            If p.c.name <> "title" Then sb.Draw(blackBox, New Rectangle(0, 0, p.c.camera.viewport.Width, p.c.camera.viewport.Height), New Rectangle(0, 0, 1, 1), Color.White * 0.4, 0, New Vector2(0, 0), SpriteEffects.None, 0.002)
            p.pauseMenu.draw(p, sb) : sb.End() : Exit Sub
        End If

        'DRAWS GUI TEXT
        For counter As Integer = 0 To p.GUI.messages.Count - 1
            If ((p.GUI.messages(counter).debug And p.debugmode) Or p.GUI.messages(counter).debug = False) Then
                sb.DrawString(p.GUI.messages(counter).textFont, p.GUI.messages(counter).text, p.GUI.messages(counter).location, p.GUI.messages(counter).color * (p.GUI.messages(counter).alpha / 100), 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.001)
                sb.DrawString(p.GUI.messages(counter).textFont, p.GUI.messages(counter).text, New Vector2(p.GUI.messages(counter).location.X + 2, p.GUI.messages(counter).location.Y + 2), Color.Black * (p.GUI.messages(counter).alpha / 130), 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0011)
            End If
        Next

        'DRAWS TEXTBOXES AND CONSOLE
        If p.c.gamestate = "speaking" Then p.displayMessage(sb)
        If p.typing = True Then p.cmd.draw(p, sb)

        'DRAWS INVENTORY SCREEN
        If p.c.gamestate = "inventory" Or p.c.gamestate = "looting" Or p.c.gamestate = "trading" Then
            sb.Draw(blackBox, New Rectangle(0, 0, p.c.camera.viewport.Width, p.c.camera.viewport.Height), New Rectangle(0, 0, 1, 1), Color.White * 0.4, 0, New Vector2(0, 0), SpriteEffects.None, 0.002)
            Dim size As Vector2 = New Vector2(p.c.camera.viewport.Width / 3 * 1.4, p.c.camera.viewport.Height / 1.8) : sb.End()

            If p.c.gamestate = "inventory" Then
                p.c.inventory.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2, p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4, size.X, size.Y), "Inventory", True)
                'DRAW SELECTEDITEM
                p.c.inventory.selectedItem.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2, (p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4) + size.Y + 10, size.X, size.Y * 0.4))
            End If

            If p.c.gamestate = "trading" Or p.c.gamestate = "looting" Then
                Dim dsi1 As Boolean = False : If p.side = 1 Then dsi1 = True
                Dim dsi2 As Boolean = False : If p.side = 2 Then dsi2 = True
                p.c.inventory.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2 - size.X / 2 - 5, p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4, size.X, size.Y), "Inventory", dsi1)
                p.interact.inventory.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2 + size.X / 2 + 5, p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4, size.X, size.Y), p.interact.name, dsi2)

                'DRAW SELECTED ITEM
                If dsi1 Then
                    p.c.inventory.selectedItem.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2 - size.X / 2 - 5, (p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4) + size.Y + 10, size.X * 2 + 10, size.Y * 0.4), New Rectangle(size.X / 2, 0, size.X + 10, size.Y * 0.4))
                ElseIf dsi2 Then
                    p.interact.inventory.selecteditem.draw(graphics.GraphicsDevice, sb, New Viewport(p.c.camera.viewport.X + (p.c.camera.viewport.Width - size.X) / 2 - size.X / 2 - 5, (p.c.camera.viewport.Y + (p.c.camera.viewport.Height - size.Y) / 4) + size.Y + 10, size.X * 2 + 10, size.Y * 0.4), New Rectangle(size.X / 2, 0, size.X + 10, size.Y * 0.4))
                End If
            End If
        Else
            sb.End()
        End If

    End Sub

    Protected Overrides Sub update(gameTime As GameTime)

        If IsActive Then
            keyboardstate = Keyboard.GetState() : mouseState = Mouse.GetState()
            gameDebug.doMiscProcesses() : Dim processedAreas As New List(Of String)

            For counter As Integer = 0 To players.Count - 1

                If players(counter).loading = False Then
                    'HANDLES PLAYER CONTROLS
                    If players(counter).controlMethod = " " Then players(counter).setControls(" ")
                    If players(counter).controlMethod = "keyboard/mouse" Then
                        players(counter).keystate = keyboardstate
                        players(counter).mouseState = mouseState
                    ElseIf players(counter).controlMethod.Split(" ")(0) = "GamePad" Then
                        If players(counter).controlMethod.Split(" ")(1) = 1 Then players(counter).padState = GamePad.GetState(PlayerIndex.One)
                        If players(counter).controlMethod.Split(" ")(1) = 2 Then players(counter).padState = GamePad.GetState(PlayerIndex.Two)
                        If players(counter).controlMethod.Split(" ")(1) = 3 Then players(counter).padState = GamePad.GetState(PlayerIndex.Three)
                        If players(counter).controlMethod.Split(" ")(1) = 4 Then players(counter).padState = GamePad.GetState(PlayerIndex.Four)
                    End If

                    'GETS INPUT AND UPDATES PLAYERS
                    players(counter).update()

                    'EXITS LOOP EARLY WHEN PAUSED IN SINGLEPLAYER
                    If players.Count = 1 And players(0).paused And players(0).c.name <> "title" Then
                        If players(counter).controlMethod = "keyboard/mouse" Then players(counter).oldKeyState = players(counter).keystate : players(counter).oldMouseState = players(counter).mouseState
                        If players(counter).controlMethod.Split(" ")(0) = "GamePad" Then players(counter).oldPadState = players(counter).padState
                        Exit For
                    End If

                    'TESTS FOR ALREADY LOADED AREAS
                    Dim alreadyProcessed As Boolean = False
                    For counter2 As Integer = 0 To processedAreas.Count - 1 : If processedAreas(counter2) = players(counter).c.area.name Then alreadyProcessed = True
                    Next

                    'PROCESSES WORLD EVENTS (only need to do once per area)
                    If alreadyProcessed = False Then
                        For counter2 As Integer = 0 To players(counter).c.area.npcs.Count - 1
                            players(counter).c.area.npcs(counter2).update()
                            If players(counter).c.area.npcs(counter2).c.health <= 0 Then players(counter).c.area.place(New terrainObject(terrainManager.getObjectType(5), New Vector2(Math.Truncate(players(counter).c.area.npcs(counter2).c.location.X), Math.Truncate(players(counter).c.area.npcs(counter2).c.location.Y)), "open", players(counter).c.area.npcs(counter2).c.name, players(counter).c.area.npcs(counter2).c.inventory.copy)) : players(counter).c.area.npcs.Remove(players(counter).c.area.npcs(counter2)) : Exit For
                        Next
                        'other world events go here
                        processedAreas.Add(players(counter).c.area.name)
                    End If

                    'SETS OLD CONTROL STATES
                    If players(counter).controlMethod = "keyboard/mouse" Then
                        players(counter).oldKeyState = players(counter).keystate
                        players(counter).oldMouseState = players(counter).mouseState
                    ElseIf players(counter).controlMethod.Split(" ")(0) = "GamePad" Then
                        players(counter).oldPadState = players(counter).padState
                    End If

                End If

                'ALLOWS PLAYERS TO LEAVE THE GAME
                If players(counter).leaving Then
                    For counter2 As Integer = 0 To players.Count - 1
                        If players(counter2).plrIndex > players(counter).plrIndex Then players(counter2).plrIndex -= 1
                    Next : players.RemoveAt(counter) : gameDebug.allocatePlayerScreenSpace(players, resolution)
                    Exit For
                End If

            Next

            'UPDATES AVAILABLE CONTROLS
            availableControls = getAvailableControls()

            MyBase.Update(gameTime)
        End If
    End Sub
    Public Shared Function getAvailableControls() As List(Of String)
        Dim controls As New List(Of String) : controls.Add("keyboard/mouse")

        'NEED TO FIX ORDER OF CONTROL SWITCHING AND DISPLAY ISSUES
        If GamePad.GetState(1).IsConnected Then controls.Add("GamePad 1")
        If GamePad.GetState(2).IsConnected Then controls.Add("GamePad 2")
        If GamePad.GetState(3).IsConnected Then controls.Add("GamePad 3")
        If GamePad.GetState(4).IsConnected Then controls.Add("GamePad 4")

        'TESTS IF PLAYER'S CONTROL METHOD IS CONNECTED
        For counter As Integer = 0 To players.Count - 1
            If controls.Contains(players(counter).controlMethod) = False Then players(counter).controlMethod = " "
        Next

        'REMOVES CONTROL METHODS ALREADY IN USE
        For counter As Integer = 0 To players.Count - 1
            If controls.Contains(players(counter).controlMethod) Then controls.Remove(players(counter).controlMethod)
        Next

        For counter As Integer = 0 To controls.Count - 1
            System.Console.Write(controls(counter) & " ")
        Next
        Return controls
    End Function
End Class