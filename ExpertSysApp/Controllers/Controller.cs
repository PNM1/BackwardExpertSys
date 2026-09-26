using ExpertSysApp.Models;
using Microsoft.VisualBasic;
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
            int iteration = 1;

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
                        trace.Add(new TraceStep { RuleDescription = $"[Шаг {iteration}] {FormatRule(rule)}", Status = TraceStatus.Success });
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
                        if (workingMemory.Contains(temp_rule)) { continue; }
                        trace.Add(new TraceStep { RuleDescription = $"[Шаг {iteration}] {FormatRule(rule)}", Status = TraceStatus.Partial });
                    }
                    else
                    {
                        trace.Add(new TraceStep { RuleDescription = $"[Шаг {iteration}] {FormatRule(rule)}", Status = TraceStatus.Unmatched });
                    }
                }
                iteration++;
            } while (ruleApplied);
            var rulesWithConclusionInDb = Rules
                .Where(r => workingMemory.Contains(r.Conclusion.ToLower().Replace(" ", "")))
                .ToList();

            var rulesWithConditionInDb = Rules
                .Where(r => r.Conditions.Any(cond => workingMemory.Contains(cond.ToLower().Replace(" ", ""))))
                .ToList();

            var conclusionsInDb = rulesWithConclusionInDb
                            .Select(r => r.Conclusion)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();

            var intersectionConclusions = rulesWithConclusionInDb
                .Where(rInc => rulesWithConditionInDb
                    .Any(rCond => rCond.Conditions.Contains(rInc.Conclusion, StringComparer.OrdinalIgnoreCase)))
                .Select(r => r.Conclusion)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var uniqueConclusions = conclusionsInDb
                .Except(intersectionConclusions, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var resultBuilder = new StringBuilder();

            if (!rulesWithConclusionInDb.Any())
            {
                resultBuilder.AppendLine("Результат не найден");
            }
            else
            {
                var targetConclusions = uniqueConclusions.Any() ? uniqueConclusions : intersectionConclusions;

                foreach (var conclusion in targetConclusions)
                {
                    resultBuilder.AppendLine(conclusion);

                    if (intersectionConclusions.Contains(conclusion, StringComparer.OrdinalIgnoreCase))
                    {
                        resultBuilder.AppendLine("Найденный системой результат можно уточнить. Введите дополнительные исходные данные");
                    }

                    resultBuilder.AppendLine();
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
