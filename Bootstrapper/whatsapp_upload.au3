; ============================================================
; whatsapp_upload.au3
; Static, defensive AutoIt script for file upload
; Method: OpenFileDialogWithAutoIT (static version)
; FIXED PATHS (no command-line parameters)
; ============================================================

Opt("WinTitleMatchMode", 2)
Opt("WinWaitDelay", 100)
Opt("SendKeyDelay", 30)
Opt("MouseClickDelay", 50)

Global Const $OPEN_DIALOG_TIMEOUT = 15 ; seconds

; ------------------------------------------------------------
; FIXED CONFIGURATION
; ------------------------------------------------------------
Global Const $LOG_FILE       = "E:\Marketing-Logs\AutoItLog\autoItLog.log"
Global Const $FILE_TO_UPLOAD = "E:\imagenes\goku.png"

; ------------------------------------------------------------
; BOOTSTRAP: Validate fixed configuration
; ------------------------------------------------------------
_LogRaw("============================================================")
_LogRaw("BOOT - Static configuration mode")
_LogRaw("FILE_TO_UPLOAD=" & $FILE_TO_UPLOAD)
_LogRaw("LOG_FILE=" & $LOG_FILE)

If StringStripWS($FILE_TO_UPLOAD, 3) = "" Then
    _FailRaw("FILE_TO_UPLOAD is empty")
EndIf

If StringStripWS($LOG_FILE, 3) = "" Then
    _FailRaw("LOG_FILE is empty")
EndIf

If Not FileExists($FILE_TO_UPLOAD) Then
    _FailRaw("File to upload does not exist: " & $FILE_TO_UPLOAD)
EndIf

; Ensure log directory exists
Local $logDir = _GetDirName($LOG_FILE)
If $logDir <> "" And Not FileExists($logDir) Then
    DirCreate($logDir)
EndIf

; ------------------------------------------------------------
; SCRIPT START
; ------------------------------------------------------------
_Log("============================================================")
_Log("SCRIPT STARTED")
_Log("Target file: " & $FILE_TO_UPLOAD)
_Log("Log file: " & $LOG_FILE)
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

; Logging usable before $LOG_FILE is set (bootstrap stage)
Func _LogRaw($msg)
    ConsoleWrite("[BOOT] " & $msg & @CRLF)
EndFunc

Func _Fail($reason)
    _Log("ERROR - " & $reason)
    _Log("SCRIPT TERMINATED WITH ERROR")
    _Log("============================================================")
    Exit 1
EndFunc

Func _FailRaw($reason)
    ConsoleWrite("[BOOT][ERROR] " & $reason & @CRLF)
    Exit 1
EndFunc

Func _GetDirName($fullPath)
    Local $pos = StringInStr($fullPath, "\", 0, -1)
    If $pos <= 0 Then Return ""
    Return StringLeft($fullPath, $pos - 1)
EndFunc
