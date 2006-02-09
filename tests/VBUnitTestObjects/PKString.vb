Option Strict Off
Option Explicit On

Imports Sooda
Imports Sooda.UnitTests.BaseObjects
Imports System
Imports System.Collections
Imports System.Data
Imports System.Diagnostics

Namespace Sooda.UnitTests.VBObjects
    Public Class PKString
        Inherits Sooda.UnitTests.VBObjects.Stubs.PKString_Stub
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
