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
        public (string result, List<TraceStep> trace, string workingMemoryLog) SearchBackward(List<string> userFacts, string targetGoal)
        {
            var trace = new List<TraceStep>();
            var workingMemory = new HashSet<string>(userFacts.Select(f => f.ToLower().Replace(" ", "")));

            var logingMemory = new HashSet<string>(userFacts);
            var memoryLogBuilder = new StringBuilder();
            int memoryStepCounter = 1;
            int searchStepCounter = 1;

            targetGoal = targetGoal.Trim();
            if (string.IsNullOrEmpty(targetGoal))
            {
                return ("Ошибка: Не указана целевая ситуация для проверки.", trace, "Рабочая память пуста.");
            }

            memoryLogBuilder.AppendLine($"{memoryStepCounter}. Исходная РБД:");
            foreach (var fact in logingMemory)
            {
                memoryLogBuilder.AppendLine($"- {fact}");
            }
            memoryLogBuilder.AppendLine($"\nЦель (гипотеза): {targetGoal}\n");
            memoryStepCounter++;

            bool isProven = ProveGoalWithSteps(targetGoal, Rules, workingMemory, logingMemory, trace, memoryLogBuilder, ref memoryStepCounter, ref searchStepCounter);

            string resultText = isProven
                ? $"Т.о., факты достоверны, цель подтвердилась: «{targetGoal}»."
                : $"Целевую ситуацию «{targetGoal}» доказать не удалось (недостаточно данных или отсутствуют правила).";

            return (resultText, trace, memoryLogBuilder.ToString());
        }

        private bool ProveGoalWithSteps(string goal, List<Rule> allRules, HashSet<string> workingMemory, HashSet<string> logingMemory, List<TraceStep> trace, StringBuilder memoryLogBuilder, ref int memoryStepCounter, ref int searchStepCounter)
        {
            string normalizedGoal = goal.ToLower().Replace(" ", "");

            if (workingMemory.Contains(normalizedGoal))
            {
                return true;
            }

            var matchingRules = allRules
                .Where(r => r.Conclusion.Equals(goal, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!matchingRules.Any())
            {
                trace.Add(new TraceStep
                {
                    RuleDescription = $"Цель «{goal}» не найдена в заключениях правил.",
                    Status = TraceStatus.Unmatched
                });
                return false;
            }

            foreach (var rule in matchingRules)
            {
                trace.Add(new TraceStep
                {
                    RuleDescription = $"{searchStepCounter}. Ищется цель «{goal}» в заключениях правил\nПодходит правило П{rule.Id}: {FormatRule(rule)}",
                    Status = TraceStatus.Partial
                });
                searchStepCounter++;

                bool canApplyRule = true;

                foreach (var condition in rule.Conditions)
                {
                    string normCond = condition.ToLower().Replace(" ", "");

                    if (workingMemory.Contains(normCond))
                    {
                        continue;
                    }

                    trace.Add(new TraceStep
                    {
                        RuleDescription = $"Не все условия выполнены - новая цель: «{condition}»",
                        Status = TraceStatus.Partial
                    });

                    bool subGoalProven = ProveGoalWithSteps(condition, allRules, workingMemory, logingMemory, trace, memoryLogBuilder, ref memoryStepCounter, ref searchStepCounter);

                    if (!subGoalProven)
                    {
                        canApplyRule = false;
                        break;
                    }
                }

                if (canApplyRule)
                {
                    workingMemory.Add(normalizedGoal);
                    logingMemory.Add(goal);

                    trace.Add(new TraceStep
                    {
                        RuleDescription = $"Все условия выполнены\nВ РБД добавляется факт: «{goal}»",
                        Status = TraceStatus.Success
                    });

                    memoryLogBuilder.AppendLine($"{memoryStepCounter}. РБД после срабатывания правила П{rule.Id}:");
                    foreach (var item in logingMemory)
                    {
                        memoryLogBuilder.AppendLine($"- {item}");
                    }
                    memoryLogBuilder.AppendLine();
                    memoryStepCounter++;

                    return true;
                }
            }

            return false;
        }

        private string FormatRule(Rule r)
        {
            return $"ЕСЛИ {string.Join(" И ", r.Conditions)} ТО {r.Conclusion}";
        }
    }
}
