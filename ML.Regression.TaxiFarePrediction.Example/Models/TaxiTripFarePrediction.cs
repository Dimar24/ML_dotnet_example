using Microsoft.ML.Data;

namespace ML.Regression.TaxiFarePrediction.Example.Models;

public class TaxiTripFarePrediction
{
    [ColumnName("Score")]
    public float FareAmount { get; set; }
}