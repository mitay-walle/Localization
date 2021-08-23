namespace Plugins.GoogleSheetWindow
{
	public class SpreadsheetInt : SpreadsheetType {
		public SpreadsheetInt() : base(VarType._int, "int") {
		}
		override public bool IsValid(string cell) {
			int valInt;
			if (int.TryParse(cell, out valInt))
				return true;
			return false;
		}
		override public string GetValue(string cell, string columnName) {
			int parseTry;
			if(cell.Trim().Length == 0) {
				return "0";
			}else if (int.TryParse(cell, out parseTry)) {
				return parseTry.ToString();
			} else {
				return cell;
			}
		}
	}
}

