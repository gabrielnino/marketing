; AutoIt Script - whatsapp_upload.au3
Opt('WinTitleMatchMode', 2)
Local $timeout = 10

If WinWaitActive('Open', '', $timeout) = 0 Then
    If WinWaitActive('Abrir', '', $timeout) = 0 Then
        Exit 1
    EndIf
EndIf

Local $title = ''
If WinActive('Open') Then
    $title = 'Open'
ElseIf WinActive('Abrir') Then
    $title = 'Abrir'
Else
    Exit 2
EndIf

ControlSetText($title, '', '[CLASS:Edit; INSTANCE:1]', "C:\imagenes\goku.png")
Sleep(300)

If ControlClick($title, '', '[CLASS:Button; INSTANCE:1]') = 0 Then
    ControlSend($title, '', '[CLASS:Edit; INSTANCE:1]', '{ENTER}')
EndIf

Exit 0
