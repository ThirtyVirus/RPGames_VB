Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports Microsoft.VisualBasic
Imports Microsoft.Xna.Framework
Imports Microsoft.Xna.Framework.Audio
Imports Microsoft.Xna.Framework.Graphics
Imports Microsoft.Xna.Framework.Input

Public Class console
    Public Property location As Vector2 : Public Property textFont As SpriteFont
    Public Property text As String : Public Property inputPos As Integer
    Public Property capsLock As Boolean : Public Property maxCharacters As Integer = 128

    Public Property leftHoldTime As Integer : Public Property rightHoldTime As Integer
    Public Property backHoldTime As Integer

    Public Sub New(loc As Vector2, tf As SpriteFont)
        location = loc : textFont = tf : capsLock = False : text = "" : inputPos = 0
    End Sub
    Public Sub close(p As player)
        commandPrompt.performCommand(p, text) : p.typing = False
    End Sub
    Public Sub draw(p As player, sb As SpriteBatch)

        'OFFSETS TEXT IF TOO LONG TO FIT IN TEXTBOX
        Dim offset As Integer = 0
        If textFont.MeasureString(text.Substring(0, inputPos)).X > p.c.camera.viewport.Width - 32 - textFont.MeasureString(" ").X Then
            offset = textFont.MeasureString(text.Substring(0, inputPos)).X - p.c.camera.viewport.Width + 32
        End If

        'DRAWS TEXT BEFORE INPUT POSITION
        If text.Length > 0 Then sb.DrawString(textFont, text.Substring(0, inputPos), New Vector2(location.X - offset, location.Y), Color.White)
        Dim newLocation As Vector2 = location : If text.Length > 0 Then newLocation.X += textFont.MeasureString(text.Substring(0, inputPos)).X

        'DRAWS INPUT POSITION
        sb.DrawString(textFont, "|", New Vector2(newLocation.X - offset - textFont.MeasureString("|").X / 2, newLocation.Y - 2), Color.Red)

        'DRAWS TEXT AFTER INPUT POSITION
        If text.Length > 0 And inputPos < text.Length Then
            wrapText(New Rectangle(newLocation.X, newLocation.Y, (p.c.camera.viewport.Width - 32) - (newLocation.X - location.X), textFont.MeasureString(" ").Y * 0.5), textFont, text.Substring(inputPos), sb, Color.White, 0)
        End If

        'DRAWS TEXTBOX FOR CONSOLE
        sb.Draw(Game.blackBox, New Rectangle(location.X, location.Y, p.c.camera.viewport.Width - 32, textFont.MeasureString(" ").Y - 6), New Rectangle(0, 0, 1, 1), Color.Gray * 0.5, 0, New Vector2(0, 0), SpriteEffects.None, 0.0019)

        'DRAWS CHAT
        If p.GUI.chat.Count > 0 Then
            Dim l As Vector2 = New Vector2(location.X, location.Y - textFont.MeasureString(" ").Y * 0.7) : Dim white As Boolean = False
            For counter As Integer = 0 To p.GUI.chat.Count - 1
                l = New Vector2(l.X, l.Y - textFont.MeasureString(" ").Y * 0.7) : white = Not white

                'WRAPS TEXT
                Dim size As Vector2 = measureWrappedText(p.c.camera.viewport.Width - p.c.camera.viewport.Width / 4, textFont, p.GUI.chat(p.GUI.chat.Count - 1 - counter))
                size = New Vector2(size.X, size.Y + textFont.MeasureString(" ").Y) : If size.Y > textFont.MeasureString(" ").Y * 0.5 Then l = New Vector2(l.X, l.Y - size.Y + textFont.MeasureString(" ").Y)
                wrapText(New Rectangle(l.X, l.Y, p.c.camera.viewport.Width - p.c.camera.viewport.Width / 4, size.Y), textFont, p.GUI.chat(p.GUI.chat.Count - 1 - counter), sb, Color.White, 0)

                If counter = 10 Then Exit For
            Next
            'DRAWS TEXTBOX FOR CHAT
            sb.Draw(Game.blackBox, New Rectangle(l.X, l.Y, p.c.camera.viewport.Width - p.c.camera.viewport.Width / 4, location.Y - l.Y - 6), New Rectangle(0, 0, 1, 1), Color.Gray * 0.5, 0, New Vector2(0, 0), SpriteEffects.None, 0.0019)
        End If
    End Sub
    Public Shared Function wrapText(textPane As Rectangle, font As SpriteFont, text As String, sb As SpriteBatch, color As Color, layer As Double) As Boolean
        Dim progress As Vector2 = New Vector2(0, 0)
        For Each ch As Char In text
            If progress.X + font.MeasureString(ch).X <= textPane.Width Then
                sb.DrawString(font, ch, New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(ch).X
            Else
                progress.Y += font.MeasureString(" ").Y * 0.7 : progress.X = 25
                If progress.Y > textPane.Height Then Return False
                sb.DrawString(font, ch, New Vector2(textPane.X + progress.X, textPane.Y + progress.Y), color, 0, New Vector2(), 1, SpriteEffects.None, layer) : progress.X += font.MeasureString(ch).X
            End If
        Next
        Return True
    End Function
    Public Shared Function measureWrappedText(width As Integer, font As SpriteFont, text As String) As Vector2
        Dim progress As Vector2 = New Vector2(0, 1)
        For Each ch As Char In text
            If progress.X + font.MeasureString(ch).X <= width Then
                progress.X += font.MeasureString(ch).X
            Else
                'NEW LINE
                progress.Y += font.MeasureString(" ").Y * 0.7 : progress.X = 25
                progress.X += font.MeasureString(ch).X
            End If
        Next
        Return progress
    End Function

    Public Sub getKeyboardInput(p As player)
        If text.Count < maxCharacters Then
            If p.keystate.IsKeyDown(Keys.Q) And p.oldKeyState.IsKeyDown(Keys.Q) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "Q") : inputPos += 1 Else text = text.Insert(inputPos, "q") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.W) And p.oldKeyState.IsKeyDown(Keys.W) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "W") : inputPos += 1 Else text = text.Insert(inputPos, "w") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.E) And p.oldKeyState.IsKeyDown(Keys.E) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "E") : inputPos += 1 Else text = text.Insert(inputPos, "e") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.R) And p.oldKeyState.IsKeyDown(Keys.R) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "R") : inputPos += 1 Else text = text.Insert(inputPos, "r") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.T) And p.oldKeyState.IsKeyDown(Keys.T) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "T") : inputPos += 1 Else text = text.Insert(inputPos, "t") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.Y) And p.oldKeyState.IsKeyDown(Keys.Y) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "Y") : inputPos += 1 Else text = text.Insert(inputPos, "y") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.U) And p.oldKeyState.IsKeyDown(Keys.U) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "U") : inputPos += 1 Else text = text.Insert(inputPos, "u") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.I) And p.oldKeyState.IsKeyDown(Keys.I) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "I") : inputPos += 1 Else text = text.Insert(inputPos, "i") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.O) And p.oldKeyState.IsKeyDown(Keys.O) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "O") : inputPos += 1 Else text = text.Insert(inputPos, "o") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.P) And p.oldKeyState.IsKeyDown(Keys.P) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "P") : inputPos += 1 Else text = text.Insert(inputPos, "p") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.A) And p.oldKeyState.IsKeyDown(Keys.A) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "A") : inputPos += 1 Else text = text.Insert(inputPos, "a") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.S) And p.oldKeyState.IsKeyDown(Keys.S) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "S") : inputPos += 1 Else text = text.Insert(inputPos, "s") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D) And p.oldKeyState.IsKeyDown(Keys.D) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "D") : inputPos += 1 Else text = text.Insert(inputPos, "d") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.F) And p.oldKeyState.IsKeyDown(Keys.F) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "F") : inputPos += 1 Else text = text.Insert(inputPos, "f") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.G) And p.oldKeyState.IsKeyDown(Keys.G) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "G") : inputPos += 1 Else text = text.Insert(inputPos, "g") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.H) And p.oldKeyState.IsKeyDown(Keys.H) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "H") : inputPos += 1 Else text = text.Insert(inputPos, "h") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.J) And p.oldKeyState.IsKeyDown(Keys.J) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "J") : inputPos += 1 Else text = text.Insert(inputPos, "j") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.K) And p.oldKeyState.IsKeyDown(Keys.K) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "K") : inputPos += 1 Else text = text.Insert(inputPos, "k") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.L) And p.oldKeyState.IsKeyDown(Keys.L) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "L") : inputPos += 1 Else text = text.Insert(inputPos, "l") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.Z) And p.oldKeyState.IsKeyDown(Keys.Z) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "Z") : inputPos += 1 Else text = text.Insert(inputPos, "z") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.X) And p.oldKeyState.IsKeyDown(Keys.X) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "X") : inputPos += 1 Else text = text.Insert(inputPos, "x") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.C) And p.oldKeyState.IsKeyDown(Keys.C) = False And p.keystate.IsKeyDown(Keys.LeftControl) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "C") : inputPos += 1 Else text = text.Insert(inputPos, "c") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.V) And p.oldKeyState.IsKeyDown(Keys.V) = False And p.keystate.IsKeyDown(Keys.LeftControl) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "V") : inputPos += 1 Else text = text.Insert(inputPos, "v") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.B) And p.oldKeyState.IsKeyDown(Keys.B) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "B") : inputPos += 1 Else text = text.Insert(inputPos, "b") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.N) And p.oldKeyState.IsKeyDown(Keys.N) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "N") : inputPos += 1 Else text = text.Insert(inputPos, "n") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.M) And p.oldKeyState.IsKeyDown(Keys.M) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "M") : inputPos += 1 Else text = text.Insert(inputPos, "m") : inputPos += 1

            If p.keystate.IsKeyDown(Keys.D1) And p.oldKeyState.IsKeyDown(Keys.D1) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "!") : inputPos += 1 Else text = text.Insert(inputPos, "1") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D2) And p.oldKeyState.IsKeyDown(Keys.D2) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "@") : inputPos += 1 Else text = text.Insert(inputPos, "2") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D3) And p.oldKeyState.IsKeyDown(Keys.D3) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "#") : inputPos += 1 Else text = text.Insert(inputPos, "3") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D4) And p.oldKeyState.IsKeyDown(Keys.D4) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "$") : inputPos += 1 Else text = text.Insert(inputPos, "4") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D5) And p.oldKeyState.IsKeyDown(Keys.D5) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "%") : inputPos += 1 Else text = text.Insert(inputPos, "5") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D6) And p.oldKeyState.IsKeyDown(Keys.D6) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "^") : inputPos += 1 Else text = text.Insert(inputPos, "6") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D7) And p.oldKeyState.IsKeyDown(Keys.D7) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "&") : inputPos += 1 Else text = text.Insert(inputPos, "7") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D8) And p.oldKeyState.IsKeyDown(Keys.D8) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "*") : inputPos += 1 Else text = text.Insert(inputPos, "8") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D9) And p.oldKeyState.IsKeyDown(Keys.D9) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "(") : inputPos += 1 Else text = text.Insert(inputPos, "9") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.D0) And p.oldKeyState.IsKeyDown(Keys.D0) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, ")") : inputPos += 1 Else text = text.Insert(inputPos, "0") : inputPos += 1

            If p.keystate.IsKeyDown(Keys.OemTilde) And p.oldKeyState.IsKeyDown(Keys.OemTilde) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "~") : inputPos += 1 Else text = text.Insert(inputPos, "`") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemMinus) And p.oldKeyState.IsKeyDown(Keys.OemMinus) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "_") : inputPos += 1 Else text = text.Insert(inputPos, "-") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemPlus) And p.oldKeyState.IsKeyDown(Keys.OemPlus) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "+") : inputPos += 1 Else text = text.Insert(inputPos, "=") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemOpenBrackets) And p.oldKeyState.IsKeyDown(Keys.OemOpenBrackets) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "{") : inputPos += 1 Else text = text.Insert(inputPos, "[") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemCloseBrackets) And p.oldKeyState.IsKeyDown(Keys.OemCloseBrackets) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "}") : inputPos += 1 Else text = text.Insert(inputPos, "]") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemBackslash) And p.oldKeyState.IsKeyDown(Keys.OemBackslash) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "|") : inputPos += 1 Else text = text.Insert(inputPos, "\") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemSemicolon) And p.oldKeyState.IsKeyDown(Keys.OemSemicolon) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, ":") : inputPos += 1 Else text = text.Insert(inputPos, ";") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemQuotes) And p.oldKeyState.IsKeyDown(Keys.OemQuotes) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "''") : inputPos += 1 Else text = text.Insert(inputPos, "'") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemComma) And p.oldKeyState.IsKeyDown(Keys.OemComma) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "<") : inputPos += 1 Else text = text.Insert(inputPos, ",") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemPeriod) And p.oldKeyState.IsKeyDown(Keys.OemPeriod) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, ">") : inputPos += 1 Else text = text.Insert(inputPos, ".") : inputPos += 1
            If p.keystate.IsKeyDown(Keys.OemQuestion) And p.oldKeyState.IsKeyDown(Keys.OemQuestion) = False Then If (p.keystate.IsKeyDown(Keys.LeftShift) Or capsLock) Then text = text.Insert(inputPos, "?") : inputPos += 1 Else text = text.Insert(inputPos, "/") : inputPos += 1

            If p.keystate.IsKeyDown(Keys.Space) And p.oldKeyState.IsKeyDown(Keys.Space) = False Then text = text.Insert(inputPos, " ") : inputPos += 1

        End If
        If p.keystate.IsKeyDown(Keys.CapsLock) And p.oldKeyState.IsKeyDown(Keys.CapsLock) = False Then capsLock = Not capsLock

        If p.keystate.IsKeyDown(Keys.Back) And (p.oldKeyState.IsKeyDown(Keys.Back) = False Or (backHoldTime > 20 And backHoldTime Mod 2 = 0)) And inputPos >= 1 Then text = text.Remove(inputPos - 1, 1) : inputPos -= 1
        If p.keystate.IsKeyDown(Keys.Back) Then backHoldTime += 1 Else backHoldTime = 0 'DETECTS IF YOU HOLD DOWN BACKSPACE

        If p.keystate.IsKeyDown(Keys.Left) And (p.oldKeyState.IsKeyDown(Keys.Left) = False Or (leftHoldTime > 20 And leftHoldTime Mod 2 = 0)) And inputPos > 0 Then inputPos -= 1
        If p.keystate.IsKeyDown(Keys.Left) Then leftHoldTime += 1 Else leftHoldTime = 0 'DETECTS IF YOU HOLD DOWN LEFT

        If p.keystate.IsKeyDown(Keys.Right) And (p.oldKeyState.IsKeyDown(Keys.Right) = False Or (rightHoldTime > 20 And rightHoldTime Mod 2 = 0)) And inputPos < text.Length Then inputPos += 1
        If p.keystate.IsKeyDown(Keys.Right) Then rightHoldTime += 1 Else rightHoldTime = 0 'DETECTS IF YOU HOLD DOWN RIGHT

        If p.keystate.IsKeyDown(Keys.Tab) And p.oldKeyState.IsKeyDown(Keys.Tab) = False And p.prevCommand <> "" Then Dim s As String = text : text = p.prevCommand : inputPos = text.Length : p.prevCommand = s
        If p.keystate.IsKeyDown(Keys.Enter) And p.oldKeyState.IsKeyDown(Keys.Enter) = False Then p.prevCommand = text : close(p)

        If p.keystate.IsKeyDown(Keys.LeftControl) Then
            'DELETES ALL TEXT
            If p.keystate.IsKeyDown(Keys.Back) And p.oldKeyState.IsKeyDown(Keys.Back) = False Then text = "" : inputPos = 0
            'COPIES TEXT TO CLIPBOARD
            If p.keystate.IsKeyDown(Keys.C) And p.oldKeyState.IsKeyDown(Keys.C) = False Then My.Computer.Clipboard.SetText(text)
            'BRINGS TEXT IN FROM CLIPBOARD
            If p.keystate.IsKeyDown(Keys.V) And p.oldKeyState.IsKeyDown(Keys.V) = False Then
                If text.Count + My.Computer.Clipboard.GetText().Count <= maxCharacters Then
                    text = text.Insert(inputPos, My.Computer.Clipboard.GetText())
                    inputPos += My.Computer.Clipboard.GetText().Length
                Else
                    Dim spaceLeft As Integer = maxCharacters - text.Count
                    text = text.Insert(inputPos, My.Computer.Clipboard.GetText().Substring(0, spaceLeft))
                    inputPos += spaceLeft
                End If

            End If
        End If
    End Sub
    Public Sub getMouseInput(p As player)

        If p.mouseState.Y >= location.Y Then
            Dim x As Integer = p.mouseState.X + p.c.camera.viewport.X - location.X
            If p.mouseState.LeftButton = ButtonState.Pressed And p.oldMouseState.LeftButton <> ButtonState.Pressed Then
                Dim newText As String = "" : Dim counter As Integer = 0
                For Each ch As Char In text
                    If textFont.MeasureString(newText).X > x Then
                        inputPos = counter
                        Exit For
                    End If
                    newText = newText & ch : counter += 1 : Next
                inputPos = counter
            End If
        End If
    End Sub
End Class
Public Class boundCommand
    Public Property key As String
    Public Property command As String
End Class
Public Class coordinate
    Public Property area As String
    Public Property location As Vector2
    Public Property direction As Integer
End Class

Public Class command
    'Want to eventually define commands using a new function in here that also includes code
    'Also want sorting capabilities and intellisense- style command recommendations in chat
    Public Property name As String

    Public Property format As String 'want to contain sudo-code for how to read command (aslso displays to user how to use command)
    Public Property description As String

    Public Sub New(n, f, d)
        name = n
        format = f
        description = d
    End Sub

End Class

Public Class commandPrompt
    Delegate Function saveGame(savename As String) As Integer
    Delegate Function doCommandLater(p As player, command As String, time As Integer) As Integer

    Public Shared Sub performCommand(ByRef p As player, command As String, Optional overrideDebug As Boolean = False)
        If command = Nothing Then Exit Sub
        Dim word = command.Split(" ") : p.typing = False
        '/debug COMMAND IS FOR DEVELOPMENT PURPOSES ONLY
        If p.debugmode = False And overrideDebug = False And word(0) <> "/debug" Then
            If command(0) = "/" Then p.GUI.addChatMessage(p, "Sorry! You must be in debug mode to use commands!", True) : Exit Sub
        End If

        If command(0) = "/" Then
            Try
                Select Case word(0)
                    Case "/sudo" : sudo(p, command) '/sudo Bob /noclip (EMULATES COMMAND AS ANOTHER PLAYER)
                    Case "/logon" : logon(p, command) '/logon playerName (ADDS PLAYER TO GAME)    
                    Case "/addEffect" : addEffect(p, command) '/addEffect effectName quantity
                    Case "/removeEffect" : removeEffect(p, command) '/removeEffect effectName OR /removeEffect all
                    Case "/setRotation" : p.c.camera.Rotation = word(1) 'setRotation amount
                    Case "/help" : help(p, command) 'DISPLAYS HOW TO TYPE COMMANDS

                    Case "/playSound" : gameDebug.playSound("sfx/" & word(1),, Game.soundVolume)
                    Case "/playMusic" : If word.Count = 2 Then p.c.updateMusic("", "music/" & word(1), 100) Else p.c.updateMusic("", "music/" & word(1), word(2)) '/playMusic musicName fadeRate
                    Case "/stopSounds" : For counter As Integer = 0 To Game.sounds.Count - 1 : Game.sounds(counter).sound.Stop() : Next : Game.sounds.Clear()
                    Case "/stopMusic" : For counter As Integer = 0 To Game.music.Count - 1 : Game.music(counter).sound.Stop() : Next : Game.music.Clear()

                    'COMMANDS THAT ONLY EFFECT PLAYER THAT TYPES IT
                    Case "/selectMe" : p.subject = p.c : p.GUI.addChatMessage(p, "You Have Selected Yourself!") 'MAKES PLAYER SUBJECT                
                    Case "/back" : Dim times As Integer = 1 : If word.Count = 2 Then times = word(1) '/back # (TELEPORTS TO PREVIOUS LOCATIONS)
                        p.c.changeArea("quickSave", p.c.previousAreas(p.c.previousAreas.Count - times), p.c.previousLocations(p.c.previousLocations.Count - times), p.c.direction)
                    Case "/bindKey" : bindKey(p, command) '/bindkey key(Capital) commmand ---> ex: /bindkey H /back
                    Case "/clearChat" : p.GUI.chat.Clear()
                    Case "/listSaves"
                        Dim saves As String = Nothing : Dim saveDirectories As String() = Directory.GetDirectories(Game.directory & "/saves/")
                        For counter As Integer = 0 To saveDirectories.Count - 1 : saves = saves & saveDirectories(counter).Substring(Game.directory.Count + 7) & ", " : Next
                        p.GUI.addChatMessage(p, saves, True)
                    Case "/debug" : p.debugmode = Not p.debugmode

                    'COMMANDS THAT CHANGE GAME SETTINGS
                    Case "/setResolution" : Game.graphics.PreferredBackBufferWidth = word(1) : Game.graphics.PreferredBackBufferHeight = word(2) : Game.graphics.ApplyChanges() '/setResolution width height
                    Case "/setVolume" : setVolume(p, command) '/setVolume sound # OR  /setVolume music #
                    Case "/setBrightness" : setBrightness(p, command)
                    Case "/setTimeTick" : Game.timeTick = word(1) '/setTimeTick #

                    'COMMANDS THAT CHANGE THE WORLD
                    Case "/setTime" : setTime(p, command) '/setTime day/night OR /setTime #1-24
                    Case "/setAreaBrightness" : setAreaBrightness(p, command) '/setAreaBrightness # (only applies to area player is in)
                    Case "/setMusic" : setMusic(p, command) 'setMusic songName
                    Case "/setOutside" : p.c.area.outside = word(1) : p.GUI.addChatMessage(p, "Area Outside: " & word(1)) '/setOutside true OR /setOutside false
                    Case "/setSaveable" : p.c.area.saveAble = word(1) : p.GUI.addChatMessage(p, "Area SaveAble: " & word(1)) '/setSaveable true OR /setSaveable false
                    Case "/setGreet" : p.c.area.greetMessage = Replace(command.Substring(10, command.Length - 10), " ", "_") : p.GUI.addChatMessage(p, "Area greet message set to: " & Replace(p.c.area.greetMessage, "_", " "))  '/setGreet greetMessage

                    'COMMANDS THAT MANAGE FILES
                    Case "/saveGame" : Dim d As saveGame = AddressOf FileManager.save : d.BeginInvoke(word(1), Nothing, Nothing) '/saveGame saveName
                    Case "/loadSave" : loadSave(command, Game.players) '/loadSave saveName
                    Case "/deleteSave" : If word(1) <> "quicksave" Then My.Computer.FileSystem.DeleteDirectory(Game.directory & "saves\" & word(1), FileIO.DeleteDirectoryOption.DeleteAllContents) : p.GUI.addChatMessage(p, "Save Deleted: " & word(1)) '/deleteSave saveName

                    'COMMANDS THAT EFFECT PLAYERS ONLY
                    Case "/kick" : kick(p, command) '/kick playerName (KICKS PLAYER FROM GAME)
                    Case "/sendMessage" : sendMessage(p, command) '/sendMessage playerName messageText (brings up textprompt and shows text)

                    'COMMANDS THAT EFFECT PLAYERS AND SUBJECTS
                    Case "/tp" : teleport(p, command) '/tp playerName OR /tp player1 player2 OR /tp lobby 20 20 OR /tp playerName lobby 20 20 (TELEPORTS PLAYERS)
                    Case "/invSee" : invSee(p, command) '/invSee subject OR /invsee playerName (ALLOWS USER LOOT INVENTORIES OF SUBJECTS AND PLAYERS)
                    Case "/clearInv" : clearInventory(p, command) '/clearInv OR /clearInv subject OR /clearInv playerName (CLEARS INVENTORY OF SUBJECTS AND PLAYERS)
                    Case "/giveItem" : giveItem(p, command) '/giveItem playerName id/name quantity OR /giveItem subject id/name quantity (GIVES ITEM TO SUBJECT OR PLAYER)
                    Case "/noclip" : noclip(p, command) '/noclip OR /noclip subject OR /noclip playerName (TOGGLES NOCLIP FOR SUBJECTS[npcs only] AND PLAYERS)
                    Case "/setSpeed" : setSpeed(p, command) '/setSpeed # OR /setSpeed playerName # OR /setSpeed subject # (FORCES SPEED FOR SUBJECTS[npcs only] AND PLAYERS)
                    Case "/setGamestate" : setGamestate(p, command) '/setGamestate inGame
                    Case "/v" : v(p, command) '/v OR /v playerName OR /v subject  (TOGGLES INVISIBILITY OF SUBJECT OR PLAYER)

                    Case "/kill" : kill(p, command) '/kill subject OR /kill playername OR /kill all
                    Case "/heal" : heal(p, command) '/heal subject OR /heal playername OR /heal all                   
                    Case "/freeze" : freeze(p, command) '/freeze subject OR /freeze PlayerName OR /freeze all
                    Case "/instigate" : instigate(p, command) '/instigate subject OR instigate playerName OR /instigate all
                    Case "/scare" : scare(p, command) '/scare subject OR /scare playerName OR /scare all
                    Case "/calm" : calm(p, command) '/calm subject OR /calm playerName OR /calm all
                    Case "/possess" '/posses (POSSESSES SUBJECT [npc only])

                        'STORES PLAYER'S CHARACTER IN "CELL" NPC
                        Dim n As npc = New npc(New character(p.c.skin, p.c.texture, p.c.name, p.c.inventory, p.c.health, p.c.maxHealth, "still"), "cell")
                        n.c.hostNPC = n : n.c.area = p.c.area : n.c.location = p.c.location : n.c.direction = p.c.direction

                        p.subject.hostPlayer = p
                        p.c = p.subject : gameDebug.allocatePlayerScreenSpace(Game.players, Game.resolution)
                        p.c.host = "player" : p.c.area.npcs.Remove(p.subject.hostNPC) : p.c.prevGamestate = p.c.gamestate : p.c.gamestate = "inGame"
                        p.c.area.npcs.Add(n) : p.subject = n.c

                    'COMMANDS THAT EFFECT SUBJECTS
                    Case "/rename" : p.subject.name = word(1) '/rename newName (RENAMES SUBJECTS)
                    Case "/move" 'TELEPORTS SUBJECT
                        If word(1) = "mousePos" Then
                            If TypeOf p.subject Is npc Then p.subject.c.location = p.mp Else p.subject.location = p.mp
                        Else
                            If TypeOf p.subject Is npc Then p.subject.c.location = New Vector2(word(1), word(2)) Else p.subject.location = New Vector2(word(1), word(2))
                        End If
                    Case "/getPos" : My.Computer.Clipboard.SetText(p.c.area.name & " " & p.subject.location.x & " " & p.subject.location.y) : p.GUI.addChatMessage(p, "Subject's location copied to clipboard!", True)
                    Case "/setText" : p.subject.text = command.Substring(9, command.Length - 9) '/setText newText ( ~ reprsents a new line [must have space before and after it], ` represents a new prompt) (SETS SUBJECT'S TEXT)
                    Case "/setDirection" '/setDirection (1-4)
                        If TypeOf p.subject Is character Then p.subject.direction = word(1)
                        If TypeOf p.subject Is Object Then If p.subject.type.type = 3 Then p.subject.destination.direction = word(1)
                    Case "/setCondition" : If TypeOf p.subject Is terrainObject Then p.subject.condition = word(1) '/setCondition condition (CHANGES OBJECT CONDITION)
                    Case "/setDestination" : If TypeOf p.subject Is terrainObject Then p.subject.destination = New worldPosition(word(1), New Vector2(word(2), word(3)), "0") 'setDestination areaName x y (SETS'S SUBJECT'S DESTINATION)
                    Case "/sendTo" : If TypeOf p.subject Is character Then p.subject.path = p.subject.findPath(New Vector2(Math.Truncate(p.subject.location.x), Math.Truncate(p.subject.location.y)), New Vector2(word(1), word(2))) : p.subject.prevGamestate = p.subject.gamestate : p.subject.gamestate = "followPath" : p.GUI.addChatMessage(p, "Sending subject to: " & word(1) & "," & word(2))

                    'COMMANDS USED IN THE WORLD EDITOR
                    Case "/createArea" : worldEditor.createArea(p, word(1), New Vector2(word(2), word(3))) : p.GUI.addChatMessage(p, "New area created: '" & word(1) & "', Dimentions: " & word(2) & ", " & word(3)) '/createArea name width height
                    Case "/deleteArea" : My.Computer.FileSystem.DeleteDirectory(Game.directory & "saves\quickSave\areas\" & word(1), FileIO.DeleteDirectoryOption.DeleteAllContents)
                    Case "/resizeArea" : resizeArea(p, command) 'reSizeArea length width (CHANGES AREA SIZE)
                    Case "/createDoor" : createDoor(p, command) '/createDoor skinID x1 y1 condition destination x2 y2
                    Case "/createNPC" : createNPC(p, command) '/createNPC area x y direction(1-4) filename name

                    Case "/use" : use(p, command) '/use modeName
                    Case "/give" : give(p, command) '/give id

                    Case "/setGrid" : setGrid(p, command) '/setGrid width height
                    Case "/setGridOffset" : p.grid = New Vector4(p.grid.X, p.grid.Y, word(1), word(2)) : p.GUI.addChatMessage(p, "Grid Offset: " & word(1) & ", " & word(2)) '/setGridOffset x y
                    Case "/setBrush" : p.brush = New Vector2(word(1), word(2)) : p.GUI.addChatMessage(p, "Brush size: " & word(1) & ", " & word(2)) '/setBrush width height

                    Case "/saveSelection" : FileManager.saveSelection(p.copiedRegion, word(1)) '/saveSelection selectionName
                    Case "/loadSelection" : p.copiedRegion = FileManager.loadSelection(word(1)) '/loadSelection selectionName
                    Case "/deleteSelection" : My.Computer.FileSystem.DeleteDirectory(Game.directory & "selections\" & word(1), FileIO.DeleteDirectoryOption.DeleteAllContents)
                    Case "/flip" : flipSelection(p, command) '/flip x OR /flip Y (FLIPS SELECTIONS)
                    Case "/rotate" : rotateSelection(p, command) '/rotate 90,180,270 (ROTATES SELECTIONS)

                    Case Else : p.GUI.addChatMessage(p, "Command not Recognized!")
                End Select
            Catch : p.GUI.addChatMessage(p, "Error, check your format with the /help command!") : p.GUI.addChatMessage(p, "You can also correct your command with tab")
            End Try
        Else
            For Each pl As player In Game.players : pl.GUI.addChatMessage(p, p.c.name & ": " & command, True)
            Next
        End If

    End Sub

    Public Shared Sub simulateCommand(ByRef p As player, command As String)
        performCommand(p, command, True)
    End Sub

    'EXECUTES COMMAND AFTER DELAY
    Public Shared Function executeCommandLater(ByRef p As player, command As String, time As Integer) As Integer
        Dim d As doCommandLater = AddressOf doCommand : d.BeginInvoke(p, command, time, Nothing, Nothing)
        Return 0
    End Function
    Public Shared Function doCommand(p As player, command As String, time As Integer) As Integer
        Thread.Sleep(time) : commandPrompt.performCommand(p, command, True)
        Return 0
    End Function

    'MISC COMMANDS
    Public Shared Sub sudo(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        For Each pl As player In Game.players
            If pl.c.name = word(1) And pl.c.name <> p.c.name Then simulateCommand(pl, "/" & command.Split("/")(2))
        Next
    End Sub
    Public Shared Sub logon(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).c.name = word(1) Then p.GUI.addChatMessage(p, "Player is already in the game!") : Exit Sub
        Next

        Game.players.Add(FileManager.loadPlayer("quicksave", word(1), True))
        gameDebug.allocatePlayerScreenSpace(Game.players, Game.resolution)
    End Sub
    Public Shared Sub bindKey(ByRef p As player, command As String)
        Dim word = command.Split(" ") : Dim key As New boundCommand
        key.key = word(1) : key.command = "/" & command.Split("/")(2)
        p.boundCommands.Add(key)
        p.GUI.addChatMessage(p, "Command '" & "/" & command.Split("/")(2) & "' has been bound to: " & key.key)
    End Sub
    Public Shared Sub help(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.GUI.addChatMessage(p, "Type '/help ' then a command(no dash) for how to use it!", True) : p.GUI.addChatMessage(p, "You can also get help for concepts such as subjects, modes, and selections!", True) : Exit Sub

        Select Case word(1)
            Case "sudo" : p.GUI.addChatMessage(p, "Sudo enters a command as another player.", True) : p.GUI.addChatMessage(p, "Format: /sudo playerName command", True)
            Case "logon" : p.GUI.addChatMessage(p, "Logon adds a player to the game (splitscreen).", True) : p.GUI.addChatMessage(p, "Format: /logon playerName", True)
            Case "selectMe" : p.GUI.addChatMessage(p, "SelectMe sets you as the subject.", True) : p.GUI.addChatMessage(p, "Format: /selectMe", True)
            Case "back" : p.GUI.addChatMessage(p, "Back sends you to a previous location.", True) : p.GUI.addChatMessage(p, "Format: /back OR /back number", True)
            Case "bindKey" : p.GUI.addChatMessage(p, "BindKey binds a command to a key press.", True) : p.GUI.addChatMessage(p, "Format: /bindKey key(capital letter) command", True)
            Case "clearChat" : p.GUI.addChatMessage(p, "ClearChat empties the chat.", True) : p.GUI.addChatMessage(p, "Format: /clearChat", True)
            Case "help" : p.GUI.addChatMessage(p, "Do I really need to tell you how to use this command AGAIN?", True)
            Case "listSaves" : p.GUI.addChatMessage(p, "ListSaves displays all saves in the game directory.", True) : p.GUI.addChatMessage(p, "Format: /listSaves", True)
            Case "debug" : p.GUI.addChatMessage(p, "Debug toggles debug mode, which allows command input and level editing.", True) : p.GUI.addChatMessage(p, "Format: /debug", True)
            Case "addEffect" : p.GUI.addChatMessage(p, "AddEffect applies an effect to the player's camera. (Quantity  and SpecialCondition are optional)", True) : p.GUI.addChatMessage(p, "Format: /addEffect effectName quantity specialCondition", True)
            Case "removeEffect" : p.GUI.addChatMessage(p, "RemoveEffect removes an effect from the player's camera.", True) : p.GUI.addChatMessage(p, "Format: /removeEffect effectName OR /removeEffect all", True)

            Case "setResolution" : p.GUI.addChatMessage(p, "SetResolution resizes the game window.", True) : p.GUI.addChatMessage(p, "Format: /setResolution width height", True)
            Case "setVolume" : p.GUI.addChatMessage(p, "SetVolume changes the volume of different aspects of the game.", True) : p.GUI.addChatMessage(p, "Format: /setVolume sound number(1-100) OR /setVolume music number(1-100)", True)
            Case "setBrightness" : p.GUI.addChatMessage(p, "SetBrightness sets the game brightness.", True) : p.GUI.addChatMessage(p, "Format: /setBrightness number(0-100)", True)
            Case "setTimeTick" : p.GUI.addChatMessage(p, "SetTimeTick changes how fast the day/night cycle progresses.", True) : p.GUI.addChatMessage(p, "Format: /setTimeTick number", True)

            Case "setTime" : p.GUI.addChatMessage(p, "SetTime changes the time of day.", True) : p.GUI.addChatMessage(p, "Format: /setTime day OR /setTime night OR /setTime number(1-24)", True)
            Case "setAreaBrightness" : p.GUI.addChatMessage(p, "SetBrightness sets the current area's default brightness.", True) : p.GUI.addChatMessage(p, "Format: /setAreaBrightness number(0-100)", True)
            Case "setMusic" : p.GUI.addChatMessage(p, "SetMusic sets the current area's defualt music.", True) : p.GUI.addChatMessage(p, "Format: /setMusic filepath(already assuming sounds/music/)", True)
            Case "setOutside" : p.GUI.addChatMessage(p, "SetOutside toggles if an area is outside.", True) : p.GUI.addChatMessage(p, "Format: /setOuside true OR /setOutside false", True)
            Case "setSaveable" : p.GUI.addChatMessage(p, "setSaveable toggles if a player can save the game in an area.", True) : p.GUI.addChatMessage(p, "Format: /setSaveable true OR /setSaveable false", True)
            Case "setGreet" : p.GUI.addChatMessage(p, "SetGreet sets the greet message for an area. ('_' represents a space)", True) : p.GUI.addChatMessage(p, "Format: /setGreet greetMessage", True)

            Case "saveGame" : p.GUI.addChatMessage(p, "SaveGame makes a new save or overwrites old saves.", True) : p.GUI.addChatMessage(p, "Format: /saveGame saveName", True)
            Case "loadSave" : p.GUI.addChatMessage(p, "LoadSave loads a pre-existing save.", True) : p.GUI.addChatMessage(p, "Format: /loadSave saveName", True)

            Case "tp" : p.GUI.addChatMessage(p, "tp teleports players to a specific location or other players.", True) : p.GUI.addChatMessage(p, "Format: /tp playerName OR /tp player1 player2 OR /tp areaName x y OR /tp playerName areaName x y", True)
            Case "invSee" : p.GUI.addChatMessage(p, "InvSee lets you interact with a subject or player's inventory.", True) : p.GUI.addChatMessage(p, "Format: /invSee subject OR /invSee playerName", True)
            Case "clearInv" : p.GUI.addChatMessage(p, "ClearInv clears a subject or player's inventory.", True) : p.GUI.addChatMessage(p, "Format: /clearInv OR /clearInv subject OR /clearInv playerName", True)
            Case "giveItem" : p.GUI.addChatMessage(p, "GiveItem adds an item to a subject or player's inventory. (Quantity is optional)", True) : p.GUI.addChatMessage(p, "Format: /giveItem playerName id quantity OR /giveItem subject id quantity", True)

            Case "noclip" : p.GUI.addChatMessage(p, "NoClip toggles hit detection for a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /noclip OR /noclip subject OR /noclip playerName", True)
            Case "setSpeed" : p.GUI.addChatMessage(p, "SetSpeed forces a subject or player's speed. 'default' resets their speed", True) : p.GUI.addChatMessage(p, "Format: /setSpeed # OR /setSpeed playerName # OR /setSpeed subject #", True)
            Case "v" : p.GUI.addChatMessage(p, "V toggles visibility for a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /v OR /v playerName OR /v subject", True)

            Case "kill" : p.GUI.addChatMessage(p, "Kill kills a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /kill OR /kill all OR /kill subject OR /kill playername", True)
            Case "heal" : p.GUI.addChatMessage(p, "Heal heals a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /heal OR /heal all OR /heal subject OR /heal playername", True)

            Case "freeze" : p.GUI.addChatMessage(p, "Freeze freezes a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /freeze OR /freeze all OR /freeze subject OR /freeze playername", True)
            Case "instigate" : p.GUI.addChatMessage(p, "Instigate instigates a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /instigate OR /instigate all OR /instigate subject OR /instigate playername", True)
            Case "scare" : p.GUI.addChatMessage(p, "Scare scares a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /scare OR /scare all OR /scare subject OR /scare playername", True)
            Case "calm" : p.GUI.addChatMessage(p, "Calm calms a subject or player.", True) : p.GUI.addChatMessage(p, "Format: /calm OR /calm all OR /calm subject OR /calm playername", True)

            Case "rename" : p.GUI.addChatMessage(p, "Rename renames a subject.", True) : p.GUI.addChatMessage(p, "Format: /rename subject", True)
            Case "move" : p.GUI.addChatMessage(p, "Move teleports a subject.", True) : p.GUI.addChatMessage(p, "Format: /move subject x y OR /move subject mousePos", True)  '(TELEPORTS SUBJECT)
            Case "getPos" : p.GUI.addChatMessage(p, "getPos copies a subject's world position to your clipboard.", True) : p.GUI.addChatMessage(p, "Format: /getPos", True)
            Case "setText" : p.GUI.addChatMessage(p, "setText defines the text of a subject (usually a sign).", True) : p.GUI.addChatMessage(p, "Format: /setText subject text", True)  '/setText newText ( ~ represents a new line [must have space before and after it], ` represents a new prompt) (SETS SUBJECT'S TEXT)
            Case "setDirection" : p.GUI.addChatMessage(p, "setDirection changes the direction of a subject, or forces the destination direction in doors.", True) : p.GUI.addChatMessage(p, "Format: /setDirection direction(numbers 1-4)", True)
            Case "setCondition" : p.GUI.addChatMessage(p, "setCondition sets a condition for an object (example: open or locked).", True) : p.GUI.addChatMessage(p, "Format: /setCondition condition", True)
            Case "setDestination" : p.GUI.addChatMessage(p, "setDestination sets the destination for a door object.", True) : p.GUI.addChatMessage(p, "Format: /setDestination areaName x y", True)
            Case "sendTo" : p.GUI.addChatMessage(p, "SendTo forces a subject to walk to a specific coordinate. (CURRENTLY ONLY PLAYERS)", True) : p.GUI.addChatMessage(p, "Format: /sendTo x y", True)

            Case "createArea" : p.GUI.addChatMessage(p, "CreateArea makes a new area.", True) : p.GUI.addChatMessage(p, "Format: /createArea name length width", True)
            Case "deleteArea" : p.GUI.addChatMessage(p, "DeleteArea deletes an area.", True) : p.GUI.addChatMessage(p, "Format: /deleteArea name", True)
            Case "resizeArea" : p.GUI.addChatMessage(p, "ResizeArea changes the size of the current area", True) : p.GUI.addChatMessage(p, "Format: /resizeArea", True)
            Case "createDoor" : p.GUI.addChatMessage(p, "CreateDoor makes a new door.", True) : p.GUI.addChatMessage(p, "Format: /createDoor [skin ID] [x1] [y1] [condition] [destination] [x2] [y2]", True)
            Case "createNPC" : p.GUI.addChatMessage(p, "CreateDoor makes a new door.", True) : p.GUI.addChatMessage(p, "Format: /createNPC area x y direction(1-4) filename name", True)

            Case "give" : p.GUI.addChatMessage(p, "Give changes the ID that you are using in the level editor.", True) : p.GUI.addChatMessage(p, "Format: /give id OR /give name", True)
            Case "use" : p.GUI.addChatMessage(p, "Use changes the mode that you are using in the level editor.", True) : p.GUI.addChatMessage(p, "Format: /use mode", True)

            Case "setGrid" : p.GUI.addChatMessage(p, "SetGrid sets where the level editor's grid snaps.", True) : p.GUI.addChatMessage(p, "Format: /setGrid width height", True)
            Case "setGridOffset" : p.GUI.addChatMessage(p, "SetGridOffset offsets the level editor grid.", True) : p.GUI.addChatMessage(p, "Format: /setGridOffset x y", True)
            Case "setBrush" : p.GUI.addChatMessage(p, "SetBrush changes brush size in the level editor.", True) : p.GUI.addChatMessage(p, "Format: /setBrush width height", True)

            Case "saveSelection" : p.GUI.addChatMessage(p, "SaveSelection saves a copied region to a file.", True) : p.GUI.addChatMessage(p, "Format: /saveSelection selectionName", True)
            Case "loadSelection" : p.GUI.addChatMessage(p, "LoadSelection loads a copied region from a file.", True) : p.GUI.addChatMessage(p, "Format: /loadSelection selectionName", True)
            Case "deleteSelection" : p.GUI.addChatMessage(p, "DeleteSelection deletes a copied region.", True) : p.GUI.addChatMessage(p, "Format: /deleteSelection selectionName", True)
            Case "flip" : p.GUI.addChatMessage(p, "Flip flips a selection horizonally or vertically.", True) : p.GUI.addChatMessage(p, "Format: /flip x OR /flip y", True)
            Case "rotate" : p.GUI.addChatMessage(p, "Rotate rotates a selection by 90 degree intervals.", True) : p.GUI.addChatMessage(p, "Format: /rotate 90 OR /rotate 180 OR /rotate 270", True)

            Case "effect" : p.GUI.addChatMessage(p, "All camera Effects: Earthquake, lightning, fadeIn, fadeOut, rotateLeft, rotateRight, followPath, spectate", True)
            Case "mode" : p.GUI.addChatMessage(p, "All level editor modes: Tile, Fill, Object, Item, NPC, Select, link, Duplicate, Erase", True)
            Case "subject" : p.GUI.addChatMessage(p, "Subjects are an object/NPC/player that can be acted upon by commands such as rename or giveItem. To get a subject, click on an object, NPC, or player while using the 'select' mode in the level editor!", True)
            Case "selection" : p.GUI.addChatMessage(p, "Selections are copied regions that you can place, save, lad, or delete in the level editor.", True)
            Case "list" : p.GUI.addChatMessage(p, "sudo, selectMe, back, bindkey, clearChat, setResolution, setVolume, setAreaBrightness, setTimeTick, setTime, setAreaBrightness, setMusic, setOutside, setSaveable, setGreet, saveGame, loadSave, deleteSave, tp, invSee, clearInv, giveItem, noclip, setSpeed, v, kill, heal, freeze, instigate, scare, calm, rename, move, getPos, setText, setDirection, setCondition, setDestination, createArea, createDoor, createNPC, give, use, setGrid, setGridOffset, setBrush, saveSelection, loadSelection, deleteSelection, flip, rotate", True)

            Case Else : p.GUI.addChatMessage(p, "Command not Recognized! Try '/help list' for a list of all commands!")
        End Select
    End Sub

    Public Shared Sub addEffect(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        Dim effect As New gameEffect()
        If word.Count = 2 Then effect = New gameEffect(word(1)) '/addEffect effect
        If word.Count = 3 Then effect = New gameEffect(word(1), word(2)) '/addEffect effect effectQuantity
        If word.Count = 4 Then
            If IsNumeric(word(3)) = True Then effect = New gameEffect(word(1), word(2), word(3)) '/addEffect effect effectQuantity timeleft
            If IsNumeric(word(3)) = False Then effect = New gameEffect(word(1), word(2)) '/addEffect effect effectQuantity timeleft
        End If
        If word.Count = 5 Then effect = New gameEffect(word(1), word(2), word(3), word(4)) '/addEffect effect effectQuantity subjectName timeleft (0 default)

        Select Case word(1)
            Case "spectate" : effect.target = p.subject
            Case "fadeOut" : p.c.area.forcedBrightness = p.c.area.brightness
            Case "fadeMusicIn", "fadeMusicOut"
                'addEffect fadeMusicIn 2 musicName
                effect.musicToFade = New List(Of gameSound)
                Select Case word(3)
                    Case "all"
                        For counter As Integer = 0 To Game.music.Count - 1
                            effect.musicToFade.Add(Game.music(counter))

                            For counter2 As Integer = 0 To p.c.effects.Count - 1
                                p.c.effects(counter2).musicToFade.Clear()
                            Next
                        Next
                    Case Else
                        For counter As Integer = 0 To Game.music.Count - 1
                            If Game.music(counter).name = word(3) Then
                                effect.musicToFade.Add(Game.music(counter))

                                For counter2 As Integer = 0 To p.c.effects.Count - 1
                                    p.c.effects(counter2).musicToFade.Remove(Game.music(counter))
                                Next
                            End If
                        Next
                End Select

                If effect.musicToFade.Count = 0 Then Exit Sub
        End Select
        p.c.effects.Add(effect)
    End Sub
    Public Shared Sub removeEffect(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        Dim effectsToRemove As New List(Of gameEffect)

        For counter As Integer = 0 To p.c.effects.Count - 1
            If p.c.effects(counter).name = word(1) Or word(1) = "all" Then
                effectsToRemove.Add(p.c.effects(counter))

                Select Case p.c.effects(counter).name
                    Case "lightning" : p.c.area.forcedBrightness = -1
                    Case "rotateLeft", "rotateRight" : p.c.camera.Rotation = 0
                End Select
            End If
        Next

        'REMOVES EFFECTS
        For counter As Integer = 0 To effectsToRemove.Count - 1
            p.c.effects.Remove(effectsToRemove(counter))
        Next
    End Sub

    'CHANGES GAME AND AREA SETTINGS
    Public Shared Sub setVolume(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word(2) >= 0 And word(2) <= 100 Then
            Select Case word(1)
                Case "sound" : Game.soundVolume = word(2) : p.GUI.addChatMessage(p, "Sound volume changed to: " & word(2)) : For Each s As gameSound In Game.sounds : s.sound.Volume = (word(2) / 100) : Next
                Case "music" : Game.musicVolume = word(2) : p.GUI.addChatMessage(p, "Music volume changed to: " & word(2)) : For Each s As gameSound In Game.music : s.sound.Volume = (word(2) / 100) : Next

                Case Else : p.GUI.addChatMessage(p, "'" & word(1) & "' is not a valid volume, sorry!")
            End Select
        Else
            p.GUI.addChatMessage(p, "Sorry! Volume must be from 0 to 100")
        End If
    End Sub
    Public Shared Sub setTime(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If IsNumeric(word(1)) Then
            Game.time = TimeSpan.FromHours(word(1))
        Else
            Select Case word(1)
                Case "day" : Game.time = TimeSpan.FromHours(11) : p.GUI.addChatMessage(p, "Good Morning!")
                Case "night" : Game.time = TimeSpan.FromHours(23) : p.GUI.addChatMessage(p, "Good Night!")

                Case Else : p.GUI.addChatMessage(p, "'" & word(1) & "' is not a valid time, sorry!")
            End Select
        End If
    End Sub
    Public Shared Sub setBrightness(ByRef p As player, command As String)
        Dim word = command.Split(" ")

        If word(1) >= 0 And word(1) <= 100 Then
            Game.brightness = word(1)
            p.GUI.addChatMessage(p, "Brightness: " & word(1))
        Else
            p.GUI.addChatMessage(p, "Sorry! Brightness must be from 0 to 100!")
        End If
    End Sub
    Public Shared Sub setAreaBrightness(ByRef p As player, command As String)
        Dim word = command.Split(" ")

        If word(1) >= 0 And word(1) <= 100 Then
            p.c.area.brightness = word(1)
            p.GUI.addChatMessage(p, "Area light level: " & word(1))
        Else
            p.GUI.addChatMessage(p, "Sorry! Brightness must be from 0 to 100!")
        End If

    End Sub
    Public Shared Sub setMusic(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If p.c.area.music <> word(1) Then
            p.c.area.music = "music/" & word(1) : gameDebug.stopSounds("music")
            If p.c.area.music <> "none" Then gameDebug.playSound(p.c.area.music, "music", Game.musicVolume, True)
            p.GUI.addChatMessage(p, "Area Music: " & word(1))
        Else : p.GUI.addChatMessage(p, "Area Music Unchanged. It is: " & word(1))
        End If

    End Sub

    Public Shared Sub loadSave(command As String, ByRef players As List(Of player))
        Dim word = command.Split(" ")
        If players.Count = 1 Then FileManager.load(word(1), players(0).c.name)
        If players.Count = 2 Then FileManager.load(word(1), players(0).c.name, players(1).c.name)
        If players.Count = 3 Then FileManager.load(word(1), players(0).c.name, players(1).c.name, players(2).c.name)
        If players.Count = 4 Then FileManager.load(word(1), players(0).c.name, players(1).c.name, players(2).c.name, players(3).c.name)
    End Sub

    'SUBJECT AND PLAYER COMMANDS
    Public Shared Sub kick(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word(1) = p.c.name Then p.GUI.addChatMessage(p, "You can't kick yourself!") : Exit Sub

        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).c.name = word(1) Then
                FileManager.savePlayer("quicksave", Game.players(counter)) : Game.players(counter).leaving = True
                For counter2 As Integer = 0 To Game.players.Count - 1 : Game.players(counter2).GUI.addChatMessage(p, Game.players(counter).c.name & " was kicked.") : Next
            End If
        Next
    End Sub
    Public Shared Sub teleport(ByRef p As player, command As String) 'NPC aspect needs work
        Dim word = command.Split(" ")

        If word.Count = 2 Then '/tp Bob
            For Each pl As player In Game.players
                If pl.c.name = word(1) Then p.c.changeArea("quickSave", pl.c.area.name, pl.c.location, pl.c.direction) : Exit Sub
            Next
        End If
        If word.Count = 3 Then '/tp thirtyvirus Bob
            Dim p1 As player = p : Dim p2 As player = p
            For Each pl As player In Game.players
                If pl.c.name = word(1) Then p1 = pl
                If pl.c.name = word(2) Then p2 = pl
            Next
            p1.c.changeArea("quickSave", p2.c.area.name, p2.c.location, p2.c.direction) : Exit Sub
        End If
        If word.Count = 4 Then '/tp lobby 20 20
            p.c.changeArea("quickSave", word(1), New Vector2(word(2), word(3)), p.c.direction) : Exit Sub
        End If
        If word.Count = 5 Then '/tp thirtyvirus lobby 20 20
            For Each pl As player In Game.players
                If pl.c.name = word(1) Then pl.c.changeArea("quickSave", word(2), New Vector2(word(3), word(4)), pl.c.direction) : Exit Sub
            Next
        End If
    End Sub
    Public Shared Sub invSee(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word(1) = p.c.name Then p.GUI.addChatMessage(p, "You can't loot yourself!") : Exit Sub
        If word(1) = "subject" Then
            If p.c.gamestate = "mapEdit" Then worldEditor.exitEditor(p)
            p.interact = p.subject : p.c.gamestate = "looting"
            If p.interact.inventory.items.Count = 0 Then p.side = 1 Else p.side = 2
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    If p.c.gamestate = "mapEdit" Then worldEditor.exitEditor(p)
                    p.interact = Game.players(counter) : p.c.gamestate = "looting"
                    If p.interact.inventory.items.Count = 0 Then p.side = 1 Else p.side = 2
                End If
            Next
        End If
    End Sub
    Public Shared Sub clearInventory(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.inventory.items.Clear() : p.c.inventory.money = 0 : Exit Sub
        If word(1) = "subject" Then
            p.subject.inventory.items.clear() : p.subject.inventory.money = 0 : Exit Sub
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.inventory.items.Clear() : Game.players(counter).c.inventory.money = 0 : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub giveItem(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        Dim i As New item : Dim q As Integer = 0
        If word.Count = 3 Then
            If IsNumeric(word(2)) Then
                i = New item(terrainManager.getItemType(word(2)), 1, New Vector2) : q = 1
            Else
                For counter As Integer = 0 To terrainManager.itemTypes.Count - 1
                    If terrainManager.itemTypes(counter).name = word(2) Then i = New item(terrainManager.itemTypes(counter), 1, New Vector2) : q = 1
                Next
            End If
        End If
        If word.Count = 4 Then
            If IsNumeric(word(2)) Then
                i = New item(terrainManager.getItemType(word(2)), word(3), New Vector2) : q = word(3)
            Else
                For counter As Integer = 0 To terrainManager.itemTypes.Count - 1
                    If terrainManager.itemTypes(counter).name = word(2) Then i = New item(terrainManager.itemTypes(counter), word(3), New Vector2) : q = word(3)
                Next
            End If
        End If

        If word(1) = "subject" Then
            p.subject.inventory.add(i) : p.GUI.addChatMessage(p, "Subject given " & q & " " & i.type.name) : Exit Sub
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.inventory.add(i) : p.GUI.addChatMessage(p, Game.players(counter).c.name & " given " & q & " " & i.type.name) : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub sendMessage(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        Dim length As Integer = (word(0) & " " & word(1) & " ").Length
        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).c.name = word(1) Then Game.players(counter).recieveMessage(command.Substring(length, command.Length - length))
        Next
    End Sub

    Public Shared Sub noclip(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.noClip = Not p.c.noClip : p.GUI.addChatMessage(p, "Noclip: " & p.c.noClip) : Exit Sub
        If word(1) = "subject" Then
            p.subject.noClip = Not p.subject.noClip : Exit Sub
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.noClip = Not Game.players(counter).c.noClip : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub setSpeed(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 2 Then
            If word(1) = "default" Then p.c.forcedSpeed = 0 Else : If IsNumeric(word(1)) Then p.c.forcedSpeed = word(1)
            p.GUI.addChatMessage(p, "Speed: " & p.c.forcedSpeed) : Exit Sub
        End If
        If word.Count = 3 Then
            If word(1) = "subject" Then
                If word(2) = "default" Then p.subject.forcedSpeed = 0 Else : If IsNumeric(word(2)) Then p.subject.forcedSpeed = word(2)
                Exit Sub
            Else
                For counter As Integer = 0 To Game.players.Count - 1
                    If Game.players(counter).c.name = word(1) Then
                        If word(2) = "default" Then Game.players(counter).c.forcedSpeed = 0 Else : If IsNumeric(word(2)) Then Game.players(counter).c.forcedSpeed = word(2)
                        Exit Sub
                    End If
                Next
            End If
        End If
    End Sub
    Public Shared Sub setGamestate(ByRef p As player, command As String)
        Dim word = command.Split(" ")

        p.c.gamestate = word(1)
    End Sub
    Public Shared Sub v(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.visible = Not p.c.visible : p.GUI.addChatMessage(p, "Visible: " & p.c.visible) : Exit Sub
        If word(1) = "subject" Then
            p.subject.visible = Not p.subject.visible : Exit Sub
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.visible = Not Game.players(counter).c.visible : Exit Sub
                End If
            Next
        End If
    End Sub

    Public Shared Sub kill(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.health = 0 : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).c.health = 0 : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been killed, you monster.") : Exit Sub
        If word(1) = "subject" Then
            p.subject.health = 0
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.health = 0 : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub heal(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.health = p.c.maxHealth : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).c.health = p.c.area.npcs(counter).c.maxHealth : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been healed!") : Exit Sub
        If word(1) = "subject" Then
            p.subject.health = p.subject.maxhealth
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.health = Game.players(counter).c.maxHealth : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub freeze(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.gamestate = "frozen" : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).c.gamestate = "frozen" : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been frozen!") : Exit Sub
        If word(1) = "subject" Then
            p.subject.gamestate = "frozen"
            p.GUI.addChatMessage(p, p.subject.name & " has been frozen!")
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.gamestate = "frozen"
                    p.GUI.addChatMessage(p, Game.players(counter).c.name & " has been frozen!") : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub instigate(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.gamestate = "hostile" : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).target = p.c : p.c.area.npcs(counter).c.gamestate = "hostile" : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been instigated!") : Exit Sub
        If word(1) = "subject" Then
            p.subject.gamestate = "hostile" : p.subject.hostNPC.target = p.c
            p.subject.hostNPC.target.updateMusic("", "music/battle", 2)
            p.GUI.addChatMessage(p, p.subject.name & " has been instigated!")
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.gamestate = "hostile"
                    p.GUI.addChatMessage(p, Game.players(counter).c.name & " has been insitgated!") : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub scare(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.gamestate = "scared" : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).target = p.c : p.c.area.npcs(counter).c.gamestate = "scared" : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been scared!") : Exit Sub
        If word(1) = "subject" Then
            p.subject.gamestate = "scared" : p.subject.gamestate.target = p
            p.GUI.addChatMessage(p, p.subject.name & " has been scared!")
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.gamestate = "scared"
                    p.GUI.addChatMessage(p, Game.players(counter).c.name & " has been scared!") : Exit Sub
                End If
            Next
        End If
    End Sub
    Public Shared Sub calm(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word.Count = 1 Then p.c.gamestate = "idle" : Exit Sub
        If word(1) = "all" Then For counter As Integer = 0 To p.c.area.npcs.Count - 1 : p.c.area.npcs(counter).c.gamestate = "idle" : Next : p.GUI.addChatMessage(p, "All nearby NPCs have been calmed!") : Exit Sub
        If word(1) = "subject" Then
            p.subject.gamestate = "idle"
            p.GUI.addChatMessage(p, p.subject.name & " has been calmed!")
        Else
            For counter As Integer = 0 To Game.players.Count - 1
                If Game.players(counter).c.name = word(1) Then
                    Game.players(counter).c.gamestate = "idle"
                    p.GUI.addChatMessage(p, Game.players(counter).c.name & " has been calmed!") : Exit Sub
                End If
            Next
        End If
    End Sub

    'LEVEL EDITOR COMMANDS
    Public Shared Sub createDoor(ByRef p As player, command As String)
        Dim word = command.Split(" ") 'format: /createDoor [skin ID] [x1] [y1] [condition] [destination] [x2] [y2]

        p.c.area.place(New terrainObject(terrainManager.getObjectType(word(1)), New Vector2(word(2), word(3)), word(4), New worldPosition(word(5), New Vector2(word(6), word(7)), word(8))))
        p.GUI.addChatMessage(p, "Created door in: " & p.c.area.name & " at " & word(2) & ", " & word(3) & ". Goes to: " & word(5) & " at " & word(6) & ", " & word(7) & ".")
    End Sub
    Public Shared Sub createNPC(ByRef p As player, command As String) 'WORKS EVEN IN UNOCCUPIED AREAS
        Dim word = command.Split(" ") '/createNPC area x y direction(1-4) filename name

        'PLACES NEW NPC INTO WORLD
        If word(1) = p.c.area.name Then : Dim inventory As New Inventory
            'CREATES NPC
            Dim newNPC As npc = New npc(New character(FileManager.createTexture(Game.directory & "textures/characters/" & word(5) & ".png"), word(5), word(6), inventory, 100, 100, "idle"), "villager")

            'PLACES NPC INTO WOLRD
            newNPC.c.hostNPC = newNPC : newNPC.c.area = p.c.area : newNPC.c.location = New Vector2(word(2), word(3)) : newNPC.c.direction = word(4) : p.c.area.npcs.Add(newNPC)
        End If

        'SAVES NPC TO FILE
        Dim npc As String = "" : Dim location As String = Game.directory & "saves\quickSave\NPCs\" & word(6) & ".txt"
        npc = npc & word(1) & "-" & word(2) & "-" & word(3) & "-" & word(4) & "-" 'writes location
        npc = npc & 100 & "-" & 100 & "-" & word(5) & "-" & word(6) & "-" & "villager" & "-" & "idle" & "-"
        FileManager.writeTextFile(location, npc)
    End Sub
    Public Shared Sub resizeArea(ByRef p As player, command As String)
        Dim word = command.Split(" ") 'reSizeArea length width

        'COPIES AREA
        p.copyRegion = New Vector4(0, 0, p.c.area.size.X, p.c.area.size.Y)
        worldEditor.copyRegion(p)

        'MOVES ALL PLAYERS AND NPCS IN AREA TO TOP LEFT CORNER
        For counter As Integer = 0 To p.c.area.npcs.Count - 1
            p.c.area.npcs(counter).c.location = New Vector2(1, 1)
        Next
        For counter As Integer = 0 To Game.players.Count - 1
            If Game.players(counter).c.area.name = p.c.area.name Then Game.players(counter).c.location = New Vector2(1, 1)
        Next

        'RESIZES AREA AND CLEARS EVERYTHING
        p.c.area.size = New Vector2(word(1), word(2))
        p.c.area.objects.Clear()
        p.c.area.items.Clear()
        p.c.area.tileMap.Clear()
        For counter As Integer = 0 To p.c.area.size.X * p.c.area.size.Y : p.c.area.tileMap.Add(0) : Next
        ReDim p.c.area.hitDetect(p.c.area.tileMap.Count)

        'PLACES BACK EVERYTHING
        worldEditor.placeRegion(p, New Vector2(0, 0))
        worldEditor.exitEditor(p)
    End Sub

    Public Shared Sub use(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If p.c.gamestate <> "mapEdit" Then p.GUI.addChatMessage(p, "You can only use that in the world editor!") : Exit Sub
        If word(1) = p.mode Then p.GUI.addChatMessage(p, "You are already using " & word(1) & "!") : Exit Sub

        Select Case word(1)
            Case "tile", "fill", "object", "item", "npc", "select", "link", "duplicate", "erase" : p.id = 0 : p.mode = word(1) : p.subType = 0 : p.GUI.addChatMessage(p, "Using: " & word(1))
        End Select
        Select Case word(1)

            Case "tile", "fill", "object", "item", "npc", "select", "erase"
            Case "duplicate" : If IsNothing(p.copiedRegion) Then p.copiedRegion = New copiedRegion(New Vector2(1, 1))
            Case "link" : p.GUI.addChatMessage(p, "Click on any spot on a door to select it")

            Case Else : p.GUI.addChatMessage(p, "'" & word(1) & "' is not a valid tool!", True)
        End Select
    End Sub
    Public Shared Sub give(p As player, command As String)
        Dim word = command.Split(" ")
        If IsNumeric(word(1)) = True Then
            Select Case p.mode
                Case "tile", "fill"
                    For counter As Integer = 0 To terrainManager.tileTypes.Count - 1
                        If word(1) = terrainManager.tileTypes(counter).id Then p.id = terrainManager.tileTypes(counter).id : p.GUI.addChatMessage(p, "Given: " & terrainManager.tileTypes(counter).name) : Exit Sub
                    Next
                Case "object"
                    For counter As Integer = 0 To terrainManager.objectTypes.Count - 1
                        If word(1) = terrainManager.objectTypes(counter).id Then p.id = terrainManager.objectTypes(counter).id : p.GUI.addChatMessage(p, "Given:  " & terrainManager.objectTypes(counter).name) : Exit Sub
                    Next
                Case "item"
                    For counter As Integer = 0 To terrainManager.itemTypes.Count - 1
                        If word(1) = terrainManager.itemTypes(counter).id Then p.id = terrainManager.itemTypes(counter).id : p.GUI.addChatMessage(p, "Given: " & terrainManager.itemTypes(counter).name) : Exit Sub
                    Next
            End Select
        Else
            Select Case p.mode
                Case "tile", "fill"
                    For counter As Integer = 0 To terrainManager.tileTypes.Count - 1
                        If word(1) = terrainManager.tileTypes(counter).name Then p.id = terrainManager.tileTypes(counter).id : p.GUI.addChatMessage(p, "Given: " & terrainManager.tileTypes(counter).name) : Exit Sub
                    Next
                Case "object"
                    For counter As Integer = 0 To terrainManager.objectTypes.Count - 1
                        If word(1) = terrainManager.objectTypes(counter).name Then p.id = terrainManager.objectTypes(counter).id : p.GUI.addChatMessage(p, "Given: " & terrainManager.objectTypes(counter).name) : Exit Sub
                    Next
                Case "item"
                    For counter As Integer = 0 To terrainManager.itemTypes.Count - 1
                        If word(1) = terrainManager.itemTypes(counter).name Then p.id = terrainManager.itemTypes(counter).id : p.GUI.addChatMessage(p, "Given: " & terrainManager.itemTypes(counter).name) : Exit Sub
                    Next
            End Select
        End If
        p.GUI.addChatMessage(p, "'" & word(1) & "' is not a valid id or name!")
    End Sub

    Public Shared Sub setGrid(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word(1) = "selection" Then p.grid = New Vector4(p.copiedRegion.size.X, p.copiedRegion.size.Y, 0, 0) : p.GUI.addChatMessage(p, "Grid size set to: " & p.copiedRegion.size.X & ", " & p.copiedRegion.size.Y) : Exit Sub
        If word.Count = 2 Then p.GUI.addChatMessage(p, "Grid size set to: " & word(1)) : p.grid = New Vector4(word(1), word(1), 0, 0) : Exit Sub
        If word.Count = 3 Then p.GUI.addChatMessage(p, "Grid size set to: " & word(1) & ", " & word(2)) : p.grid = New Vector4(word(1), word(2), 0, 0) : Exit Sub
        If word.Count = 4 Then p.GUI.addChatMessage(p, "Grid size and offset set to: " & word(1) & ", " & word(2) & ", " & word(3) & ", 0") : p.grid = New Vector4(word(1), word(2), word(3), 0) : Exit Sub
        If word.Count = 5 Then p.GUI.addChatMessage(p, "Grid size and offset set to: " & word(1) & ", " & word(2) & ", " & word(3) & ", " & word(4)) : p.grid = New Vector4(word(1), word(2), word(3), word(4)) : Exit Sub
    End Sub
    Public Shared Sub flipSelection(ByRef p As player, command As String)
        Dim word = command.Split(" ")
        If word(1) = "x" Then

            'FLIPS TILEMAP
            Dim x As Integer = 0 : Dim y As Integer = 0 : Dim newTilemap As New List(Of Integer) : Dim row As New List(Of Integer)
            For counter As Integer = 0 To p.copiedRegion.tileMap.Count - 1
                If x < p.copiedRegion.size.X Then
                    row.Add(p.copiedRegion.tileMap(counter))
                    x += 1
                End If
                If x >= p.copiedRegion.size.X Then
                    For counter2 = 0 To row.Count - 1 : newTilemap.Add(row(row.Count - 1 - counter2)) : Next
                    row = New List(Of Integer) : x = 0 : y += 1
                End If
            Next
            p.copiedRegion.tileMap = newTilemap

            'FLIPS OBJECTS / ITEMS
            For counter As Integer = 0 To p.copiedRegion.objects.Count - 1 : p.copiedRegion.objects(counter).location = New Vector2(p.copiedRegion.size.X - p.copiedRegion.objects(counter).location.X - p.copiedRegion.objects(counter).type.size.X, p.copiedRegion.objects(counter).location.Y) : Next
            For counter As Integer = 0 To p.copiedRegion.items.Count - 1 : p.copiedRegion.items(counter).location = New Vector2(p.copiedRegion.size.X - 1 - p.copiedRegion.items(counter).location.X, p.copiedRegion.items(counter).location.Y) : Next
        End If

        If word(1) = "y" Then
            'FLIPS TILEMAP
            p.copiedRegion.tileMap.Reverse()
            Dim x As Integer = 0 : Dim y As Integer = 0 : Dim newTilemap As New List(Of Integer) : Dim row As New List(Of Integer)
            For counter As Integer = 0 To p.copiedRegion.tileMap.Count - 1
                If x < p.copiedRegion.size.X Then
                    row.Add(p.copiedRegion.tileMap(counter))
                    x += 1
                End If
                If x >= p.copiedRegion.size.X Then
                    For counter2 = 0 To row.Count - 1 : newTilemap.Add(row(row.Count - 1 - counter2)) : Next
                    row = New List(Of Integer) : x = 0 : y += 1
                End If
            Next
            p.copiedRegion.tileMap = newTilemap

            'FLIPS OBJECTS / ITEMS
            For counter As Integer = 0 To p.copiedRegion.objects.Count - 1 : p.copiedRegion.objects(counter).location = New Vector2(p.copiedRegion.objects(counter).location.X, p.copiedRegion.size.Y - p.copiedRegion.objects(counter).location.Y - p.copiedRegion.objects(counter).type.size.Y) : Next
            For counter As Integer = 0 To p.copiedRegion.items.Count - 1 : p.copiedRegion.items(counter).location = New Vector2(p.copiedRegion.items(counter).location.X, p.copiedRegion.size.Y - 1 - p.copiedRegion.items(counter).location.Y) : Next
        End If
    End Sub
    Public Shared Sub rotateSelection(ByRef p As player, command As String)
        Dim word = command.Split(" ") : Dim amount As Integer = word(1) / 90
        If amount <> Int(amount) Then p.GUI.addChatMessage(p, "You can only rotate by multiples of 90!") : Exit Sub
        If amount > 3 Then amount = amount Mod 4

        Do Until amount = 0
            'ROTATES 90 DEGREES CLOCKWISE
            Dim x As Integer = 0 : Dim y As Integer = 0 : Dim newTilemap As New List(Of Integer)
            Dim rows As New List(Of List(Of Integer)) : Dim row As New List(Of Integer)

            For counter As Integer = 0 To p.copiedRegion.tileMap.Count - 1
                If x < p.copiedRegion.size.X Then row.Add(p.copiedRegion.tileMap(counter)) : x += 1
                If x >= p.copiedRegion.size.X Then rows.Add(row) : row = New List(Of Integer) : x = 0 : y += 1
            Next
            For counter As Integer = 0 To p.copiedRegion.size.X - 1
                For counter2 As Integer = 0 To rows.Count - 1
                    newTilemap.Add(rows(p.copiedRegion.size.Y - 1 - counter2)(counter))
                Next
            Next
            p.copiedRegion.tileMap = newTilemap
            p.copiedRegion.size = New Vector2(p.copiedRegion.size.Y, p.copiedRegion.size.X)

            For counter As Integer = 0 To p.copiedRegion.objects.Count - 1

                Dim y1 As Integer = p.copiedRegion.objects(counter).location.X
                Dim x1 As Integer = p.copiedRegion.size.Y - p.copiedRegion.objects(counter).type.size.Y - p.copiedRegion.objects(counter).location.Y

                p.copiedRegion.objects(counter).location = New Vector2(x1, y1)
            Next
            amount -= 1
        Loop
    End Sub
End Class