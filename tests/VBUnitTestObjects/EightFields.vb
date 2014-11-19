Imports System
Imports System.Collections
Imports System.Diagnostics
Imports System.Data
Imports Sooda
Imports SoodaUnitTestsVBObjectsStubs = Sooda.UnitTests.VBObjects.Stubs
Imports Sooda.UnitTests.BaseObjects

Namespace Sooda.UnitTests.VBObjects
    
    Public Class EightFields
        Inherits SoodaUnitTestsVBObjectsStubs.EightFields_Stub
        
        Public Sub New(ByVal c As SoodaConstructor)
            MyBase.New(c)
            'Do not modify this constructor.
        End Sub
        
        Public Sub New(ByVal transaction As SoodaTransaction)
            MyBase.New(transaction)
            '
            'TODO: Add construction logic here.
            '
        End Sub
        
        Public Sub New()
            Me.New(SoodaTransaction.ActiveTransaction)
            'Do not modify this constructor.
        End Sub
    End Class
End Namespace
