using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBApplicationForWork
{
	internal class Connector : IDisposable
	{
		private SqlConnection _connection;
		private readonly string _connectionString;
		string[] table_names = new string[] { "CartridgeRecords", "PrinterRecords", "ComputerRecords" };
		public Connector(string connectionString)
		{
			_connectionString = connectionString;
			_connection = new SqlConnection(connectionString);
		}
		private void OpenConnection()
		{
			if(_connection.State != ConnectionState.Open)
				_connection.Open();
		}
		public void CloseConnection()
		{
			if(_connection.State == ConnectionState.Closed)
				_connection.Close();
		}
		public int InsertOneFieldTable(string table, string name)
		{
			if(string.IsNullOrEmpty(name))
				throw new ArgumentNullException("Value cannot be null or empty", nameof(name));
			int result = 0;
			string query = $"MERGE {table} WITH (HOLDLOCK) AS target USING (SELECT @name AS name) AS source ON target.name = source.name WHEN MATCHED THEN UPDATE SET target.name = target.name WHEN NOT MATCHED THEN INSERT (name) VALUES (source.name) OUTPUT INSERTED.id;";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = name;
					var id = cmd.ExecuteScalar();
					if (id != null && id != DBNull.Value)
						result = Convert.ToInt32(id);
					else
						throw new Exception("Failed to insert or find the value");
				}
			} finally {CloseConnection(); }
			return result;
		}
		public int InsertRecords(int tabpage_index, int order_number, string recording_date, int request_number, short department, List<short> device, List<int> inventory_number)
		{
			List<string> temp = new List<string>();
			for(int i = 0; i < device.Count; i++)
				temp.Add($"(@order_number, @recording_date, @request_number, @department, @device{i}, @inventory_number{i}, 1)");
			//string query = $"INSERT INTO CartridgeRecords (order_number, recording_date, request_number, department, cartridge, inventory_number, [state]) VALUES {string.Join(", ", temp)};";
			string[] queries = new string[]
			{
				$"INSERT INTO CartridgeRecords (order_number, recording_date, request_number, department, cartridge, inventory_number, [state]) VALUES {string.Join(", ", temp)};",
				$"INSERT INTO PrinterRecords (order_number, recording_date, request_number, department, printer, inventory_number, [state]) VALUES {string.Join(", ", temp)};",
				$"INSERT INTO ComputerRecords (order_number, recording_date, request_number, department, computer, inventory_number, [state]) VALUES {string.Join(", ", temp)};"
			};
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(queries[tabpage_index], _connection))
				{
					cmd.Parameters.Add("@order_number", SqlDbType.Int).Value = order_number;
					cmd.Parameters.Add("@recording_date", SqlDbType.Date).Value = recording_date;
					cmd.Parameters.Add("@request_number", SqlDbType.Int).Value = request_number;
					cmd.Parameters.Add("@department", SqlDbType.SmallInt).Value = department;
					for(int i = 0; i < device.Count; i ++)
					{
						cmd.Parameters.Add($"@device{i}", SqlDbType.SmallInt).Value = device[i];
						cmd.Parameters.Add($"@inventory_number{i}", SqlDbType.Int).Value = inventory_number[i];
					}
					return cmd.ExecuteNonQuery();
				}
			} finally {CloseConnection(); }
		}
		public int UpdateRecords(int tabpage_index, int field_index, string value, List<int> id)
		{
			string[][] field_names =
			{
				new string[] { "", "order_number", "recording_date", "request_number", "department", "cartridge", "inventory_number", "remark", "company_date", "company_act", "complection_date", "date_of_issue", "[state]" },
				new string[] { "", "order_number", "recording_date", "request_number", "department", "printer", "inventory_number", "remark", "company_date", "company_act", "complection_date", "date_of_issue", "[state]" },
				new string[] { "", "order_number", "recording_date", "request_number", "department", "computer", "inventory_number", "remark", "company_date", "company_act", "complection_date", "date_of_issue", "[state]" }
			};
			string query = $"UPDATE {table_names[tabpage_index]} SET {field_names[tabpage_index][field_index]} = @value WHERE id IN ({string.Join(", ", id)});";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					switch(field_index)
					{
						case 7:
							cmd.Parameters.Add("@value", SqlDbType.NVarChar, 50).Value = value;
							break;
						case 2:
							cmd.Parameters.Add("@value", SqlDbType.Date).Value = value;
							break;
						case 8:
						case 10:
						case 11:
							if (!string.IsNullOrWhiteSpace(value)) cmd.Parameters.Add("@value", SqlDbType.Date).Value = value;
							else cmd.Parameters.Add("@value", SqlDbType.Date).Value = DBNull.Value;
							break;
						case 1:
						case 3:
						case 6:
							cmd.Parameters.Add("@value", SqlDbType.Int).Value = Convert.ToInt32(value);
							break;
						case 4:
						case 5:
							cmd.Parameters.Add("@value", SqlDbType.SmallInt).Value = Convert.ToInt16(value);
							break;
						case 9:
							if (!string.IsNullOrWhiteSpace(value)) cmd.Parameters.Add("@value", SqlDbType.SmallInt).Value = Convert.ToInt16(value);
							else cmd.Parameters.Add("@value", SqlDbType.SmallInt).Value = DBNull.Value;
							break;
						case 12:
							cmd.Parameters.Add("@value", SqlDbType.TinyInt).Value = Convert.ToByte(value);
							break;
						default:
							return -1;
					}
					return cmd.ExecuteNonQuery();
				}
			} finally {CloseConnection(); }
		}
		public int DeleteCartridgeRecords(int tabpage_index, List<int> id)
		{
			string query = $"DELETE FROM {table_names[tabpage_index]} WHERE id IN ({string.Join(", ", id)});";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					return cmd.ExecuteNonQuery();
				}
			}
			finally { CloseConnection(); }
		}
		public DataTable SelectRecords(string table_name)
		{
			string[] queries = new string[]
			{
				"SELECT CartridgeRecords.id, order_number AS N'Номер наряда', recording_date AS N'Дата поступления', RequestNumbers.name AS N'Номер заявки', Departments.name AS N'Подразделение', Cartridges.name AS N'Наименование', CartridgeInventorys.name AS N'Инвентарный', remark AS N'Заметки', company_date AS N'Дата передачи в фирму', company_act AS N'Номер акта фирмы', complection_date AS N'Дата готовности', date_of_issue AS N'Дата выдачи', States.name AS N'Статус' FROM CartridgeRecords, Cartridges, Departments, CartridgeInventorys, RequestNumbers, States WHERE Cartridges.id=cartridge AND Departments.id=department AND CartridgeInventorys.id=inventory_number AND RequestNumbers.id=request_number AND States.id=[state];",
				"SELECT PrinterRecords.id, order_number AS N'Номер наряда', recording_date AS N'Дата поступления', RequestNumbers.name AS N'Номер заявки', Departments.name AS N'Подразделение', Printers.name AS N'Наименование', CartridgeInventorys.name AS N'Инвентарный', remark AS N'Заметки', company_date AS N'Дата передачи в фирму', company_act AS N'Номер акта фирмы', complection_date AS N'Дата готовности', date_of_issue AS N'Дата выдачи', States.name AS N'Статус' FROM PrinterRecords, Printers, Departments, CartridgeInventorys, RequestNumbers, States WHERE Printers.id=printer AND Departments.id=department AND CartridgeInventorys.id=inventory_number AND RequestNumbers.id=request_number AND States.id=[state];",
				"SELECT ComputerRecords.id, order_number AS N'Номер наряда', recording_date AS N'Дата поступления', RequestNumbers.name AS N'Номер заявки', Departments.name AS N'Подразделение', Computers.name AS N'Наименование', ComputerInventorys.name AS N'Инвентарный', remark AS N'Заметки', company_date AS N'Дата передачи в фирму', company_act AS N'Номер акта фирмы', complection_date AS N'Дата готовности', date_of_issue AS N'Дата выдачи', States.name AS N'Статус' FROM ComputerRecords, Computers, Departments, ComputerInventorys, RequestNumbers, States WHERE Computers.id=computer AND Departments.id=department AND ComputerInventorys.id=inventory_number AND RequestNumbers.id=request_number AND States.id=[state];"
			};
			DataTable table = new DataTable();
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(queries[Array.IndexOf(table_names, table_name)], _connection))
				{
					using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
					{
						adapter.Fill(table);
					}
				}
			} finally { CloseConnection(); }
			return table;
		}
		public DataTable SelectInventorys(short department)
		{
			DataTable table = new DataTable();
			string query = "SELECT CartridgeInventorys.id, CartridgeInventorys.[name] FROM CartridgeInventorys, CartridgeRecords WHERE CartridgeInventorys.id = CartridgeRecords.inventory_number AND department = @department";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query,_connection))
				{
					cmd.Parameters.AddWithValue("@department", department);
					using (SqlDataAdapter adapter = new SqlDataAdapter())
					{
						adapter.Fill(table);
					}
				}
			} finally { CloseConnection(); }
			return table;
		}
		public DataTable SelectSmallTable(string table)
		{
			DataTable dt = new DataTable();
			dt.Columns.Add("id",  typeof(int));
			dt.Columns.Add("name",  typeof(string));
			string query = $"SELECT id, [name] FROM {table};";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
					{
						adapter.Fill(dt);
					}
				}
			} finally { CloseConnection(); }
			return dt;
		}
		public int GetLastOrderNumber(int table_index)
		{
			int number = 0;
			string query = $"SELECT TOP 1 order_number FROM {table_names[table_index]} ORDER BY id DESC;";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					var result = cmd.ExecuteScalar();
					if (result != null && result != DBNull.Value)
						number = Convert.ToInt32(result);
				}
			} finally { CloseConnection(); }
			return number;
		}
		public void Dispose()
		{
			_connection?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
