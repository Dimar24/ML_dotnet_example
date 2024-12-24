using Microsoft.ML;
using ML.Regression.TaxiFarePrediction.Example.Models;
using System;
using System.IO;

namespace ML.Regression.TaxiFarePrediction.Example;

public static class Program
{
    private static readonly string _trainDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "taxi-fare-train.csv");
    private static readonly string _testDataPath = Path.Combine(Environment.CurrentDirectory, "Data", "taxi-fare-test.csv");

    public static void Main(string[] args)
    {
        // Начальный объект для работы с МЛ НЕТ
        MLContext mlContext = new MLContext(seed: 0);

        // Подгрузка данных для тренировки модели
        IDataView trainData = LoadData(mlContext, _trainDataPath);

        // Тренировка модели
        ITransformer model = Train(mlContext, trainData);

        // Подгрузка данных для тестирования обученной модели
        IDataView testData = LoadData(mlContext, _testDataPath);

        // Проверка на точность обученной модели
        Evaluate(mlContext, model, testData);

        // Проверка обученной модели на единном элементе
        TestSinglePrediction(mlContext, model);
    }

    private static IDataView LoadData(
        MLContext mlContext,
        string dataPath)
    {
        // Загрузка данных из файла в контекст
        IDataView dataView = mlContext.Data
            .LoadFromTextFile<TaxiTrip>(dataPath, hasHeader: true, separatorChar: ',');

        return dataView;
    }

    private static ITransformer Train(
        MLContext mlContext,
        IDataView trainData)
    {
        var pipeline = mlContext.Transforms

            // Выставление параметра для предсказания с помощью - Label
            .CopyColumns(outputColumnName: "Label", inputColumnName: "FareAmount")

            // Преобразование данных для обучения модели
            // Регрессионное Машинное обучение не умеет работать с строковыми типоми - string/char
            .Append(mlContext.Transforms.Categorical
                .OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: "VendorId"))
            .Append(mlContext.Transforms.Categorical
                .OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: "RateCode"))
            .Append(mlContext.Transforms.Categorical
                .OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: "PaymentType"))

            // Выставление элементов используемых для обучения и предсказания
            .Append(mlContext.Transforms
                .Concatenate(
                    "Features",
                    "VendorIdEncoded",
                    "RateCodeEncoded",
                    "PassengerCount",
                    "TripDistance",
                    "PaymentTypeEncoded"))

            // Выбор алгоритма для обучения
            .Append(mlContext.Regression.Trainers.FastTree());

        // Вызов метода для обучения
        var model = pipeline.Fit(trainData);

        return model;
    }

    private static void Evaluate(
        MLContext mlContext,
        ITransformer model,
        IDataView testData)
    {
        var predictions = model
            .Transform(testData);

        var metrics = mlContext.Regression
            .Evaluate(predictions, "Label", "Score");

        Console.WriteLine();
        Console.WriteLine($"*************************************************");
        Console.WriteLine($"*       Model quality metrics evaluation         ");
        Console.WriteLine($"*------------------------------------------------");
        Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
        Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:0.##}");
    }

    private static void TestSinglePrediction(MLContext mlContext, ITransformer model)
    {
        // Создаем элемент для предсказывания будущего значения
        var predictionFunction = mlContext.Model
            .CreatePredictionEngine<TaxiTrip, TaxiTripFarePrediction>(model);

        var taxiTripSample = new TaxiTrip()
        {
            VendorId = "VTS",
            RateCode = "1",
            PassengerCount = 1,
            TripTime = 1140,
            TripDistance = 3.75f,
            PaymentType = "CRD",
            FareAmount = 0 // To predict. Actual/Observed = 15.5
        };

        var prediction = predictionFunction
            .Predict(taxiTripSample);

        Console.WriteLine($"**********************************************************************");
        Console.WriteLine($"Predicted fare: {prediction.FareAmount:0.####}, actual fare: 15.5");
        Console.WriteLine($"**********************************************************************");
    }
}