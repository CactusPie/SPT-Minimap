using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace CactusPie.MapLocation.Minimap.Helpers;

public static class PolynomialHelper
{
    public static double[] FitPolynomial(double[] x, double[] y, int degree)
    {
        // Source: https://rosettacode.org/wiki/Polynomial_regression#C.23
        var v = new DenseMatrix(x.Length, degree + 1);
        for (int i = 0; i < v.RowCount; i++)
        {
            for (int j = 0; j <= degree; j++)
            {
                v[i, j] = Math.Pow(x[i], j);
            }
        }
            
        var yv = new DenseVector(y).ToColumnMatrix();
        QR<double> qr = v.QR();

        var r = qr.R.SubMatrix(0, degree + 1, 0, degree + 1);
        var q = v.Multiply(r.Inverse());
        var p = r.Inverse().Multiply(q.TransposeThisAndMultiply(yv));
        return p.Column(0).ToArray();
    }
    
    public static double CalculatePolynomialValue(double x, IReadOnlyList<double> coefficients)
    {
        double result = 0;

        if (coefficients.Count > 0)
        {
            result = coefficients[0];
        }
        
        if (coefficients.Count > 1)
        {
            result += coefficients[1] * x;
        }
        
        for (int i = 2; i < coefficients.Count; i++)
        {
            result += coefficients[i] * Math.Pow(x, i);
        }

        return result;
    }
}