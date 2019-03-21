Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class actionControl
    Public Property key As KeyState : Public Property gamePadButton As ButtonState
    Public Property thumbstick As GamePadThumbSticks : Public Property trigger As GamePadTriggers

    Public Sub New(k As Keys, Optional g As Buttons = Nothing, Optional t As GamePadThumbSticks = Nothing, Optional tr As GamePadTriggers = Nothing)
        key = k : gamePadButton = g : thumbstick = t : trigger = tr
    End Sub
    Public Function pressed(ByRef p As player) As Boolean
        If p.controlMethod = "keyboard/mouse" And p.keystate.IsKeyDown(key) Then Return True
        If p.controlMethod.Split(" ")(0) = "GamePad" Then
            If gamePadButton <> Nothing And p.padState.IsButtonDown(gamePadButton) Then Return True
            If key = Keys.W And leftThumbMoved(p) = 1 Then Return True
            If key = Keys.S And leftThumbMoved(p) = 2 Then Return True
            If key = Keys.A And leftThumbMoved(p) = 3 Then Return True
            If key = Keys.D And leftThumbMoved(p) = 4 Then Return True

            If key = Keys.Up And rightThumbMoved(p) = 1 Then Return True
            If key = Keys.Down And rightThumbMoved(p) = 2 Then Return True
            If key = Keys.Left And rightThumbMoved(p) = 3 Then Return True
            If key = Keys.Right And rightThumbMoved(p) = 4 Then Return True
        End If
        Return False
    End Function
    Public Function justPressed(ByRef p As player) As Boolean
        If p.controlMethod = "keyboard/mouse" And p.keystate.IsKeyDown(key) And p.oldKeyState.IsKeyDown(key) = False Then Return True
        If p.controlMethod.Split(" ")(0) = "GamePad" Then
            If gamePadButton <> Nothing And p.padState.IsButtonDown(gamePadButton) And p.oldPadState.IsButtonDown(gamePadButton) = False Then Return True

            If key = Keys.W And leftThumbJustMoved(p) = 1 Then Return True
            If key = Keys.S And leftThumbJustMoved(p) = 2 Then Return True
            If key = Keys.A And leftThumbJustMoved(p) = 3 Then Return True
            If key = Keys.D And leftThumbJustMoved(p) = 4 Then Return True

            If key = Keys.Up And rightThumbJustMoved(p) = 1 Then Return True
            If key = Keys.Down And rightThumbJustMoved(p) = 2 Then Return True
            If key = Keys.Left And rightThumbJustMoved(p) = 3 Then Return True
            If key = Keys.Right And rightThumbJustMoved(p) = 4 Then Return True
        End If

        Return False
    End Function

    Public Function leftThumbMoved(ByRef p As player) As Integer
        If p.padState.ThumbSticks.Left.Y > 0 Then Return 1
        If p.padState.ThumbSticks.Left.Y < 0 Then Return 2
        If p.padState.ThumbSticks.Left.X < 0 Then Return 3
        If p.padState.ThumbSticks.Left.X > 0 Then Return 4
        Return 0
    End Function
    Public Function leftThumbJustMoved(ByRef p As player) As Integer
        If p.padState.ThumbSticks.Left.Y > 0 And p.oldPadState.ThumbSticks.Left.Y <= 0 Then Return 1
        If p.padState.ThumbSticks.Left.Y < 0 And p.oldPadState.ThumbSticks.Left.Y >= 0 Then Return 2
        If p.padState.ThumbSticks.Left.X < 0 And p.oldPadState.ThumbSticks.Left.X >= 0 Then Return 3
        If p.padState.ThumbSticks.Left.X > 0 And p.oldPadState.ThumbSticks.Left.X <= 0 Then Return 4
        Return 0
    End Function
    Public Function rightThumbMoved(ByRef p As player) As Integer
        If p.padState.ThumbSticks.Right.Y > 0 Then Return 1
        If p.padState.ThumbSticks.Right.Y < 0 Then Return 2
        If p.padState.ThumbSticks.Right.X < 0 Then Return 3
        If p.padState.ThumbSticks.Right.X > 0 Then Return 4
        Return 0
    End Function
    Public Function rightThumbJustMoved(ByRef p As player) As Integer
        If p.padState.ThumbSticks.Right.Y > 0 And p.oldPadState.ThumbSticks.Right.Y <= 0 Then Return 1
        If p.padState.ThumbSticks.Right.Y < 0 And p.oldPadState.ThumbSticks.Right.Y >= 0 Then Return 2
        If p.padState.ThumbSticks.Right.X < 0 And p.oldPadState.ThumbSticks.Right.X >= 0 Then Return 3
        If p.padState.ThumbSticks.Right.X > 0 And p.oldPadState.ThumbSticks.Right.X <= 0 Then Return 4
        Return 0
    End Function
End Class
Public Class controlScheme
    'MOVEMENT
    Public Property up As actionControl
    Public Property down As actionControl
    Public Property left As actionControl
    Public Property right As actionControl

    'LEVEL EDIT
    Public Property cursorUp As actionControl
    Public Property cursorDown As actionControl
    Public Property cursorLeft As actionControl
    Public Property cursorRight As actionControl
    Public Property action1 As actionControl
    Public Property action2 As actionControl
    Public Property action3 As actionControl

    'WORLD
    Public Property interact As actionControl
    Public Property itemTransfer As actionControl
    Public Property useItem As actionControl
    Public Property back As actionControl
    Public Property sprint As actionControl
    Public Property attack As actionControl

    'MENUS
    Public Property pressButton As actionControl
    Public Property downSlider As actionControl
    Public Property upSlider As actionControl
    Public Property exitTextBox As actionControl
    Public Property prevMenu As actionControl

    'MISC.
    Public Property console As actionControl
    Public Property consolePlus As actionControl
    Public Property levelEditor As actionControl
    Public Property pause As actionControl
    Public Property screenShot As actionControl
    Public Property quickSave As actionControl
    Public Property fullScreen As actionControl
End Class

Public Class quest
    Public Property questName As String
    Public Property instructions As List(Of goal)
    Public Property reward As item

    Public Sub New(name As String, rew As item)
        questName = name : reward = rew
    End Sub
    Public Sub addGoal(g As goal)
        instructions.Add(g)
    End Sub
    Public Sub removeGoal(g As goal)
        instructions.Remove(g)
    End Sub
    Public Sub checkFinishQuest(p As player)
        Dim finished As Boolean = True
        For Each g As goal In instructions : If g.achieved = False Then finished = False
        Next : If finished = True Then
            p.c.inventory.add(reward)

        End If
    End Sub
End Class
Public Class goal
    Public Property instruction As String
    Public Property achieved As Boolean

    Public Sub New(inst As String)
        instruction = inst
    End Sub
    Public Sub achieveGoal()
        achieved = True
    End Sub
End Class

Public Class player
    Public Property c As New character
    Public Property GUI As GUI

    'SPEAKING/TRADING
    Public Property incomingText As String : Public Property incomingTextProgress As Integer
    Public Property wait As Boolean : Public Property choices As List(Of String) : Public Property currentChoice As Integer : Public Property choice As String
    Public Property interact As Object : Public Property side As Integer

    'LEVEL EDITOR (soon will be its own object)
    Public Property mode As String : Public Property id As Single : Public Property subject As Object : Public Property mp As Vector2
    Public Property brush As Vector2 : Public Property grid As Vector4 'sizeX, sizeY, offsetX, offsetY 
    Public Property subType As Integer : Public Property copyRegion As Vector4
    Public Property copiedPos As worldPosition : Public Property copiedPos2 As worldPosition
    Public Property copiedRegion As copiedRegion

    'INPUT
    Public Property controls As controlScheme : Public Property controlMethod As String : Public Property plrIndex As Integer
    Public Property keystate As KeyboardState : Public Property oldKeyState As KeyboardState : Public Property typing As Boolean
    Public Property mouseState As MouseState : Public Property oldMouseState As MouseState
    Public Property padState As GamePadState : Public Property oldPadState As GamePadState

    'MISC
    Public Property paused As Boolean : Public Property loading As Boolean : Public Property debugmode As Boolean
    Public Property tookScreenShot As Boolean : Public Property leaving As Boolean
    Public Property pauseMenu As menu : Public Property previousMenus As List(Of String)
    Public Property cmd As console : Public Property boundCommands As New List(Of boundCommand) : Public Property prevCommand As String

    Public Sub New()

    End Sub
    Public Sub New(ch As character)
        c = ch : c.host = "player"

        If My.Computer.FileSystem.FileExists(Game.directory & "textures\characters\" & c.name & ".png") = True Then
            c.skin = FileManager.createTexture(Game.directory & "textures\characters\" & c.name & ".png") : c.texture = c.name
        Else : c.skin = FileManager.createTexture(Game.directory & "textures\characters\thirtyvirus.png") : c.texture = "thirtyvirus"
        End If

    End Sub

    Public Sub setControls(preferred As String)
        If Game.availableControls.Contains(preferred) Then
            controlMethod = preferred
        Else
            If Game.availableControls.Count > 0 Then controlMethod = Game.availableControls(0)
        End If
    End Sub

    Public Sub update()
        getInput()
        c.manageMovement()

        'UPDATES WHETHER STANDING ON A USED DOOR OR NOT
        If c.justUsedDoor Then
            Dim testPoint As Vector2 = New Vector2(Math.Truncate(c.location.X) + 1, Math.Truncate(c.location.Y - 0.5) + 1)
            If c.area.hitDetect(c.area.size.X * testPoint.Y + testPoint.X).X <> 2 And c.area.hitDetect(c.area.size.X * testPoint.Y + testPoint.X - 1).X <> 2 Then c.justUsedDoor = False
        End If

        'TESTS FOR DEATH
        If c.health <= 0 Then c.gamestate = "dead"

        'PROCESSES GUI ELEMENTS
        GUI.processMessages()

        c.tileJustStoodOn = c.tileStandingOn
    End Sub
    Public Sub getInput()
        If controls.screenShot.justPressed(Me) Then tookScreenShot = True

        'FULLSCREEN
        If controls.fullScreen.justPressed(Me) Then
            If Game.graphics.IsFullScreen = False Then
                Game.graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                Game.graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
            Else
                Game.graphics.PreferredBackBufferWidth = Game.windowedResolution.X
                Game.graphics.PreferredBackBufferHeight = Game.windowedResolution.Y
                Game.resolution = Game.windowedResolution
            End If
            Game.graphics.IsFullScreen = Not Game.graphics.IsFullScreen : Game.graphics.ApplyChanges() : gameDebug.allocatePlayerScreenSpace(Game.players, Game.resolution)
        End If

        'TITLE SCREEN INPUT
        If c.name = "title" Then
            If IsNothing(pauseMenu) Then pauseMenu = New menu(Me, "title")
            pauseMenu.getInput(Me)
            If controls.pause.justPressed(Me) And pauseMenu.name <> "title" Then gameDebug.playSound("sfx/menu/click", "sound", Game.soundVolume, False) : pauseMenu = New menu(Me, "title")
            Exit Sub
        End If

        'PAUSE SCREEN INPUT
        If controls.pause.justPressed(Me) Then paused = Not paused : If paused Then previousMenus = New List(Of String) : pauseMenu = New menu(Me, "mainPaused")
        If paused Then pauseMenu.getInput(Me) : Exit Sub

        'CONSOLE INPUT
        If typing = True Then cmd.getKeyboardInput(Me) : cmd.getMouseInput(Me) : Exit Sub

        'INVENTORY INPUT
        If c.gamestate = "inventory" Or c.gamestate = "trading" Or c.gamestate = "looting" Then c.inventory.getInput(Me) : c.inventory.getMouseInput(Me) : Exit Sub

        'MESSAGE INPUT
        If c.gamestate = "speaking" Then getMessageInput() : Exit Sub

        'LEVEL EDITOR INPUT
        If c.gamestate = "mapEdit" Then worldEditor.getInput(Me)

        'BOUND COMMAND KEY PRESSES
        If typing = False Then
            For Each k As Keys In keystate.GetPressedKeys
                For Each key As boundCommand In boundCommands
                    'D# format for numbers
                    If k.ToString = key.key Then
                        Dim justPressed As Boolean = True
                        For Each nk As Keys In oldKeyState.GetPressedKeys()
                            If nk = k Then justPressed = False
                        Next
                        If justPressed = True Then commandPrompt.simulateCommand(Me, key.command)
                    End If
                Next
            Next
        End If

        'DETECTS BACK BUTTON
        If controls.back.justPressed(Me) Then
            If c.gamestate = "inGame" Then c.prevGamestate = c.gamestate : c.gamestate = "inventory"
        End If

        'DETECTS INTERACT BUTTON
        If controls.interact.justPressed(Me) Then
            If c.gamestate = "inGame" Then
                terrainManager.interactNPC(c)
                terrainManager.interactObject(Me) : Exit Sub
            End If
        End If

        'DETECTS ATTACK BUTTON (will have seperate function for this soon)
        If controls.attack.justPressed(Me) Then
            If c.gamestate = "inGame" Then
                c.swingSword()
            End If
        End If

        'DETECTS MOVEMENT BUTTONS
        If c.gamestate = "inGame" Then
            If controls.sprint.justPressed(Me) Then c.speedChange = True
            If controls.up.pressed(Me) Or (c.directions.Contains(1) And c.animationprogress <> 1) Then
                c.stepping = True : If c.directions.Contains(1) = False Then c.directions.Add(1)
            Else c.directions.Remove(1)
            End If
            If controls.down.pressed(Me) Or (c.directions.Contains(2) And c.animationprogress <> 1) Then
                c.stepping = True : If c.directions.Contains(2) = False Then c.directions.Add(2)
            Else c.directions.Remove(2)
            End If
            If controls.left.pressed(Me) Or (c.directions.Contains(3) And c.animationprogress <> 1) Then
                c.stepping = True : If c.directions.Contains(3) = False Then c.directions.Add(3)
            Else c.directions.Remove(3)
            End If
            If controls.right.pressed(Me) Or (c.directions.Contains(4) And c.animationprogress <> 1) Then
                c.stepping = True : If c.directions.Contains(4) = False Then c.directions.Add(4)
            Else c.directions.Remove(4)
            End If
            If c.stepping = False Then c.speedChange = False : c.stepProgress = 0 : c.animationprogress = 1
        End If

        'DETECTS WORLD EDITOR BUTTON
        If controls.levelEditor.justPressed(Me) And debugmode = True Then
            If c.gamestate = "inGame" Then : c.gamestate = "mapEdit"
                If Game.resolution <> New Vector2 Then GUI.messages.Add(New message(New Vector2(c.camera.viewport.Width / 2 - (Game.defaultFont4.MeasureString("World Editor").X) / 2, c.camera.viewport.Height / 5 - (Game.defaultFont4.MeasureString("World Editor").Y) / 2), "World Editor", Game.defaultFont4, Color.White, True, False, 100, 10))
                If mode = Nothing Then mode = "tile"
                If brush = Nothing Then brush = New Vector2(1, 1)
                If grid = Nothing Then grid = New Vector4(1, 1, 0, 0)
            Else : If c.gamestate = "mapEdit" Then worldEditor.exitEditor(Me)
            End If
        End If

        'DETECTS CONSOLE BUTTONS
        If (controls.console.justPressed(Me) Or controls.consolePlus.justPressed(Me)) And typing = False Then
            cmd = New console(New Vector2(16, c.camera.viewport.Height - 30), Game.defaultFont) : typing = True
            If controls.consolePlus.justPressed(Me) Then cmd.text = "/" : cmd.inputPos = cmd.text.Count
            If GUI.chat.Count > 0 Then
                For counter As Integer = 0 To GUI.messages.Count - 1
                    If GUI.messages.Count <= counter Then Exit For
                    If GUI.messages(counter).moveWithChat Then GUI.removeMessage(GUI.messages(counter).text)
                Next
            End If
        End If

        'DETECTS QUICKSAVE BUTTON
        If controls.quickSave.justPressed(Me) And c.area.saveAble Then commandPrompt.simulateCommand(Me, "/saveGame " & Game.saveName)

        'DEBUG CONTROLS
        If keystate.IsKeyDown(Keys.K) And oldKeyState.IsKeyDown(Keys.K) = False Then
            System.Console.WriteLine(gameDebug.getDistance(New Vector2(10, 10), New Vector2(20, 20)))
        End If

        'PATHFINDING TEST
        If c.gamestate = "inGame" And c.area.name = "maze" And keystate.IsKeyDown(Keys.J) And oldKeyState.IsKeyDown(Keys.J) = False Then
            commandPrompt.simulateCommand(Me, "/selectMe") : commandPrompt.simulateCommand(Me, "/sendTo 94 94")
        End If
        If debugmode Then
            'MOUSE WHEEL ZOOMING
            Dim newZoom As Single
            If mouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue And c.camera.Zoom > 0.3 Then newZoom = c.camera.Zoom - 0.5F : c.camera.Zoom = MathHelper.Lerp(c.camera.Zoom, newZoom, 0.1F)
            If mouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue And c.camera.Zoom < 0.9999 Then newZoom = c.camera.Zoom + 0.5F : c.camera.Zoom = MathHelper.Lerp(c.camera.Zoom, newZoom, 0.1F)
        End If
    End Sub

    'MESSAGE RULES
    '@ -> Player's Name
    '~ -> New Line 
    '` -> New Text Prompt
    '^ -> Option Dialogue (ex: yes or no) yes^no

    Public Sub recieveMessage(text As String)
        text = text.Replace("@", c.name)
        c.prevGamestate = c.gamestate : c.gamestate = "speaking" : incomingTextProgress = 0 : incomingText = text
        choices = New List(Of String) : currentChoice = 0 : choice = ""

        Dim word = text.Split(" ")
        For counter As Integer = 0 To word.Count - 1
            If word(counter).Contains("^") Then
                choices = New List(Of String) : currentChoice = 0
                For counter2 As Integer = 0 To word(counter).Split("^").Count - 1 : choices.Add(word(counter).Split("^")(counter2)) : Next
            End If
        Next
    End Sub
    Public Sub closeMessage()
        If wait = True Then 'STARTS OFF NEW TEXTBOX FOR REMAINDER OF TEXT
            If incomingText(incomingTextProgress) = "`" Then recieveMessage(incomingText.Substring(incomingTextProgress + 1, incomingText.Length - (incomingTextProgress + 1))) : wait = False : Exit Sub
            Dim word = incomingText.Split(" ")
            Dim wordProgress As Integer = 0 : Dim charProgress As Integer = 0
            For counter As Integer = 0 To incomingTextProgress - 1 : charProgress += 1
                If incomingText(counter) = " " Then wordProgress += 1 : charProgress = 0
            Next : incomingTextProgress -= charProgress
            recieveMessage(incomingText.Substring(incomingTextProgress, incomingText.Length - incomingTextProgress)) : wait = False : Exit Sub
        Else
            If IsNothing(interact) = False And TypeOf interact Is character Then interact.gamestate = interact.prevGamestate
            c.gamestate = "inGame" : incomingTextProgress = 0 : incomingText = ""
        End If
    End Sub
    Public Sub displayMessage(sb As SpriteBatch)
        Dim scale As Single = 1.7 'SIZE OF TEXTBOX (SUPPORTS UP TO 5 LINES OF TEXT IN SMALLEST RESOLUTION)
        Dim textBox As New Rectangle((c.camera.viewport.Width - c.camera.viewport.Width / 4 * scale) / 2, (c.camera.viewport.Height - c.camera.viewport.Height / 5 * scale), c.camera.viewport.Width / 4 * scale, c.camera.viewport.Height / 6 * scale)
        Dim size As New Rectangle(0, 0, Game.textPane.Width, Game.textPane.Height)
        sb.Draw(Game.textPane, textBox, size, Color.White * 0.8, 0, New Vector2(0, 0), SpriteEffects.None, 0.0001) 'DRAWS BACKGROUND OF TEXT PROMPT

        If incomingTextProgress < incomingText.Length Then If incomingText(incomingTextProgress) = "`" Then wait = True

        If incomingTextProgress < incomingText.Length And wait = False Then
            incomingTextProgress += 1 : gameDebug.playSound("sfx/misc/key", "sound", Game.soundVolume, False, 2)
        End If

        'DRAWS TEXT AND OPENS NEW TEXT PROMPT IF OUT OF SPACE
        If drawText(New Rectangle(textBox.X + 5, textBox.Y, textBox.Width - 5, textBox.Height), Game.defaultFont2, incomingText, incomingTextProgress, sb, Color.Black, 0.0001) = False Then wait = True
    End Sub
    Public Function drawText(textPane As Rectangle, font As SpriteFont, text As String, stopPoint As Integer, sb As SpriteBatch, color As Color, layer As Integer) As Boolean
        Dim words = text.Substring(0, stopPoint).Split(" ") : Dim checkTooFar = text.Split(" ") : Dim progress As Vector2 = New Vector2(0, 0)
        For counter As Integer = 0 To words.Count - 1

            If progress.X + font.MeasureString(checkTooFar(counter)).X <= textPane.Width And words(counter).Contains("^") Then
                'DRAWS CHOICES
                If words(counter).Contains("^") Then
                    For counter2 As Integer = 0 To choices.Count - 1
                        Dim co As Color = color : If currentChoice = counter2 Then co = Color.Goldenrod
                        sb.DrawString(font, choices(counter2) & " ", New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), co, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(choices(counter2) & " ").X
                    Next
                    progress.X = textPane.Width 'INITIATES NEW LINE AT END OF CHOICE
                End If
            Else
                If progress.X + font.MeasureString(checkTooFar(counter)).X <= textPane.Width And words(counter) <> "~" Then
                    sb.DrawString(font, words(counter) & " ", New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(words(counter) & " ").X
                Else
                    'NEW LINE
                    progress.Y += font.MeasureString(" ").Y * 0.7 : progress.X = 0 : If progress.Y + font.MeasureString(" ").Y * 0.7 > textPane.Height Then Return False
                    If words(counter) <> "~" And words(counter).Contains("^") = False Then sb.DrawString(font, words(counter) & " ", New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(words(counter) & " ").X
                End If
            End If

        Next
        Return True
    End Function

    Public Sub getMessageInput()
        If wait = True Then
            If (controls.back.justPressed(Me) Or controls.interact.justPressed(Me)) Then closeMessage() : Exit Sub
        Else
            If choices.Count > 0 And choice = "" Then
                'CONTROLS FOR CHOICE SYSTEM
                If controls.left.justPressed(Me) And currentChoice > 0 Then gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : currentChoice -= 1
                If controls.right.justPressed(Me) And currentChoice < choices.Count - 1 Then gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : currentChoice += 1
                If controls.interact.justPressed(Me) Then gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2) : choice = choices(currentChoice) : closeMessage() : interpretChoice()
            End If
            'LETS PLAYER SKIP TEXT
            If (controls.back.justPressed(Me) Or controls.interact.justPressed(Me)) And incomingTextProgress < incomingText.Length - 1 And incomingTextProgress <> 0 Then
                incomingTextProgress = incomingText.Length - 2
                For counter As Integer = 0 To incomingText.Length - 1
                    If incomingText(counter) = "`" Then incomingTextProgress = counter : Exit For
                Next

            End If
            'EXITS TEXT PROMPT THAT DOES NOT HAVE CHOICES
            If (controls.back.justPressed(Me) Or controls.interact.justPressed(Me)) And incomingTextProgress >= incomingText.Length - 1 And choices.Count = 0 Then closeMessage()
        End If
    End Sub
    Public Sub interpretChoice()

        If interact.type.id = 17.2F Then
            If choice = "Yes" Then
                If c.area.brightness = 20 Then recieveMessage("The machine is broken") : Exit Sub
                c.gamestate = "none"
                commandPrompt.simulateCommand(Me, "/addEffect earthquake")
                commandPrompt.simulateCommand(Me, "/playSound misc/machine")
                commandPrompt.executeCommandLater(Me, "/addEffect lightning", 1600)
                commandPrompt.executeCommandLater(Me, "/setAreaBrightness 20", 1700)
                commandPrompt.executeCommandLater(Me, "/stopSounds", 2000)
                commandPrompt.executeCommandLater(Me, "/playSound misc/crash", 2000)
                commandPrompt.executeCommandLater(Me, "/removeEffect all", 2000)
                commandPrompt.executeCommandLater(Me, "/sendMessage " & c.name & " The Power Is Out!", 5000)
            End If
            If choice = "No" Then
                recieveMessage("You left the machine alone")
            End If

        End If
        If interact.type.id = 18 Then
            If choice = "Yes" Then
                c.gamestate = "none"
                commandPrompt.simulateCommand(Me, "/addEffect fadeOut 2")
                commandPrompt.executeCommandLater(Me, "/settime day", 2100)
                commandPrompt.executeCommandLater(Me, "/addEffect fadeIn 2", 2200)
                commandPrompt.executeCommandLater(Me, "/sendMessage " & c.name & " You feel well rested, HP fully restored.", 2600)
                commandPrompt.executeCommandLater(Me, "/heal", 2600)
            End If
            If choice = "No" Then
                recieveMessage("You left the bed alone")
            End If
        End If

    End Sub
End Class