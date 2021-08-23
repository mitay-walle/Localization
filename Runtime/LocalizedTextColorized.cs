using System;
using UnityEngine;

namespace mitaywalle.Plugins.Localization
{
    public class LocalizedTextColorized : LocalizedText
    {
        public Data[] data = new Data[1];


        [Serializable]
        public class Data
        {
            public Color Color;
            public string start = "$";
            public string end = "@";
        }

        internal const string endString = "</color>";

        public override string Localize(bool logs = true)
        {
            if (!logs) // OnValidate
            {
                if (!Target) return null;
            }

            var mess = 1.L(Key, logs);

            if (data != null)
                for (int i = 0; i < data.Length; i++)
                {
                    if (!String.IsNullOrEmpty(data[i].start)) mess = mess.Replace(data[i].start, $"<color=#{ColorUtility.ToHtmlStringRGBA(data[i].Color)}>");
                    if (!String.IsNullOrEmpty(data[i].end)) mess = mess.Replace(data[i].end, endString);
                }

            return Target.text = mess;
        }
    }
}