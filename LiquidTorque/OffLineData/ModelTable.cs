using SQLite;

namespace liquidtorque.OffLineData
{
    public class ModelTable
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [MaxLength(30), NotNull,Unique]
        public string ParseObjectId { get; set; }

        [MaxLength(30), NotNull]
        public string Model { get; set; }

        [MaxLength(30), NotNull]
        public string Make { get; set; }
    }
}
