Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class GUI
    Public Property coords As message : Public Property areaName As message : Public Property time As message : Public Property frameRate As message
    Public Property messages As New List(Of message) : Public Property chat As New List(Of String)

    Public Sub New(p As player)
        coords = New message(New Vector2(8, 0), "test", Game.defaultFont2, Color.White, True, False) : messages.Add(coords)
        areaName = New message(New Vector2(8, Game.defaultFont2.MeasureString(" ").Y * 0.7), "", Game.defaultFont2, Color.White, True, False) : messages.Add(areaName)
        time = New message(New Vector2(p.c.camera.viewport.Width - (8 + Game.defaultFont2.MeasureString("Time:   " & Game.time.ToString(("hh\:mm"))).X), 0), "", Game.defaultFont2, Color.White, True, False) : messages.Add(time)
        frameRate = New message(New Vector2(p.c.camera.viewport.Width - (8 + Game.defaultFont2.MeasureString("60").X), Game.defaultFont2.MeasureString(" ").Y * 0.7), "", Game.defaultFont2, Color.White, True, False) : messages.Add(frameRate)
    End Sub

    Public Sub addChatMessage(p As player, text As String, Optional temporary As Boolean = False)
        If temporary = False And p.debugmode = False Then Exit Sub
        If temporary = True Then chat.Add(text)
        If p.paused Or p.typing Then Exit Sub

        'MOVES PREVIOUS CHAT MESSAGES UP TO MAKE ROOM FOR NEW ONE
        For counter As Integer = 0 To messages.Count - 1
            If messages(counter).moveWithChat Then messages(counter).location = New Vector2(messages(counter).location.X, messages(counter).location.Y - messages(counter).textFont.MeasureString(" ").Y * 0.7)
            If messages(counter).location.Y < p.c.camera.viewport.Height / 2 Then messages(counter).countDown = True : messages(counter).displayTime = 0
        Next

        'LIMITS MESSAGE LENGTH TO THREE/FOURTHS THE SCREEN
        If Game.defaultFont2.MeasureString(text).X > p.c.camera.viewport.Width - p.c.camera.viewport.Width / 4 Then
            Dim width As Integer = Game.defaultFont2.MeasureString(text).X
            Do Until width < p.c.camera.viewport.Width - p.c.camera.viewport.Width / 4
                text = text.Substring(0, text.Length - 2)
                width = Game.defaultFont2.MeasureString(text).X
            Loop : text = text & "..."
        End If

        If contains(text) = False Then messages.Add(New message(New Vector2(Game.tilesize, p.c.camera.viewport.Height - 30 - Game.defaultFont2.MeasureString(" ").Y * 0.7), text, Game.defaultFont2, Color.White, False, True, 200, 8))
    End Sub
    Public Sub addMessage(m As message)
        If contains(m.text) = False Then
            messages.Add(m)
        Else
            For counter As Integer = 0 To messages.Count - 1
                If messages(counter).text = m.text Then messages(counter).displayTime = m.displayTime : messages(counter).countDown = False
            Next
        End If
    End Sub
    Public Sub removeMessage(t As String, Optional fadeOut As Boolean = False, Optional fadeRate As Integer = 0)
        For counter As Integer = 0 To messages.Count - 1
            If messages(counter).text = t Then
                If fadeOut = False Then messages.RemoveAt(counter) : Exit Sub
                If fadeOut = True Then messages(counter).countDown = True : messages(counter).displayTime = 0 : If fadeRate <> 0 Then messages(counter).fadeRate = fadeRate
            End If
        Next
    End Sub

    Public Function contains(text As String) As Boolean
        For counter As Integer = 0 To messages.Count - 1
            If messages(counter).text = text Then Return True
        Next
        Return False
    End Function

    Public Sub processMessages()
        For counter As Integer = 0 To messages.Count - 1
            messages(counter).update()
            If messages(counter).alpha < 0 Then messages.Remove(messages(counter)) : Exit For
        Next
    End Sub
    Public Sub processTextElements(p As player)
        If p.c.gamestate = "mapEdit" Then
            p.GUI.coords.text = "Mouse Coords: " & CInt((p.c.camera.location.X + p.mouseState.X) / Game.tilesize - 1) & " , " & CInt((p.c.camera.location.Y + p.mouseState.Y) / Game.tilesize - 1)
        Else : p.GUI.coords.text = "Coordinates: " & p.c.location.X & " , " & p.c.location.Y
        End If

        'REMOVES TEMPORARY CHAT MESSAGES WHEN CHAT IS OPENED
        If p.typing Then
            For counter As Integer = 0 To p.GUI.messages.Count - 1
                If p.GUI.messages(counter).moveWithChat Then p.GUI.messages.RemoveAt(counter) : Exit For
            Next
        End If

        'DISPLAYS TIME
        Dim hours As Integer = Game.time.ToString("hh")
        If hours >= 12 Then
            If hours > 12 Then hours -= 12
            time.text = "Time: " & hours & ":" & Game.time.ToString("mm") & "pm"
        Else
            If hours = 0 Then hours = 12
            time.text = "Time: " & hours & ":" & Game.time.ToString("mm") & "am"
        End If

        time.location = New Vector2(p.c.camera.viewport.Width - time.textFont.MeasureString(time.text).X - 10, time.location.Y)

        frameRate.text = "FPS: " & (Game.frames.FramesPerSecond)
        frameRate.location = New Vector2(p.c.camera.viewport.Width - frameRate.textFont.MeasureString(frameRate.text).X - 10, frameRate.location.Y)
    End Sub

End Class
Public Class message
    Public Property location As Vector2 : Public Property text As String : Public Property textFont As SpriteFont : Public Property color As Color
    Public Property displayTime As Integer : Public Property fadeRate As Integer : Public Property alpha As Integer
    Public Property countDown As Boolean : Public Property debug As Boolean : Public Property moveWithChat As Boolean

    Public Sub New(l As Vector2, t As String, f As SpriteFont, co As Color, Optional d As Boolean = False, Optional m As Boolean = False, Optional dt As Integer = 0, Optional fr As Integer = 0)
        location = l : text = t : textFont = f : color = co

        debug = d : moveWithChat = m : displayTime = dt : fadeRate = fr
        countDown = False
        If fr = 0 Then alpha = 100
    End Sub

    Public Sub update()
        If fadeRate <> 0 Then
            If countDown Then
                If displayTime > 0 Then displayTime -= 1
                If displayTime <= 0 Then alpha -= fadeRate
            Else
                If displayTime > 0 And alpha <= 100 Then alpha += fadeRate
                If alpha > 100 Then alpha = 100 : countDown = True
            End If
        End If
    End Sub
End Class

Public Class menu
    Public Property elements As New List(Of Object)
    Public Property selectedElement As Object
    Public Property name As String
    Public Property justClicked As Boolean
    '10 = PADDING BETWEEN ELEMENTS
    Public Sub New(p As player, n As String)
        name = n : justClicked = True
        elements.Clear()
        Select Case name
            Case "title"
                elements.Add(New image("TitleImage", New Vector2(400, 200), 0, FileManager.createTexture(Game.directory & "\textures\misc\title.png")))
                elements.Add(New button("Load", New Vector2(250, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Load Save", True))
                elements.Add(New button("Options", New Vector2(150, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Options", True))
                elements.Add(New button("Quit", New Vector2(150, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Quit Game", True))
                elements.Add(New label("Version Label", New Vector2(250, 50), 3, Game.defaultFont2, "Version: Alpha 1.7"))
            Case "mainPaused"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Main Menu"))
                elements.Add(New button("", New Vector2(250, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Return to Game", True))
                elements.Add(New button("", New Vector2(200, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Save Game", p.c.area.saveAble))
                elements.Add(New button("", New Vector2(200, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Load Save", True))
                elements.Add(New button("", New Vector2(150, 50), 3, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Options", True))
                elements.Add(New button("", New Vector2(320, 50), 4, FileManager.createTexture(Game.directory & "\textures\buttons\320x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\320x50s.png"), Game.defaultFont2, "Save and Quit to Title", True))
            Case "Options"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Options"))
                elements.Add(New button("", New Vector2(200, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Video Settings", True))
                elements.Add(New button("", New Vector2(200, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Sound Settings", True))
                elements.Add(New button("", New Vector2(200, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Controls", True))
                elements.Add(New toggle("", New Vector2(305, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\305x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\305x50s.png"), Game.defaultFont2, "Debug", New List(Of String) From {{True}, {False}}, p.debugmode, True))
                elements.Add(New button("", New Vector2(305, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\305x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\305x50s.png"), Game.defaultFont2, "Players", False))
                elements.Add(New button("", New Vector2(150, 50), 3, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
            Case "Video Settings"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Video Settings"))
                elements.Add(New slider("", New Vector2(200, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\200x50b.png"), FileManager.createTexture(Game.directory & "\textures\buttons\slider.png"), FileManager.createTexture(Game.directory & "\textures\buttons\sliders.png"), Game.defaultFont2, "Brightness", 20, 100, Game.brightness, True))
                elements.Add(New toggle("", New Vector2(200, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, "Shadows", New List(Of String) From {{True}, {False}}, Game.doShadow, True))
                elements.Add(New button("", New Vector2(150, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
            Case "Sound Settings"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Sound Settings"))
                elements.Add(New slider("", New Vector2(250, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\250x50b.png"), FileManager.createTexture(Game.directory & "\textures\buttons\slider.png"), FileManager.createTexture(Game.directory & "\textures\buttons\sliders.png"), Game.defaultFont2, "Sound Effects", 0, 100, Game.soundVolume, True))
                elements.Add(New slider("", New Vector2(250, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\250x50b.png"), FileManager.createTexture(Game.directory & "\textures\buttons\slider.png"), FileManager.createTexture(Game.directory & "\textures\buttons\sliders.png"), Game.defaultFont2, "Music", 0, 100, Game.musicVolume, True))
                elements.Add(New button("", New Vector2(250, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Play Test Sound", False))
                elements.Add(New button("", New Vector2(250, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Play Test Tune", False))
                elements.Add(New button("", New Vector2(150, 50), 3, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
            Case "Controls"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Controls"))
                Dim controls As List(Of String) = Game.availableControls : controls.Insert(0, p.controlMethod)
                elements.Add(New toggle("", New Vector2(320, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\320x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\320x50s.png"), Game.defaultFont2, "Controls", controls, p.controlMethod, True))
                elements.Add(New button("", New Vector2(150, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
                elements.Add(New label("", New Vector2(250, 50), 3, Game.defaultFont2, "WASD - Movement"))
                elements.Add(New label("", New Vector2(250, 50), 3, Game.defaultFont2, "Shift - Sprint"))
                elements.Add(New label("", New Vector2(250, 50), 4, Game.defaultFont2, "F - Interact"))
                elements.Add(New label("", New Vector2(250, 50), 4, Game.defaultFont2, "Esc - Pause"))
                elements.Add(New label("", New Vector2(300, 50), 5, Game.defaultFont2, "E - Inventory"))
                elements.Add(New label("", New Vector2(300, 50), 5, Game.defaultFont2, "Enter - Use Item (From Inventory)"))
                elements.Add(New label("", New Vector2(250, 50), 6, Game.defaultFont2, "T - Command Prompt / Chat"))
                elements.Add(New label("", New Vector2(250, 50), 6, Game.defaultFont2, "X - Level Editor"))
                elements.Add(New label("", New Vector2(250, 50), 7, Game.defaultFont2, "F2 - Screenshot"))
                elements.Add(New label("", New Vector2(250, 50), 7, Game.defaultFont2, "F11 - Fullscreen"))
            Case "Saves"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Save Files"))
                Dim progress As Integer = 1
                Dim saves As String() = Directory.GetDirectories(Game.directory & "saves\")
                For counter As Integer = 0 To saves.Count - 1
                    Dim s = saves(counter).Split("\") : Dim name As String = s(s.Count - 1)
                    If name <> "quickSave" Then
                        elements.Add(New button("delete_" & name, New Vector2(50, 50), progress, FileManager.createTexture(Game.directory & "\textures\buttons\50x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\50x50s.png"), Game.defaultFont2, "-", False))
                        elements.Add(New button(name, New Vector2(200, 50), progress, FileManager.createTexture(Game.directory & "\textures\buttons\200x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\200x50s.png"), Game.defaultFont2, name, True))
                        elements.Add(New button(name & "_edit", New Vector2(50, 50), progress, FileManager.createTexture(Game.directory & "\textures\buttons\50x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\50x50s.png"), Game.defaultFont2, "+", False))
                        progress += 1
                    End If
                Next
                elements.Add(New button("", New Vector2(250, 50), progress, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Done", True))
                elements.Add(New button("", New Vector2(250, 50), progress, FileManager.createTexture(Game.directory & "\textures\buttons\250x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\250x50s.png"), Game.defaultFont2, "Create New World", p.debugmode))
            Case "Create New World"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Create New World"))
                elements.Add(New textBox("", New Vector2(305, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\305x50b.png"), Game.defaultFont3, "World Name", 15, True))
                elements.Add(New slider("", New Vector2(250, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\250x50b.png"), FileManager.createTexture(Game.directory & "\textures\buttons\slider.png"), FileManager.createTexture(Game.directory & "\textures\buttons\sliders.png"), Game.defaultFont2, "Start Area Width", 10, 200, 50, True))
                elements.Add(New slider("", New Vector2(250, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\250x50b.png"), FileManager.createTexture(Game.directory & "\textures\buttons\slider.png"), FileManager.createTexture(Game.directory & "\textures\buttons\sliders.png"), Game.defaultFont2, "Start Area Length", 10, 200, 50, True))
                elements.Add(New button("", New Vector2(150, 50), 3, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Create World!", True))
                elements.Add(New button("", New Vector2(150, 50), 3, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
                elements.Add(New label("", New Vector2(250, 50), 4, Game.defaultFont2, "This creates an empty world from which to start building!"))
            Case "Players"
                elements.Add(New label("", New Vector2(250, 50), 0, Game.defaultFont2, "Players"))
                elements.Add(New label("", New Vector2(250, 50), 1, Game.defaultFont2, "Nothing here yet, Sorry!"))
                'elements.Add(New textBox(New Vector2(305, 50), 1, FileManager.createTexture(Game.directory & "\textures\buttons\305x50b.png"), Game.defaultFont3, "Player Name", 15))
                elements.Add(New button("", New Vector2(150, 50), 2, FileManager.createTexture(Game.directory & "\textures\buttons\150x50.png"), FileManager.createTexture(Game.directory & "\textures\buttons\150x50s.png"), Game.defaultFont2, "Done", True))
            Case "Game Over"

        End Select

    End Sub

    Public Sub draw(p As player, sb As SpriteBatch)
        'DRAWS ELEMENTS
        For counter As Integer = 0 To elements.Count - 1
            If elements(counter).location = Nothing Then Continue For
            elements(counter).draw(p, sb)
        Next

    End Sub
    Public Sub getInput(p As player)
        If elements.Count = 0 Then Exit Sub
        'SELECTS BUTTON UPON PAGE LOAD
        If IsNothing(selectedElement) Then
            For counter As Integer = 0 To elements.Count - 1
                If elements(counter).selectable Then selectedElement = elements(counter) : selectedElement.selected = True : Exit For
            Next
        End If

        'SETS LOCATION FOR ELEMENTS
        Dim screen As New List(Of List(Of Object))
        Dim progress As Integer = 0 : Dim amount As Integer = 0

        'SORTS ELEMENTS INTO 2 DIMENTIONAL LIST
        Do Until amount = elements.Count
            Dim row As New List(Of Object)
            For counter As Integer = 0 To elements.Count - 1
                If elements(counter).priority = progress Then row.Add(elements(counter)) : amount += 1
            Next
            screen.Add(row) : progress += 1
        Loop
        progress = 0
        For counter As Integer = 0 To screen.Count - 1
            amount = 0
            'GETS ROW WIDTH IN PIXELS
            Dim rowWidth As Integer = 0
            For counter2 As Integer = 0 To screen(counter).Count - 1 : rowWidth += (screen(counter)(counter2).size.X + 10) : Next
            'PLACES ELEMENTS
            Dim taken As Integer = 0
            For counter2 As Integer = 0 To screen(counter).Count - 1
                If screen(counter)(counter2).size.Y > amount Then amount = screen(counter)(counter2).size.Y
                screen(counter)(counter2).location = New Vector2(p.c.camera.viewport.Width / 2 - rowWidth / 2 + taken, progress) : taken += screen(counter)(counter2).size.X + 10
            Next
            progress += amount + 10
        Next

        'CONTROLS

        'MOUSE CONTROLS
        If p.mouseState.LeftButton = ButtonState.Released Then justClicked = False
        Dim mp As Vector2 = New Vector2(p.mouseState.X + p.c.camera.viewport.X, p.mouseState.Y + p.c.camera.viewport.Y)

        'SELECTS ELEMENT
        If p.controlMethod = "keyboard/mouse" And p.typing = False Then
            'MOUSE HOVER
            If p.mouseState.LeftButton <> ButtonState.Pressed Then
                For counter As Integer = 0 To elements.Count - 1
                    If elements(counter).selectable Then
                        If elements(counter).isHoveredOver(mp) And mp <> New Vector2(p.oldMouseState.X, p.oldMouseState.Y) Then selectedElement.selected = False : selectedElement = elements(counter) : selectedElement.selected = True
                    End If
                Next
            End If

            'DESELECTS ELEMENT IF NOT HOVERED OVER OR NOT ENABLED
            If ((selectedElement.isHoveredOver(mp) = False And TypeOf selectedElement Is slider = False) And mp <> New Vector2(p.oldMouseState.X, p.oldMouseState.Y)) Or selectedElement.enabled = False Then selectedElement.selected = False
            If TypeOf selectedElement Is slider And p.mouseState.LeftButton <> ButtonState.Pressed And selectedElement.isHoveredOver(mp) = False Then selectedElement.selected = False
        End If

        'DESELECTS TEXT BOX IF CLIKING OUTSIDE OF BOX
        If p.typing And selectedElement.isHoveredOver(mp) = False And p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then selectedElement.selected = False : p.typing = False

        If selectedElement.enabled And selectedElement.selected Then

            'PRESSING BUTTONS/TOGGLES
            If TypeOf selectedElement Is button Or TypeOf selectedElement Is toggle Or TypeOf selectedElement Is textBox Then
                If p.controls.pressButton.justPressed(p) Then selectedElement.press(p)
                If p.controlMethod = "keyboard/mouse" And selectedElement.isHoveredOver(mp) And p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then selectedElement.press(p)
            End If

            'CHANGING SLIDER VALUES
            If TypeOf selectedElement Is slider Then
                If p.controls.downSlider.pressed(p) Then
                    selectedElement.moveslider(-1)
                End If
                If p.controls.upSlider.pressed(p) Then
                    selectedElement.moveslider(1)
                End If

                If p.controlMethod = "keyboard/mouse" And justClicked = False Then
                    If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then gameDebug.playSound("sfx/menu/click", "sound", Game.soundVolume, False)
                    If p.mouseState.LeftButton = ButtonState.Pressed Then selectedElement.moveSlider(mp)
                End If
            End If

            'TYPING INTO TEXTBOX
            If TypeOf selectedElement Is textBox And p.typing Then
                If p.controls.exitTextBox.justPressed(p) Then selectedElement.selected = False : p.typing = False
                selectedElement.console.getKeyboardInput(p)

                'OFFSETS MOUSE CLICK DETECTION TO COMPENSATE FOR CENTERED TEXT
                selectedElement.console.location = New Vector2(selectedElement.location.x + selectedElement.size.x / 2 - selectedElement.console.textFont.measureString(selectedElement.console.text).x / 2, selectedElement.location.y)

                selectedElement.console.getMouseInput(p)
                Exit Sub
            End If
        End If

        'BACK/EXITING MENUS
        If p.controls.prevMenu.justPressed(p) Then
            If p.pauseMenu.name = "mainPaused" Then p.paused = False Else Dim b As New button : b.text = "Done" : b.enabled = True : b.press(p)
        End If

        'GETS INDEX OF SELECTED ELEMENT
        Dim selectedIndex As Integer = 0
        For counter As Integer = 0 To elements.Count - 1
            If elements(counter).selectable Then If elements(counter).text = selectedElement.text Then selectedIndex = counter
        Next

        'UP
        If p.controls.up.justPressed(p) = True Then
            Dim a As Integer = 1
            Do While a < screen.Count
                For counter As Integer = 0 To elements.Count - 1
                    If selectedElement.priority - a < 0 Then Continue For
                    If elements(counter).priority = selectedElement.priority - a And elements(counter).selectable And elements(counter).enabled Then selectedElement.selected = False : selectedElement = elements(counter) : selectedElement.selected = True : Exit Do
                Next : a += 1
            Loop
        End If
        'DOWN
        If p.controls.down.justPressed(p) = True Then
            Dim a As Integer = 1
            Do While a < screen.Count
                For counter As Integer = 0 To elements.Count - 1
                    If selectedElement.priority + a > screen.Count - 1 Then Continue For
                    If elements(counter).priority = selectedElement.priority + a And elements(counter).selectable And elements(counter).enabled Then selectedElement.selected = False : selectedElement = elements(counter) : selectedElement.selected = True : Exit Do
                Next : a += 1
            Loop
        End If
        'LEFT
        If p.controls.left.justPressed(p) = True Then
            Dim a As Integer = 1
            Do While a < screen(selectedElement.priority).Count
                For counter As Integer = 0 To screen(selectedElement.priority).Count - 1
                    If selectedIndex - a < 0 Then Exit Do
                    If elements(selectedIndex - a).priority <> selectedElement.priority Then Exit For
                    If elements(selectedIndex - a).priority = selectedElement.priority And elements(selectedIndex - a).selectable And elements(selectedIndex - a).enabled Then selectedElement.selected = False : selectedElement = elements(selectedIndex - a) : selectedElement.selected = True : Exit Do
                Next : a += 1
            Loop
        End If
        'RIGHT
        If p.controls.right.justPressed(p) = True And selectedIndex + 1 < elements.Count Then
            Dim a As Integer = 1
            Do While a < screen(selectedElement.priority).Count
                For counter As Integer = 0 To screen(selectedElement.priority).Count - 1
                    If selectedIndex + a > elements.Count - 1 Then Exit Do
                    If elements(selectedIndex + a).priority <> selectedElement.priority Then Exit For
                    If elements(selectedIndex + a).priority = selectedElement.priority And elements(selectedIndex + a).selectable And elements(selectedIndex + a).enabled Then selectedElement.selected = False : selectedElement = elements(selectedIndex + a) : selectedElement.selected = True : Exit Do
                Next : a += 1
            Loop
        End If

    End Sub

    Public Function findElement(n As String) As Object
        For counter As Integer = 0 To elements.Count - 1
            If elements(counter).name = n Then Return elements(counter)
        Next
        Return False
    End Function
End Class
Public Class image
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property texture As Texture2D
    Public Property selectable As Boolean = False : Public Property enabled As Boolean

    Public Sub New(n As String, s As Vector2, p As Integer, img As Texture2D)
        name = n : location = New Vector2 : size = s : priority = p
        texture = img
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        sb.Draw(texture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)
    End Sub
End Class
Public Class space
    Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property selectable As Boolean = False : Public Property enabled As Boolean

    Public Sub New(s As Vector2, p As Integer)
        location = New Vector2 : size = s : priority = p
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        'NOTHING DRAWS BECAUSE SPACES ARE JUST PLACEHOLDERS
    End Sub
End Class
Public Class label
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)

    Public Property font As SpriteFont : Public Property text As String
    Public Property selectable As Boolean = False : Public Property enabled As Boolean

    Public Sub New(n As String, s As Vector2, p As Integer, f As SpriteFont, t As String)
        name = n : location = New Vector2 : size = s : priority = p
        font = f : text = t
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        Dim newLoc As Vector2 = New Vector2(location.X + size.X / 2 - font.MeasureString(text).X / 2, location.Y + size.Y / 2 - font.MeasureString(text).Y / 2)
        sb.DrawString(font, text, newLoc, Color.White, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0009)
        sb.DrawString(font, text, New Vector2(newLoc.X + 2, newLoc.Y + 2), Color.Black * 0.8, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.00091)
    End Sub
End Class
Public Class button
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property texture As Texture2D : Public Property enabled As Boolean

    Public Property selected As Boolean : Public Property selectedTexture As Texture2D
    Public Property font As SpriteFont : Public Property text As String
    Public Property selectable As Boolean = True

    Public Sub New()

    End Sub
    Public Sub New(n As String, s As Vector2, p As Integer, img As Texture2D, selectImg As Texture2D, f As SpriteFont, t As String, e As Boolean)
        name = n : location = New Vector2 : size = s : priority = p
        texture = img : selectedTexture = selectImg
        font = f : text = t : enabled = e
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        'DRAWS BUTTON FACE
        Dim e As Double = 1 : If enabled = False Then e = 0.5
        If selected = False Then sb.Draw(texture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White * e, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)
        If selected = True Then sb.Draw(selectedTexture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White * e, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)

        'DRAWS TEXT
        Dim col As Color = Color.White : If selected Then col = Color.LightGoldenrodYellow
        Dim newLoc As Vector2 = New Vector2(location.X + size.X / 2 - font.MeasureString(Replace(text, "_", " ")).X / 2, location.Y + size.Y / 2 - font.MeasureString(Replace(text, "_", " ")).Y / 2)
        sb.DrawString(font, Replace(text, "_", " "), newLoc, col * e, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0009)
        sb.DrawString(font, Replace(text, "_", " "), New Vector2(newLoc.X + 2, newLoc.Y + 2), Color.Black * 0.8 * e, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.00091)
    End Sub
    Public Sub press(p As player)
        If enabled = False Then Exit Sub
        'HANDLES PREVIOUS MENU SCREENS
        If p.previousMenus.Count = 0 Then p.previousMenus.Add(p.pauseMenu.name)
        If p.previousMenus.Count > 1 And p.pauseMenu.name = p.previousMenus(p.previousMenus.Count - 1) Then p.previousMenus.RemoveAt(p.previousMenus.Count - 1)
        If text <> "Done" And p.pauseMenu.name <> p.previousMenus(p.previousMenus.Count - 1) Then p.previousMenus.Add(p.pauseMenu.name)

        gameDebug.playSound("sfx/menu/click", "sound", Game.soundVolume, False)
        Select Case text
            Case "Return to Game" : p.paused = False
            Case "Done" : If p.previousMenus.Count > 0 Then p.pauseMenu = New menu(p, p.previousMenus(p.previousMenus.Count - 1))
            Case "Options" : p.pauseMenu = New menu(p, "Options")
            Case "Video Settings" : p.pauseMenu = New menu(p, "Video Settings")
            Case "Sound Settings" : p.pauseMenu = New menu(p, "Sound Settings")
            Case "Controls" : p.pauseMenu = New menu(p, "Controls")
            Case "Players" : p.pauseMenu = New menu(p, "Players")
            Case "Save Game" : p.paused = False : commandPrompt.simulateCommand(p, "/saveGame " & Game.saveName)
            Case "Load Save" : p.pauseMenu = New menu(p, "Saves")
            Case "Save and Quit to Title" : commandPrompt.simulateCommand(p, "/saveGame " & Game.saveName) : Game.returnTitle = True
            Case "Quit Game"
                If Game.players.Count = 1 Then
                    Process.GetCurrentProcess().CloseMainWindow()
                Else : FileManager.savePlayer("quickSave", p) : p.leaving = True
                    For counter As Integer = 0 To Game.players.Count - 1 : Game.players(counter).GUI.addChatMessage(p, p.c.name & " left the game") : Next
                End If
            Case "Create New World"
                p.pauseMenu = New menu(p, "Create New World")
            Case "Create World!"
                Dim saveName As String = ""
                For counter As Integer = 0 To p.pauseMenu.elements.Count - 1
                    If p.pauseMenu.elements(counter).text = "World Name" Then saveName = Replace(p.pauseMenu.elements(counter).console.text, " ", "_")
                Next : If saveName = "" Then Exit Sub
                Dim x As Integer = 10 : Dim y As Integer = 10
                For counter As Integer = 0 To p.pauseMenu.elements.Count - 1
                    If p.pauseMenu.elements(counter).text.contains("Start Area Width") Then x = p.pauseMenu.elements(counter).value
                    If p.pauseMenu.elements(counter).text.contains("Start Area Length") Then y = p.pauseMenu.elements(counter).value
                Next

                'NEED TO ADD FUNCTIONALITY FOR MULTIPLAYER
                FileManager.createNewSave(saveName, saveName & "_Start", New Vector2(x, y))
                FileManager.load(saveName, "ThirtyVirus") : Game.players(0).controlMethod = p.controlMethod : Game.players(0).debugmode = p.debugmode
            Case Else
                If p.pauseMenu.name = "Saves" Then commandPrompt.simulateCommand(p, "/addEffect fadeMusicOut 2 all") : FileManager.load(text, "ThirtyVirus") : Game.players(0).controlMethod = p.controlMethod : Game.players(0).debugmode = p.debugmode

        End Select
    End Sub

    Public Function isHoveredOver(mp As Vector2) As Boolean
        If enabled = False Then Return False
        If mp.X > location.X And mp.Y > location.Y And mp.X < location.X + size.X And mp.Y < location.Y + size.Y Then Return True
        Return False
    End Function
End Class
Public Class toggle
    'TOGGLES ARE BUTTONS THAT CHANGE TEXT AND GAME PROPERTIES WHEN PRESSED
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property texture As Texture2D : Public Property enabled As Boolean

    Public Property selected As Boolean : Public Property selectedTexture As Texture2D
    Public Property font As SpriteFont : Public Property text As String
    Public Property values As List(Of String) : Public Property currentValue As Integer
    Public Property selectable As Boolean = True

    Public Sub New(n As String, s As Vector2, p As Integer, img As Texture2D, selectImg As Texture2D, f As SpriteFont, t As String, v As List(Of String), cv As String, e As Boolean)
        name = n : location = New Vector2 : size = s : priority = p
        texture = img : selectedTexture = selectImg
        font = f : text = t : values = v
        enabled = e
        For counter As Integer = 0 To values.Count - 1
            If values(counter) = cv Then currentValue = counter
        Next
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        'DRAWS BUTTON FACE
        If selected = False Then sb.Draw(texture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)
        If selected = True Then sb.Draw(selectedTexture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)

        'DRAWS TEXT
        Dim col As Color = Color.White : If selected Then col = Color.LightGoldenrodYellow
        Dim newLoc As Vector2 = New Vector2(location.X + size.X / 2 - font.MeasureString(text & ": " & values(currentValue)).X / 2, location.Y + size.Y / 2 - font.MeasureString(text & ": " & values(currentValue)).Y / 2)
        sb.DrawString(font, text & ": " & values(currentValue), newLoc, col, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0009)
        sb.DrawString(font, text & ": " & values(currentValue), New Vector2(newLoc.X + 2, newLoc.Y + 2), Color.Black * 0.8, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.00091)
    End Sub
    Public Sub press(p As player)
        If enabled = False Then Exit Sub
        gameDebug.playSound("sfx/menu/click", "sound", Game.soundVolume, False)
        If text = "Controls" Then values = Game.getAvailableControls
        If currentValue < values.Count - 1 Then currentValue += 1 Else currentValue = 0

        'GIVES FUNCTIONALITY TO TOGGLES DEPENDING ON TEXT
        Select Case text
            Case "Debug" : p.debugmode = values(currentValue)
            Case "Controls" : p.setControls(values(currentValue))
            Case "Shadows" : Game.doShadow = values(currentValue)

        End Select
    End Sub

    Public Function isHoveredOver(mp As Vector2) As Boolean
        If mp.X > location.X And mp.Y > location.Y And mp.X < location.X + size.X And mp.Y < location.Y + size.Y Then Return True
        Return False
    End Function
End Class
Public Class slider
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property minValue As Integer : Public Property maxValue As Integer
    Public Property value As Integer : Public Property enabled As Boolean

    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property texture As Texture2D
    Public Property sliderTexture As Texture2D : Public Property selectedSliderTexture As Texture2D
    Public Property selected As Boolean

    Public Property font As SpriteFont : Public Property text As String
    Public Property selectable As Boolean = True

    Public Sub New(n As String, s As Vector2, p As Integer, img As Texture2D, slTexture As Texture2D, slsTexture As Texture2D, f As SpriteFont, t As String, minV As Integer, maxV As Integer, v As Integer, e As Boolean)
        name = n : location = New Vector2 : size = s : priority = p
        texture = img : sliderTexture = slTexture : selectedSliderTexture = slsTexture
        font = f : text = t : value = v : minValue = minV : maxValue = maxV
        enabled = e
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        'DRAWS BUTTON FACE
        sb.Draw(texture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)

        Dim x As Integer = (size.X - 14) / maxValue * value
        If selected Then
            sb.Draw(selectedSliderTexture, New Rectangle(location.X + x, location.Y, 15, 50), New Rectangle(0, 0, 15, 50), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.0009)
        Else
            sb.Draw(sliderTexture, New Rectangle(location.X + x, location.Y, 15, 50), New Rectangle(0, 0, 15, 50), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.0009)
        End If

        'DRAWS TEXT
        Dim second As String = value
        If value = minValue Then second = "min"
        If value = maxValue Then second = "max"

        Dim col As Color = Color.White : If selected Then col = Color.LightGoldenrodYellow
        Dim newLoc As Vector2 = New Vector2(location.X + size.X / 2 - font.MeasureString(text & ": " & second).X / 2, location.Y + size.Y / 2 - font.MeasureString(text & ": " & second).Y / 2)
        sb.DrawString(font, text & ": " & second, newLoc, col, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0008)
        sb.DrawString(font, text & ": " & second, New Vector2(newLoc.X + 2, newLoc.Y + 2), Color.Black * 0.8, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.00081)
    End Sub

    Public Sub moveSlider(mp As Vector2)
        Dim pos As Integer = maxValue * (mp.X - location.X - 7) / (size.X - 14)
        If pos >= minValue And pos <= maxValue Then
            value = pos
        Else
            If pos < minValue Then value = minValue
            If pos > maxValue Then value = maxValue
        End If

        changeVal()
    End Sub
    Public Sub moveSlider(change As Integer)
        Dim pos As Integer = value + change
        If pos >= minValue And pos <= maxValue Then value = pos

        changeVal()
    End Sub
    Public Sub changeVal()
        'CHANGES VALUES OF VARAIBLES BASED ON TEXT
        Select Case text
            Case "Sound Effects" : Game.soundVolume = value
            Case "Music" : Game.musicVolume = value : For counter As Integer = 0 To Game.music.Count - 1 : Game.music(counter).sound.Volume = value / 100 : Next
            Case "Brightness" : Game.brightness = value
        End Select
    End Sub

    Public Function isHoveredOver(mp As Vector2) As Boolean
        If mp.X > location.X And mp.Y > location.Y And mp.X < location.X + size.X And mp.Y < location.Y + size.Y Then Return True
        Return False
    End Function
End Class
Public Class textBox
    Public Property name As String : Public Property location As Vector2 : Public Property size As Vector2
    Public Property priority As Integer 'ORDER DISPLAYED/SELECTED IN MENU (Y)(0 IS FIRST)
    Public Property texture As Texture2D : Public Property enabled As Boolean
    Public Property text As String : Public Property console As console
    Public Property selected = True
    Public Property selectable As Boolean = True

    Public Sub New(n As String, s As Vector2, p As Integer, img As Texture2D, tf As SpriteFont, t As String, m As Integer, e As Boolean)
        name = n : location = New Vector2 : size = s : priority = p : texture = img
        text = t : console = New console(location, tf) : console.maxCharacters = m
        enabled = e
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)
        'DRAWS BACKGROUND
        sb.Draw(texture, New Rectangle(location.X, location.Y, size.X, size.Y), New Rectangle(0, 0, size.X, size.Y), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.001)

        'DRAWS DEFAULT TEXT IF NOT SELECTED AND TEXT EQUALS NOTHING
        If console.text = "" And (selected = False Or (selected = True And p.typing = False)) Then
            Dim co As Color = Color.White : If selected Then co = Color.LightGoldenrodYellow
            sb.DrawString(console.textFont, text, New Vector2(location.X + (size.X - console.textFont.MeasureString(text).X) / 2, location.Y), co * 0.7, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.0008)
            sb.DrawString(console.textFont, text, New Vector2(location.X + 2 + (size.X - console.textFont.MeasureString(text).X) / 2, location.Y + 2), Color.Black * 0.8, 0, New Vector2(0, 0), 1, SpriteEffects.None, 0.00081)
        End If

        'CENTERS TEXT IN BOX
        Dim offset As Integer = console.textFont.MeasureString(console.text).X / 2 - size.X / 2

        'DRAWS TEXT
        If console.text.Length > 0 Then
            sb.DrawString(console.textFont, console.text, New Vector2(location.X - offset, location.Y + 2), Color.White, 0, New Vector2, 1, SpriteEffects.None, 0.0009)
            sb.DrawString(console.textFont, console.text, New Vector2(location.X - offset + 2, location.Y + 4), Color.Black * 0.8, 0, New Vector2, 1, SpriteEffects.None, 0.00091)
        End If

        'DRAWS INPUT POSITION
        If p.typing Then
            sb.DrawString(console.textFont, "|", New Vector2(location.X + console.textFont.MeasureString(console.text.Substring(0, console.inputPos)).X - offset - console.textFont.MeasureString("|").X / 2, location.Y), Color.Red, 0, New Vector2, 1, SpriteEffects.None, 0.0009)
            sb.DrawString(console.textFont, "|", New Vector2(location.X + console.textFont.MeasureString(console.text.Substring(0, console.inputPos)).X - offset - console.textFont.MeasureString("|").X / 2 + 2, location.Y + 2), Color.Black * 0.8, 0, New Vector2, 1, SpriteEffects.None, 0.00091)
        End If

    End Sub
    Public Sub press(p As player)
        selected = True : p.typing = True
    End Sub

    Public Function isHoveredOver(mp As Vector2) As Boolean
        If mp.X > location.X And mp.Y > location.Y And mp.X < location.X + size.X And mp.Y < location.Y + size.Y Then Return True
        Return False
    End Function
End Class
Public Class dropDown
    Public Property name As String
End Class

Public Class Inventory
    Public Property items As New List(Of item) : Public Property selectedItem As item : Public Property money As Integer
    Public Property upHoldTime As Integer : Public Property downHoldTime As Integer

    Public Sub add(ByRef item As item, Optional quantity As Integer = 0)
        If item.type.id = 3 Then money += item.quantity : Exit Sub 'CONVERTS COIN ITEM TO CURRENCY
        If quantity <> 0 Then item.quantity = quantity

        For counter As Integer = 0 To items.Count - 1
            If items(counter).type.id = item.type.id And items(counter).quantity < items(counter).type.maximumStack Then
                If (items(counter).quantity + item.quantity) <= item.type.maximumStack Then items(counter).quantity += item.quantity : Exit Sub
                If (items(counter).quantity + item.quantity) > item.type.maximumStack Then Dim a As Integer = items(counter).quantity : items(counter).quantity = items(counter).type.maximumStack : item.quantity -= (items(counter).type.maximumStack - a)
            End If : Next

        If item.quantity > item.type.maximumStack Then
            Do Until item.quantity <= item.type.maximumStack
                Dim a As Integer = item.quantity - item.type.maximumStack
                item.quantity = item.type.maximumStack
                items.Add(item.copy) : item.quantity = a

                If items.Count = 1 Then selectedItem = items(0)
            Loop : End If
        If item.quantity <= item.type.maximumStack Then items.Add(item)

        sort()
    End Sub
    Public Sub remove(ByRef item As item, Optional quantity As Integer = 0)
        If quantity = 0 Then items.Remove(item)
        If quantity > 0 Then item.quantity -= quantity
        If item.quantity < 1 Then items.Remove(item)

        If item.location.Y = items.Count And items.Count > 0 Then item = items(item.location.Y - 1)
        If item.location.Y >= 0 And items.Count > 0 Then item = items(item.location.Y)

        sort()
    End Sub

    Public Sub sort()
        Dim names As New List(Of String) : For Each item As item In items : names.Add(item.type.name) : Next : names.Sort() 'makes the sorted name list
        Dim newInv As New List(Of item) : Dim counter As Integer = 0
        Dim n As Integer = items.Count
        Do Until counter = n
            For Each item As item In items
                If item.type.name = names(counter) Then item.location = New Vector2(item.location.X, counter) : items.Remove(item) : newInv.Add(item) : Exit For
            Next : counter += 1 : Loop : items = newInv

        'HANDLES SELECTED ITEM
        If items.Count = 1 Then selectedItem = items(0)
        For Each item As item In items
            If item.type.name = selectedItem.type.name Then selectedItem = item : Exit Sub
        Next
        If selectedItem.location.Y = 0 Then
            For Each item As item In items
                If item.location.Y = selectedItem.location.Y Then selectedItem = item : Exit Sub
            Next : End If
        If selectedItem.location.Y >= 1 Then
            For Each item As item In items
                If item.location.Y = selectedItem.location.Y - 1 Then selectedItem = item : Exit Sub
            Next : End If
    End Sub
    Public Sub takeAll(takeInv As Inventory)
        For counter As Integer = 0 To takeInv.items.Count - 1
            add(takeInv.items(counter))
        Next
        takeInv.items.Clear()
    End Sub

    Public Sub draw(graphics As GraphicsDevice, sb As SpriteBatch, viewport As Viewport, name As String, dsi As Boolean)
        Dim prevViewport As Viewport = New Viewport(graphics.Viewport.X, graphics.Viewport.Y, graphics.Viewport.Width, graphics.Viewport.Height) : graphics.Viewport = viewport
        If IsNothing(selectedItem) Then selectedItem = New item

        Dim invOffset As Integer = (viewport.Height / 20 / 2 - 1) + (selectedItem.location.Y * -1) : If selectedItem.location.Y < (viewport.Height / 20 / 2 - 1) Then invOffset = 0
        Dim textRange As New Rectangle(170, 20, viewport.Width - 170, 100)

        sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, Nothing, Nothing)

        'DRAWS BACKGROUND IMAGE
        sb.Draw(Game.invPane, New Rectangle(0, 0, viewport.Width, viewport.Height), New Rectangle(0, 0, 250, 400), Color.White * 0.8, Nothing, Nothing, SpriteEffects.None, 0.0001) 'DRAWS BACKGROUND OF INVENTORY
        sb.DrawString(Game.defaultFont2, name, New Vector2(0, 0), Color.ForestGreen, 0, New Vector2(), 1, SpriteEffects.None, 0)

        'DRAWS ITEMS
        For counter As Integer = 0 To items.Count - 1
            Dim quantity As String = "" : If items(counter).quantity > 1 Then quantity = " (" & items(counter).quantity & ")"
            sb.DrawString(Game.defaultFont2, items(counter).type.name & quantity, New Vector2(10, items(counter).location.Y * 20 + 20 + (20 * invOffset)), Color.Black, 0, New Vector2(), 1, SpriteEffects.None, 0)
        Next

        If items.Count > 0 And dsi Then
            'DRAWS RED BOX BEHIND SELECTED ITEM
            sb.Draw(gameDebug.createBox(New Vector2(1, 1), Color.Red), New Rectangle(10, selectedItem.location.Y * 20 + 24 + (20 * invOffset), 150, 20), New Rectangle(0, 0, 1, 1), Color.White, 0, New Vector2(0, 0), SpriteEffects.None, 0.00001)
        End If

        'DRAWS CURRENCY
        If money > 0 Then sb.DrawString(Game.defaultFont2, "Money: " & money, New Vector2(viewport.Width - Game.defaultFont2.MeasureString("Money: " & money).X, viewport.Height - Game.defaultFont2.MeasureString(" ").Y), Color.Black, 0, New Vector2(), 1, SpriteEffects.None, 0)

        sb.End() : graphics.Viewport = prevViewport
    End Sub

    Public Sub getInput(ByRef p As player)
        If p.side = 0 Or p.side = 1 Then
            If p.controls.up.pressed(p) And p.controls.down.pressed(p) Then upHoldTime = 0
            If p.controls.up.pressed(p) Then upHoldTime += 1 Else upHoldTime = 0 'DETECTS IF YOU HOLD UP
            If (p.controls.up.justPressed(p) Or (upHoldTime > 20 And upHoldTime Mod 2 = 0)) And selectedItem.location.Y > 0 Then
                For Each i As item In items : If i.location.Y = selectedItem.location.Y - 1 Then selectedItem = i : gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : Exit Sub
                Next : Exit Sub : End If
            If p.controls.down.pressed(p) Then downHoldTime += 1 Else downHoldTime = 0 'DETECTS IF YOU HOLD DOWN
            If (p.controls.down.justPressed(p) Or (downHoldTime > 20 And downHoldTime Mod 2 = 0)) And selectedItem.location.Y < items.Count - 1 Then
                For Each i As item In items : If i.location.Y = selectedItem.location.Y + 1 Then selectedItem = i : gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : Exit Sub
                Next : Exit Sub : End If
            'DETECTS IF YOU PRESS ITEM TRANSFER
            If p.controls.itemTransfer.justPressed(p) And p.side = 1 And items.Count > 0 Then
                If p.c.gamestate <> "trading" Or (p.c.gamestate = "trading" And p.interact.inventory.money >= selectedItem.type.value) Then
                    p.interact.inventory.add(selectedItem.copy, 1) : If p.c.gamestate = "trading" Then p.c.inventory.money += selectedItem.type.value : p.interact.inventory.money -= selectedItem.type.value
                    remove(selectedItem, 1) : gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
                    If items.Count = 0 Then p.side = 2 : Exit Sub
                End If
            End If
        End If
        If p.side = 2 Then
            If p.controls.up.pressed(p) And p.controls.down.pressed(p) Then p.interact.inventory.upHoldTime = 0
            If p.controls.up.pressed(p) Then p.interact.inventory.upHoldTime += 1 Else p.interact.inventory.upHoldTime = 0 'DETECTS IF YOU HOLD UP
            If (p.controls.up.justPressed(p) Or (p.interact.inventory.upHoldTime > 20 And p.interact.inventory.upHoldTime Mod 2 = 0)) And p.interact.inventory.selectedItem.location.Y > 0 Then
                For Each i As item In p.interact.inventory.items : If i.location.Y = p.interact.inventory.selectedItem.location.Y - 1 Then p.interact.inventory.selectedItem = i : gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : Exit Sub
                Next : Exit Sub : End If
            If p.controls.down.pressed(p) Then p.interact.inventory.downHoldTime += 1 Else p.interact.inventory.downHoldTime = 0 'DETECTS IF YOU HOLD DOWN
            If (p.controls.down.justPressed(p) Or (p.interact.inventory.downHoldTime > 20 And p.interact.inventory.downHoldTime Mod 2 = 0)) And p.interact.inventory.selectedItem.location.Y < p.interact.inventory.items.Count - 1 Then
                For Each i As item In p.interact.inventory.items : If i.location.Y = p.interact.inventory.selectedItem.location.Y + 1 Then p.interact.inventory.selectedItem = i : gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2) : Exit Sub
                Next : Exit Sub : End If
            'DETECTS IF YOU PRESS ITEM TRANSFER
            If p.controls.itemTransfer.justPressed(p) And p.side = 2 And p.interact.inventory.items.Count > 0 Then
                If p.c.gamestate <> "trading" Or (p.c.gamestate = "trading" And p.c.inventory.money >= p.interact.inventory.selectedItem.type.Value) Then
                    add(p.interact.inventory.selectedItem.copy, 1) : If p.c.gamestate = "trading" Then p.interact.inventory.money += p.interact.inventory.selectedItem.type.Value : p.c.inventory.money -= p.interact.inventory.selectedItem.type.Value
                    p.interact.inventory.remove(p.interact.inventory.selectedItem, 1) : gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
                    If p.interact.inventory.items.Count = 0 Then p.side = 1 : Exit Sub
                End If
            End If
        End If

        'CHANGING SIDES
        If p.controls.right.justPressed(p) And p.side = 1 Then
            If p.interact.inventory.items.Count > 0 Then p.side = 2 : gameDebug.playSound("sfx/menu/buttonClick", "sound", Game.soundVolume / 2) : Exit Sub
        End If : If p.controls.left.justPressed(p) And p.side = 2 And items.Count > 0 Then p.side = 1 : gameDebug.playSound("sfx/menu/buttonClick", "sound", Game.soundVolume / 2) : Exit Sub

        If p.controls.itemTransfer.justPressed(p) And items.Count > 0 And p.side = 0 Then
            gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
            Dim newLoc As Vector2 = New Vector2
            If p.c.direction = 1 Then newLoc = New Vector2(p.c.location.X, p.c.location.Y - 1)
            If p.c.direction = 2 Then newLoc = New Vector2(p.c.location.X, p.c.location.Y + 1)
            If p.c.direction = 3 Then newLoc = New Vector2(p.c.location.X - 2, p.c.location.Y)
            If p.c.direction = 4 Then newLoc = New Vector2(p.c.location.X + 2, p.c.location.Y)

            If p.c.detectHit(newLoc) = False Then
                p.c.area.place(selectedItem.copy, newLoc)
                remove(selectedItem) : Exit Sub
            End If

        End If : If p.controls.useItem.justPressed(p) And p.side = 0 Then selectedItem.use(p) : Exit Sub
        'EXITING INVENTORY
        If p.controls.back.justPressed(p) Then
            If TypeOf p.interact Is terrainObject And p.c.gamestate <> "inventory" Then gameDebug.playSound(p.interact.type.closeSound, "sound", Game.soundVolume)
            If TypeOf p.interact Is character Then p.interact.gamestate = p.interact.prevGamestate
            p.side = 0 : p.c.gamestate = p.c.prevGamestate
        End If

        If p.controls.useItem.justPressed(p) And p.c.gamestate = "looting" Then takeAll(p.interact.inventory) : p.c.gamestate = "inGame"
    End Sub
    Public Sub getMouseInput(ByRef p As player)
        Dim mp As Vector2 = New Vector2(p.mouseState.X + p.c.camera.viewport.X, p.mouseState.Y + p.c.camera.viewport.Y)
        'MOUSE CONTROLS
        If p.side = 0 Then
            Dim inventorySize As Vector2 = New Vector2(p.c.camera.viewport.Width / 3 * 1.4, p.c.camera.viewport.Height / 2 * 1.4)
            Dim location As Vector2 = New Vector2((p.c.camera.viewport.Width - inventorySize.X) / 2, (p.c.camera.viewport.Height - inventorySize.Y) / 2)
            Dim mousePos As Integer = (mp.Y - location.Y + 5) / 20 - 1 : Dim y As Integer
            If p.c.inventory.selectedItem.location.Y <= 7 Then
                y = mousePos
            Else
                y = mousePos + p.c.inventory.selectedItem.location.Y - 7
            End If
            If selectedItem.location.Y >= 0 And selectedItem.location.Y < items.Count And y >= 0 And y < items.Count And mp.X > location.X And mp.X < (location.X + inventorySize.X) Then
                'BUTTON CLICKS
                If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then selectedItem.use(p) : Exit Sub
                If p.mouseState.RightButton = ButtonState.Pressed And p.oldMouseState.RightButton <> ButtonState.Pressed And items.Count > 0 And p.side = 0 Then
                    gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
                    Dim newLoc As Vector2 = New Vector2
                    If p.c.direction = 1 Then newLoc = New Vector2(p.c.location.X, p.c.location.Y - 1)
                    If p.c.direction = 2 Then newLoc = New Vector2(p.c.location.X, p.c.location.Y + 1)
                    If p.c.direction = 3 Then newLoc = New Vector2(p.c.location.X - 2, p.c.location.Y)
                    If p.c.direction = 4 Then newLoc = New Vector2(p.c.location.X + 2, p.c.location.Y)

                    If p.c.detectHit(newLoc) = False Then
                        p.c.area.place(selectedItem.copy, newLoc)
                        remove(selectedItem) : Exit Sub
                    End If
                End If
                'CHANGING SELECTED ITEM
                If p.mouseState.X <> p.oldMouseState.X Or p.mouseState.Y <> p.oldMouseState.Y Then
                    If y <> selectedItem.location.Y Then gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume / 2, False, 2)
                    For Each i As item In items : If i.location.Y = y Then selectedItem = i : Exit Sub
                    Next : Exit Sub : End If : End If : End If

        'LOOTING OR TRADING
        If p.side = 1 Or p.side = 2 Then
            Dim inventorySize As Vector2 = New Vector2(p.c.camera.viewport.Width / 3 * 1.4, p.c.camera.viewport.Height / 2 * 1.4)
            Dim location As Vector2 = New Vector2((p.c.camera.viewport.Width - inventorySize.X) / 2 - inventorySize.X / 2, (p.c.camera.viewport.Height - inventorySize.Y) / 2)
            Dim location2 As Vector2 = New Vector2((p.c.camera.viewport.Width - inventorySize.X) / 2 + inventorySize.X / 2, (p.c.camera.viewport.Height - inventorySize.Y) / 2)

            'CHANGING SIDES
            If p.mouseState.X <> p.oldMouseState.X Or p.mouseState.Y <> p.oldMouseState.Y Then
                If mp.Y > location.Y And mp.Y < (location.Y + inventorySize.Y) And mp.X > location.X And mp.X < (location.X + inventorySize.X) And items.Count > 0 And p.side <> 1 Then p.side = 1 : gameDebug.playSound("sfx/menu/buttonClick", "sound", Game.soundVolume / 2)
                If mp.Y > location2.Y And mp.Y < (location2.Y + inventorySize.Y) And mp.X > location2.X And mp.X < (location2.X + inventorySize.X) And p.interact.inventory.items.Count > 0 And p.side <> 2 Then p.side = 2 : gameDebug.playSound("sfx/menu/buttonClick", "sound", Game.soundVolume / 2)
            End If

            If p.side = 1 Then
                Dim mousePos As Integer = (mp.Y - location.Y + 5) / 20 - 1 : Dim y As Integer
                If p.c.inventory.selectedItem.location.Y <= 7 Then y = mousePos Else y = mousePos + p.c.inventory.selectedItem.location.Y - 7

                If selectedItem.location.Y >= 0 And selectedItem.location.Y < items.Count And y >= 0 And y < items.Count And mp.X > location.X And mp.X < (location.X + inventorySize.X) Then
                    'BUTTON CLICKS
                    If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed And items.Count > 0 Then
                        If p.c.gamestate <> "trading" Or (p.c.gamestate = "trading" And p.interact.inventory.money >= selectedItem.type.value) Then
                            p.interact.inventory.add(selectedItem.copy, 1) : If p.c.gamestate = "trading" Then p.c.inventory.money += selectedItem.type.value : p.interact.inventory.money -= selectedItem.type.value
                            remove(selectedItem, 1) : gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
                            If items.Count = 0 Then p.side = 2 : Exit Sub
                        End If
                    End If
                    'CHANGING SELECTED ITEM
                    If p.mouseState.X <> p.oldMouseState.X Or p.mouseState.Y <> p.oldMouseState.Y Then
                        If y <> selectedItem.location.Y Then gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2)
                        For Each i As item In items : If i.location.Y = y Then selectedItem = i : Exit Sub
                        Next : Exit Sub : End If : End If : End If

            If p.side = 2 Then
                Dim mousePos As Integer = (mp.Y - location.Y + 5) / 20 - 1 : Dim y As Integer
                If p.c.inventory.selectedItem.location.Y <= 7 Then y = mousePos Else y = mousePos + p.c.inventory.selectedItem.location.Y - 7

                If p.interact.inventory.selectedItem.location.Y >= 0 And p.interact.inventory.selectedItem.location.Y < p.interact.inventory.items.Count And y >= 0 And y < p.interact.inventory.items.Count And mp.X > location2.X And mp.X < (location2.X + inventorySize.X) Then
                    'BUTTON CLICKS
                    If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed And p.interact.inventory.items.Count > 0 Then
                        If p.c.gamestate <> "trading" Or (p.c.gamestate = "trading" And p.c.inventory.money >= p.interact.inventory.selectedItem.type.Value) Then
                            add(p.interact.inventory.selectedItem.copy, 1) : If p.c.gamestate = "trading" Then p.interact.inventory.money += p.interact.inventory.selectedItem.type.Value : p.c.inventory.money -= p.interact.inventory.selectedItem.type.Value
                            p.interact.inventory.remove(p.interact.inventory.selectedItem, 1) : gameDebug.playSound("sfx/menu/switchToggle", "sound", Game.soundVolume, False, 2)
                            If p.interact.inventory.items.Count = 0 Then p.side = 1 : Exit Sub
                        End If
                    End If
                    'CHANGING SELECTED ITEM
                    If p.mouseState.X <> p.oldMouseState.X Or p.mouseState.Y <> p.oldMouseState.Y Then
                        If y <> p.interact.inventory.selectedItem.location.Y Then gameDebug.playSound("sfx/menu/switchClick", "sound", Game.soundVolume, False, 2)
                        For Each i As item In p.interact.inventory.items : If i.location.Y = y Then p.interact.inventory.selectedItem = i : Exit Sub
                        Next : Exit Sub : End If : End If : End If
        End If

    End Sub

    Public Function copy() As Inventory
        Dim i As New Inventory
        For counter As Integer = 0 To items.Count - 1 : i.items.Add(items(counter).copy) : Next
        If i.items.Count > 0 Then i.selectedItem = i.items(0)
        Return i
    End Function
End Class