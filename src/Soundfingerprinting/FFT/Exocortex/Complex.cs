namespace Soundfingerprinting.FFT.Exocortex
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    // Comments? Questions? Bugs? Tell Ben Houston at ben@exocortex.org
    // Version: May 4, 2002

    /// <summary>
    ///   <p>A double-precision complex number representation.</p>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:CodeAnalysisSuppressionMustHaveJustification", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1404:CodeAnalysisSuppressionMustHaveJustification", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:SingleLineCommentsMustBeginWithSingleSpace", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1512:SingleLineCommentsMustNotBeFollowedByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:StaticReadonlyFieldsMustBeginWithUpperCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:SingleLineCommentMustBePrecededByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:ClosingCurlyBracketMustBeFollowedByBlankLine", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1405:DebugAssertMustProvideMessageText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:StatementMustNotUseUnnecessaryParenthesis", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1003:SymbolsMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1407:ArithmeticExpressionsMustDeclarePrecedence", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1008:OpeningParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:ConstructorSummaryDocumentationMustBeginWithStandardText",
        Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1616:ElementReturnValueDocumentationMustHaveText", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:StaticElementsMustAppearBeforeInstanceElements", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "Reviewed. Suppression is OK here.")]
    internal struct Complex : IComparable, ICloneable
    {
        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   The real component of the complex number
        /// </summary>
        public double Re;

        /// <summary>
        ///   The imaginary component of the complex number
        /// </summary>
        public double Im;

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Create a complex number from a real and an imaginary component
        /// </summary>
        /// <param name = "real"></param>
        /// <param name = "imaginary"></param>
        public Complex(double real, double imaginary)
        {
            Re = real;
            Im = imaginary;
        }

        /// <summary>
        ///   Create a complex number based on an existing complex number
        /// </summary>
        /// <param name = "c"></param>
        public Complex(Complex c)
        {
            Re = c.Re;
            Im = c.Im;
        }

        /// <summary>
        ///   Create a complex number from a real and an imaginary component
        /// </summary>
        /// <param name = "real"></param>
        /// <param name = "imaginary"></param>
        /// <returns></returns>
        public static Complex FromRealImaginary(double real, double imaginary)
        {
            Complex c;
            c.Re = real;
            c.Im = imaginary;
            return c;
        }

        /// <summary>
        ///   Create a complex number from a modulus (length) and an argument (radian)
        /// </summary>
        /// <param name = "modulus"></param>
        /// <param name = "argument"></param>
        /// <returns></returns>
        public static Complex FromModulusArgument(double modulus, double argument)
        {
            Complex c;
            c.Re = (modulus * Math.Cos(argument));
            c.Im = (modulus * Math.Sin(argument));
            return c;
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        object ICloneable.Clone()
        {
            return new Complex(this);
        }

        /// <summary>
        ///   Clone the complex number
        /// </summary>
        /// <returns></returns>
        public Complex Clone()
        {
            return new Complex(this);
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   The modulus (length) of the complex number
        /// </summary>
        /// <returns></returns>
        public double GetModulus()
        {
            double x = Re;
            double y = Im;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        ///   The squared modulus (length^2) of the complex number
        /// </summary>
        /// <returns></returns>
        public double GetModulusSquared()
        {
            double x = Re;
            double y = Im;
            return x * x + y * y;
        }

        /// <summary>
        ///   The argument (radians) of the complex number
        /// </summary>
        /// <returns></returns>
        public double GetArgument()
        {
            return Math.Atan2(Im, Re);
        }

        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Get the conjugate of the complex number
        /// </summary>
        /// <returns></returns>
        public Complex GetConjugate()
        {
            return FromRealImaginary(Re, -Im);
        }

        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Scale the complex number to 1.
        /// </summary>
        public void Normalize()
        {
            double modulus = GetModulus();
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (modulus == 0) // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                throw new DivideByZeroException("Can not normalize a complex number that is zero.");
            }
            Re = (Re / modulus);
            Im = (Im / modulus);
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Convert to a from double precision complex number to a single precison complex number
        /// </summary>
        /// <param name = "cF"></param>
        /// <returns></returns>
        public static explicit operator Complex(ComplexF cF)
        {
            Complex c;
            c.Re = cF.Re;
            c.Im = cF.Im;
            return c;
        }

        /// <summary>
        ///   Convert from a single precision real number to a complex number
        /// </summary>
        /// <param name = "d"></param>
        /// <returns></returns>
        public static explicit operator Complex(double d)
        {
            Complex c;
            c.Re = d;
            c.Im = 0;
            return c;
        }

        /// <summary>
        ///   Convert from a single precision complex to a real number
        /// </summary>
        /// <param name = "c"></param>
        /// <returns></returns>
        public static explicit operator double(Complex c)
        {
            return c.Re;
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Are these two complex numbers equivalent?
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static bool operator ==(Complex a, Complex b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.Re == b.Re && a.Im == b.Im;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///   Are these two complex numbers different?
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static bool operator !=(Complex a, Complex b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (a.Re != b.Re) || (a.Im != b.Im);
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        /// <summary>
        ///   Get the hash code of the complex number
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return (Re.GetHashCode() ^ Im.GetHashCode());
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        /// <summary>
        ///   Is this complex number equivalent to another object?
        /// </summary>
        /// <param name = "o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (o is Complex)
            {
                Complex c = (Complex)o;
                return (this == c);
            }
            return false;
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Compare to other complex numbers or real numbers
        /// </summary>
        /// <param name = "o"></param>
        /// <returns></returns>
        public int CompareTo(object o)
        {
            if (o == null)
            {
                return 1; // null sorts before current
            }
            if (o is Complex)
            {
                return GetModulus().CompareTo(((Complex)o).GetModulus());
            }
            if (o is double)
            {
                return GetModulus().CompareTo((double)o);
            }
            if (o is ComplexF)
            {
                return GetModulus().CompareTo(((ComplexF)o).GetModulus());
            }
            if (o is float)
            {
                return GetModulus().CompareTo((float)o);
            }
            throw new ArgumentException();
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   This operator doesn't do much. :-)
        /// </summary>
        /// <param name = "a"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a)
        {
            return a;
        }

        /// <summary>
        ///   Negate the complex number
        /// </summary>
        /// <param name = "a"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a)
        {
            a.Re = -a.Re;
            a.Im = -a.Im;
            return a;
        }

        /// <summary>
        ///   Add a complex number to a real
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "f"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, double f)
        {
            a.Re = (a.Re + f);
            return a;
        }

        /// <summary>
        ///   Add a real to a complex number
        /// </summary>
        /// <param name = "f"></param>
        /// <param name = "a"></param>
        /// <returns></returns>
        public static Complex operator +(double f, Complex a)
        {
            a.Re = (a.Re + f);
            return a;
        }

        /// <summary>
        ///   Add to complex numbers
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, Complex b)
        {
            a.Re = a.Re + b.Re;
            a.Im = a.Im + b.Im;
            return a;
        }

        /// <summary>
        ///   Subtract a real from a complex number
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "f"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a, double f)
        {
            a.Re = (a.Re - f);
            return a;
        }

        /// <summary>
        ///   Subtract a complex number from a real
        /// </summary>
        /// <param name = "f"></param>
        /// <param name = "a"></param>
        /// <returns></returns>
        public static Complex operator -(double f, Complex a)
        {
            a.Re = (float)(f - a.Re);
            a.Im = (float)(0 - a.Im);
            return a;
        }

        /// <summary>
        ///   Subtract two complex numbers
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a, Complex b)
        {
            a.Re = a.Re - b.Re;
            a.Im = a.Im - b.Im;
            return a;
        }

        /// <summary>
        ///   Multiply a complex number by a real
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "f"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, double f)
        {
            a.Re = (a.Re * f);
            a.Im = (a.Im * f);
            return a;
        }

        /// <summary>
        ///   Multiply a real by a complex number
        /// </summary>
        /// <param name = "f"></param>
        /// <param name = "a"></param>
        /// <returns></returns>
        public static Complex operator *(double f, Complex a)
        {
            a.Re = (a.Re * f);
            a.Im = (a.Im * f);

            return a;
        }

        /// <summary>
        ///   Multiply two complex numbers together
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, Complex b)
        {
            // (x + yi)(u + vi) = (xu – yv) + (xv + yu)i. 
            double x = a.Re, y = a.Im;
            double u = b.Re, v = b.Im;

            a.Re = (x * u - y * v);
            a.Im = (x * v + y * u);

            return a;
        }

        /// <summary>
        ///   Divide a complex number by a real number
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "f"></param>
        /// <returns></returns>
        public static Complex operator /(Complex a, double f)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (f == 0) // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                throw new DivideByZeroException();
            }

            a.Re = (a.Re / f);
            a.Im = (a.Im / f);

            return a;
        }

        /// <summary>
        ///   Divide a complex number by a complex number
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <returns></returns>
        public static Complex operator /(Complex a, Complex b)
        {
            double x = a.Re, y = a.Im;
            double u = b.Re, v = b.Im;
            double denom = u * u + v * v;

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (denom == 0) // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                throw new DivideByZeroException();
            }

            a.Re = ((x * u + y * v) / denom);
            a.Im = ((y * u - x * v) / denom);

            return a;
        }

        /// <summary>
        ///   Parse a complex representation in this fashion: "( %f, %f )"
        /// </summary>
        /// <param name = "s"></param>
        /// <returns></returns>
        public static Complex Parse(string s)
        {
            throw new NotImplementedException("Complex Complex.Parse( string s ) is not implemented.");
        }

        /// <summary>
        ///   Get the string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("( {0}, {1}i )", Re, Im);
        }

        //-----------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------

        /// <summary>
        ///   Determine whether two complex numbers are almost (i.e. within the tolerance) equivalent.
        /// </summary>
        /// <param name = "a"></param>
        /// <param name = "b"></param>
        /// <param name = "tolerance"></param>
        /// <returns></returns>
        public static bool IsEqual(Complex a, Complex b, double tolerance)
        {
            return (Math.Abs(a.Re - b.Re) < tolerance) && (Math.Abs(a.Im - b.Im) < tolerance);
        }

        //----------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------

        /// <summary>
        ///   Represents zero
        /// </summary>
        public static Complex Zero
        {
            get
            {
                return new Complex(0, 0);
            }
        }

        /// <summary>
        ///   Represents the result of sqrt( -1 )
        /// </summary>
        public static Complex I
        {
            get
            {
                return new Complex(0, 1);
            }
        }

        /// <summary>
        ///   Represents the largest possible value of Complex.
        /// </summary>
        public static Complex MaxValue
        {
            get
            {
                return new Complex(double.MaxValue, double.MaxValue);
            }
        }

        /// <summary>
        ///   Represents the smallest possible value of Complex.
        /// </summary>
        public static Complex MinValue
        {
            get
            {
                return new Complex(double.MinValue, double.MinValue);
            }
        }
    }
}