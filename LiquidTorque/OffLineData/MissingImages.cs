using SQLite;

namespace liquidtorque.OffLineData
{
    public class MissingImages
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [MaxLength(30), NotNull, Unique]
        public string Objectid { get; set; }
    }
}