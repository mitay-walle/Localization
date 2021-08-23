namespace Plugins.GoogleSheetWindow
{
	public class SpreadsheetFloat : SpreadsheetType {
		public SpreadsheetFloat() : base(VarType._float, "float") {
		}
		public override bool IsValid(string cell) {
			float valFloat;
			if (float.TryParse(cell, out valFloat)) {
				return true;
			}
			return false;
		}
		public override string GetValue(string cell, string columnName) {
			cell = Trim(cell);
			float parseTry;
			if(cell.Length == 0) {
				return "0f";
			}else if (float.TryParse(cell, out parseTry)) {
				return parseTry.ToString() + "f";
			} else {
				return cell;
			}
		}
	}
}

