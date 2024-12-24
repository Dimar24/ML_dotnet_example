using DeepMorphy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeepMorphy.Example;

public static class Program
{
    static void Main(string[] args)
    {
        // Основной элемент для работы с DeepMorphy
        var morph = new MorphAnalyzer();

        // Входные данные для примера
        var firstPeople = new People()
        {
            Name = "Дмитрий",
            Surname = "Марковцом",
            Middlename = "Олегович"
        };

        // Анализ входных данных с помощью DeepMorphy
        var firstPeopleMorph = morph
            .Parse(words: new List<string>() {
                firstPeople.Surname,
                firstPeople.Name,
                firstPeople.Middlename
            })
            .ToArray();


        // Входные данные для примера
        var secondPeople = new People()
        {
            Name = "Даниил",
            Surname = "Зыкин",
            Middlename = "Батькович"
        };
        // Анализ входных данных с помощью DeepMorphy
        var secondPeopleMorph = morph
            .Parse(words: new List<string>() {
                secondPeople.Surname,
                secondPeople.Name,
                secondPeople.Middlename
            })
            .ToArray();


        // Создание тегов
        // Теги из себя представляют набор морфологических данных
        // и содержит информацию, по которой требуется преобразовать ваше словосочетание
        // Список всех поддерживаемых в DeepMorphy категорий и граммем тут ->
        // https://github.com/lepeap/DeepMorphy/blob/master/gram.md
        var datvTag = morph.TagHelper.CreateTag("сущ", gndr: "муж", nmbr: "ед", @case: "тв");
        var abltTag = morph.TagHelper.CreateTag("сущ", gndr: "муж", nmbr: "ед", @case: "дт");
        var nomnTag = morph.TagHelper.CreateTag("сущ", gndr: "муж", nmbr: "ед", @case: "зв");

        // Пример небольшого шаблончика, в который надо подставить преобразованные слова
        var reference = @"
                         СПРАВКА
           Выдана кем - {0}
           Выдана кому - {1}
        Причина в том, что {2} не нуждается
        более в получении никаких задач.";

        // Пример вывода нашего преобразования
        Console.WriteLine(
            reference,
            GetCorrectFullName(morph, secondPeopleMorph, datvTag),
            GetCorrectFullName(morph, firstPeopleMorph, abltTag),
            GetCorrectFullName(morph, firstPeopleMorph, nomnTag));

        #region Helpers_Call_WriteLineLexeme
        WriteLineLexeme(morph, secondPeople.Surname);
        #endregion Helpers_Call_WriteLineLexeme
    }

    /// <summary>
    /// Метод преобразования.
    /// </summary>
    /// <param name="morphAnalyzer">Анализатор</param>
    /// <param name="morphInfos">Набор слов с информацией</param>
    /// <param name="resultTag">Тег с нужным преобразованием</param>
    /// <returns></returns>
    static string GetCorrectFullName(
        MorphAnalyzer morphAnalyzer,
        MorphInfo[] morphInfos,
        Tag resultTag)
    {
        return
            string.Join(
                " ",
                morphAnalyzer
                    .Inflect(
                    [
                        new InflectTask(
                            word: morphInfos[0].Text,
                            wordTag: morphInfos[0].Tags.Where(x => x.GramsDic["род"] == "муж").OrderByDescending(x => x.Power).First(),
                            resultTag: resultTag),
                        new InflectTask(
                            word: morphInfos[1].Text,
                            wordTag: morphInfos[1].Tags.Where(x => x.GramsDic["род"] == "муж").OrderBy(x => x.Power).First(),
                            resultTag: resultTag),
                        new InflectTask(
                            word: morphInfos[2].Text,
                            wordTag: morphInfos[2].Tags.Where(x => x.GramsDic["род"] == "муж").OrderBy(x => x.Power).First(),
                            resultTag: resultTag)
                    ]).Select(FirstCharToUpper));
    }

    class People
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Middlename { get; set; }

        public People()
        {

        }
    }

    #region Helpers

    /// <summary>
    /// Метод для вывода всех лексем слова.
    /// </summary>
    /// <param name="morphAnalyzer"></param>
    /// <param name="word"></param>
    static void WriteLineLexeme(
        MorphAnalyzer morphAnalyzer,
        string word)
    {
        var tag = morphAnalyzer.Parse(new List<string> { word }).First();
        var results = morphAnalyzer.Lexeme(word, tag.Tags[1]).ToArray();
        foreach (var result in results)
            Console.WriteLine($"{result.text} : {result.tag}");
    }

    public static string FirstCharToUpper(this string input)
        => input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };

    #endregion Helpers
}