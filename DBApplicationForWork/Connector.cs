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
			string query = $"INSERT INTO {table} ([name]) VALUES (@name);";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					cmd.Parameters.AddWithValue("@name", name);
					return cmd.ExecuteNonQuery();
				}
			} finally {CloseConnection(); }
		}
		public int InsertCartridgeRecords(int order_number, int request_number, short department, List<short> cartridge, List<int> inventory_number)
		{
			DateTime date = DateTime.Now;
			string recording_date = date.ToString("yyyy-MM-dd");
			List<string> temp = new List<string>();
			for(int i = 0; i < cartridge.Count; i++)
				temp.Add($"(@order_number, @recording_date, @request_number, @department, @cartridge{i}, @inventory_number{i}, 1)");
			string query = $"INSERT INTO CartridgeRecords (order_number, recording_date, request_number, department, cartridge, inventory_number, [state]) VALUES {string.Join(", ", temp)};";
			try
			{
				OpenConnection();
				using (SqlCommand cmd = new SqlCommand(query, _connection))
				{
					cmd.Parameters.AddWithValue("@order_number", order_number);
					cmd.Parameters.AddWithValue("@recording_date", recording_date);
					cmd.Parameters.AddWithValue("@request_number", request_number);
					cmd.Parameters.AddWithValue("@department", department);
					for(int i = 0; i < cartridge.Count; i ++)
					{
						cmd.Parameters.AddWithValue($"@cartridge{i}", cartridge[i]);
						cmd.Parameters.AddWithValue($"@inventory_number{i}", inventory_number[i]);
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
			string query = "SELECT id, order_number, recording_date, request_number, department, cartridge, inventory_number, remark, company_date, company_act, complection_date, [state] FROM CartridgeRecords;";
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
