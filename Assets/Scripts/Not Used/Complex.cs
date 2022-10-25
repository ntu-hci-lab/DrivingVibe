using System.Collections;
using System.Collections.Generic;

public class ComplexNumber
{
    private float realPart;
    private float imgPart;

    public float RealPart => realPart;
    public float ImaginePart => imgPart;

    public ComplexNumber(float real, float img)
    {
        realPart = real;
        imgPart = img;
    }

    public ComplexNumber Add(ComplexNumber a, ComplexNumber b)
    {
        // (x_1+x_2i) + (y_1+y_2)i = (x_1+y_1)+(x_2+y_2)i
        return new ComplexNumber(a.realPart + b.realPart, a.imgPart + b.imgPart);
    }

    public ComplexNumber Multiply(ComplexNumber a, ComplexNumber b)
    {
        // (x_1+x_2i) * (y_1+y_2)i = (x_1y_1 - x_2y_2)+(x_1y_2 + x_2y_1)i
        return new ComplexNumber(a.realPart *  b.realPart - a.imgPart * b.imgPart , a.realPart * b.imgPart + a.imgPart * b.realPart);
    }
}
