namespace AutoLab.Domain.Entities
{
    public class Country
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Iso2 { get; private set; }

        public Country(string id, string name, string iso2)
        {
            Id = id;
            Name = name;
            Iso2 = iso2;
        }
    }
} 