namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var projects = context
                .Projects
                .ToArray()
                .Where(x => x.Tasks.Any())
                .Select(x => new ExportProjectModel
                {
                    TasksCount = x.Tasks.Count,
                    ProjectName = x.Name,
                    HasEndDate = x.DueDate == null ? "No" : "Yes",
                    Tasks = x.Tasks.Select(t => new ExportTaskModel
                    {
                        Name = t.Name,
                        Label = t.LabelType.ToString()
                    })
                    .OrderBy(t => t.Name)
                    .ToArray()
                })
                .OrderByDescending(x => x.Tasks.Length)
                .ThenBy(x => x.ProjectName)
                .ToArray();

            var xml = XmlConverter.Serialize<ExportProjectModel>(projects, "Projects");

            return xml;
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context
               .Employees
               .ToArray()
               .Where(x => x.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
               .Select(x => new 
               {
                   Username = x.Username,
                   Tasks = x.EmployeesTasks
                       .Where(et => et.Task.OpenDate >= date)
                       .OrderByDescending(et => et.Task.DueDate)
                       .ThenBy(et => et.Task.Name)
                       .Select(et => new
                       {
                           TaskName = et.Task.Name,
                           OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                           DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                           LabelType = et.Task.LabelType.ToString(),
                           ExecutionType = et.Task.ExecutionType.ToString()
                       })
                       .ToArray()
               })
               .OrderByDescending(x => x.Tasks.Length)
               .ThenBy(x => x.Username)
               .Take(10)
               .ToArray();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}