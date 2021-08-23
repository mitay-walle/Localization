using System;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
    public class LocalizedTextColorizedFormat : LocalizedTextFormat
    {
        public LocalizedTextColorized.Data[] data = new LocalizedTextColorized.Data[1];

        public override string Localize(bool logs = true)
        {
            if (!logs && !Target) return string.Empty;

            string text;

            if (formatItems.Length == 0)
                text = prefix + 1.L(Key, logs) + postfix;
            else
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    try
                    {
                        text = prefix + 1.LF(Key, formatItems, logs) + postfix;
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e);
                        //throw;
                        text = prefix + Key + postfix;
                        Debug.LogError($"неверный форма строчки!{e}",this);
                    }
                }
                else
#endif
                {
                    text = prefix + 1.LF(Key, formatItems, logs) + postfix;
                }
            }


            if (data != null)
                for (int i = 0; i < data.Length; i++)
                {
                    if (!string.IsNullOrEmpty(data[i].start))
                        text = text.Replace(data[i].start, $"<color=#{ColorUtility.ToHtmlStringRGBA(data[i].Color)}>");
                    if (!string.IsNullOrEmpty(data[i].end))
                        text = text.Replace(data[i].end, LocalizedTextColorized.endString);
                }

            ResizeInternal();
        
            return Target.text = text;
        }
    
    }
}