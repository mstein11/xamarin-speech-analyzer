using SQLite;

namespace Happimeter.Data
{
    public class Config : IEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
