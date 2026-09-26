using ExpertSysApp.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;

namespace ExpertSysApp.Controllers
{
    public class Controller
    {
        private string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rules.json");
        public List<Rule> Rules { get; private set; } = new List<Rule>();

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

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
                _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rules.json");
            }
            SaveRulesToPath(_filePath);
        }
        public void SaveRulesToPath(string path)
        {
            _filePath = path;
            string json = JsonSerializer.Serialize(Rules, _jsonOptions);
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
            var rulesWithConclusionInDb = Rules
                .Where(r => workingMemory.Contains(r.Conclusion.ToLower().Replace(" ", "")))
                .ToList();

            var rulesWithConditionInDb = Rules
                .Where(r => r.Conditions.Any(cond => workingMemory.Contains(cond.ToLower().Replace(" ", ""))))
                .ToList();

            var intersection = rulesWithConditionInDb
                .Where(r1 => rulesWithConclusionInDb
                    .Any(r2 => r1.Conditions.Contains(r2.Conclusion)))
                .ToList();

            foreach (var rule in intersection)
            {
                if (rulesWithConclusionInDb.Contains(rule)) { rulesWithConclusionInDb.Remove(rule); }
                if (rulesWithConditionInDb.Contains(rule)) { rulesWithConditionInDb.Remove(rule); }
            }

            var resultBuilder = new StringBuilder();

            if (!rulesWithConclusionInDb.Any() || !intersection.Any())
            {
                resultBuilder.AppendLine("Результат не найден");
            }
            else
            {
                foreach (var rule in intersection)
                {
                    foreach(var condition in rulesWithConditionInDb)
                    {

                    }
                    bool allMatchCondition = true;
                    foreach (var cond in rule.Conditions)
                    {
                        if (!workingMemory.Contains(cond.ToLower().Replace(" ", "")))
                        {
                            allMatchCondition = false;
                            break;
                        }
                    }
                    if (!allMatchCondition)
                    {
                        rulesWithConclusionInDb.Remove(rule);
                        resultBuilder.AppendLine(rule.Conclusion);
                        resultBuilder.AppendLine("Найденный системой результат можно уточнить. Введите дополнительные исходные данные");
                        resultBuilder.AppendLine();
                    }
                }
            }

            return (resultBuilder.ToString().TrimEnd(), trace, memoryLogBuilder.ToString());
        }

        private string FormatRule(Rule r)
        {
            return $"ЕСЛИ {string.Join(" И ", r.Conditions)} ТО {r.Conclusion}";
        }
    }
}
