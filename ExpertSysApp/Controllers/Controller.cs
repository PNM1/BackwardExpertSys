using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ExpertSysApp.Models;

namespace ExpertSysApp.Controllers
{
    public class Controller
    {
        private string _filePath = "rules.json";
        public List<Rule> Rules { get; private set; } = new List<Rule>();

        public Controller()
        {
            LoadRules();
        }

        public void LoadRules()
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                Rules = JsonSerializer.Deserialize<List<Rule>>(json) ?? new List<Rule>();
            }
        }

        public void SaveRules()
        {
            string json = JsonSerializer.Serialize(Rules, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public (string result, List<TraceStep> trace) SearchAnswer(List<string> userFacts)
        {
            var trace = new List<TraceStep>();
            var workingMemory = new HashSet<string>(userFacts);
            bool ruleApplied;

            do
            {
                ruleApplied = false;
                foreach (var rule in Rules)
                {
                    bool matchAll = true;
                    bool matchPartial = false;

                    foreach (var cond in rule.Conditions)
                    {
                        if (workingMemory.Contains(cond))
                        {
                            matchPartial = true;
                        }
                        else
                        {
                            matchAll = false;
                        }
                    }

                    if (matchAll && !workingMemory.Contains(rule.Conclusion))
                    {
                        workingMemory.Add(rule.Conclusion);
                        trace.Add(new TraceStep { RuleDescription = FormatRule(rule), Status = TraceStatus.Success });
                        ruleApplied = true;
                        break;
                    }
                    else if (matchPartial)
                    {
                        trace.Add(new TraceStep { RuleDescription = FormatRule(rule), Status = TraceStatus.Partial });
                    }
                }
            } while (ruleApplied);
            foreach (var rule in Rules)
            {
                if (workingMemory.Contains(rule.Conclusion))
                {
                    return ($"Результат: {rule.Conclusion}", trace);
                }
            }

            return ("Результат не найден. Уточните правила или исходные данные.", trace);
        }

        private string FormatRule(Rule r)
        {
            return $"ЕСЛИ {string.Join(" И ", r.Conditions)} ТО {r.Conclusion}";
        }
    }
}
