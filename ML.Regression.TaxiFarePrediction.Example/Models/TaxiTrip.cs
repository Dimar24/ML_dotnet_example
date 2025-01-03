﻿using Microsoft.ML.Data;

namespace ML.Regression.TaxiFarePrediction.Example.Models;

/// <summary>
/// Модель с структурой данных из файла
/// </summary>
public class TaxiTrip
{
    [LoadColumn(0)]
    public string VendorId { get; set; }

    [LoadColumn(1)]
    public string RateCode { get; set; }

    [LoadColumn(2)]
    public float PassengerCount { get; set; }

    [LoadColumn(3)]
    public float TripTime { get; set; }

    [LoadColumn(4)]
    public float TripDistance { get; set; }

    [LoadColumn(5)]
    public string PaymentType { get; set; }

    [LoadColumn(6)]
    public float FareAmount { get; set; }
}