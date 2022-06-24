Public Class Form1

#Region "Variables"
    Public windrive = IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System))
    Private windir As String = System.Environment.GetEnvironmentVariable("WINDIR")
    Private userprofile As String = System.Environment.GetEnvironmentVariable("USERPROFILE")

    Private HKCU As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser

    Private SelectionHover As Image
    Private SelectionSelected As Image
    Private SelectionUnfocused As Image
    Private SelectionSelectedBG As Color
    Private SelectionSelectedFG As Color
    Private DefaultArrow As Image
#End Region

#Region "DWM stuff"
    Private Panel As New Panel
    Private dwmenabled As Boolean

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure MARGINS
        Public Lf, Rt, T, B As Integer
    End Structure

    Public Shared WTNCA_NODRAWCAPTION As UInteger = &H1
    Public Shared WTNCA_NODRAWICON As UInteger = &H2
    Public Shared WTNCA_NOSYSMENU As UInteger = &H4
    Public Shared WTNCA_NOMIRRORHELP As UInteger = &H8

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure WTA_OPTIONS
        Public Flags As UInteger
        Public Mask As UInteger
    End Structure

    Public Enum WindowThemeAttributeType
        WTA_NONCLIENT = 1
    End Enum

    <DllImport("dwmapi.dll", SetLastError:=True)> _
    Public Shared Function DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef pMarinset As MARGINS) As Integer
    End Function
    <DllImport("dwmapi.dll", SetLastError:=True)> _
    Private Shared Function DwmIsCompositionEnabled(ByRef enabled As Boolean) As Integer
    End Function
    <DllImport("user32.dll", EntryPoint:="GetForegroundWindow")> Private Shared Function GetForegroundWindow() As IntPtr
    End Function
    <DllImport("UxTheme.dll")> _
    Public Shared Function SetWindowThemeAttribute(ByVal hWnd As IntPtr, ByVal wtype As WindowThemeAttributeType, ByRef attributes As WTA_OPTIONS, ByVal size As UInteger) As Integer
    End Function
    <DllImport("UxTheme.DLL", BestFitMapping:=False, CallingConvention:=CallingConvention.Winapi, CharSet:=CharSet.Unicode, EntryPoint:="#65")> _
    Shared Function SetSystemVisualStyle(ByVal pszFilename As String, ByVal pszColor As String, ByVal pszSize As String, ByVal dwReserved As Integer) As Integer
    End Function

#End Region

#Region "API reciever and functions"
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Const WM_DWMCOMPOSITIONCHANGED = &H31E
        Const WM_THEMECHANGED = &H31A
        Const WM_ACTIVATE = &H6

        If (m.Msg = WM_DWMCOMPOSITIONCHANGED) Then
            RefreshDWM()
        End If
        If (m.Msg = WM_THEMECHANGED) Then
            RefreshHC()
            Me.Refresh()
        End If
        If (m.Msg = WM_ACTIVATE) Then
            RefreshActiveBasic()
        End If

        MyBase.WndProc(m)
    End Sub

    Private Sub RefreshDWM()
        Dim bounds As MARGINS = New MARGINS
        Dim hwnd As IntPtr = Handle
        If SystemInformation.HighContrast = True Then
            With bounds
                .Lf = 0
                .T = 0
                .Rt = 0
                .B = 0
            End With
        Else
            With bounds
                .Lf = 0
                .T = Panel1.Height
                .Rt = 0
                .B = 0
            End With
        End If

        DwmExtendFrameIntoClientArea(hwnd, bounds)
        Dim ops As New WTA_OPTIONS()
        ops.Flags = WTNCA_NODRAWCAPTION Or WTNCA_NODRAWICON
        ops.Mask = WTNCA_NODRAWCAPTION Or WTNCA_NODRAWICON
        SetWindowThemeAttribute(Me.Handle, WindowThemeAttributeType.WTA_NONCLIENT, ops, CUInt(Marshal.SizeOf(GetType(WTA_OPTIONS))))
        DwmIsCompositionEnabled(dwmenabled)
        
        RefreshActiveBasic()
    End Sub

    Private Sub RefreshHC()
        RefreshDWM()

        If SystemInformation.HighContrast = True Then
            SelectionHover = Nothing
            SelectionSelected = Nothing
            SelectionUnfocused = Nothing
            SelectionSelectedBG = Color.FromKnownColor(KnownColor.Highlight)
            SelectionSelectedFG = Color.FromKnownColor(KnownColor.HighlightText)

            Header.BackColor = Color.FromKnownColor(KnownColor.Highlight)
            Header.ForeColor = Color.FromKnownColor(KnownColor.HighlightText)
            ButtonMiddle.Image = My.Resources.goclassic
            DefaultArrow = My.Resources.goclassic

            Header.BackgroundImage = Nothing
            Dim r As Integer = Color.FromKnownColor(KnownColor.Highlight).R + 46
            Dim g As Integer = Color.FromKnownColor(KnownColor.Highlight).G + 46
            Dim b As Integer = Color.FromKnownColor(KnownColor.Highlight).B + 46
            If r > 255 Then
                r = 255
            End If
            If g > 255 Then
                g = 255
            End If
            If b > 255 Then
                b = 255
            End If
            WelcomePageLbl.ForeColor = Color.FromArgb((Color.FromKnownColor(KnownColor.Highlight).A), r, g, b)

            ButtonLeft.Visible = False
            ButtonRight.Visible = False
            ButtonMiddle.FlatStyle = FlatStyle.System

            BkupGettingStarted1.Visible = True
            BkupGettingStarted2.Visible = True
            Panel5.Visible = False
            AddressBarContainer.BackColor = Color.FromKnownColor(KnownColor.Window)
            AddressBarContainer.BorderStyle = BorderStyle.FixedSingle
            Panel3.BackColor = Color.FromKnownColor(KnownColor.Window)
            Panel3.BorderStyle = BorderStyle.FixedSingle
        Else
            SelectionSelectedBG = Color.Transparent
            SelectionSelectedFG = Nothing

            Header.BackColor = Nothing

            If HKCU.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\ThemeManager").GetValue("DllName").ToUpper.EndsWith("\AERO\AEROLITE.MSSTYLES") Then
                SelectionHover = My.Resources.selectionhoverlite
                SelectionSelected = My.Resources.selectionfocusedlite
                SelectionUnfocused = My.Resources.selectionunfocusedlite

                Header.ForeColor = Color.White
                ButtonMiddle.Image = My.Resources.goclassic
                DefaultArrow = My.Resources.goclassic

                Header.BackgroundImage = My.Resources.headerclassic
                WelcomePageLbl.ForeColor = My.Settings.ClassicWelcFore
            Else
                SelectionHover = My.Resources.selectionhover
                SelectionSelected = My.Resources.selectionfocused
                SelectionUnfocused = My.Resources.selectionunfocused

                Header.ForeColor = Color.FromKnownColor(KnownColor.WindowText)
                ButtonMiddle.Image = My.Resources.gonormal
                DefaultArrow = My.Resources.gonormal

                Header.BackgroundImage = My.Resources.headernormal
                WelcomePageLbl.ForeColor = My.Settings.NormalWelcFore
            End If

            Panel1.BackColor = Color.Black

            ButtonLeft.Visible = True
            ButtonRight.Visible = True
            ButtonMiddle.FlatStyle = FlatStyle.Flat

            BkupGettingStarted1.Visible = False
            BkupGettingStarted2.Visible = False
            Panel5.Visible = True
            AddressBarContainer.BackColor = Color.Transparent
            AddressBarContainer.BorderStyle = BorderStyle.None
            Panel3.BackColor = Color.Transparent
            Panel3.BorderStyle = BorderStyle.None
        End If

        For Each btn In {Option1, Option2, Option3, Option4, Option5, Option6, Option7, Option8, Option9, Option10, Option11, Option12}
            If btn.Checked = True Then
                btn.BackgroundImage = SelectionSelected
                btn.FlatAppearance.CheckedBackColor = SelectionSelectedBG
                btn.ForeColor = SelectionSelectedFG
            Else
                btn.BackgroundImage = Nothing
                btn.FlatAppearance.CheckedBackColor = Color.Transparent
                btn.ForeColor = Nothing
            End If
            btn.FlatAppearance.MouseDownBackColor = SelectionSelectedBG
        Next

        'reused from TextChanged code
        Panel8.Width = ButtonLeft.Width + ButtonMiddle.Width + ButtonRight.Width
        ButtonContainer.Width = Panel8.Width
        If Option1.Checked = True Then
            HeaderText.Width = Panel9.Width - ButtonContainer.Width
        Else
            HeaderText.Width = Panel9.Width
        End If
    End Sub
#End Region

#Region "Text for Getting Started pages"
    Private Text1 As String = <a>• Discover new features that can help you be productive, stay safer, and have fun
• Find out everything you need to know about setting up your computer</a>
    Private Text2 As String = <a>• Make your computer look the way you want it to
• Choose a theme to change the desktop background, window color, sounds, and screen saver all at once
• Create and save your own themes
• Share theme with friends and family
• Or change the picture, color, and sounds individually</a>
    Private Text3 As String = <a>• Make the move to your new computer easier by transferring files and settings from your old computer all at once
• Move documents, music, photos, favorites, e-mail, program settings, and more</a>
    Private Text4 As String = <a>• Share pictures, music, videos, documents, and printers with other computers in your homegroup
• Choose which files and folders to share
• Create a new homegroup or add this computer to an existing one</a>
    Private Text5 As String = <a>Make changes to your User Account Control settings</a>
    Private Text6 As String = <a>Windows Live Essentials is a set of free programs to help you:
• Stay in touch with the people you care about using Windows Live Messenger and Mail
• Edit and publish photos, blogs, and videos using the Windows Live Photo Gallery, Writer, and Movie Maker
• Customize your web browser and stay safer online using Windows Live Toolbar and Family Safety

As of 10 January 2017, Windows Live Essentials is no longer officially available from Microsoft. If you continue, an archived copy of the installer will be downloaded from MajorGeeks (not sanctioned by Microsoft).</a>
    Private Text7 As String = <a>• Automatically save copies of your photos, music, and other files
• Set up automatic backup and learn about other ways to recover information</a>
    Private Text8 As String = <a>• Give other users access to this computer
• Change your account type
• Change your account picture</a>
    Private Text9 As String = <a>• Make text and other items on your screen larger or smaller
• Adjust your screen resolution</a>
    Private Text10 As String = <a>• Optimise for blindness
• Optimise visual display
• Set up alternative input devices
• Adjust settings for the mouse or other pointing devices
• Adjust settings for the keyboard
• Set up alternatives for sounds
• Adjust settings for touch and tablets</a>
    Private Text11 As String = <a>• Revert your Windows installation to how it was before you installed this transformation pack
• Delete the transformation pack backups and installation data from your system if you don't want to go back
• View the credits for this transformation pack, and support the people who made programs involved in this transformation</a>
    Private Text12 As String = <a>Learn how to use a Windows 7 installation or ISO file to make DVD Maker function on your Windows installation</a>
#End Region

#Region "Support for No DWM, and HC toolbar background"
    Private Sub RefreshActiveBasic()
        If dwmenabled = True And SystemInformation.HighContrast = False Then
            Panel1.BackColor = Color.Black
            Exit Sub
        End If

        If CBool(GetForegroundWindow() = Me.Handle) Then 'If window is active
            If SystemInformation.HighContrast = True Then
                Panel1.BackColor = Color.FromKnownColor(KnownColor.ActiveCaption)
            ElseIf dwmenabled = False Then
                Panel1.BackColor = Color.FromKnownColor(KnownColor.GradientActiveCaption)
            End If
        Else
            If SystemInformation.HighContrast = True Then 'HC uses primary color
                Panel1.BackColor = Color.FromKnownColor(KnownColor.InactiveCaption)
            ElseIf dwmenabled = False Then
                Panel1.BackColor = Color.FromKnownColor(KnownColor.GradientInactiveCaption)
            End If
        End If
    End Sub
#End Region

#Region "Page changing"
    Private Sub Option1_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option1.CheckedChanged
        If Option1.Checked = True Then
            WelcomePageLbl.Visible = True
            HeaderText.Dock = DockStyle.None
            HeaderText.Anchor = AnchorStyles.Left + AnchorStyles.Top + AnchorStyles.Right

            HeaderIcon.Image = My.Resources.mainbeeg
            HeaderTitle.Text = Option1.Text
            HeaderText.Text = Text1
            ButtonMiddle.Text = "Go online to learn more"

            Option1.BackgroundImage = SelectionSelected
            Option1.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option1.ForeColor = SelectionSelectedFG
        Else
            WelcomePageLbl.Visible = False
            HeaderText.Dock = DockStyle.Top

            Option1.BackgroundImage = Nothing
            Option1.FlatAppearance.CheckedBackColor = Color.Transparent
            Option1.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option2_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option2.CheckedChanged
        If Option2.Checked = True Then
            HeaderIcon.Image = My.Resources.personalisebeeg
            HeaderTitle.Text = Option2.Text
            HeaderText.Text = Text2
            ButtonMiddle.Text = "Personalize Windows"

            Option2.BackgroundImage = SelectionSelected
            Option2.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option2.ForeColor = SelectionSelectedFG
        Else
            Option2.BackgroundImage = Nothing
            Option2.FlatAppearance.CheckedBackColor = Color.Transparent
            Option2.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option3_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option3.CheckedChanged
        If Option3.Checked = True Then
            HeaderIcon.Image = My.Resources.easytransbeeg
            HeaderTitle.Text = Option3.Text
            HeaderText.Text = Text3
            ButtonMiddle.Text = "Open Windows Easy Transfer"

            Option3.BackgroundImage = SelectionSelected
            Option3.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option3.ForeColor = SelectionSelectedFG
        Else
            Option3.BackgroundImage = Nothing
            Option3.FlatAppearance.CheckedBackColor = Color.Transparent
            Option3.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option4_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option4.CheckedChanged
        If Option4.Checked = True Then
            HeaderIcon.Image = My.Resources.homegroupbeeg
            HeaderTitle.Text = Option4.Text
            HeaderText.Text = Text4
            ButtonMiddle.Text = "Set up a homegroup"

            Option4.BackgroundImage = SelectionSelected
            Option4.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option4.ForeColor = SelectionSelectedFG
        Else
            Option4.BackgroundImage = Nothing
            Option4.FlatAppearance.CheckedBackColor = Color.Transparent
            Option4.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option5_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option5.CheckedChanged
        If Option5.Checked = True Then
            HeaderIcon.Image = My.Resources.uacbeeg
            HeaderTitle.Text = Option5.Text
            HeaderText.Text = Text5
            ButtonMiddle.Text = "Change settings"

            Option5.BackgroundImage = SelectionSelected
            Option5.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option5.ForeColor = SelectionSelectedFG
        Else
            Option5.BackgroundImage = Nothing
            Option5.FlatAppearance.CheckedBackColor = Color.Transparent
            Option5.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option6_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option6.CheckedChanged
        If Option6.Checked = True Then
            HeaderIcon.Image = My.Resources.livebeeg
            HeaderTitle.Text = Option6.Text
            HeaderText.Text = Text6
            ButtonMiddle.Text = "Go online to get Windows Live Essentials"

            Option6.BackgroundImage = SelectionSelected
            Option6.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option6.ForeColor = SelectionSelectedFG
        Else
            Option6.BackgroundImage = Nothing
            Option6.FlatAppearance.CheckedBackColor = Color.Transparent
            Option6.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option7_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option7.CheckedChanged
        If Option7.Checked = True Then
            HeaderIcon.Image = My.Resources.backupbeeg
            HeaderTitle.Text = Option7.Text
            HeaderText.Text = Text7
            ButtonMiddle.Text = "Back up your files"

            Option7.BackgroundImage = SelectionSelected
            Option7.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option7.ForeColor = SelectionSelectedFG
        Else
            Option7.BackgroundImage = Nothing
            Option7.FlatAppearance.CheckedBackColor = Color.Transparent
            Option7.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option8_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option8.CheckedChanged
        If Option8.Checked = True Then
            HeaderIcon.Image = My.Resources.usersbeeg
            HeaderTitle.Text = Option8.Text
            HeaderText.Text = Text8
            ButtonMiddle.Text = "Add new users"

            Option8.BackgroundImage = SelectionSelected
            Option8.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option8.ForeColor = SelectionSelectedFG
        Else
            Option8.BackgroundImage = Nothing
            Option8.FlatAppearance.CheckedBackColor = Color.Transparent
            Option8.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option9_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option9.CheckedChanged
        If Option9.Checked = True Then
            HeaderIcon.Image = My.Resources.displaybeeg
            HeaderTitle.Text = Option9.Text
            HeaderText.Text = Text9
            ButtonMiddle.Text = "Change text size"

            Option9.BackgroundImage = SelectionSelected
            Option9.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option9.ForeColor = SelectionSelectedFG
        Else
            Option9.BackgroundImage = Nothing
            Option9.FlatAppearance.CheckedBackColor = Color.Transparent
            Option9.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option10_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option10.CheckedChanged
        If Option10.Checked = True Then
            HeaderIcon.Image = My.Resources.a11ybeeg
            HeaderTitle.Text = Option10.Text
            HeaderText.Text = Text10
            ButtonMiddle.Text = "Open Ease of Access"

            Option10.BackgroundImage = SelectionSelected
            Option10.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option10.ForeColor = SelectionSelectedFG
        Else
            Option10.BackgroundImage = Nothing
            Option10.FlatAppearance.CheckedBackColor = Color.Transparent
            Option10.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option11_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option11.CheckedChanged
        If Option11.Checked = True Then
            HeaderIcon.Image = My.Resources.uninstallbeeg
            HeaderTitle.Text = Option11.Text
            HeaderText.Text = Text11
            ButtonMiddle.Text = "Uninstall or manage transformation pack"

            Option11.BackgroundImage = SelectionSelected
            Option11.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option11.ForeColor = SelectionSelectedFG
        Else
            Option11.BackgroundImage = Nothing
            Option11.FlatAppearance.CheckedBackColor = Color.Transparent
            Option11.ForeColor = Nothing
        End If
    End Sub

    Private Sub Option12_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles Option12.CheckedChanged
        If Option12.Checked = True Then
            HeaderIcon.Image = My.Resources.dvdmakerbeeg
            HeaderTitle.Text = Option12.Text
            HeaderText.Text = Text12
            ButtonMiddle.Text = "View Guide"

            Option12.BackgroundImage = SelectionSelected
            Option12.FlatAppearance.CheckedBackColor = SelectionSelectedBG
            Option12.ForeColor = SelectionSelectedFG
        Else
            Option12.BackgroundImage = Nothing
            Option12.FlatAppearance.CheckedBackColor = Color.Transparent
            Option12.ForeColor = Nothing
        End If
    End Sub
#End Region

#Region "Button double-click/actions"
    Private Sub Option1_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option1.DoubleClick
        Process.Start("http://go.microsoft.com/fwlink/?LinkID=139460")
    End Sub

    Private Sub Option2_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option2.DoubleClick
        Shell("rundll32.exe shell32.dll,Control_RunDLL desk.cpl,,2", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option3_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option3.DoubleClick
        Process.Start(windir + "\System32\migwiz\migwiz.exe")
    End Sub

    Private Sub Option4_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option4.DoubleClick
        Shell("control /name Microsoft.HomeGroup", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option5_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option5.DoubleClick
        Shell("UserAccountControlSettings.exe", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option6_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option6.DoubleClick
        Process.Start("https://www.majorgeeks.com/mg/getmirror/windows_live,1.html")
    End Sub

    Private Sub Option7_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option7.DoubleClick
        Shell("control /name Microsoft.FileHistory", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option8_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option8.DoubleClick
        Shell("control /name Microsoft.UserAccounts", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option9_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option9.DoubleClick
        Shell("control desk.cpl", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option10_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option10.DoubleClick
        Shell("control /name Microsoft.EaseOfAccessCenter", AppWinStyle.NormalFocus, False)
    End Sub

    Private Sub Option11_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option11.DoubleClick
        Try
            Process.Start(windir + "\Win8To7\SetupTools\setup.exe")
        Catch
        End Try
    End Sub

    Private Sub Option12_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Option12.DoubleClick
        DVDMakerGuide.Show()
    End Sub
#End Region

#Region "Top right button's states and button press actions"
    Private Sub ButtonMiddle_TextChanged(sender As System.Object, e As System.EventArgs) Handles ButtonMiddle.TextChanged
        Panel8.Width = ButtonLeft.Width + ButtonMiddle.Width + ButtonRight.Width
        ButtonContainer.Width = Panel8.Width
        If Option1.Checked = True Then
            HeaderText.Width = Panel9.Width - ButtonContainer.Width
        Else
            HeaderText.Width = Panel9.Width
        End If
    End Sub

    Private Sub ButtonMiddle_MouseEnter(sender As System.Object, e As System.EventArgs) Handles ButtonMiddle.MouseEnter
        ButtonLeft.BackgroundImage = My.Resources.buttonhoverleft
        ButtonMiddle.BackgroundImage = My.Resources.buttonhovermiddle
        ButtonRight.BackgroundImage = My.Resources.buttonhoverright

        ButtonMiddle.Image = My.Resources.gonormal
    End Sub

    Private Sub ButtonMiddle_MouseLeave(sender As System.Object, e As System.EventArgs) Handles ButtonMiddle.MouseLeave
        ButtonLeft.BackgroundImage = Nothing
        ButtonMiddle.BackgroundImage = Nothing
        ButtonRight.BackgroundImage = Nothing

        ButtonMiddle.Image = DefaultArrow
    End Sub

    Private Sub ButtonMiddle_MouseDown(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles ButtonMiddle.MouseDown
        ButtonLeft.BackgroundImage = My.Resources.buttonpressedleft
        ButtonMiddle.BackgroundImage = My.Resources.buttonpressedmiddle
        ButtonRight.BackgroundImage = My.Resources.buttonpressedright
    End Sub

    Private Sub ButtonMiddle_MouseUp(sender As System.Object, e As System.Windows.Forms.MouseEventArgs) Handles ButtonMiddle.MouseUp
        ButtonLeft.BackgroundImage = My.Resources.buttonhoverleft
        ButtonMiddle.BackgroundImage = My.Resources.buttonhovermiddle
        ButtonRight.BackgroundImage = My.Resources.buttonhoverright
    End Sub

    Private Sub ButtonMiddle_Click(sender As System.Object, e As System.EventArgs) Handles ButtonMiddle.Click
        If Option1.Checked = True Then
            Option1_DoubleClick(sender, e)
        ElseIf Option2.Checked = True Then
            Option2_DoubleClick(sender, e)
        ElseIf Option3.Checked = True Then
            Option3_DoubleClick(sender, e)
        ElseIf Option4.Checked = True Then
            Option4_DoubleClick(sender, e)
        ElseIf Option5.Checked = True Then
            Option5_DoubleClick(sender, e)
        ElseIf Option6.Checked = True Then
            Option6_DoubleClick(sender, e)
        ElseIf Option7.Checked = True Then
            Option7_DoubleClick(sender, e)
        ElseIf Option8.Checked = True Then
            Option8_DoubleClick(sender, e)
        ElseIf Option9.Checked = True Then
            Option9_DoubleClick(sender, e)
        ElseIf Option10.Checked = True Then
            Option10_DoubleClick(sender, e)
        ElseIf Option11.Checked = True Then
            Option11_DoubleClick(sender, e)
        ElseIf Option12.Checked = True Then
            Option12_DoubleClick(sender, e)
        End If
    End Sub
#End Region

#Region "All page buttons' states"
    ' Option1
    Private Sub Option1_Enter(sender As System.Object, e As System.EventArgs) Handles Option1.Enter
        If Option1.Checked = True Then
            Option1.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option1_Leave(sender As System.Object, e As System.EventArgs) Handles Option1.Leave
        If Option1.Checked = True Then
            Option1.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option1_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option1.MouseEnter
        If Option1.Checked = False Then
            Option1.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option1_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option1.MouseLeave
        If Option1.Checked = False Then
            Option1.BackgroundImage = Nothing
        End If
    End Sub

    ' Option2
    Private Sub Option2_Enter(sender As System.Object, e As System.EventArgs) Handles Option2.Enter
        If Option2.Checked = True Then
            Option2.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option2_Leave(sender As System.Object, e As System.EventArgs) Handles Option2.Leave
        If Option2.Checked = True Then
            Option2.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option2_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option2.MouseEnter
        If Option2.Checked = False Then
            Option2.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option2_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option2.MouseLeave
        If Option2.Checked = False Then
            Option2.BackgroundImage = Nothing
        End If
    End Sub

    ' Option3
    Private Sub Option3_Enter(sender As System.Object, e As System.EventArgs) Handles Option3.Enter
        If Option3.Checked = True Then
            Option3.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option3_Leave(sender As System.Object, e As System.EventArgs) Handles Option3.Leave
        If Option3.Checked = True Then
            Option3.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option3_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option3.MouseEnter
        If Option3.Checked = False Then
            Option3.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option3_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option3.MouseLeave
        If Option3.Checked = False Then
            Option3.BackgroundImage = Nothing
        End If
    End Sub

    ' Option4
    Private Sub Option4_Enter(sender As System.Object, e As System.EventArgs) Handles Option4.Enter
        If Option4.Checked = True Then
            Option4.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option4_Leave(sender As System.Object, e As System.EventArgs) Handles Option4.Leave
        If Option4.Checked = True Then
            Option4.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option4_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option4.MouseEnter
        If Option4.Checked = False Then
            Option4.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option4_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option4.MouseLeave
        If Option4.Checked = False Then
            Option4.BackgroundImage = Nothing
        End If
    End Sub

    ' Option5
    Private Sub Option5_Enter(sender As System.Object, e As System.EventArgs) Handles Option5.Enter
        If Option5.Checked = True Then
            Option5.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option5_Leave(sender As System.Object, e As System.EventArgs) Handles Option5.Leave
        If Option5.Checked = True Then
            Option5.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option5_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option5.MouseEnter
        If Option5.Checked = False Then
            Option5.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option5_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option5.MouseLeave
        If Option5.Checked = False Then
            Option5.BackgroundImage = Nothing
        End If
    End Sub

    ' Option6
    Private Sub Option6_Enter(sender As System.Object, e As System.EventArgs) Handles Option6.Enter
        If Option6.Checked = True Then
            Option6.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option6_Leave(sender As System.Object, e As System.EventArgs) Handles Option6.Leave
        If Option6.Checked = True Then
            Option6.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option6_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option6.MouseEnter
        If Option6.Checked = False Then
            Option6.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option6_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option6.MouseLeave
        If Option6.Checked = False Then
            Option6.BackgroundImage = Nothing
        End If
    End Sub

    ' Option7
    Private Sub Option7_Enter(sender As System.Object, e As System.EventArgs) Handles Option7.Enter
        If Option7.Checked = True Then
            Option7.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option7_Leave(sender As System.Object, e As System.EventArgs) Handles Option7.Leave
        If Option7.Checked = True Then
            Option7.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option7_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option7.MouseEnter
        If Option7.Checked = False Then
            Option7.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option7_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option7.MouseLeave
        If Option7.Checked = False Then
            Option7.BackgroundImage = Nothing
        End If
    End Sub

    ' Option8
    Private Sub Option8_Enter(sender As System.Object, e As System.EventArgs) Handles Option8.Enter
        If Option8.Checked = True Then
            Option8.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option8_Leave(sender As System.Object, e As System.EventArgs) Handles Option8.Leave
        If Option8.Checked = True Then
            Option8.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option8_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option8.MouseEnter
        If Option8.Checked = False Then
            Option8.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option8_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option8.MouseLeave
        If Option8.Checked = False Then
            Option8.BackgroundImage = Nothing
        End If
    End Sub

    ' Option9
    Private Sub Option9_Enter(sender As System.Object, e As System.EventArgs) Handles Option9.Enter
        If Option9.Checked = True Then
            Option9.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option9_Leave(sender As System.Object, e As System.EventArgs) Handles Option9.Leave
        If Option9.Checked = True Then
            Option9.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option9_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option9.MouseEnter
        If Option9.Checked = False Then
            Option9.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option9_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option9.MouseLeave
        If Option9.Checked = False Then
            Option9.BackgroundImage = Nothing
        End If
    End Sub

    ' Option10
    Private Sub Option10_Enter(sender As System.Object, e As System.EventArgs) Handles Option10.Enter
        If Option10.Checked = True Then
            Option10.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option10_Leave(sender As System.Object, e As System.EventArgs) Handles Option10.Leave
        If Option10.Checked = True Then
            Option10.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option10_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option10.MouseEnter
        If Option10.Checked = False Then
            Option10.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option10_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option10.MouseLeave
        If Option10.Checked = False Then
            Option10.BackgroundImage = Nothing
        End If
    End Sub

    ' Option11
    Private Sub Option11_Enter(sender As System.Object, e As System.EventArgs) Handles Option11.Enter
        If Option11.Checked = True Then
            Option11.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option11_Leave(sender As System.Object, e As System.EventArgs) Handles Option11.Leave
        If Option11.Checked = True Then
            Option11.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option11_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option11.MouseEnter
        If Option11.Checked = False Then
            Option11.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option11_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option11.MouseLeave
        If Option11.Checked = False Then
            Option11.BackgroundImage = Nothing
        End If
    End Sub

    ' Option12
    Private Sub Option12_Enter(sender As System.Object, e As System.EventArgs) Handles Option12.Enter
        If Option12.Checked = True Then
            Option12.BackgroundImage = SelectionSelected
        End If
    End Sub
    Private Sub Option12_Leave(sender As System.Object, e As System.EventArgs) Handles Option12.Leave
        If Option12.Checked = True Then
            Option12.BackgroundImage = SelectionUnfocused
        End If
    End Sub
    Private Sub Option12_MouseEnter(sender As System.Object, e As System.EventArgs) Handles Option12.MouseEnter
        If Option12.Checked = False Then
            Option12.BackgroundImage = SelectionHover
        End If
    End Sub
    Private Sub Option12_MouseLeave(sender As System.Object, e As System.EventArgs) Handles Option12.MouseLeave
        If Option12.Checked = False Then
            Option12.BackgroundImage = Nothing
        End If
    End Sub
#End Region

#Region "Registry Backupper - GS edition"
    'We can't just use regchange here as it gets deleted during uninstallation, plus it isn't designed for local logged in user backups, so we adapt it for such in here
    Public Sub BackupRegistryClasses(ByVal Key As String, ByVal Value As String, ByVal LocationKey As String)
        Dim KeyPath As String
        Dim tempArray As String()
        Dim pathToCreate As String

        Dim LocationPath As String

        LocationPath = Join(LocationKey.Split("\").Skip(1).ToArray(), "\") 'Get rid of prefix, keep rest as path for HKCU

        'First, make sure the backups area is created
        pathToCreate = ""
        tempArray = LocationPath.Split("\")
        For Each item In tempArray
            If pathToCreate = "" Then
                pathToCreate = item 'We need to start from somewhere, and having it begin with \ isn't so favourable
            Else
                pathToCreate = pathToCreate + "\" + item 'Append next key in path so far to path
            End If
            If HKCU.OpenSubKey(pathToCreate) Is Nothing Then 'If it doesn't exist as a key...
                HKCU.CreateSubKey(pathToCreate) 'Make it a key.
            End If
        Next
        tempArray = Key.Split("\").Skip(3).ToArray() 'Get rid of useless prefix from path
        KeyPath = Join(tempArray, "\").ToString() 'Now make it a string path

        'First, make sure the keys exist in our Backups area
        pathToCreate = ""
        For Each item In tempArray
            If pathToCreate = "" Then
                pathToCreate = item
            Else
                pathToCreate = pathToCreate + "\" + item
            End If
            If HKCU.OpenSubKey(LocationPath + "\" + pathToCreate) Is Nothing Then
                HKCU.CreateSubKey(LocationPath + "\" + pathToCreate) 'Repeat the above for the area to back the value(s) to
            End If
        Next

        If Value = "" Then
            'Now, save all values in the key
            If HKCU.OpenSubKey(KeyPath) Is Nothing Then
                Exit Sub 'If the key doesn't exist, we can't back its values up - just quit out
            End If
            Try
                For Each ValueItem In HKCU.OpenSubKey(KeyPath).GetValueNames 'Set value ValueItem of backup storage location's key to value of ValueItem on key we're backing up
                    HKCU.OpenSubKey(LocationPath + "\" + KeyPath, True).SetValue(ValueItem, HKCU.OpenSubKey(KeyPath).GetValue(ValueItem), HKCU.OpenSubKey(KeyPath).GetValueKind(ValueItem))
                Next
            Catch ex As Exception
                MsgBox(Key + " allvalues " + KeyPath + " " + ex.Message)
            End Try
        ElseIf Value = "(Default)" Then
            'Now, save default value in the key
            If HKCU.OpenSubKey(KeyPath) Is Nothing Then
                Exit Sub
            End If
            If HKCU.OpenSubKey(KeyPath).GetValue("") Is Nothing Then
                Exit Sub '"" = Default, and if there is no Default value (e.g.: "(value not set)"), we can't continue - just quit out
            End If
            Try 'Set default value of backup storage location's key to default value of the key we're backing up
                HKCU.OpenSubKey(LocationPath + "\" + KeyPath, True).SetValue("", HKCU.OpenSubKey(KeyPath).GetValue(""), HKCU.OpenSubKey(KeyPath).GetValueKind(""))
            Catch ex As Exception
                MsgBox(Key + " defaultvalue " + KeyPath + " " + ex.Message)
            End Try
        Else
            'Now, save the data in the backups area.
            If HKCU.OpenSubKey(KeyPath) Is Nothing Then
                Exit Sub
            End If
            If Not HKCU.OpenSubKey(KeyPath).GetValueNames.Contains(Value) Then
                Exit Sub
            End If
            Try 'Set value Value of backup storage location's key to default value of the key we're backing up
                HKCU.OpenSubKey(LocationPath + "\" + KeyPath, True).SetValue(Value, HKCU.OpenSubKey(KeyPath).GetValue(Value), HKCU.OpenSubKey(KeyPath).GetValueKind(Value))
            Catch ex As Exception
                MsgBox(Key + " " + Value + " system " + KeyPath + " " + ex.Message)
            End Try
        End If
    End Sub
#End Region

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim HKLM As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine 'Only used in this code.
        If HKCU.OpenSubKey("Software\Win8To7GS") Is Nothing Then 'FIRST LAUNCH
            HKCU.CreateSubKey("Software\Win8To7GS")
            'Set remaining user configurations
            If HKCU.OpenSubKey("Software\Classes\CLSID") Is Nothing Then
                HKCU.CreateSubKey("Software\Classes\CLSID")
            End If
            If HKCU.OpenSubKey("Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}") Is Nothing Then
                HKCU.CreateSubKey("Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}")
            End If
            BackupRegistryClasses("HKCU\Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}", "SortOrderIndex", "HKCU\Software\Win8To7\RegBackup\HKCU\UserConfigClasses") 'Back up values first
            BackupRegistryClasses("HKCU\Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}", "System.IsPinnedToNameSpaceTree", "HKCU\Software\Win8To7\RegBackup\HKCU\UserConfigClasses")
            HKCU.OpenSubKey("Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}", True).SetValue("SortOrderIndex", 69, Microsoft.Win32.RegistryValueKind.DWord) 'nice
            HKCU.OpenSubKey("Software\Classes\CLSID\{031E4825-7B94-4dc3-B131-E946B44C8DD5}", True).SetValue("System.IsPinnedToNameSpaceTree", 1, Microsoft.Win32.RegistryValueKind.DWord)

            If System.Environment.OSVersion.Version.Minor = 3 Then 'Windows 8.0 doesn't need this as Libraries are still given Public Folders too.
                'While we're here, add shortcuts to the Public folders
                If Not IO.File.Exists(userprofile + "\Music\Sample Music.lnk") Then
                    ' Music
                    Shell(windir + "\System32\cmd.exe /c mklink /j """ + userprofile + "\Music\Sample Music"" """ + windrive + "Users\Public\Music\Sample Music""", AppWinStyle.Hide, True)
                End If
                If Not IO.File.Exists(userprofile + "\Pictures\Sample Pictures.lnk") Then
                    ' Pictures
                    Shell(windir + "\System32\cmd.exe /c mklink /j """ + userprofile + "\Pictures\Sample Pictures"" """ + windrive + "Users\Public\Pictures\Sample Pictures""", AppWinStyle.Hide, True)
                End If
                If Not IO.File.Exists(userprofile + "\Videos\Sample Videos.lnk") Then
                    ' Videos
                    Shell(windir + "\System32\cmd.exe /c mklink /j """ + userprofile + "\Videos\Sample Videos"" """ + windrive + "Users\Public\Videos\Sample Videos""", AppWinStyle.Hide, True)
                End If
            End If

            HKCU.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True).DeleteValue("GettingStarted", False) 'Remove G.S. from autostart

            'If it's Incomplete High Contrast, fix that.
            If SystemInformation.HighContrast = True Then
                SetSystemVisualStyle(windir + "\resources\Themes\Aero\Aerolite.msstyles", "NormalColor", "NormalSize", 0)
            End If

            HKCU.Close() 'Save changes to Registry
            HKCU = Microsoft.Win32.Registry.CurrentUser 'Reopen after doing this

            Shell("taskkill /f /im explorer.exe", AppWinStyle.Hide, True)
            If IO.File.Exists(windir + "\explorer7\explorer.exe") Then
                Shell(windir + "\explorer7\explorer.exe", AppWinStyle.NormalNoFocus, False) 'If the user has configured Ex7ForW8, launch that shell instead
            Else
                Shell("explorer", AppWinStyle.NormalNoFocus, False)
            End If

            'Open Ex7ForWin8's main window if it isn't configured just yet
            If IO.File.Exists(windir + "\explorer7\Ex7ForW8.exe") And Not IO.File.Exists(windir + "\explorer7\explorer.exe") Then
                Shell(windir + "\System32\cmd.exe /c " + windir + "\explorer7\Ex7ForW8.exe", AppWinStyle.Hide, False)
            End If
        End If

        RefreshDWM() 'Do the DWM stuff

        Option1_CheckedChanged(sender, e)

        Option11.Visible = IO.File.Exists(windir + "\Win8To7\SetupTools\setup.exe")
        Option2.Visible = Not Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "EditionID", "Professional").ToString.StartsWith("Starter")
        'Option12.Visible = Not IO.Directory.Exists(windrive + "Program Files\DVD Maker\codecs")
        Option12.Visible = False

        RefreshHC()
        Option1_CheckedChanged(sender, e)

        'Detect if we chose Windows 8 branding, and if so change some text accordingly
        If HKLM.OpenSubKey("SOFTWARE\Win8To7GS").GetValue("Branding").ToString().StartsWith("win8") Then
            Option1.Text = "Go online to find out what's new in Windows 8"
            HeaderTitle.Text = "Go online to find out what's new in Windows 8"
        End If
    End Sub
End Class