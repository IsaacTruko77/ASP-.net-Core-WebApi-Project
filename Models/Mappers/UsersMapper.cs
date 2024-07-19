using System;
using System.Collections.Generic;
using System.Data;
using Google.Protobuf.WellKnownTypes;

namespace SysCribBackend.Models
{
    public static class UsersMapper
    {
        /// <summary>
        /// Maps a DataRow to a Users object.
        /// </summary>
        /// <param name="row">The DataRow to map.</param>
        /// <returns>The mapped Users object.</returns>
        public static Users ToUser(DataRow row)
        {
            return new Users
            {
                User_Id = Convert.ToInt32(row["user_id"]),
                UserName = row["username"].ToString(),
                FirstName = row["first_name"].ToString(),
                LastName = row["last_name"].ToString(),
                Email = row["email"].ToString(),
                User_Role = row["user_role"].ToString(),
                RegisteredAt = Timestamp.FromDateTime(DateTime.SpecifyKind(Convert.ToDateTime(row["datetime"]), DateTimeKind.Utc)),
                Active = Convert.ToBoolean(row["active"]),
                Suscribed = Convert.ToBoolean(row["suscribed"])

            };
        }

        /// <summary>
        /// Maps a DataTable to a list of Users objects.
        /// </summary>
        /// <param name="table">The DataTable to map.</param>
        /// <returns>The list of mapped Users objects.</returns>
        public static List<Users> ToUserList(DataTable table)
        {
            var users = new List<Users>();
            foreach (DataRow row in table.Rows)
            {
                users.Add(ToUser(row));
            }
            return users;
        }
    }
}
