using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace DBApplicationForWork
{
	internal class Connector : IDisposable
	{
		private SqlConnection _connection;
		private readonly string _connectionString;
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
			string query = $"MERGE {table} WITH (HOLDLOCK) AS target USING (SELECT @name AS name) AS source ON target.name = source.name WHEN MATCHED THEN UPDATE SET target.name = target.name WHEN NOT MATCHED THEN INSERT (name) VALUE (source.name) OUTPUT INSERTED.id;";
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
		public int InsertCartridgeRecords(int order_number, string recording_date, int request_number, short department, List<short> cartridge, List<int> inventory_number)
		{
			List<string> temp = new List<string>();
			for(int i = 0; i < cartridge.Count; i++)
				temp.Add($"(@order_number, @recording_date, @request_number, @department, @cartridge{i}, @inventory_number{i}, 1)");
			string query = $"INSERT INTO CartridgeRecords (order_number, recording_date, request_number, department, cartridge, inventory_number, [state]) VALUES {string.Join(", ", temp)};";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					cmd.Parameters.Add("@order_number", SqlDbType.Int).Value = order_number;
					cmd.Parameters.Add("@recording_date", SqlDbType.Date).Value = recording_date;
					cmd.Parameters.Add("@request_number", SqlDbType.Int).Value = request_number;
					cmd.Parameters.Add("@department", SqlDbType.SmallInt).Value = department;
					for(int i = 0; i < cartridge.Count; i ++)
					{
						cmd.Parameters.Add($"@cartridge{i}", SqlDbType.SmallInt).Value = cartridge[i];
						cmd.Parameters.Add($"@inventory_number{i}", SqlDbType.Int).Value = inventory_number[i];
					}
					return cmd.ExecuteNonQuery();
				}
			} finally {CloseConnection(); }
		}
		public int UpdateCartridgeRecords(string field, string value, List<int> id)
		{
			string query = $"UPDATE CartridgeRecords SET {field} = @value WHERE id IN ({string.Join(", ", id)});";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					cmd.Parameters.AddWithValue("@value", value);
					return cmd.ExecuteNonQuery();
				}
			} finally {CloseConnection(); }
		}
		public int DeleteCartridgeRecords(List<int> id)
		{
			string query = $"DELETE FROM CartridgeRecords WHERE id IN ({string.Join(", ", id)});";
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
		public DataTable SelectCartridgeRecords()
		{
			DataTable table = new DataTable();
			string query = "SELECT id, order_number, recording_date, request_number, department, cartridge, inventory_number, remark, company_date, company_act, complection_date, date_of_issue, [state] FROM CartridgeRecords;";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
					{
						adapter.Fill(table);
					}
				}
			} finally { CloseConnection() ; }
			return table;
		}
		public DataTable SelectCartridgeInventorys(short department, short cartridge)
		{
			DataTable table = new DataTable();
			string query = "SELECT CartridgeInventorys.id, CartridgeInventorys.[name] FROM CartridgeInventorys, CartridgeRecords WHERE CartridgeInventorys.id = CartridgeRecords.inventory_number AND department = @department AND cartridge = @cartridge;";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query,_connection))
				{
					cmd.Parameters.AddWithValue("@department", department);
					cmd.Parameters.AddWithValue("@cartridge", cartridge);
					using (SqlDataAdapter adapter = new SqlDataAdapter())
					{
						adapter.Fill(table);
					}
				}
			} finally { CloseConnection(); }
			return table;
		}
		public void Dispose()
		{
			_connection?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
