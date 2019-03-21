Namespace safeprojectname
    ''' <summary>
    ''' The main class.
    ''' </summary>
    Public Module Program

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Public Sub Main()
            Using game = New Game()
                game.Run()
            End Using
        End Sub
    End Module
End Namespace