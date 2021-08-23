using System;

namespace Plugins.GoogleSheetWindow
{
	public enum VarType {
		_Vector2,
		_Vector3,
		_bool,
		_float,
		_int,
		_string,
	}
	public class SpreadsheetType {
		public readonly VarType type;
		public readonly string columnKeyWord;
		public readonly bool isCommaValue;
		public SpreadsheetType(VarType type, string columnKeyWord, bool isCommaValue = false) {
			this.type = type;
			this.columnKeyWord = columnKeyWord;
			this.isCommaValue = isCommaValue;
		}
		public virtual string GetTypeName(string cellValue, string columnName) {
			return columnKeyWord;
		}
		public virtual bool IsValid(string cell) {
			return false;
		}
		public virtual string GetValue(string cell, string columnName) {
			return "null" + String.Format("", type.ToString(), cell);
		}
		protected string Trim(string cell) {
			return cell.Replace(" ", "").Replace("\t", "").Trim();
		}
	}
}
