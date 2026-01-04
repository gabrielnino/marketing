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

_Log("============================================================")
_Log("SCRIPT STARTED")
_Log("Target file: " & $FILE_TO_UPLOAD)
_Log("Open dialog timeout: " & $OPEN_DIALOG_TIMEOUT & "s")
_Log("============================================================")

; ------------------------------------------------------------
; STEP 1: Wait for Open File dialog
; ------------------------------------------------------------
_Log("STEP 1 - Waiting for Open File dialog [CLASS:#32770]")

Local $hDialog = WinWait("[CLASS:#32770]", "", $OPEN_DIALOG_TIMEOUT)

If $hDialog = 0 Then
    _Fail("STEP 1 FAILED - Open File dialog not detected within timeout")
EndIf

_Log("STEP 1 OK - Dialog detected. Handle=" & $hDialog)

; ------------------------------------------------------------
; STEP 2: Ensure dialog is active
; ------------------------------------------------------------
_Log("STEP 2 - Activating Open File dialog")

If Not WinActivate($hDialog) Then
    _Fail("STEP 2 FAILED - Unable to activate dialog. Handle=" & $hDialog)
EndIf

Sleep(300)
_Log("STEP 2 OK - Dialog activated")

; ------------------------------------------------------------
; STEP 3: Validate File name input control
; ------------------------------------------------------------
_Log("STEP 3 - Locating file input control (Edit1)")

Local $hEdit = ControlGetHandle($hDialog, "", "Edit1")

If $hEdit = "" Then
    _Fail("STEP 3 FAILED - File input control (Edit1) not found")
EndIf

_Log("STEP 3 OK - File input control found. Handle=" & $hEdit)

; ------------------------------------------------------------
; STEP 4: Set file path directly
; ------------------------------------------------------------
_Log("STEP 4 - Setting file path into input control")
_Log("STEP 4 INFO - Path=" & $FILE_TO_UPLOAD)

ControlSetText($hDialog, "", $hEdit, $FILE_TO_UPLOAD)
Sleep(200)

; Validate text was set
Local $sCheck = ControlGetText($hDialog, "", $hEdit)
If $sCheck <> $FILE_TO_UPLOAD Then
    _Fail("STEP 4 FAILED - File path mismatch. Current value=[" & $sCheck & "]")
EndIf

_Log("STEP 4 OK - File path set correctly")

; ------------------------------------------------------------
; STEP 5: Click Open button
; ------------------------------------------------------------
_Log("STEP 5 - Locating Open button (Button1)")

Local $hOpenBtn = ControlGetHandle($hDialog, "", "Button1")

If $hOpenBtn = "" Then
    _Fail("STEP 5 FAILED - Open button not found")
EndIf

_Log("STEP 5 INFO - Open button handle=" & $hOpenBtn)
_Log("STEP 5 - Clicking Open button")

ControlClick($hDialog, "", $hOpenBtn)

_Log("STEP 5 OK - Open button clicked")

; ------------------------------------------------------------
; STEP 6: Wait for dialog to close
; ------------------------------------------------------------
_Log("STEP 6 - Waiting for dialog to close")

If Not WinWaitClose($hDialog, "", 10) Then
    _Fail("STEP 6 FAILED - Dialog did not close after clicking Open")
EndIf

_Log("STEP 6 OK - Dialog closed")

; ------------------------------------------------------------
; SCRIPT COMPLETED
; ------------------------------------------------------------
_Log("SCRIPT COMPLETED SUCCESSFULLY")
_Log("============================================================")
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
    _Log("ERROR - " & $reason)
    _Log("SCRIPT TERMINATED WITH ERROR")
    _Log("============================================================")
    Exit 1
EndFunc
