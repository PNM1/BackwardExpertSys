using ExpertSysApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ExpertSysApp.Controllers
{
    public class Controller
    {
        private string _filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "rules.json");
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
            else
            {
                Rules = new List<Rule>();
            }
        }
        public void LoadRulesFromPath(string path)
        {
            _filePath = path;
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                Rules = JsonSerializer.Deserialize<List<Rule>>(json) ?? new List<Rule>();
            }
            else
            {
                Rules = new List<Rule>();
            }
        }
        public void SaveRules()
        {
            if (string.IsNullOrEmpty(_filePath))
            {
                _filePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "rules.json");
            }
            SaveRulesToPath(_filePath);
        }
        public void SaveRulesToPath(string path)
        {
            _filePath = path;
            string json = JsonSerializer.Serialize(Rules, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        public (string result, List<TraceStep> trace, string workingMemoryLog) SearchAnswer(List<string> userFacts)
        {
            var trace = new List<TraceStep>();
            var logingMemory = new HashSet<string>(userFacts);

            var memoryLogBuilder = new StringBuilder();
            int stepCounter = 1;

            memoryLogBuilder.AppendLine($"{stepCounter}.");
            foreach (var fact in logingMemory)
            {
                memoryLogBuilder.AppendLine(fact);
            }
            stepCounter++;


            var workingMemory = new HashSet<string>(
                userFacts.Select(fact => fact.ToLower().Replace(" ", ""))
                );
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
                        string temp_cond = cond.ToLower().Replace(" ", "");
                        if (workingMemory.Contains(temp_cond))
                        {
                            matchPartial = true;
                        }
                        else
                        {
                            matchAll = false;
                        }
                    }
                    string temp_rule = rule.Conclusion.ToLower().Replace(" ", "");
                    if (matchAll && !workingMemory.Contains(temp_rule))
                    {
                        workingMemory.Add(temp_rule);
                        logingMemory.Add(rule.Conclusion);
                        trace.Add(new TraceStep { RuleDescription = FormatRule(rule), Status = TraceStatus.Success });
                        memoryLogBuilder.AppendLine($"{stepCounter}.");
                        foreach (var item in logingMemory)
                        {
                            memoryLogBuilder.AppendLine(item);
                        }
                        stepCounter++;
                        ruleApplied = true;
                        break;
                    }
                    else if (matchPartial)
                    {
                        trace.Add(new TraceStep { RuleDescription = FormatRule(rule), Status = TraceStatus.Partial });
                    }
                    else
                    {
                        trace.Add(new TraceStep { RuleDescription = FormatRule(rule), Status = TraceStatus.Unmatched });
                    }
                }
            } while (ruleApplied);
            foreach (var rule in Rules)
            {
                if (workingMemory.Contains(rule.Conclusion))
                {
                    return ($"Результат: {rule.Conclusion}", trace, memoryLogBuilder.ToString());
                }
            }

            return ("Результат не найден. Уточните правила или исходные данные.", trace, memoryLogBuilder.ToString());
        }

        private string FormatRule(Rule r)
        {
            return $"ЕСЛИ {string.Join(" И ", r.Conditions)} ТО {r.Conclusion}";
        }
    }
}
