﻿Imports System.IO
Imports System.Net

Public Class Form1

    Public Function GetCRC32(ByVal sFileName As String) As String
        Try
            Dim FS As FileStream = New FileStream(sFileName, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            Dim CRC32Result As Integer = &HFFFFFFFF
            Dim Buffer(4096) As Byte
            Dim ReadSize As Integer = 4096
            Dim Count As Integer = FS.Read(Buffer, 0, ReadSize)
            Dim CRC32Table(256) As Integer
            Dim DWPolynomial As Integer = &HEDB88320
            Dim DWCRC As Integer
            Dim i As Integer, j As Integer, n As Integer

            'Create CRC32 Table
            For i = 0 To 255
                DWCRC = i
                For j = 8 To 1 Step -1
                    If (DWCRC And 1) Then
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                        DWCRC = DWCRC Xor DWPolynomial
                    Else
                        DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                    End If
                Next j
                CRC32Table(i) = DWCRC
            Next i

            'Calcualting CRC32 Hash
            Do While (Count > 0)
                For i = 0 To Count - 1
                    n = (CRC32Result And &HFF) Xor Buffer(i)
                    CRC32Result = ((CRC32Result And &HFFFFFF00) \ &H100) And &HFFFFFF
                    CRC32Result = CRC32Result Xor CRC32Table(n)
                Next i
                Count = FS.Read(Buffer, 0, ReadSize)
            Loop
            Return Hex(Not (CRC32Result))
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function CercaArcadeDatabase(ByVal sFileName As String) As String
        Dim strReq As String = "" 'Testo della richiesta/query
        Dim strData As String = "" 'Testo recuperato dalla richiesta
        Dim dataStream As Stream
        Dim reader As StreamReader
        Dim request As WebRequest
        Dim response As WebResponse

        Dim query As String = "query_mame&game_name="
        Dim lingua As String = "it"

        If RadioButton2.Checked Then
            lingua = "en"
        End If

        strReq = "http://adb.arcadeitalia.net/service_scraper.php?ajax=" & query & sFileName & "&lang=" & lingua
        'strReq = "http://www.progettoemma.net/index.php?gioco=4dwarrio"
        request = WebRequest.Create(strReq)
        response = request.GetResponse()
        dataStream = response.GetResponseStream()
        reader = New StreamReader(dataStream)
        strData = reader.ReadToEnd()
        CercaArcadeDatabase = strData
        reader.Close()
        response.Close()
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim cartella As String = ""

        FolderBrowserDialog1.ShowDialog()
        cartella = FolderBrowserDialog1.SelectedPath
        Label1.Text = cartella
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim FILE_NAME As String = "log.txt"
        Dim sw As StreamWriter
        Dim fs As FileStream = Nothing

        Dim game As String = ""
        Dim info As String = ""

        Dim crc32 As String = ""
        Dim contatore As Integer = 0
        Dim contatoreScartati As Integer = 0

        If File.Exists(FILE_NAME) = True Then 'se esite un file di log lo cancelliamo
            File.Delete(FILE_NAME)
        End If

        fs = File.Create(FILE_NAME)
        fs.Close()
        sw = File.AppendText(FILE_NAME)

        Dim inizio As DateTime = Now

        For Each file As String In Directory.GetFiles(Label1.Text)

            game = file.Substring(Label1.Text.Length + 1, file.Length - Label1.Text.Length - 5)
            Label2.Text = game

            info = CercaArcadeDatabase(game)
            If info.Chars(11) <> "]" Then
                crc32 = GetCRC32(file)

                sw.WriteLine(game & " - " & crc32 & " - " & info)
                contatore += 1
            Else
                game = file.Substring(Label1.Text.Length + 1, file.Length - Label1.Text.Length - 1)
                sw.WriteLine(game & " - Fallito")
                contatoreScartati += 1
            End If
        Next
        Dim fine As DateTime = Now
        sw.Close()

        MsgBox("Scansione terminata! Elementi individuati:" & contatore & " in " & fine.Subtract(inizio).Seconds)

    End Sub
End Class
