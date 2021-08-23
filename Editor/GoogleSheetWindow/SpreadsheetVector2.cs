using System.Collections.Generic;

namespace Plugins.GoogleSheetWindow
{
	public class SpreadsheetVector2 : SpreadsheetType {
		public SpreadsheetVector2() : base(VarType._Vector2, "Vector2", true) {
		}
		public override bool IsValid(string cell) {
			if (cell.Contains(",")) {
				string[] array = cell.Split(',');
				int countFloat = 0;
				float result;
				foreach (string s in array) {
					if (float.TryParse(s, out result)) {
						countFloat++;
					}
				}
				if (countFloat == array.Length && countFloat == 2) {
					return true;
				}
			}
			return false;
		}
		public override string GetValue(string cell, string columnName) {
			cell = Trim(cell);
			if (cell == "") {
				return "[0.0,0.0]";
			} else {
				List<string> values = new List<string>();
				foreach(string s in cell.Split(',')) {
					float parsed;
					if(s.Trim().Length == 0) {
						values.Add("0");
					}else if(float.TryParse(s, out parsed)){
						values.Add(parsed.ToString());
					} else {
						values.Add(s.Trim());
					}
				}
				return "[" + string.Join(",", values.ToArray()) + "]";
			}
		}
	}
}

