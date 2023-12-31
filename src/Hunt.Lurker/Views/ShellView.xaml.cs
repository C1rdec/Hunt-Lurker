﻿using System.Windows;

namespace Hunt.Lurker.Views;

/// <summary>
/// Interaction logic for ShellView.xaml
/// </summary>
public partial class ShellView : Window
{
    private Window _parent;

    public ShellView()
    {
        InitializeComponent();
        HideFromAltTab();
    }

    private void HideFromAltTab()
    {
        _parent = new Window
        {
            Top = -100,
            Left = -100,
            Width = 1,
            Height = 1,

            WindowStyle = WindowStyle.ToolWindow, // Set window style as ToolWindow to avoid its icon in AltTab
            ShowInTaskbar = false,
        };

        _parent.Show();
        Owner = _parent;
        _parent.Hide();
    }
}
