﻿using Clippy.Common;
using System;
using System.Windows;

namespace Clippy.Resources
{
    [Serializable]
    public class PlainTextItem : ClipboardItemBase
    {
        private const string s_titleTemplate = "PlainText";

        public PlainTextItem(long index, string text) : base(index)
        {
            Title = $"{s_titleTemplate}_{index}";
            m_type = DataKind.PlainText;
            m_data = text;
        }

        public override void CopyToClipboard()
        {
            if (Data == null)
            {
                return;
            }

            Clipboard.SetText(GetText());
        }

        public void UpdateText(string newText)
        {
            m_data = newText;
        }

        public string GetText()
        {
            return Data as string;
        }
    }
}
