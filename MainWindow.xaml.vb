
Imports System
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Security
Imports System.Windows.Threading

Class MainWindow

    Dim authkey As String
    Dim input As String
    Dim output As String
    Dim url As String = "https://api.tinypng.com/shrink"
    Dim auth As String

    Public Sub New()

        ' この呼び出しはデザイナーで必要です。
        InitializeComponent()

        ' InitializeComponent() 呼び出しの後で初期化を追加します。

        authkey = "FNekLtZzc1cWP-z3QCyQ8iG4fVP_4K0z"
        auth = Convert.ToBase64String(Encoding.UTF8.GetBytes("api:" + authkey))

        txtbox.Background = New SolidColorBrush(Colors.White)

    End Sub

    Private Sub txtbox_PreviewDragOver(ByVal sender As System.Object, ByVal e As System.Windows.DragEventArgs)

        ' ファイルをドロップされた場合のみe.Handled を True にする
        txtbox.Background = New SolidColorBrush(Colors.Aqua)
        If e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, True) Then
            e.Effects = System.Windows.DragDropEffects.Copy
            e.Handled = True
        Else
            e.Effects = System.Windows.DragDropEffects.None
            e.Handled = False
        End If

    End Sub

    Private Sub txtbox_PreviewDragLeave(sender As Object, e As DragEventArgs)
        txtbox.Background = New SolidColorBrush(Colors.White)
    End Sub

    Private Sub txtbox_Drop(ByVal sender As System.Object, ByVal e As System.Windows.DragEventArgs)

        Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())

        If files IsNot Nothing Then

            txtbox.Text = ""

            For Each f In files

                If File.Exists(f) = True Then
                    tinyConvert(f)
                ElseIf System.IO.Directory.Exists(f) = True Then
                    DirSearch(f)
                End If

            Next

        End If

        Console.WriteLine("Convert Complete." & vbCrLf)
        txtbox.Text += "Convert Complete." & vbCrLf

        If txtbox.Background.ToString() = "#FF00FFFF" Then
            txtbox.Background = New SolidColorBrush(Colors.White)
        End If

    End Sub

    Sub tinyConvert(ByVal fDir As String)

        Dim client As WebClient = New WebClient()
        'SSLエラー回避
        Try
            ServicePointManager.ServerCertificateValidationCallback = _
                New System.Net.Security.RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
        Catch ex As Exception
        End Try

        client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + auth)

        Dim expDir As String = IO.Path.GetDirectoryName(fDir) & "\tiny"
        Dim fName As String = IO.Path.GetFileName(fDir)

        If Directory.Exists(expDir) = False Then
            Directory.CreateDirectory(expDir)
        End If

        Try
            client.UploadData(New Uri(url), File.ReadAllBytes(fDir))

            Call UIUpdate()
            System.Threading.Thread.Sleep(100)

            '/* Compression was successful, retrieve output from Location header. */
            client.DownloadFile(client.ResponseHeaders("Location"), expDir & "\" & fName)
            txtbox.Text += expDir & "\" & fName & vbCrLf

            Call UIUpdate()
            System.Threading.Thread.Sleep(100)

            '非同期未対応
            'AddHandler client.UploadDataCompleted,
            '    Sub(sender As Object, e As System.Net.UploadDataCompletedEventArgs)
            '        client.DownloadFile(client.ResponseHeaders("Location"), expDir & "\" & fName)
            '        txtbox.Text += expDir & "\" & fName & vbCrLf
            '    End Sub
            'client.UploadDataAsync(New Uri(url), File.ReadAllBytes(fDir))

        Catch ex As WebException
            '/* Something went wrong! You can parse the JSON body for details. */
            Console.WriteLine("Compression failed.[{0}]" & vbCrLf, ex.Message)
            txtbox.Text += String.Format("Compression failed.[{0}]" & vbCrLf, ex.Message)
            txtbox.Background = New SolidColorBrush(Colors.Pink)
        End Try

    End Sub

    'SSLエラー回避用（強制True）
    Public Function AcceptAllCertifications(ByVal sender As Object,
                                            ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate,
                                            ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain,
                                            ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function

    Public Shared Sub UIUpdate()
        Dim Frame = New DispatcherFrame
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, New DispatcherOperationCallback(AddressOf ExitFrames), Frame)
        Dispatcher.PushFrame(Frame)
    End Sub

    Public Shared Function ExitFrames(frames As Object) As Object
        DirectCast(frames, DispatcherFrame).Continue = False
        Return Nothing
    End Function

    Sub DirSearch(ByVal sDir As String)

        Try
            For Each d In Directory.GetDirectories(sDir)
                For Each f In Directory.GetFiles(d, ".png")
                    Console.WriteLine(f)
                    tinyConvert(f)
                Next
                DirSearch(d)
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
            txtbox.Text += ex.Message & vbCrLf
            txtbox.Background = New SolidColorBrush(Colors.Pink)
            Exit Sub
        End Try

    End Sub

End Class
