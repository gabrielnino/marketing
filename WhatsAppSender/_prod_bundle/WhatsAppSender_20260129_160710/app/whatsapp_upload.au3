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
Global Const $LOG_FILE       = "__AUTOIT_LOG_FILE__"
Global Const $FILE_TO_UPLOAD = "__FILE_TO_UPLOAD__"

; ------------------------------------------------------------
; Ensure log folder exists
; ------------------------------------------------------------
Local $logDir = _GetDirName($LOG_FILE)
If $logDir <> "" And Not FileExists($logDir) Then
    DirCreate($logDir)
EndIf

; ------------------------------------------------------------
; FOCUS GUARD - Close Windows Backup / OneDrive prompt if present
; ------------------------------------------------------------
Func _CloseBackupPopupIfPresent()
    Local $popupTitle = "Turn On Windows Backup"
    If WinExists($popupTitle) Then
        _Log("FOCUS GUARD - Popup detected: '" & $popupTitle & "' -> attempting to close.")
        WinActivate($popupTitle)
        Sleep(200)

        WinClose($popupTitle)
        Sleep(500)

        If WinExists($popupTitle) Then
            _Log("FOCUS GUARD - Popup still present after WinClose -> sending Alt+F4.")
            Send("!{F4}")
            Sleep(500)
        EndIf

        If WinExists($popupTitle) Then
            _Log("FOCUS GUARD - Popup STILL present. Will continue but focus may be affected.")
        Else
            _Log("FOCUS GUARD - Popup closed successfully.")
        EndIf
    Else
        _Log("FOCUS GUARD - No Windows Backup popup detected.")
    EndIf
EndFunc

; ------------------------------------------------------------
; Start log
; ------------------------------------------------------------
_Log("============================================================")
_Log("SCRIPT STARTED")
_Log("Target file: " & $FILE_TO_UPLOAD)
_Log("Log file: " & $LOG_FILE)
_Log("Open dialog timeout: " & $OPEN_DIALOG_TIMEOUT & "s")
_Log("============================================================")

_CloseBackupPopupIfPresent()

; ------------------------------------------------------------
; STEP 1: Wait for Open File dialog
; ------------------------------------------------------------
_Log("STEP 1 - Waiting for Open File dialog [CLASS:#32770] (smart match)")

Local $hDialog = _FindBestOpenDialog($OPEN_DIALOG_TIMEOUT)

If $hDialog = 0 Then
    _DumpAllCommonDialogs()
    _Fail("STEP 1 FAILED - Open File dialog not detected within timeout (smart match)")
EndIf

_Log("STEP 1 OK - Dialog detected. Handle=" & $hDialog)

; ------------------------------------------------------------
; STEP 2: Try to activate dialog (NON-FATAL) + diagnostics
; ------------------------------------------------------------
_Log("STEP 2 - Activating Open File dialog (diagnostic, non-fatal)")

Local $dlg = "[HANDLE:" & $hDialog & "]"

; Baseline info about dialog
Local $dlgTitle = WinGetTitle($dlg)
Local $dlgState = WinGetState($dlg)
Local $dlgPID   = WinGetProcess($dlg)
_Log("STEP 2 BEFORE - Title='" & $dlgTitle & "' State=" & $dlgState & " PID=" & $dlgPID)

; Try a defensive activation sequence (but do not fail if it can't be foreground)
WinSetState($dlg, "", @SW_RESTORE)
Sleep(200)

Local $act = WinActivate($dlg)
Local $actErr = @error
_Log("STEP 2 INFO - WinActivate returned=" & $act & " @error=" & $actErr)
Sleep(300)

; Snapshot what is actually active now
Local $hActive = WinGetHandle("[ACTIVE]")
Local $activeTitle = WinGetTitle("[ACTIVE]")
Local $activeClass = WinGetClassList("[ACTIVE]")
_Log("STEP 2 ACTIVE WINDOW (after WinActivate) - ACTIVE Title='" & $activeTitle & "' ClassList=" & $activeClass)

; Log whether dialog got foreground, but do not stop execution.
If $hActive <> $hDialog Then
    _Log("STEP 2 WARN - Dialog is NOT the active window. ExpectedHandle=" & $hDialog & " ActiveHandle=" & $hActive)
Else
    _Log("STEP 2 OK - Dialog is active.")
EndIf

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
; STEP 4: Set the file path in Edit1 (direct)
; ------------------------------------------------------------
_Log("STEP 4 - Setting file path in Edit1")

; Focus the edit control directly; this may work even if the dialog is not the active window.
ControlFocus($hDialog, "", $hEdit)
Sleep(100)

If Not ControlSetText($hDialog, "", $hEdit, $FILE_TO_UPLOAD) Then
    _Fail("STEP 4 FAILED - Unable to set text into file input control")
EndIf

Sleep(200)

Local $currentText = ControlGetText($hDialog, "", $hEdit)
_Log("STEP 4 INFO - Edit1 now contains: " & $currentText)

If StringStripWS($currentText, 3) = "" Then
    _Fail("STEP 4 FAILED - Edit1 text appears empty after setting file path")
EndIf

_Log("STEP 4 OK - File path set")

; ------------------------------------------------------------
; STEP 5: Click Open button (Button1)
; ------------------------------------------------------------
_Log("STEP 5 - Clicking Open button (Button1)")

Local $hOpenBtn = ControlGetHandle($hDialog, "", "Button1")

If $hOpenBtn = "" Then
    _Fail("STEP 5 FAILED - Open button (Button1) not found")
EndIf

_Log("STEP 5 INFO - Open button handle: " & $hOpenBtn)

; Use ControlClick. It usually works even if window is not active.
If Not ControlClick($hDialog, "", $hOpenBtn) Then
    _Fail("STEP 5 FAILED - Unable to click Open button")
EndIf

_Log("STEP 5 OK - Open clicked")

; ------------------------------------------------------------
; STEP 6: Verify dialog closes
; ------------------------------------------------------------
_Log("STEP 6 - Waiting for dialog to close")

Local $closed = WinWaitClose($dlg, "", 10)
If $closed = 0 Then
    _DumpWindowInfo($dlg, "STEP 6 DIAG - dialog still present")
    _Fail("STEP 6 FAILED - Dialog did not close after clicking Open")
EndIf

_Log("STEP 6 OK - Dialog closed. Upload should proceed")

_Log("============================================================")
_Log("SCRIPT COMPLETED SUCCESSFULLY")
_Log("============================================================")
Exit 0


; ------------------------------------------------------------
; Diagnostic helpers
; ------------------------------------------------------------
Func _DumpWindowInfo($wnd, $label)
    Local $t = WinGetTitle($wnd)
    Local $s = WinGetState($wnd)
    Local $p = WinGetProcess($wnd)
    _Log($label & " - Title='" & $t & "' State=" & $s & " PID=" & $p)
EndFunc

Func _DumpActiveWindow($label)
    Local $t = WinGetTitle("[ACTIVE]")
    Local $cls = WinGetClassList("[ACTIVE]")
    _Log($label & " - ACTIVE Title='" & $t & "' ClassList=" & $cls)
EndFunc

; Try to pick the correct Open File dialog when there are multiple #32770 windows.
; Heuristic: must have Edit1 (File name) and Button1 (Open).
Func _FindBestOpenDialog($timeoutSec)
    Local $t0 = TimerInit()

    While (TimerDiff($t0) < ($timeoutSec * 1000))
        Local $wl = WinList("[CLASS:#32770]")
        ; wl[0][0] = count
        For $i = 1 To $wl[0][0]
            Local $h = $wl[$i][1]
            If $h = 0 Then ContinueLoop

            Local $w = "[HANDLE:" & $h & "]"
            Local $state = WinGetState($w)
            If @error Then ContinueLoop

            ; Must have the "File name" Edit control
            Local $hEdit = ControlGetHandle($w, "", "Edit1")
            If $hEdit = "" Then ContinueLoop

            ; Must have an "Open" button (often Button1)
            Local $hBtn = ControlGetHandle($w, "", "Button1")
            If $hBtn = "" Then ContinueLoop

            _Log("STEP 1 INFO - Candidate dialog: Handle=" & $h & " State=" & $state & " Title='" & WinGetTitle($w) & "'")
            Return $h
        Next

        Sleep(200)
    WEnd

    Return 0
EndFunc

Func _DumpAllCommonDialogs()
    _Log("DIAG - Dumping all [CLASS:#32770] windows found right now:")
    Local $wl = WinList("[CLASS:#32770]")
    For $i = 1 To $wl[0][0]
        Local $h = $wl[$i][1]
        Local $w = "[HANDLE:" & $h & "]"
        _Log("DIAG - #32770 Handle=" & $h & " Title='" & WinGetTitle($w) & "' State=" & WinGetState($w) & " PID=" & WinGetProcess($w))
    Next
EndFunc

; ------------------------------------------------------------
; Logging helpers
; ------------------------------------------------------------
Func _Log($msg)
    Local $line = "[" & @YEAR & "-" & StringFormat("%02d", @MON) & "-" & StringFormat("%02d", @MDAY) & " " & _
                  StringFormat("%02d", @HOUR) & ":" & StringFormat("%02d", @MIN) & ":" & StringFormat("%02d", @SEC) & "] " & $msg

    Local $h = FileOpen($LOG_FILE, 1)
    If $h <> -1 Then
        FileWriteLine($h, $line)
        FileClose($h)
    EndIf

    ConsoleWrite($line & @CRLF)
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
