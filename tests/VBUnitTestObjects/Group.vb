Option Strict Off
Option Explicit On

Imports Sooda
Imports System
Imports System.Collections
Imports System.Data
Imports System.Diagnostics

Namespace Sooda.UnitTests.Objects
    Public Class Group
        Inherits Sooda.UnitTests.Objects.Stubs.Group_Stub
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
        Public Sub New(ByVal transaction As SoodaTransaction, ByVal factory As ISoodaObjectFactory)
            MyBase.New(transaction, factory)
            'Do not modify this constructor.
        End Sub
    End Class
End Namespace
