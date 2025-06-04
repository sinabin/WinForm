using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Icheon.DTO;
using MySql.Data.MySqlClient;
using System.Linq;

namespace Icheon
{
    /// <summary>
    /// DB 연결 및 Insert 작업을 전담하는 헬퍼 클래스
    /// </summary>
    public class DB_Manager
    {
        private readonly string _connectionString;

        /// <summary>
        /// 생성자. 실제 DB 접속에 필요한 연결 문자열을 지정
        /// </summary>
        /// <param name="connectionString">DB 연결 문자열</param>
        public DB_Manager(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        
        // 생산 Data DB Insert
        public async Task InsertOutputDataAsync(string s)
        {
            try
            {
                // s는 CSV 문자열이라고 가정 (예: "order_number,prd_date,prd_line,order_place,usage,liter,serial")
                string[] cols = s.Split(',');
                if (cols.Length < 7)
                {
                    throw new Exception("CSV 데이터 형식이 올바르지 않습니다.");
                }

                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 쿼리 작성: 테이블의 컬럼 이름에 맞게 prd_date 사용
                    string query = @"
                INSERT INTO production_data
                    (serial, prd_date, prd_line, order_place, `usage`, liter, order_number)
                VALUES 
                    (@Serial, @PrdDate, @ProdLine, @OrderPlace, @Usage, @Liter, @OrderNumber);";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@PrdDate", cols[1]);
                        cmd.Parameters.AddWithValue("@ProdLine", cols[2]);
                        cmd.Parameters.AddWithValue("@OrderPlace", cols[3]);
                        cmd.Parameters.AddWithValue("@Usage", cols[4]);
                        cmd.Parameters.AddWithValue("@Liter", cols[5]);
                        cmd.Parameters.AddWithValue("@Serial", cols[6]);
                        cmd.Parameters.AddWithValue("@OrderNumber", cols[0]);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertOutputDataAsync error: " + ex.Message);
                throw;
            }
        }

        
        // 작업계획량 DB Insert
        public async Task InsertWorkPlanningAsync(WorkPlanAssignmentDTO dto)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO work_planning (order_id, plan_date, order_place, prd_line, `usage`, liter, st_serial, end_serial, is_done)
            SELECT @Order_id, NOW(), @OrderPlace, @ProdLine, @Usage, @Liter, @st_serial, @end_serial, 'N'
            FROM DUAL
            WHERE NOT EXISTS (
                SELECT 1 
                FROM work_planning
                WHERE order_id = @Order_id 
                  AND `usage`   = @Usage 
                  AND liter     = @Liter 
                  AND st_serial = @st_serial
                  AND is_done  = 'N');";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        // app.config의 RegionCodeToKr 값으로 발주처(order_place) 설정
                        string orderPlace = ConfigurationManager.AppSettings["RegionCodeToKr"];
                        cmd.Parameters.AddWithValue("@Order_id", dto.Order_id); // 추가된 주문번호
                        cmd.Parameters.AddWithValue("@OrderPlace", orderPlace);
                        cmd.Parameters.AddWithValue("@ProdLine", dto.ProductionLine);
                        cmd.Parameters.AddWithValue("@Usage", dto.Usage);
                        cmd.Parameters.AddWithValue("@Liter", dto.CurrentPrintText);
                        cmd.Parameters.AddWithValue("@st_serial", dto.st_serial); // 시작 시리얼번호
                        cmd.Parameters.AddWithValue("@end_serial", dto.end_serial); // 끝 시리얼번호

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            // 중복된 미완료 작업계획이 이미 존재할 경우 예외 발생.
                            throw new Exception("DuplicateFound");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertWorkPlanningAsync error: " + ex.Message);
                throw; // 호출한 측에서 이 예외를 처리하도록 전달
            }
        }

        
        /// 작업계획 레코드의 is_done을 'Y'로 업데이트
        public async Task UpdateWorkPlanningDoneAsync(int idx)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                            UPDATE work_planning
                            SET is_done = 'Y'
                            WHERE idx = @Idx";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        // int idx 값을 파라미터로 전달
                        cmd.Parameters.AddWithValue("@Idx", idx);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateWorkPlanningDoneAsync error: " + ex.Message);
            }
        }
        
        
        public async Task<List<Dictionary<string, object>>> LoadIncompleteWorkPlansAsync()
        {
            var results = new List<Dictionary<string, object>>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                SELECT idx, order_id, prd_line, `usage`, liter
                FROM work_planning
                WHERE is_done = 'N'";
            
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var record = new Dictionary<string, object>
                                {
                                    ["idx"] = reader["idx"],
                                    ["order_id"] = reader["order_id"],
                                    ["prd_line"] = reader["prd_line"],
                                    ["usage"] = reader["usage"],
                                    ["liter"] = reader["liter"],
                                };
                                results.Add(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadIncompleteWorkPlansAsync error: " + ex.Message);
            }
            return results;
        }
        
        // 생산내역 Insert
        public async Task InsertOrderHistoryAsync(OrderHistoryRecordDTO record)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"
                            INSERT INTO order_history (order_number, order_date, `usage`, liter, order_amount)
                            SELECT @OrderNumber, @OrderDate, @Usage, @Liter, @OrderAmount
                            FROM DUAL
                            WHERE NOT EXISTS (SELECT 1 FROM order_history WHERE order_number = @OrderNumber AND `usage` = @Usage AND liter = @Liter)";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderNumber", record.order_number);
                        cmd.Parameters.AddWithValue("@OrderDate", record.order_date);
                        cmd.Parameters.AddWithValue("@Usage", record.usage);
                        cmd.Parameters.AddWithValue("@Liter", record.liter);
                        cmd.Parameters.AddWithValue("@OrderAmount", record.order_amount);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertOrderHistoryAsync error: " + ex.Message);
                throw;
            }
        }
        

        /// Select OrderHistory
        public async Task<Dictionary<string, Tuple<int, string, DateTime>>> LoadOrderHistoryAsync()
        {
            var result = new Dictionary<string, Tuple<int, string, DateTime>>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // "order_number,usage,liter" 
                    string query = @"
                            SELECT `usage`, liter, order_number, order_date, SUM(order_amount) AS total_order
                            FROM order_history
                            WHERE is_done = 0
                            GROUP BY order_number, `usage`, liter";
            
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string usage = reader["usage"].ToString();
                                string liter = reader["liter"].ToString();
                                int orderAmount = Convert.ToInt32(reader["total_order"]);
                                string orderNumber = reader["order_number"].ToString();
                                DateTime orderDate = Convert.ToDateTime(reader["order_date"]);

                                // 키를 order_number, usage, liter의 조합으로 구성
                                string key = string.Format("{0},{1},{2}", orderNumber, usage, liter);
                                // 동일한 키가 이미 있다면 여기서는 최초 건만 사용 (필요에 따라 합산 또는 최신 건 사용)
                                if (!result.ContainsKey(key))
                                {
                                    result.Add(key, new Tuple<int, string, DateTime>(orderAmount, orderNumber, orderDate));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadOrderHistoryAsync error: " + ex.Message);
            }
            return result;
        }

        public async Task UpdateWorkPlanningEndSerialAsync(string newEndSerial, int orderId, string usage, string liter)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    // 동일한 order_id, usage, liter 조합을 가진 미완료된 레코드만 업데이트
                    string query = @"
                            UPDATE work_planning
                            SET end_serial = @NewEndSerial
                            WHERE is_done = 'N'
                              AND order_id = @OrderId
                              AND `usage` = @Usage
                              AND liter = @Liter";
                    
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewEndSerial", newEndSerial);
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.Parameters.AddWithValue("@Usage", usage);
                        cmd.Parameters.AddWithValue("@Liter", liter);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateWorkPlanningEndSerialAsync error: " + ex.Message);
                throw;
            }
        }


        public async Task<Dictionary<string, int>> LoadProductionStatusByOrderNumbersAsync(IEnumerable<string> orderNumbers)
        {
            var result = new Dictionary<string, int>();
            if (!orderNumbers.Any())
                return result;

            string inClause = string.Join(",", orderNumbers.Select(o => "@o" + o));
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = $@"
            SELECT order_number, `usage`, liter, COUNT(*) AS totalProduction
            FROM production_data
            WHERE order_number IN ({inClause})
            GROUP BY order_number, `usage`, liter";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    foreach (var o in orderNumbers)
                        cmd.Parameters.AddWithValue("@o" + o, o);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string key = $"{reader["order_number"]},{reader["usage"]},{reader["liter"]}";
                            result[key] = Convert.ToInt32(reader["totalProduction"]);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<long> GetNextSerialAsync(string orderNumber)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // order_number에 해당하는 레코드만 카운트
                    string query = @"
                SELECT COUNT(*) + 1 AS nextSerial
                FROM production_data
                WHERE order_number = @OrderNumber";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                        var result = await cmd.ExecuteScalarAsync();
                        return Convert.ToInt64(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetNextSerialAsync error: " + ex.Message);
                throw;
            }
        }

        
        public async Task<(string st_serial, string end_serial)> GetWorkPlanningSerialsAsync(int idx)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
            SELECT st_serial, end_serial 
            FROM work_planning 
            WHERE idx = @Idx";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Idx", idx);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return (
                                st_serial: reader["st_serial"].ToString(),
                                end_serial: reader["end_serial"].ToString()
                            );
                        }
                    }
                }
            }
            throw new KeyNotFoundException($"Work planning idx={idx} not found");
        }

    }
}
