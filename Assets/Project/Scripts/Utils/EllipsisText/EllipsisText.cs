using UnityEngine;
using UnityEngine.UI;

namespace UnityArchitectureShowcase.Utils
{
    public static class EllipsisText
    {
        public static void SetEllipsisText(this InputField target, string content, string mask = "...", int maxLine = int.MaxValue - 1)
        {
            if (string.IsNullOrEmpty(content))
            {
                target.text = "";
                return;
            }

            using (var textGenerator = new TextGenerator(content.Length))
            {
                var setting = target.textComponent.GetGenerationSettings(target.textComponent.rectTransform.rect.size);

                if (textGenerator.PopulateWithErrors(content, setting, target.textComponent.gameObject))
                {
                    var endIndex = GetLineEndPosition(textGenerator, maxLine - 1);

                    if (endIndex < content.Length && endIndex > 0)
                    {
                        do
                        {
                            content = content.Substring(0, endIndex--);

                            textGenerator.PopulateWithErrors($"{content}{mask}", setting, target.gameObject);
                        }
                        while ((content.Length + mask.Length) != GetLineEndPosition(textGenerator, maxLine - 1));

                        content = $"{content}{mask}";
                    }
                }
                target.text = content;
            }
        }

        public static void SetEllipsisText(this Text target, string content, string mask = "...", int maxLine = int.MaxValue - 1)
        {
            if (string.IsNullOrEmpty(content))
            {
                target.text = "";
                return;
            }

            using (var textGenerator = new TextGenerator(content.Length))
            {
                var setting = target.GetGenerationSettings(target.rectTransform.rect.size);

                if (textGenerator.PopulateWithErrors(content, setting, target.gameObject))
                {
                    var endIndex = GetLineEndPosition(textGenerator, maxLine - 1);

                    if (endIndex < content.Length && endIndex > 0)
                    {
                        do
                        {
                            content = content.Substring(0, endIndex--);

                            textGenerator.PopulateWithErrors($"{content}{mask}", setting, target.gameObject);
                        }
                        while ((content.Length + mask.Length) != GetLineEndPosition(textGenerator, maxLine - 1));

                        content = $"{content}{mask}";
                    }
                }
                target.text = content;
            }
        }

        public static void SetEllipsisText(this Text target, string content, Vector2 size, string mask = "...", int maxLine = int.MaxValue - 1)
        {
            if (string.IsNullOrEmpty(content))
            {
                target.text = "";
                return;
            }

            using (var textGenerator = new TextGenerator(content.Length))
            {
                var setting = target.GetGenerationSettings(size);

                if (textGenerator.PopulateWithErrors(content, setting, target.gameObject))
                {
                    var endIndex = GetLineEndPosition(textGenerator, maxLine - 1);

                    if (endIndex < content.Length && endIndex > 0)
                    {
                        do
                        {
                            content = content.Substring(0, endIndex--);

                            textGenerator.PopulateWithErrors($"{content}{mask}", setting, target.gameObject);
                        }
                        while ((content.Length + mask.Length) != GetLineEndPosition(textGenerator, maxLine - 1));

                        content = $"{content}{mask}";
                    }
                }
                target.text = content;
            }
        }

        private static int GetLineEndPosition(TextGenerator generator, int line = 0)
        {
            line = Mathf.Max(line, 0);

            if (line + 1 < generator.lines.Count)
            {
                return generator.lines[line + 1].startCharIdx - 1;
            }
            return generator.characterCountVisible;
        }
    }
}