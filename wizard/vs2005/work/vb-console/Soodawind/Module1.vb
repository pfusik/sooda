Imports Soodawind
Imports Sooda

<Assembly: SoodaStubAssembly(GetType(Soodawind._DatabaseSchema))> 

Module Module1

    Sub Main()
        '
        ' TODO - change database connection information in App.config
        '

        Using transaction As New SoodaTransaction

            '
            ' TODO - Add code that uses Sooda objects here
            '

            transaction.Commit()
        End Using
    End Sub
End Module
