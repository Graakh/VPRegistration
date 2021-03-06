Imports System
Imports System.Threading
Imports System.Collections

Imports DeathByCaptcha

Public Class ExampleSimple
	Public Shared Sub Main(ByVal args As String())
	    ' Put your DBC username & password here:
	    'Dim clnt As New HttpClient(args(0), args(1))
	    Dim clnt As New SocketClient(args(0), args(1))

	    Console.WriteLine(String.Format("Your balance is {0,2:f} US cents", clnt.Balance))

	    ' Uploading captchas with type = 2 (Coordinates API)
	    ' Dim ext_data As New Hashtable()
	    ' ext_data.Add("type", 2)
	    ' Put your CAPTCHA image file name, file object, stream, or vector
	    ' of bytes, and optional solving timeout (in seconds) here:
	    ' Dim cptch As Captcha = clnt.Decode(args(2), 2 * Client.DefaultTimeout, ext_data)

	    ' Uploading captchas with type = 3 (Image Group API)
	    ' Dim ext_data As New Hashtable()
	    ' ext_data.Add("type", 3)
	    ' ext_data.Add("banner_text", "Select all images with meat")
	    ' ext_data.Add("grid", "4x4") ' this is optional
	    ' ext_data.Add("banner", bannerFileName)
	    ' Put your CAPTCHA image file name, file object, stream, or vector
	    ' of bytes, and optional solving timeout (in seconds) here:
	    ' Dim cptch As Captcha = clnt.Decode(args(2), 2 * Client.DefaultTimeout, ext_data)

	    ' Put your CAPTCHA image file name, file object, stream, or vector
	    ' of bytes, and optional solving timeout (in seconds) here:
	    Dim cptch As Captcha = clnt.Decode(args(2), 2 * Client.DefaultTimeout)
	    If cptch IsNot Nothing Then
	        Console.WriteLine(String.Format("CAPTCHA {0:d} solved: {1}", cptch.Id, cptch.Text))

	        ' Report an incorrectly solved CAPTCHA.
	        ' Make sure the CAPTCHA was in fact incorrectly solved, do not
	        ' just report it at random, or you might be banned as abuser.
	        'If clnt.Report(cptch) Then
	        '    Console.WriteLine("Reported as incorrectly solved")
	        'Else
	        '    Console.WriteLine("Failed reporting as incorrectly solved")
	        'End If
	    End If
	End Sub
End Class



Public Class ExampleFull
    Public Shared Sub Main(ByVal args As String())
        ' Put your DBC username & password here:
        'Dim clnt As New HttpClient(args(0), args(1))
        Dim clnt As New SocketClient(args(0), args(1))

        Console.WriteLine(String.Format("Your balance is {0,2:f} US cents", clnt.Balance))

        ' Put your CAPTCHA image file name, file object, stream, or vector
        ' of bytes here:
        Dim cptch As Captcha = clnt.Upload(args(2))
        Console.WriteLine(String.Format("CAPTCHA {0:d} uploaded", cptch.Id))

        ' Poll for the CAPTCHA status.
        While cptch.Uploaded AND NOT cptch.Solved
            Thread.Sleep(Client.PollsInterval * 1000)
            cptch = clnt.GetCaptcha(cptch.Id)
        End While

        If cptch.Solved Then
            Console.WriteLine(String.Format("CAPTCHA {0:d} solved: {1}", cptch.Id, cptch.Text))

            ' Report an incorrectly solved CAPTCHA.
            ' Make sure the CAPTCHA was in fact incorrectly solved,
            ' do not just report them at random, or you might be banned
            ' as abuser.
            'If clnt.Report(cptch) Then
            '    Console.WriteLine("Reported as incorrectly solved")
            'Else
            '    Console.WriteLine("Failed reporting as incorrectly solved")
            'End If
        Else
            Console.WriteLine("CAPTCHA was not solved")
        End If
    End Sub
End Class
