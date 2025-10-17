/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using Resources = OmegaGUI.Properties.Resources;

namespace OmegaGUI.Render;

#region Enumerations
public enum MsgBoxType
{
    OK,
    OKCancel,
    YesNo
}

public enum MsgBoxResult
{
    OK,
    Cancel,
    Yes,
    No
}
#endregion

/// <summary>
/// A message-box dialog
/// </summary>
public class MessageBox : Dialog
{
    public bool Visible { get; private set; }

    private string _text = "";
    private MsgBoxType _type;
    private Action<MsgBoxResult>? _callback;
    // ReSharper disable InconsistentNaming
    private Button OKButton, CancelButton, YesButton, NoButton;
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Create a new message-box dialog
    /// </summary>
    /// <param name="manager">The <see cref="DialogManager"/> instance that provides the resources for rendering of this dialog</param>
    public MessageBox(DialogManager manager) : base(manager)
    {}

    public void Show(string text, MsgBoxType type, Action<MsgBoxResult>? callback)
    {
        _callback = callback;
        _text = text;
        _type = type;

        Reset();
        Visible = true;
    }

    public void Reset()
    {
        RemoveAllControls();
        SetBackgroundColors(Color.FromArgb(196, 64, 64, 64));

        AddStatic(0, _text, 5, 5, Width - 10, Height - 60).TextAlign = TextAlign.Center;

        switch (_type)
        {
            case MsgBoxType.OK:
                OKButton = AddButton(1, Resources.MsgBoxOK, (Width - 110) / 2, Height - 50, 110, 40);
                OKButton.Click += OKButton_Click;
                break;

            case MsgBoxType.OKCancel:
                OKButton = AddButton(1, Resources.MsgBoxOK, (Width - 110) / 2 - 60, Height - 50, 110, 40);
                OKButton.Click += OKButton_Click;
                CancelButton = AddButton(1, Resources.MsgBoxCancel, (Width - 110) / 2 + 60, Height - 50, 110, 40);
                CancelButton.Click += CancelButton_Click;
                break;

            case MsgBoxType.YesNo:
                YesButton = AddButton(1, Resources.MsgBoxYes, (Width - 110) / 2 - 60, Height - 50, 110, 40);
                YesButton.Click += YesButton_Click;
                NoButton = AddButton(1, Resources.MsgBoxNo, (Width - 110) / 2 + 60, Height - 50, 110, 40);
                NoButton.Click += NoButton_Click;
                break;
        }
    }

    #region Event handling
    private void OKButton_Click(object sender, EventArgs e)
    {
        CloseDialog(MsgBoxResult.No);
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        CloseDialog(MsgBoxResult.No);
    }

    private void YesButton_Click(object sender, EventArgs e)
    {
        CloseDialog(MsgBoxResult.Yes);
    }

    private void NoButton_Click(object sender, EventArgs e)
    {
        CloseDialog(MsgBoxResult.No);
    }

    private void CloseDialog(MsgBoxResult res)
    {
        Refresh();
        Visible = false;
        _callback?.Invoke(res);
    }
    #endregion
}
