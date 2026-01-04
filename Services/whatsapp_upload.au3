; ============================================================
; whatsapp_upload.au3
; Static, defensive AutoIt script for file upload
; Method: OpenFileDialogWithAutoIT (static version)
; ============================================================

Opt("WinTitleMatchMode", 2)
Opt("WinWaitDelay", 100)
Opt("SendKeyDelay", 30)
Opt("MouseClickDelay", 50)

Global Const $LOG_FILE = @ScriptDir & "\whatsapp_upload.log"
Global Const $FILE_TO_UPLOAD = "C:\WhatsAppSender\file\document.pdf"
Global Const $OPEN_DIALOG_TIMEOUT = 15 ; seconds

_Log("Script started")

; ------------------------------------------------------------
; Step 1: Wait for Open File dialog
; ------------------------------------------------------------
Local $hDialog = WinWait("[CLASS:#32770]", "", $OPEN_DIALOG_TIMEOUT)

If $hDialog = 0 Then
    _Fail("Open File dialog not detected within timeout")
EndIf

_Log("Open File dialog detected")

; ------------------------------------------------------------
; Step 2: Ensure dialog is active
; ------------------------------------------------------------
If Not WinActivate($hDialog) Then
    _Fail("Failed to activate Open File dialog")
EndIf

Sleep(300)

; ------------------------------------------------------------
; Step 3: Validate File name input control
; ------------------------------------------------------------
Local $hEdit = ControlGetHandle($hDialog, "", "Edit1")
If $hEdit = "" Then
    _Fail("File name input control not found")
EndIf

_Log("File input control found")

; ------------------------------------------------------------
; Step 4: Set file path directly (NO Send)
; ------------------------------------------------------------
ControlSetText($hDialog, "", $hEdit, $FILE_TO_UPLOAD)
Sleep(200)

; ------------------------------------------------------------
; Step 5: Click Open button explicitly
; ------------------------------------------------------------
Local $hOpenBtn = ControlGetHandle($hDialog, "", "Button1")
If $hOpenBtn = "" Then
    _Fail("Open button not found")
EndIf

ControlClick($hDialog, "", $hOpenBtn)

_Log("Open button clicked")

; ------------------------------------------------------------
; Step 6: Wait for dialog to close
; ------------------------------------------------------------
If Not WinWaitClose($hDialog, "", 10) Then
    _Fail("Dialog did not close after clicking Open")
EndIf

_Log("Dialog closed successfully")
_Log("Script completed OK")
Exit 0

; ============================================================
; Helper functions
; ============================================================

Func _Log($msg)
    FileWriteLine($LOG_FILE, _
        "[" & @YEAR & "-" & @MON & "-" & @MDAY & " " & _
        @HOUR & ":" & @MIN & ":" & @SEC & "] " & $msg)
EndFunc

Func _Fail($reason)
    _Log("ERROR: " & $reason)
    Exit 1
EndFunc
