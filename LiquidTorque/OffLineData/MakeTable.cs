using SQLite;

namespace liquidtorque.OffLineData
{
    public class MakeTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [MaxLength(30), NotNull, Unique]
        public string Make { get; set; }

        [MaxLength(30), NotNull, Unique]
        public string ParseObjectId { get; set; }
    }
}
