Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Content
Imports Microsoft.Xna.Framework.Graphics

Public Class gameSound
    Public Property sound As SoundEffectInstance
    Public Property name As String

    Public Sub New(s As SoundEffectInstance, n As String)
        sound = s : name = n
    End Sub
End Class

Public Class gameEffect
    Public Property name As String : Public Property quantity As Single
    Public Property selfRemoval As Boolean : Public Property timeLeft As Single

    Public Property target As Object

    'PATH FOLLOWING (recommended speeds: 2,4,8,16,32,64)
    Public Property path As List(Of Vector2) : Public Property pathProgress As Integer
    Public Property previousCoordinate As Vector2 : Public Property repeat As Boolean

    'MUSIC FADE EFFECT
    Public Property musicToFade As New List(Of gameSound)

    Public Sub New()

    End Sub
    Public Sub New(n As String)
        name = n : quantity = 1
    End Sub
    Public Sub New(n As String, q As Single)
        name = n : quantity = q
    End Sub
    Public Sub New(n As String, q As Single, tl As Single)
        name = n : quantity = q
        selfRemoval = True : timeLeft = tl
    End Sub
    Public Sub New(n As String, q As Single, o As Object, tl As Single)
        name = n : quantity = q : target = o
        If tl <> 0 Then selfRemoval = True : timeLeft = tl
    End Sub

    Public Sub run(c As character)
        If selfRemoval Then
            timeLeft -= 1
            If timeLeft <= 0 Then c.effects.Remove(Me) : c.camera.runEffects(c) : Exit Sub
        End If

        Select Case name
            Case "earthquake"
                If quantity = 0 Then quantity = 1
                Dim a As New Random : Dim b As Integer = a.Next(4) + 1
                Dim d As Integer = a.Next(quantity * Game.tilesize) + 8

                If b = 1 Then c.camera.location = New Vector2(c.camera.location.X, c.camera.location.Y - d)
                If b = 2 Then c.camera.location = New Vector2(c.camera.location.X, c.camera.location.Y + d)
                If b = 3 Then c.camera.location = New Vector2(c.camera.location.X - d, c.camera.location.Y)
                If b = 4 Then c.camera.location = New Vector2(c.camera.location.X + d, c.camera.location.Y)
            Case "rotateLeft"
                If quantity = 0 Then quantity = 0.05
                c.camera.Rotation -= quantity
            Case "rotateRight"
                If quantity = 0 Then quantity = 0.05
                c.camera.Rotation += quantity
            Case "followPath"
                Dim amountLeft As Vector2 = New Vector2(Math.Abs(path(pathProgress).X - c.camera.location.X), Math.Abs(path(pathProgress).Y - c.camera.location.Y))

                If amountLeft.X > amountLeft.Y Then
                    Dim a As Integer = 1 : If c.camera.location.X > path(pathProgress).X Then a = -1
                    c.camera.location = New Vector2(c.camera.location.X + quantity * a, c.camera.location.Y)
                End If
                If amountLeft.Y >= amountLeft.X Then
                    Dim a As Integer = 1 : If c.camera.location.Y > path(pathProgress).Y Then a = -1
                    c.camera.location = New Vector2(c.camera.location.X, c.camera.location.Y + quantity * a)
                End If

                If amountLeft.X < quantity Then c.camera.location = New Vector2(path(pathProgress).X, c.camera.location.Y)
                If amountLeft.Y < quantity Then c.camera.location = New Vector2(c.camera.location.X, path(pathProgress).Y)
                If c.camera.location = path(pathProgress) Then pathProgress += 1

                If pathProgress = path.Count Then
                    pathProgress = 0 : If repeat = False Then c.effects.Remove(Me)
                End If
            Case "lightning"
                c.area.forcedBrightness = (Rnd() * 100)
            Case "fadeIn"
                If c.area.forcedBrightness + quantity >= c.area.brightness Then c.area.forcedBrightness = -1 : c.effects.Remove(Me) : Exit Sub
                c.area.forcedBrightness += quantity
            Case "fadeOut"
                If c.area.forcedBrightness - quantity <= 0 Then c.area.forcedBrightness = 0 : c.effects.Remove(Me) : Exit Sub
                c.area.forcedBrightness -= quantity
            Case "fadeMusicIn"
                Dim done As Integer = 0
                For counter2 As Integer = 0 To musicToFade.Count - 1
                    If musicToFade(counter2).sound.Volume + quantity / 100 <= Game.musicVolume / 100 Then musicToFade(counter2).sound.Volume += quantity / 100 Else musicToFade(counter2).sound.Volume = Game.musicVolume / 100 : done += 1
                Next
                If done = musicToFade.Count Then c.effects.Remove(Me) : Exit Sub
            Case "fadeMusicOut"
                Dim done As Integer = 0
                For counter2 As Integer = 0 To musicToFade.Count - 1
                    If musicToFade(counter2).sound.Volume - quantity / 100 >= 0 Then musicToFade(counter2).sound.Volume -= quantity / 100 Else musicToFade(counter2).sound.Volume = 0 : done += 1
                Next
                If done = musicToFade.Count Then
                    For counter2 As Integer = 0 To musicToFade.Count - 1
                        musicToFade(counter2).sound.Pause()
                    Next
                    c.effects.Remove(Me) : Exit Sub
                End If
            Case "spectate"
                Dim x As Integer = (target.location.X - 1) * Game.tilesize
                Dim y As Integer = (target.location.Y - 2) * Game.tilesize

                If TypeOf target Is player Or TypeOf target Is npc Then
                    If target.tileProgress > 0 Then
                        If target.direction = 1 Then y -= Game.tilesize / target.speed * target.tileProgress
                        If target.direction = 2 Then y += Game.tilesize / target.speed * target.tileProgress
                        If target.direction = 3 Then x -= Game.tilesize / target.speed * target.tileProgress
                        If target.direction = 4 Then x += Game.tilesize / target.speed * target.tileProgress
                    End If
                End If

                c.camera.location = New Vector2(x - c.camera.viewport.Width / 2 + 24, y - c.camera.viewport.Height / 2 + 24)
        End Select
    End Sub
End Class
Public Class Camera
    Public Property viewport As Viewport : Public Property location As Vector2 : Public Property origin As Vector2
    Public Property Zoom As Single : Public Property Rotation As Single

    Public Sub New(v As Viewport)
        Viewport = v : Origin = New Vector2(Viewport.Width / 2.0F, Viewport.Height / 2) : Zoom = 1
    End Sub
    Public Function GetViewMatrix(c As character, parallax As Vector2) As Matrix
        Dim fp As Boolean = False
        For counter As Integer = 0 To c.effects.Count - 1
            If c.effects(counter).name = "followPath" Then fp = True
        Next

        If fp = False Then
            origin = New Vector2(viewport.Width / 2, viewport.Height / 2)
            Dim x As Integer = (c.location.X - 1) * Game.tilesize
            Dim y As Integer = (c.location.Y - 2) * Game.tilesize

            If c.gamestate <> "mapEdit" Then location = New Vector2(x - viewport.Width / 2 + 24, y - viewport.Height / 2 + 24)
        End If

        runEffects(c)

        Return Matrix.CreateTranslation(New Vector3(-location * parallax, 0.0F)) * Matrix.CreateTranslation(New Vector3(-origin, 0.0F)) * Matrix.CreateRotationZ(Rotation) * Matrix.CreateScale(Zoom) * Matrix.CreateTranslation(New Vector3(origin, 0.0F))
    End Function

    Public Sub runEffects(c As character)
        For counter As Integer = 0 To c.effects.Count - 1
            If counter < c.effects.Count Then c.effects(counter).run(c)
        Next
    End Sub
    Public Sub followPath(c As character, pa As List(Of Vector2), speed As Single, r As Boolean, Optional startLoc As Vector2 = Nothing)
        If IsNothing(startLoc) = False Then location = startLoc
        Dim effect As New gameEffect("followPath", speed)
        effect.path = pa : effect.repeat = r : c.effects.Add(effect)
    End Sub
End Class
Public Class FrameCounter
    Dim CurrentTime As Integer = DateTime.Now.Second
    Dim Counter As Integer
    Public Property FramesPerSecond As Integer

    Public Function Increment() As Integer
        If CurrentTime = DateTime.Now.Second Then
            Counter += 1 : Game.time += TimeSpan.FromSeconds(Game.timeTick) 'ADDS TO GAME TIME
            Return FramesPerSecond
        Else

            FramesPerSecond = Counter
            Counter = 0
            CurrentTime = DateTime.Now.Second
            Return FramesPerSecond
        End If
    End Function
End Class

Public Class Node
    Private m_parentNode As Node

    Public Property Location() As Vector2
        Get
            Return m_Location
        End Get
        Private Set
            m_Location = Value
        End Set
    End Property
    Private m_Location As Vector2

    Public Property IsWalkable() As Boolean
        Get
            Return m_IsWalkable
        End Get
        Set
            m_IsWalkable = Value
        End Set
    End Property
    Private m_IsWalkable As Boolean

    Public Property G() As Single
        Get
            Return m_G
        End Get
        Private Set
            m_G = Value
        End Set
    End Property
    Private m_G As Single

    Public Property H() As Single
        Get
            Return m_H
        End Get
        Private Set
            m_H = Value
        End Set
    End Property
    Private m_H As Single

    Public Property State() As NodeState
        Get
            Return m_State
        End Get
        Set
            m_State = Value
        End Set
    End Property
    Private m_State As NodeState

    Public ReadOnly Property F() As Single
        Get
            Return Me.G + Me.H
        End Get
    End Property

    Public Property ParentNode() As Node
        Get
            Return m_parentNode
        End Get
        Set
            ' When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
            m_parentNode = Value
            G = Me.m_parentNode.G + GetTraversalCost(Me.Location, Me.m_parentNode.Location)
        End Set
    End Property

    Public Sub New(x As Integer, y As Integer, isWalkable As Boolean, endLocation As Vector2)
        Location = New Vector2(x, y)
        State = NodeState.Untested
        Me.IsWalkable = isWalkable
        H = GetTraversalCost(Location, endLocation)
        G = 0
    End Sub

    Friend Shared Function GetTraversalCost(location As Vector2, otherLocation As Vector2) As Single
        Dim deltaX As Single = otherLocation.X - location.X
        Dim deltaY As Single = otherLocation.Y - location.Y
        Return Math.Sqrt(deltaX * deltaX + deltaY * deltaY)
    End Function
End Class
Public Enum NodeState
    Untested
    Open
    Closed
End Enum

Public Class gameDebug
    Public Shared content As ContentManager : Public Shared prevScreenshot As String

    Public Shared Sub doMiscProcesses()

        'HANDLES DEBUG LABELS
        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).debugmode = True Then
                Game.players(counter).GUI.processTextElements(Game.players(counter))
            End If
        Next

        'ADJUSTS SCREEN RESOLUTION UPON WINDOW RESIZE
        Dim windowSize As Vector2 = New Vector2(Game.win.ClientBounds.Width, Game.win.ClientBounds.Height)
        If Game.resolution <> windowSize Then
            Game.resolution = windowSize
            Game.graphics.PreferredBackBufferWidth = windowSize.X : Game.graphics.PreferredBackBufferHeight = windowSize.Y
            Game.graphics.ApplyChanges() : allocatePlayerScreenSpace(Game.players, Game.resolution)
        End If

        'MISC VARIABLE CHANGES
        Game.outSideBrightness = Math.Abs(50 - (25 * Convert.ToDouble(Game.time.ToString("hh")) / 6) + (Convert.ToDouble(Game.time.ToString("mm")) / 120)) / 50
    End Sub
    Public Shared Sub allocatePlayerScreenSpace(players As List(Of player), resolution As Vector2)
        If players.Count = 1 Then
            players(0).plrIndex = 0 : players(0).c.camera.viewport = New Viewport(0, 0, Game.resolution.X, Game.resolution.Y)
        End If
        If players.Count = 2 Then
            players(0).plrIndex = 0 : players(0).c.camera.viewport = New Viewport(0, 0, Game.resolution.X / 2, Game.resolution.Y)
            players(1).plrIndex = 1 : players(1).c.camera.viewport = New Viewport(Game.resolution.X / 2, 0, Game.resolution.X / 2, Game.resolution.Y)
        End If
        If players.Count = 3 Then
            players(0).plrIndex = 0 : players(0).c.camera.viewport = New Viewport(0, 0, Game.resolution.X, Game.resolution.Y / 2)
            players(1).plrIndex = 1 : players(1).c.camera.viewport = New Viewport(0, Game.resolution.Y / 2, Game.resolution.X / 2, Game.resolution.Y / 2)
            players(2).plrIndex = 2 : players(2).c.camera.viewport = New Viewport(Game.resolution.X / 2, Game.resolution.Y / 2, Game.resolution.X / 2, Game.resolution.Y / 2)
        End If
        If players.Count = 4 Then
            players(0).plrIndex = 0 : players(0).c.camera.viewport = New Viewport(0, 0, Game.resolution.X / 2, Game.resolution.Y / 2)
            players(1).plrIndex = 1 : players(1).c.camera.viewport = New Viewport(Game.resolution.X / 2, 0, Game.resolution.X / 2, Game.resolution.Y / 2)
            players(2).plrIndex = 2 : players(2).c.camera.viewport = New Viewport(0, Game.resolution.Y / 2, Game.resolution.X / 2, Game.resolution.Y / 2)
            players(3).plrIndex = 3 : players(3).c.camera.viewport = New Viewport(Game.resolution.X / 2, Game.resolution.Y / 2, Game.resolution.X / 2, Game.resolution.Y / 2)
        End If

        For counter As Integer = 0 To players.Count - 1
            If players(counter).leaving = True Then Exit Sub
        Next
    End Sub
    Public Shared Function getDistance(location1 As Vector2, location2 As Vector2) As Double
        Return Math.Sqrt(Math.Pow(location2.Y - location1.Y, 2) + Math.Pow(location2.X - location1.X, 2))
    End Function

    'PLAYING SOUNDS
    Delegate Function pl(path As String, type As String, volume As Integer, repeat As Boolean, amount As Integer) As Integer
    Public Shared Sub playSound(path As String, Optional type As String = "sound", Optional volume As Integer = 100, Optional repeat As Boolean = False, Optional amount As Integer = 1)
        If path = "none" Then Exit Sub
        Dim d As pl = AddressOf play : d.BeginInvoke(path, type, volume, repeat, amount, Nothing, Nothing)
    End Sub
    Public Shared Function play(path As String, type As String, volume As Integer, repeat As Boolean, amount As Integer) As Integer
        'Return 0
        Try 'SOMETIMES SOUNDS FAIL TO PLAY --- AMOUNT IS THE NUMBER OF SOUND VARIANCES PER SPECIFIC SOUND
            Dim p As String = path : If amount > 1 Then Dim rand As New Random : Dim r As Integer = rand.Next(0, amount) + 1 : p = p & r

            Dim bgEffect As SoundEffect = content.Load(Of SoundEffect)("sounds/" & p)
            Dim soundEffectInstance As SoundEffectInstance = bgEffect.CreateInstance()
            soundEffectInstance.IsLooped = repeat : soundEffectInstance.Volume = (volume / 100.0F)

            If type = "sound" Then Game.sounds.Add(New gameSound(soundEffectInstance, path))
            If type = "music" Then Game.music.Add(New gameSound(soundEffectInstance, path))

            soundEffectInstance.Play()
            Return 0
        Catch
            System.Console.WriteLine("Sound Failed to Play!")
            Return 0
        End Try
    End Function

    Public Shared Sub stopSounds(Optional type As String = "sound", Optional method As String = "stop", Optional fadeSpeed As Integer = 0.01)
        If type = "sound" Then
            If method = "stop" Then For Each s As gameSound In Game.sounds : s.sound.Stop() : Game.sounds.Remove(s) : Next
            If method = "fadeOut" Then
                For Each s As gameSound In Game.sounds : If s.sound.Volume - fadeSpeed >= 0 Then s.sound.Volume -= fadeSpeed
                Next
            End If
        End If

        If type = "music" Then
            If method = "stop" Then For Each s As gameSound In Game.music : s.sound.Stop() : Game.sounds.Remove(s) : Next
            If method = "fadeOut" Then
                For Each s As gameSound In Game.music : If s.sound.Volume - fadeSpeed >= 0 Then s.sound.Volume -= fadeSpeed
                Next
            End If
        End If
    End Sub
    Public Shared Sub removeFinishedSounds()
        For Each s As gameSound In Game.sounds
            If s.sound.State = SoundState.Stopped Then Game.sounds.Remove(s)
        Next
    End Sub

    'TEXT AND IMAGE MANIPULATION
    Public Shared Function wrapText(textPane As Rectangle, font As SpriteFont, text As String, sb As SpriteBatch, color As Color, layer As Integer) As Boolean
        Dim space = text.Split(" ") : Dim progress As Vector2 = New Vector2(0, 0)
        For Each word As String In space
            If progress.X + font.MeasureString(word).X <= textPane.Width And word <> "~" Then
                sb.DrawString(font, word & " ", New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(word & " ").X
            Else
                'NEW LINE
                progress.Y += font.MeasureString(" ").Y * 0.7 : progress.X = 0 : If progress.Y + font.MeasureString(" ").Y * 0.7 > textPane.Height Then Return False
                If word <> "~" Then sb.DrawString(font, word & " ", New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(word & " ").X
            End If
        Next
        Return True
    End Function
    Public Shared Function measureWrappedText(textPane As Rectangle, font As SpriteFont, text As String, sb As SpriteBatch, color As Color, layer As Integer) As Vector2
        Dim space = text.Split(" ") : Dim progress As Vector2 = New Vector2(0, 1)
        For Each word As String In space
            If progress.X + font.MeasureString(word).X <= textPane.Width And word <> "~" Then
                progress.X += font.MeasureString(word & " ").X
            Else
                'NEW LINE
                progress.Y += font.MeasureString(" ").Y : progress.X = 0
                If word <> "~" Then progress.X += font.MeasureString(word & " ").X
            End If
        Next
        Return progress
    End Function

    Public Shared Sub takeScreenShot(p As player, graphics As GraphicsDevice)
        'PREVENTS SPAMMING OF SCREENSHOT BUTTON
        Dim time As String = Now.ToString("yyyy-MM-dd_HH.mm.ss")
        If prevScreenshot = time Then Exit Sub Else prevScreenshot = time

        'COLLECTS DATA FOR SCREENSHOT
        Dim screenshot As Texture2D = New RenderTarget2D(graphics, p.c.camera.viewport.Width, p.c.camera.viewport.Height, False, graphics.DisplayMode.Format, graphics.PresentationParameters.DepthStencilFormat)
        graphics.SetRenderTarget(screenshot) : Dim sb As SpriteBatch = New SpriteBatch(graphics) : graphics.Clear(Color.Black) : Game.drawArea(p, sb)
        sb.End() : graphics.SetRenderTarget(Nothing)

        'SAVES DATA TO FILE
        Dim fs As New IO.FileStream(Game.directory & "screenshots\" & time & ".png", IO.FileMode.OpenOrCreate)
        screenshot.SaveAsPng(fs, p.c.camera.viewport.Width, p.c.camera.viewport.Height)
        p.GUI.addChatMessage(p, "Screenshot Taken! Name: " & time & ".png")
    End Sub
    Public Shared Function createBox(size As Vector2, co As Color) As Texture2D
        Dim box As New Texture2D(Game.graphics.GraphicsDevice, size.X, size.Y)
        Dim data As Color() = New Color(size.X * size.Y - 1) {}
        For i As Integer = 0 To data.Length - 1 : data(i) = co
        Next : box.SetData(data)
        Return box
    End Function
    Public Shared Function blurImage(img As Texture2D, sb As SpriteBatch, ByRef blurTarget1 As RenderTarget2D, ByRef blurTarget2 As RenderTarget2D) As Texture2D
        Return img 'BROKEN
        Dim radius As Integer = 2 : Dim amount As Single = 1.0F
        Dim sigma As Single = radius / amount : Dim kernel As Single() = New Single(radius * 2) {}
        Dim index As Integer = 0 : Dim total As Single = 0F : Dim distance As Single = 0F
        Dim twoSigmaSquare As Single = 2.0F * sigma * sigma : Dim sigmaRoot As Single = CSng(Math.Sqrt(twoSigmaSquare * Math.PI))
        Dim offsetsHoriz As Vector2() = New Vector2(radius * 2) {} : Dim offsetsVert As Vector2() = New Vector2(radius * 2) {}
        Dim effect As Effect = content.Load(Of Effect)("Effects\GaussianBlur")

        For i As Integer = -radius To radius
            index = i + radius
            offsetsHoriz(index) = New Vector2(i * 1.0F / img.Width, 0F)
            offsetsVert(index) = New Vector2(0F, i * 1.0F / img.Height)
        Next

        index = 0
        For i As Integer = -radius To radius
            distance = i * i : index = i + radius
            kernel(index) = CSng(Math.Exp(-distance / twoSigmaSquare)) / sigmaRoot
            total += kernel(index)
        Next
        For i As Integer = 0 To kernel.Length - 1 : kernel(i) /= total : Next

        If IsNothing(blurTarget1) Then blurTarget1 = New RenderTarget2D(Game.graphics.GraphicsDevice, img.Width, img.Height, False, Game.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None)
        If IsNothing(blurTarget2) Then blurTarget2 = New RenderTarget2D(Game.graphics.GraphicsDevice, img.Width, img.Height, False, Game.graphics.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None)

        Dim srcRect As New Rectangle(0, 0, img.Width, img.Height)
        Dim destRect1 As New Rectangle(0, 0, blurTarget1.Width, blurTarget1.Height)
        Dim destRect2 As New Rectangle(0, 0, blurTarget2.Width, blurTarget2.Height)

        effect.Parameters("weights").SetValue(kernel)
        effect.Parameters("offsets").SetValue(offsetsHoriz)
        effect.Parameters("colorMapTexture").SetValue(img)

        Game.graphics.GraphicsDevice.SetRenderTarget(blurTarget1)
        sb.Begin(0, BlendState.Opaque, Nothing, Nothing, Nothing, effect)
        sb.Draw(img, New Vector2(0, 0), Color.White)
        sb.End()

        effect.Parameters("offsets").SetValue(offsetsVert)

        Game.graphics.GraphicsDevice.SetRenderTarget(blurTarget2)
        sb.Begin(0, BlendState.Opaque, Nothing, Nothing, Nothing, effect)
        sb.Draw(blurTarget1, New Vector2(0, 0), Color.White)
        sb.End()

        Game.graphics.GraphicsDevice.SetRenderTarget(Nothing)

        Return blurTarget2
    End Function

    'TRANSLATION
    Public Shared Function translateCollisionMap(s As String) As List(Of Vector3)
        Dim collisionMap As List(Of Vector3) = New List(Of Vector3) : Dim tile = s.Split(" ")
        If tile(0) = "none" Then Return New List(Of Vector3)

        For counter As Integer = 0 To tile.Count - 2
            Dim properties = tile(counter).Split(".")
            Select Case properties.Count
                Case 1 : collisionMap.Add(New Vector3(properties(0), 0, 0))
                Case 2 : collisionMap.Add(New Vector3(properties(0), properties(1), 0))
                Case 3 : collisionMap.Add(New Vector3(properties(0), properties(1), properties(2)))
            End Select
        Next
        Return collisionMap
    End Function
    Public Shared Function translateCollisionMap(tile As List(Of Vector3)) As String
        Dim s As String = ""
        If tile.Count = 0 Then Return "none"

        For counter As Integer = 0 To tile.Count - 1
            s = s & tile(counter).X
            If tile(counter).Y <> 0 Then s = s & "." & tile(counter).Y
            If tile(counter).Z <> 0 Then s = s & "." & tile(counter).Z
            s = s & " "
        Next
        Return s
    End Function
    Public Shared Function translateCollisionMap(a As area, endLoc As Vector2) As List(Of Node)
        Dim translatedMap As New List(Of Node)
        Dim x As Integer = 0 : Dim y As Integer = 0
        For counter As Integer = 0 To a.hitDetect.Count - 1
            Dim accessable As Boolean = True

            If a.hitDetect(y * a.size.X + x).Y = 0 Or a.hitDetect(y * a.size.X + x).Y = 1 Then
                If a.hitDetect(y * a.size.X + x).X = 5 Then accessable = False
                'NEED TO EXCLUDE ALL TILE CALCULATIONS USING Y VALUE OF 2
                'FENCES
                'If y - 1 >= 0 Then
                '    If p.c.area.hitDetect((y - 1) * p.c.area.size.X + x).X = 6 Then accessable = False
                'End If

                If y * a.size.X + x - 1 >= 0 Then
                    If a.hitDetect(y * a.size.X + x - 1).X = 1 Then accessable = False
                End If
                If y * a.size.X + x + 1 < a.size.X * a.size.Y Then
                    If a.hitDetect(y * a.size.X + x + 1).X = 1 Then accessable = False
                End If
                If y - 1 >= 0 And y + 1 < a.size.Y And x > 0 Then
                    If a.hitDetect((y - 1) * a.size.X + x - 1).X = 1 Then accessable = False
                    If a.hitDetect((y - 1) * a.size.X + x + 1).X = 1 Then accessable = False
                End If
                If y - 1 >= 0 And y + 1 < a.size.Y And x < a.size.X Then
                    If a.hitDetect((y + 1) * a.size.X + x - 1).X = 1 Then accessable = False
                    If a.hitDetect((y + 1) * a.size.X + x + 1).X = 1 Then accessable = False
                End If

            End If
            translatedMap.Add(New Node(x, y, accessable, endLoc))
            'If accessable Then p.c.area.tileMap(y * p.c.area.size.X + x) = 0 'VISUALIZES ALGORITHM'S ACCESSABLE TILES

            If x < a.size.X Then x += 1
            If x >= a.size.X Then x = 0 : y += 1
        Next

        Return translatedMap
    End Function

    'may want to add money functionality to these
    Public Shared Function translateInventory(i As Inventory) As String
        Dim s As String = ""
        For counter As Integer = 0 To i.items.Count - 1
            s = s & i.items(counter).type.id & "." & i.items(counter).quantity & " "
        Next
        Return s
    End Function
    Public Shared Function translateInventory(s As String) As Inventory
        Dim i As New Inventory
        Dim itemSplit = s.Split(" ")
        For counter As Integer = 0 To itemSplit.Count - 2
            Dim currentItem As String = itemSplit(counter)
            Dim itemInfo = currentItem.Split(".")
            i.add(New item(terrainManager.getItemType(itemInfo(0)), itemInfo(1), New Vector2))
        Next
        Return i
    End Function
End Class