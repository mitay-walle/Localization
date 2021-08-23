namespace Plugins.GoogleSheetWindow
{ 

	public class SpreadsheetString : SpreadsheetType {
		public SpreadsheetString() : base(VarType._string, "string") {
		}
		override public bool IsValid(string cell) {
			return true;
		}
		override public string GetValue(string cell, string columnName) {
			return "\"" + cell.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\n") + "\"";
		}
	}
}
