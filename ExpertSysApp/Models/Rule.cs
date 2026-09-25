namespace ExpertSysApp.Models
{
    public class Rule
    {
        public int Id { get; set; }
        public List<string> Conditions { get; set; } = new List<string>();
        public string Conclusion { get; set; } = string.Empty;
        public string ConditionsString => string.Join(" И ", Conditions);
    }
}