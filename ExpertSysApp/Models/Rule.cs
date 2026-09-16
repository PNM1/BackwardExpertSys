using System.Collections.Generic;

namespace ExpertSysApp.Models
{
    public class Rule
    {
        public int Id { get; set; }
        public List<Fact> Conditions { get; set; } = new();
        public Fact Conclusion { get; set; } = new();
    }
}