namespace AssertExtensions
{
    internal class Property
    {
        public string ParentName { get; set; }
        public string Name { get; set; }

        public Property(string parentName, string name)
        {
            ParentName = parentName;
            Name = name;
        }
    }
}
