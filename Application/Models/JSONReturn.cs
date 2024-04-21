using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class JSONReturn
    {
        public int Score { get; set; }
        public List<string> Questions { get; set; }
        public List<string> Material { get; set; }
        public static JSONReturn FromString(string res)
        {
            var lines = res.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var scoreLine = lines.FirstOrDefault(line => line.StartsWith("Score:"));
            var score = int.Parse(scoreLine.Split(':')[1].Trim());

            var questionsStart = Array.IndexOf(lines, "Questions:") + 1;
            var materialStart = Array.IndexOf(lines, "Material:");

            var questions = lines.Skip(questionsStart).Take(materialStart - questionsStart).Where(q => !string.IsNullOrWhiteSpace(q)).ToList();
            var material = lines.Skip(materialStart + 1).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();

            return new JSONReturn
            {
                Score = score,
                Questions = questions,
                Material = material
            };
        }


    }
}
